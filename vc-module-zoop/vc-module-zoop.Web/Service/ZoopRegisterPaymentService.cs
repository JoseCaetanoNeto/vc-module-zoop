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
using Zoop.Web;

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

            if (!ZoopService.s_needEvents.Contains(paymentParameters.type))
                return null;

            string outerId = paymentParameters.payload.@object["id"].ToString();
            string result = null;
            var order = (await _customerOrderService.GetByIdsAsync(new[] { orderId })).FirstOrDefault();
            if (order == null)
            {
                throw new ArgumentException("Order for specified orderId not found.", "orderId");
            }

            PaymentIn payment = order.InPayments.FirstOrDefault(x => (x.GatewayCode.EqualsInvariant(nameof(ZoopMethodCard)) || x.GatewayCode.EqualsInvariant(nameof(ZoopMethodBoleto))) && x.OuterId == outerId);
            if (payment == null)
            {
                return null;
            }

            var store = await _storeService.GetByIdAsync(order.StoreId);

            var context = new PostProcessPaymentRequest
            {
                Order = order,
                Payment = payment,
                Store = store,
                OuterId = outerId,
                Parameters = new NameValueCollection() { { "x_TransactionOut", JsonConvert.SerializeObject(paymentParameters.payload.@object) } }
            };


            var paymentMethodsSearchCriteria = AbstractTypeFactory<PaymentMethodsSearchCriteria>.TryCreateInstance();
            paymentMethodsSearchCriteria.StoreId = store.Id;
            paymentMethodsSearchCriteria.Codes = new[] { payment.GatewayCode };
            paymentMethodsSearchCriteria.IsActive = true;
            var authorizePaymentMethods = await _paymentMethodsSearchService.SearchPaymentMethodsAsync(paymentMethodsSearchCriteria);

            var paymentMethod = authorizePaymentMethods.Results.FirstOrDefault();
            if (paymentMethod != null)
            {
                var retVal = paymentMethod.PostProcessPayment(context);

                await _customerOrderService.SaveChangesAsync(new[] { order });
            }
            return result;
        }
    }
}
