using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Zoop.Core;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace Zoop.Web.Managers
{
    public class ZoopMethodCard : PaymentMethod
    {
        IDynamicPropertySearchService _dynamicPropertySearchService;
        public ZoopMethodCard(IOptions<ZoopSecureOptions> options, IDynamicPropertySearchService dynamicPropertySearchService) : base(nameof(ZoopMethodCard))
        {
            _options = options?.Value ?? new ZoopSecureOptions();
            _dynamicPropertySearchService = dynamicPropertySearchService;
        }

        private readonly ZoopSecureOptions _options;

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.BankCard;

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

        private bool Capture
        {
            get
            {
                bool.TryParse(Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.Capture.Name,
                    ModuleConstants.Settings.Zoop.Capture.DefaultValue.ToString()), out bool cap);
                return cap;
            }
        }

        private int MaxNumberInstallments
        {
            get
            {
                int.TryParse(Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.MaxNumberInstallments.Name,
                    ModuleConstants.Settings.Zoop.MaxNumberInstallments.DefaultValue.ToString()), out int max);
                return max;
            }
        }

        private string installmentPlan
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.installmentPlan.Name,
                    ModuleConstants.Settings.Zoop.installmentPlan.DefaultValue.ToString());
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

        private string statusOrderOnPaid
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.Zoop.statusOrderOnPaid.Name,
                    ModuleConstants.Settings.Zoop.statusOrderOnPaid.DefaultValue.ToString());
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
            var bankCardInfo = request.BankCardInfo ?? throw new InvalidOperationException($"\"{nameof(request.BankCardInfo)}\" should not be null and of \"{nameof(BankCardInfo)}\" type.");

            var retVal = AbstractTypeFactory<ProcessPaymentRequestResult>.TryCreateInstance();
            if (payment.PaymentStatus == PaymentStatus.Paid)
            {
                //return to thanks page
                retVal.IsSuccess = true;
                return retVal;
            }

            int numberInstallments = 1;
            int.TryParse(bankCardInfo.BankCardType, out numberInstallments);

            if (numberInstallments > MaxNumberInstallments)
            {
                retVal.ErrorMessage = "Invalid number Installments";
                retVal.IsSuccess = false;
                return retVal;
            }

            IList<DynamicProperty> resultSearch = _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria() { ObjectType = "VirtoCommerce.OrdersModule.Core.Model.PaymentIn" }).GetAwaiter().GetResult().Results;

            resultSearch.SetDynamicProp(payment, ModuleConstants.K_numberIntallments, numberInstallments);

            var transactionInput = new ModelApi.TransactionIn
            {
                ReferenceId = order.Id,
                OnBehalfOf = defaultSaller,
                Amount = Convert.ToInt32(payment.Sum * 100),
                Capture = Capture,
                StatementDescriptor = store.Name,
                installmentPlan = new ModelApi.TransactionIn.InstallmentPlan { NumberInstallments = numberInstallments, Mode = installmentPlan },
                source = new ModelApi.TransactionIn.Source
                {
                    Type = "card",
                    Card = new ModelApi.TransactionIn.Card
                    {
                        CardNumber = bankCardInfo.BankCardNumber,
                        ExpirationMonth = bankCardInfo.BankCardMonth.ToString(),
                        ExpirationYear = bankCardInfo.BankCardYear.ToString(),
                        HolderName = bankCardInfo.CardholderName,
                        SecurityCode = bankCardInfo.BankCardCVV2
                    }
                }
            };

            try
            {
                ZoopService zoopService = new ZoopService(_options.marketplace_id, _options.applycation_id);
                Task.Run(() => zoopService.registerWebHook(VCmanagerURL));
                var transation = zoopService.NewCardTansation(transactionInput);
                payment.Status = PaymentStatus.Pending.ToString();
                retVal.OuterId = payment.OuterId = transation.Id;
                // aguarda retorno da zoop para mudar o status
                retVal.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Custom;
                if (transation.error != null && transation.error.status_code != 0)
                {
                    ApplyOrderStatus(order, statusOrderOnCancelAuthorization);
                    retVal.IsSuccess = false;
                    retVal.ErrorMessage = transation.error.message_display == null ? transation.error.message : transation.error.message_display;
                }
                else
                {
                    resultSearch.SetDynamicProp(payment, ModuleConstants.K_Installment_plan, installmentPlan);
                    resultSearch.SetDynamicProp(payment, ModuleConstants.K_Zoop_Fee, transation.fees.Total);

                    ApplyOrderStatus(order, statusOrderOnWaitingConfirm);
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
            decimal AmountPay = 0;
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
                    ResponseData = JsonConvert.SerializeObject(history, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                });

                if (history.OperationType == "authorization" && history.Status == "succeeded")
                {
                    AmountPay += history.Amount;

                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
                    result.IsSuccess = true;

                    payment.Status = PaymentStatus.Paid.ToString();
                    payment.CapturedDate = DateTime.UtcNow;
                    payment.IsApproved = true;
                    payment.Comment += $"Paid successfully. Transaction Info {history.Id}, authorization code: {history.AuthorizationCode}{Environment.NewLine}";
                    payment.AuthorizedDate = DateTime.UtcNow;
                    var PaymentTotal = order.InPayments.Where(p => p.PaymentStatus == PaymentStatus.Paid).Sum(p => p.Sum) + AmountPay;
                    if (PaymentTotal >= order.Total)
                        ApplyOrderStatus(order, statusOrderOnPaid);

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
