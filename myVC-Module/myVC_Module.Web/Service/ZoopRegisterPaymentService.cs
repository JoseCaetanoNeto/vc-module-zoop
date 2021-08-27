using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.PaymentModule.Model.Requests;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;
using Zoop.ModelApi;
using Zoop.Web.Managers;
using Newtonsoft.Json;
using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.Zoop.Web.Services
{
    public class ZoopRegisterPaymentService : IZoopRegisterPaymentService
    {
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IStoreService _storeService;
        private readonly IPaymentMethodsSearchService _paymentMethodsSearchService;

        public ZoopRegisterPaymentService(ICustomerOrderService customerOrderService, IStoreService storeService, IPaymentMethodsSearchService paymentMethodsSearchService)
        {
            _customerOrderService = customerOrderService;
            _storeService = storeService;
            _paymentMethodsSearchService = paymentMethodsSearchService;
        }

        public async Task<string> CallbackPaymentAsync(string orderId, WebhookIn paymentParameters)
        {
            if (string.IsNullOrEmpty(orderId))
                return null;

            switch (paymentParameters.type)
            {
                //case "buyer.transaction.canceled":
                //case "buyer.transaction.charged_back":
                //case "buyer.transaction.commission.succeeded":
                //case "buyer.transaction.dispute.succeeded":
                //case "buyer.transaction.disputed":
                //case "buyer.transaction.failed":
                //case "buyer.transaction.reversed":
                //case "buyer.transaction.succeeded":
                //case "buyer.transaction.updated":
                case "transaction.pre_authorization.succeeded":
                case "transaction.pre_authorized":
                case "transaction.canceled":
                case "transaction.charged_back":
                //case "transaction.commission.succeeded":
                //case "transaction.dispute.succeeded":
                //case "transaction.disputed":
                case "transaction.failed":
                case "transaction.reversed":
                case "transaction.succeeded":
                case "transaction.updated":
                    break;
                default:
                    return null;
            }

            string result = null;
            var order = (await _customerOrderService.GetByIdsAsync(new[] { orderId })).FirstOrDefault();
            if (order == null)
            {
                throw new ArgumentException("Order for specified orderId not found.", "orderId");
            }

            var store = await _storeService.GetByIdAsync(order.StoreId);

            var paymentMethodsSearchCriteria = AbstractTypeFactory<PaymentMethodsSearchCriteria>.TryCreateInstance();
            paymentMethodsSearchCriteria.StoreId = store.Id;
            paymentMethodsSearchCriteria.Codes = new[] { nameof(ZoopMethod) };
            paymentMethodsSearchCriteria.IsActive = true;

            var authorizePaymentMethods = await _paymentMethodsSearchService.SearchPaymentMethodsAsync(paymentMethodsSearchCriteria);
            var paymentMethod = authorizePaymentMethods.Results.FirstOrDefault(x => x.Code.EqualsInvariant(nameof(ZoopMethod)));

            if (paymentMethod != null)
            {
                PaymentIn payment = order.InPayments.FirstOrDefault(x => x.GatewayCode.EqualsInvariant(nameof(ZoopMethod)) && x.OuterId == paymentParameters.payload.@object.Id);
                if (payment == null)
                {
                    return null;
                }

                var context = new PostProcessPaymentRequest
                {
                    Order = order,
                    Payment = payment,
                    Store = store,
                    OuterId = paymentParameters.payload.@object.Id,
                    Parameters = new NameValueCollection() { { "x_TransactionOut", JsonConvert.SerializeObject(paymentParameters.payload.@object) } }
                };

                var retVal = paymentMethod.PostProcessPayment(context);

                await _customerOrderService.SaveChangesAsync(new[] { order });
            }
            return result;
        }
    }
}
