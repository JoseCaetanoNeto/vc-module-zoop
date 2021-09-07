using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace Zoop.Web.Managers
{
    public class ZoopMethodBoleto : PaymentMethod
    {
        public ZoopMethodBoleto(IOptions<ZoopSecureOptions> options, IMemberService pMemberService, UserManager<ApplicationUser> pUserManager) : base(nameof(ZoopMethodBoleto))
        {
            _options = options?.Value ?? new ZoopSecureOptions();
            _memberService = pMemberService;
            _userManager = pUserManager;
        }

        private readonly ZoopSecureOptions _options;
        private readonly IMemberService _memberService;
        private readonly UserManager<ApplicationUser> _userManager;


        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Unknown;

        public override PaymentMethodGroupType PaymentMethodGroupType => PaymentMethodGroupType.Alternative;

        private string defaultSaller
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.DefaultSaller.Name,
                    ModuleConstants.Settings.ZoopBoleto.DefaultSaller.DefaultValue.ToString());
            }
        }

        private string VCmanagerURL
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.VCmanagerURL.Name,
                    ModuleConstants.Settings.ZoopBoleto.VCmanagerURL.DefaultValue.ToString());
            }
        }

        private string statusOrderOnWaitingConfirm
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.statusOrderOnWaitingConfirm.Name,
                    ModuleConstants.Settings.ZoopBoleto.statusOrderOnWaitingConfirm.DefaultValue.ToString());
            }
        }

        private string statusOrderOverdue
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.statusOrderOnOverdue.Name,
                    ModuleConstants.Settings.ZoopBoleto.statusOrderOnOverdue.DefaultValue.ToString());
            }
        }

        private string statusOrderOnPaid
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.statusOrderOnPaid.Name,
                    ModuleConstants.Settings.ZoopBoleto.statusOrderOnPaid.DefaultValue.ToString());
            }
        }

        private string Interest_Mode
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.interestMode.Name,
                   ModuleConstants.Settings.ZoopBoleto.interestMode.DefaultValue.ToString());
            }
        }

        private decimal Interest_Amount
        {
            get
            {
                return Convert.ToDecimal(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.interestAmount.Name,
                   ModuleConstants.Settings.ZoopBoleto.interestAmount.DefaultValue));
            }
        }

        private string LateFee_Mode
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.lateFeeMode.Name,
                   ModuleConstants.Settings.ZoopBoleto.lateFeeMode.DefaultValue.ToString());
            }
        }

        private decimal LateFee_Amount
        {
            get
            {
                return Convert.ToDecimal(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.lateFeeAmount.Name,
                   ModuleConstants.Settings.ZoopBoleto.lateFeeAmount.DefaultValue));
            }
        }

        private int ExpirationInDays
        {
            get
            {
                return Convert.ToInt32(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.ExpirationInDays.Name,
                   ModuleConstants.Settings.ZoopBoleto.ExpirationInDays.DefaultValue));
            }
        }

        private int PaymentLimitInDays
        {
            get
            {
                return Convert.ToInt32(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.PaymentLimitInDays.Name,
                   ModuleConstants.Settings.ZoopBoleto.PaymentLimitInDays.DefaultValue));
            }
        }

        private string Description
        {
            get
            {
                return Convert.ToString(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.Description.Name,
                   ModuleConstants.Settings.ZoopBoleto.Description.DefaultValue));
            }
        }

        private string UrlLogo
        {
            get
            {
                return Convert.ToString(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.UrlLogo.Name,
                   ModuleConstants.Settings.ZoopBoleto.UrlLogo.DefaultValue));
            }
        }

        private string BodyInstructions
        {
            get
            {
                return Convert.ToString(Settings?.GetSettingValue(ModuleConstants.Settings.ZoopBoleto.BodyInstructions.Name,
                   ModuleConstants.Settings.ZoopBoleto.BodyInstructions.DefaultValue));
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
                Task.Run(() => zoopService.registerWebHook(VCmanagerURL));
                var customer = GetCustomerAsync(order.CustomerId).GetAwaiter().GetResult();

                ModelApi.BuyerOut buyerOut = SenderBuyer(zoopService, order, customer as Contact);
                if (buyerOut.error != null && buyerOut.error.status_code != 0)
                {
                    retVal.IsSuccess = false;
                    retVal.ErrorMessage = buyerOut.error.message_display == null ? buyerOut.error.message : buyerOut.error.message_display;
                    return retVal;
                }

                ModelApi.TransactionBoletoOut transation = SenderTransactionBoleto(zoopService, payment, order, buyerOut);
                ApplyOrderStatus(order, statusOrderOnWaitingConfirm);
                payment.Status = PaymentStatus.Pending.ToString();
                retVal.OuterId = payment.OuterId = transation.Id;
                // aguarda retorno da zoop para mudar o status
                retVal.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Custom;
                if (transation.error != null && transation.error.status_code != 0)
                {
                    retVal.IsSuccess = false;
                    retVal.ErrorMessage = transation.error.message_display == null ? transation.error.message : transation.error.message_display;
                }
                else
                {
                    Task.Run(() => zoopService.SendMailBoletoTansation(transation.paymentMethod.Id));
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

        async Task<Member> GetCustomerAsync(string customerId)
        {
            // Try to find contact
            var result = await _memberService.GetByIdAsync(customerId);

            if (result == null)
            {
                var user = await _userManager.FindByIdAsync(customerId);

                if (user != null)
                {
                    result = await _memberService.GetByIdAsync(user.MemberId);
                }
            }

            return result;
        }

        private ModelApi.TransactionBoletoOut SenderTransactionBoleto(ZoopService zoopService, PaymentIn payment, CustomerOrder order, ModelApi.BuyerOut buyerOut)
        {
            var transactionInput = new ModelApi.TransactionBoletoIn
            {
                UrlLogo = (string.IsNullOrEmpty(UrlLogo) ? null : UrlLogo),
                ReferenceId = order.Id,
                Currency = order.Currency,
                Description = Description,
                OnBehalfOf = defaultSaller,
                Customer = buyerOut.Id,
                Amount = Convert.ToInt32(payment.Sum * 100),
                paymentMethod = new ModelApi.TransactionBoletoIn.PaymentMethod()
                {
                    BodyInstructions = new List<string>() {
                        BodyInstructions.Replace("#order.Number",order.Number), // configura��o
                    },
                    ExpirationDate = DateTime.Today.AddDays(ExpirationInDays),  // configura��o
                    PaymentLimitDate = DateTime.Today.AddDays(PaymentLimitInDays), // configura��o
                }
            };

            if (LateFee_Amount > 0 || Interest_Amount > 0)
            {
                ModelApi.TransactionBoletoIn.LateFee lateFee = null;
                ModelApi.TransactionBoletoIn.Interest lnterest = null;
                if (LateFee_Amount > 0)
                {
                    lateFee = new ModelApi.TransactionBoletoIn.LateFee()
                    {
                        Mode = LateFee_Mode
                    };
                    if (LateFee_Mode == "FIXED")
                        lateFee.Amount = Convert.ToInt32(LateFee_Amount * 100);
                    if (LateFee_Mode == "PERCENTAGE")
                        lateFee.Percentage = Convert.ToInt32(LateFee_Amount * 100);
                }

                if (Interest_Amount > 0)
                {
                    lnterest = new ModelApi.TransactionBoletoIn.Interest()
                    {
                        Mode = Interest_Mode
                    };
                    if (Interest_Mode == "DAILY_AMOUNT")
                        lnterest.Amount = Convert.ToInt32(Interest_Amount * 100);
                    if (Interest_Mode == "DAILY_PERCENTAGE" || Interest_Mode == "MONTHLY_PERCENTAGE")
                        lnterest.Percentage = Convert.ToInt32(Interest_Amount * 100);
                }

                /*Discount = new List<ModelApi.TransactionBoletoIn.Discount>() { 
                    new ModelApi.TransactionBoletoIn.Discount() { 
                        Amount = 0,         // configura��o
                        LimitDate = "" ,    // configura��o
                        Mode = ""           // configura��o
                    }
                },*/

                transactionInput.paymentMethod.BillingInstructions = new ModelApi.TransactionBoletoIn.BillingInstructions() { Interest = lnterest, LateFee = lateFee };
            }

            var transation = zoopService.NewBoletoTansation(transactionInput);
            return transation;
        }

        private static ModelApi.BuyerOut SenderBuyer(ZoopService zoopService, CustomerOrder order, Contact pCustomer)
        {

            string taxpayerIdValue = pCustomer.TaxPayerId;
            if (string.IsNullOrEmpty(taxpayerIdValue))
            {
                throw new InvalidOperationException($"'TaxpayerId' should not be null and of \"{nameof(Contact)}\" type.");
            }


            var address = order.Shipments.FirstOrDefault().DeliveryAddress;

            var buyer = new ModelApi.BuyerIn
            {
                Birthdate = pCustomer?.BirthDate,
                FirstName = address.FirstName,
                LastName = address.LastName,
                TaxpayerId = taxpayerIdValue,
                Email = address.Email,
                PhoneNumber = address.Phone,
                address = new ModelApi.BuyerIn.Address()
                {
                    City = address.City,
                    CountryCode = CountryCode.ConvertThreeCodeToTwoCode(address.CountryCode),
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

        private static object GetProperty(Member pCustomer, string pName)
        {
            if (pCustomer.DynamicProperties.Count == 0)
                return null;

            var propTaxpayerId = pCustomer.DynamicProperties.FirstOrDefault(p => p.Name == pName);
            object taxpayerIdValue = null;
            if (propTaxpayerId != null && propTaxpayerId.Values.Count > 0)
                taxpayerIdValue = propTaxpayerId.Values.FirstOrDefault().Value;
            return taxpayerIdValue;
        }

        private void ApplyOrderStatus(CustomerOrder order, string pNewStatusOrder)
        {
            if (!string.IsNullOrEmpty(pNewStatusOrder))
                order.Status = pNewStatusOrder;
        }

        public override CapturePaymentRequestResult CaptureProcessPayment(CapturePaymentRequest context)
        {
            var retVal = AbstractTypeFactory<CapturePaymentRequestResult>.TryCreateInstance();
            retVal.IsSuccess = true;
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

                if (history.OperationType == "invoice.paid" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOnPaid);
                    payment.Status = PaymentStatus.Paid.ToString();
                    payment.CapturedDate = DateTime.UtcNow;
                    payment.IsApproved = true;
                    payment.Comment = $"Paid successfully. Transaction Info {history.Id}, authorization code: {history.AuthorizationCode}{Environment.NewLine}";
                    payment.AuthorizedDate = DateTime.UtcNow;
                }
                else if (transation.Status == "invoice.overdue" && history.Status == "succeeded")
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Voided;
                    result.IsSuccess = true;
                    ApplyOrderStatus(order, statusOrderOverdue);
                    payment.Status = PaymentStatus.Voided.ToString();
                    payment.VoidedDate = DateTime.UtcNow;
                    payment.IsCancelled = true;
                }

                //"invoice.created",
                //"invoice.overdue",
                //"invoice.paid",
                // TODO: FALTA testar Refunded
                //"invoice.refunded",
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
                    //webhook � quem tem de fazer a volta
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