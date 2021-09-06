using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace Zoop.Web.Managers
{
    public class ZoopMethodBoleto : PaymentMethod
    {
        public ZoopMethodBoleto(IOptions<ZoopSecureOptions> options, IMemberService pMemberService) : base(nameof(ZoopMethodBoleto))
        {
            _options = options?.Value ?? new ZoopSecureOptions();
            _memberService = pMemberService;
        }

        private readonly ZoopSecureOptions _options;
        private readonly IMemberService _memberService;

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Unknown;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;

        private string defaultSaller
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.DefaultSaller.Name,
                    ModuleConstants.Settings.Zoop.DefaultSaller.DefaultValue.ToString());
            }
        }

        private string VCmanagerURL
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.VCmanagerURL.Name,
                    ModuleConstants.Settings.Zoop.VCmanagerURL.DefaultValue.ToString());
            }
        }

        private string statusOrderOnWaitingConfirm
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnWaitingConfirm.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnWaitingConfirm.DefaultValue.ToString());
            }
        }

        private string statusOrderOnAuthorization
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnAuthorization.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnAuthorization.DefaultValue.ToString());
            }
        }

        private string statusOrderOnFailedAuthorization
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnFailedAuthorization.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnFailedAuthorization.DefaultValue.ToString());
            }
        }

        private string statusOrderOnPreAuthorization
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnPreAuthorization.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnPreAuthorization.DefaultValue.ToString());
            }
        }

        private string statusOrderOnFailedPreAuthorization
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnFailedPreAuthorization.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnFailedPreAuthorization.DefaultValue.ToString());
            }
        }

        private string statusOrderOnCancelAuthorization
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnCancelAuthorization.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnCancelAuthorization.DefaultValue.ToString());
            }
        }

        public override ProcessPaymentRequestResult ProcessPayment(ProcessPaymentRequest request)
        {

            var payment = request.Payment as PaymentIn ?? throw new InvalidOperationException($"\"{nameof(request.Payment)}\" should not be null and of \"{nameof(PaymentIn)}\" type.");
            var order = request.Order as CustomerOrder ?? throw new InvalidOperationException($"\"{nameof(request.Order)}\" should not be null and of \"{nameof(CustomerOrder)}\" type.");
#pragma warning disable S1481 // Unused local variables should be removed
            // Need to check shop existence, though not using it
            var store = request.Store as Store ?? throw new InvalidOperationException($"\"{nameof(request.Store)}\" should not be null and of \"{nameof(Store)}\" type.");
#pragma warning restore S1481 // Unused local variables should be removed

            var retVal = AbstractTypeFactory<ProcessPaymentRequestResult>.TryCreateInstance();
            if (payment.PaymentStatus == PaymentStatus.Paid)
            {
                //return to thanks page
                retVal.IsSuccess = true;
                return retVal;
            }

            ZoopService zoopService = new ZoopService(_options.marketplace_id, _options.applycation_id);

            try
            {
                zoopService.registerWebHook(VCmanagerURL);

                ModelApi.BuyerOut buyerOut = SenderBuyer(order, zoopService, request.Parameters);
                if (buyerOut.error != null && buyerOut.error.status_code != 0)
                {
                    retVal.IsSuccess = false;
                    retVal.ErrorMessage = buyerOut.error.message_display;
                    return retVal;
                }

                ModelApi.TransactionOut transation = SenderTransactionBoleto(payment, order, zoopService, buyerOut);
                zoopService.SendMailBoletoTansation(transation.Id);
                ApplyOrderStatus(order, statusOrderOnWaitingConfirm);
                payment.Status = PaymentStatus.Pending.ToString();
                retVal.OuterId = payment.OuterId = transation.Id;
                // aguarda retorno da zoop para mudar o status
                retVal.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Custom;
                if (transation.error != null && transation.error.status_code != 0)
                {
                    retVal.IsSuccess = false;
                    retVal.ErrorMessage = transation.error.message_display;
                }
                else
                {
                    retVal.IsSuccess = true;
                }
                return retVal;
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.IsSuccess = false;
                payment.VoidedDate = DateTime.UtcNow;
                retVal.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Voided;
            }

            return retVal;
        }

        private ModelApi.TransactionOut SenderTransactionBoleto(PaymentIn payment, CustomerOrder order, ZoopService zoopService, ModelApi.BuyerOut buyerOut)
        {
            var transactionInput = new ModelApi.TransactionBoletoIn
            {
                ReferenceId = order.Id,
                Currency = order.Currency,
                Description = "Vende", // configuração
                OnBehalfOf = defaultSaller,
                Customer = buyerOut.Id,
                Amount = Convert.ToInt32(payment.Sum * 100),
                paymentMethod = new ModelApi.TransactionBoletoIn.PaymentMethod()
                {
                    BodyInstructions = new List<string>() {
                        "Pedido #" + order.Number, // configuração
                    },
                    /*ExpirationDate = "",
                    PaymentLimitDate = "",*/
                    BillingInstructions = new ModelApi.TransactionBoletoIn.BillingInstructions()
                    {
                        Interest = new ModelApi.TransactionBoletoIn.Interest()
                        {
                            Amount = 0,           // configuração
                            Mode = "DAILY_AMOUNT" // configuração
                        },
                        /*Discount = new List<ModelApi.TransactionBoletoIn.Discount>() { 
                            new ModelApi.TransactionBoletoIn.Discount() { 
                                Amount = 0,         // configuração
                                LimitDate = "" ,    // configuração
                                Mode = ""           // configuração
                            }
                        },*/
                        LateFee = new ModelApi.TransactionBoletoIn.LateFee()
                        {
                            Amount = 0,             // configuração
                            Mode = "FIXED"          // configuração
                        }
                    }
                }
            };

            var transation = zoopService.NewBoletoTansation(transactionInput);
            return transation;
        }

        private static ModelApi.BuyerOut SenderBuyer(CustomerOrder order, ZoopService zoopService, NameValueCollection pParam)
        {
            var address = order.Shipments.FirstOrDefault().DeliveryAddress;
            var buyer = new ModelApi.BuyerIn
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                TaxpayerId = pParam != null ? pParam["TaxpayerId"] : null, // TODO: FALTA CPF
                Email = address.Email,
                PhoneNumber = address.Phone,
                address = new ModelApi.BuyerIn.Address()
                {
                    City = address.City,
                    CountryCode = address.CountryCode,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Neighborhood = address.RegionName,
                    PostalCode = address.PostalCode,
                    State = address.RegionId
                },
            };

            var buyerOut = zoopService.UpdateBuyer(buyer);
            return buyerOut;
        }

        private void ApplyOrderStatus(CustomerOrder order, string pNewStatusOrder)
        {
            if (!string.IsNullOrEmpty(pNewStatusOrder))
                order.Status = pNewStatusOrder;
        }

        public override CapturePaymentRequestResult CaptureProcessPayment(CapturePaymentRequest context)
        {

            var payment = context.Payment as PaymentIn ?? throw new InvalidOperationException($"\"{nameof(context.Payment)}\" should not be null and of \"{nameof(PaymentIn)}\" type.");


            var input = new ModelApi.VoidCaptureTransactionIn
            {
                Amount = Convert.ToInt32(payment.Sum * 100),
                OnBehalfOf = defaultSaller
            };

            var retVal = AbstractTypeFactory<CapturePaymentRequestResult>.TryCreateInstance();

            ZoopService zoopService = new ZoopService(_options.marketplace_id, _options.applycation_id);
            try
            {
                ModelApi.TransactionOut transation = zoopService.CaptureCardTansacton(payment.OuterId, input);


                if (transation.error != null && transation.error.status_code != 0)
                {
                    retVal.ErrorMessage = transation.error.message;
                    retVal.NewPaymentStatus = PaymentStatus.Error;
                    retVal.IsSuccess = false;
                }
                else
                {
                    payment.CapturedDate = DateTime.UtcNow;
                    retVal.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.NewPaymentStatus = PaymentStatus.Error;
                retVal.IsSuccess = false;
            }

            return retVal;
        }

        public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
        {
            var result = AbstractTypeFactory<PostProcessPaymentRequestResult>.TryCreateInstance();

            var payment = request.Payment as PaymentIn ?? throw new InvalidOperationException($"\"{nameof(request.Payment)}\" should not be null and of \"{nameof(PaymentIn)}\" type.");
            var order = request.Order as CustomerOrder ?? throw new InvalidOperationException($"\"{nameof(request.Order)}\" should not be null and of \"{nameof(CustomerOrder)}\" type.");
#pragma warning disable S1481 // Unused local variables should be removed
            // Need to check shop existence, though not using it
            var store = request.Store as Store ?? throw new InvalidOperationException($"\"{nameof(request.Store)}\" should not be null and of \"{nameof(Store)}\" type.");
#pragma warning restore S1481 // Unused local variables should be removed

            var transation = JsonConvert.DeserializeObject<ModelApi.TransactionOut>(request.Parameters["x_TransactionOut"]);
            var CreatedDate = DateTime.MinValue;

            if (payment.Transactions.Count > 0)
                CreatedDate = payment.Transactions.Max(o => o.CreatedDate);

            var historys = transation.history.Where(o => o.UpdatedAt > CreatedDate).ToList();
            foreach (var history in historys)
            {
                //"id": "bca7c3d3bc8849afbe6cb533f423b639",
                //"transaction": "607c1b9d449f43debcd8b266f9eb482a",
                //"authorizer": "cielo",
                //"authorizer_id": "000260004",
                //"amount": "160.00",
                //"operation_type": "authorization",
                //"status": "succeeded",
                //"response_code": "00",
                //"authorization_code": "133937",
                //"authorization_nsu": "20180510122911535",
                //"gatewayResponseTime": "4",
                //"created_at": "2021-08-24 12:59:55",
                //"updated_at": "2021-08-24 12:59:55"



                payment.Transactions.Add(new PaymentGatewayTransaction()
                {
                    Note = $"Transaction Info {history.Id}",
                    Status = history.OperationType,
                    ResponseCode = history.Status,
                    CurrencyCode = payment.Currency.ToString(),
                    Amount = history.Amount,
                    IsProcessed = true,
                    ProcessedDate = DateTime.UtcNow,
                    CreatedDate = history.UpdatedAt.Value,
                    ResponseData = JsonConvert.SerializeObject(history)
                });

                if (history.OperationType == "authorization" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOnAuthorization);
                    payment.Status = PaymentStatus.Paid.ToString();
                    payment.CapturedDate = DateTime.UtcNow;
                    payment.IsApproved = true;
                    payment.Comment = $"Paid successfully. Transaction Info {history.Id}, authorization code: {history.AuthorizationCode}{Environment.NewLine}";
                    payment.AuthorizedDate = DateTime.UtcNow;
                }
                else if (history.OperationType == "authorization" && history.Status == "failed")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Voided;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOnFailedAuthorization);
                    payment.Status = PaymentStatus.Voided.ToString();
                    payment.VoidedDate = DateTime.UtcNow;
                    payment.IsCancelled = true;
                }
                else if (history.OperationType == "pre_authorization" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Authorized;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOnPreAuthorization);
                    payment.Status = PaymentStatus.Authorized.ToString();
                    payment.AuthorizedDate = DateTime.UtcNow;
                }
                else if (history.OperationType == "pre_authorization" && history.Status == "failed")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Voided;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOnFailedPreAuthorization);
                    payment.Status = PaymentStatus.Voided.ToString();
                    payment.VoidedDate = DateTime.UtcNow;
                    payment.IsCancelled = true;
                }
                else if (transation.Status == "canceled" && history.OperationType == "void" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Cancelled;
                    result.IsSuccess = true;

                    if (!order.IsCancelled)
                        ApplyOrderStatus(order, statusOrderOnCancelAuthorization);

                    payment.Status = PaymentStatus.Cancelled.ToString();
                    payment.CancelledDate = DateTime.UtcNow;
                    payment.IsCancelled = true;
                }
                // TODO: FALTA TESTAR!! Incluir na jorndar de homologação
                else if (transation.Status == "charged_back" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Refunded;
                    result.IsSuccess = true;

                    payment.Status = PaymentStatus.Refunded.ToString();
                    payment.CancelledDate = DateTime.UtcNow;
                    payment.IsCancelled = true;
                }
            }
            return result;
        }

        public override RefundPaymentRequestResult RefundProcessPayment(RefundPaymentRequest context)
        {
            var payment = context.Payment as PaymentIn ?? throw new InvalidOperationException($"\"{nameof(context.Payment)}\" should not be null and of \"{nameof(PaymentIn)}\" type.");


            var input = new ModelApi.VoidCaptureTransactionIn
            {
                Amount = Convert.ToInt32(payment.Sum * 100),
                OnBehalfOf = defaultSaller
            };

            var retVal = AbstractTypeFactory<RefundPaymentRequestResult>.TryCreateInstance();

            ZoopService zoopService = new ZoopService(_options.marketplace_id, _options.applycation_id);
            try
            {
                ModelApi.TransactionOut transation = zoopService.VoidCardTansacton(payment.OuterId, input);


                if (transation.error != null && transation.error.status_code != 0)
                {
                    retVal.ErrorMessage = transation.error.message;
                    retVal.NewPaymentStatus = PaymentStatus.Error;
                    retVal.IsSuccess = false;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Refunded;
                    payment.IsCancelled = true;
                    payment.CancelledDate = DateTime.UtcNow;
                    retVal.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.NewPaymentStatus = PaymentStatus.Error;
                retVal.IsSuccess = false;
            }

            return retVal;
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            var retVal = AbstractTypeFactory<ValidatePostProcessRequestResult>.TryCreateInstance();
            retVal.IsSuccess = true;
            return retVal;
        }

        public override VoidPaymentRequestResult VoidProcessPayment(VoidPaymentRequest context)
        {
            var payment = context.Payment as PaymentIn ?? throw new InvalidOperationException($"\"{nameof(context.Payment)}\" should not be null and of \"{nameof(PaymentIn)}\" type.");


            var input = new ModelApi.VoidCaptureTransactionIn
            {
                Amount = Convert.ToInt32(payment.Sum * 100),
                OnBehalfOf = defaultSaller
            };

            var retVal = AbstractTypeFactory<VoidPaymentRequestResult>.TryCreateInstance();

            ZoopService zoopService = new ZoopService(_options.marketplace_id, _options.applycation_id);
            try
            {
                ModelApi.TransactionOut transation = zoopService.VoidCardTansacton(payment.OuterId, input);

                if (transation.error != null && transation.error.status_code != 0)
                {
                    retVal.ErrorMessage = transation.error.message;
                    retVal.NewPaymentStatus = PaymentStatus.Error;
                    retVal.IsSuccess = false;
                }
                else
                {
                    retVal.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Voided;
                    //webhook é quem tem de fazer a volta
                    //payment.IsCancelled = true;
                    //payment.VoidedDate = DateTime.UtcNow;
                    retVal.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = ex.Message;
                retVal.NewPaymentStatus = PaymentStatus.Error;
                retVal.IsSuccess = false;
            }

            return retVal;
        }

    }
}
