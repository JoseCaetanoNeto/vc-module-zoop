using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model.Search;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Zoop.Core;
using Zoop.Web.Managers;

namespace Zoop.Data.BackgroundJobs
{
    public class BoletoJob
    {
        private readonly ILogger _log;
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly ICustomerOrderService _orderService;
        private readonly IPaymentMethodsSearchService _paymentMethodsSearchService;

        public BoletoJob(ISettingsManager settingsManager, ILogger<BoletoJob> log, Func<IOrderRepository> repositoryFactory, ICustomerOrderService orderService, IPaymentMethodsSearchService paymentMethodsSearchService)
        {
            _settingsManager = settingsManager;
            _log = log;
            _repositoryFactory = repositoryFactory;
            _orderService = orderService;
            _paymentMethodsSearchService = paymentMethodsSearchService;
        }

        [DisableConcurrentExecution(10)]
        public async Task Process()
        {
            _log.LogTrace($"Start processing {nameof(BoletoJob)} job");

            using (var repository = _repositoryFactory())
            {
                var paymentMethodsSearchCriteria = AbstractTypeFactory<PaymentMethodsSearchCriteria>.TryCreateInstance();
                paymentMethodsSearchCriteria.Codes = new[] { nameof(ZoopMethodBoleto) };
                paymentMethodsSearchCriteria.IsActive = true;
                var authorizePaymentMethods = await _paymentMethodsSearchService.SearchPaymentMethodsAsync(paymentMethodsSearchCriteria);
                foreach (var authorizePaymentMethod in authorizePaymentMethods.Results)
                {
                    string statusOrderOnWaitingConfirm = Convert.ToString(_settingsManager.GetObjectSettings(ModuleConstants.Settings.ZoopBoleto.statusOrderOnWaitingConfirm.Name, nameof(ZoopMethodBoleto), authorizePaymentMethod.Id));
                    string statusOrderOnOverdue = Convert.ToString(_settingsManager.GetObjectSettings(ModuleConstants.Settings.ZoopBoleto.statusOrderOnOverdue.Name, nameof(ZoopMethodBoleto), authorizePaymentMethod.Id));
                    int days = Convert.ToInt32(_settingsManager.GetObjectSettings(ModuleConstants.Settings.ZoopBoleto.CompensationDays.Name, nameof(ZoopMethodBoleto), authorizePaymentMethod.Id));

                    try
                    {
                        var query = repository.InPayments.Where(p => p.GatewayCode == nameof(ZoopMethodBoleto) && p.CustomerOrder.Status == statusOrderOnWaitingConfirm && !p.IsCancelled && p.DynamicPropertyObjectValues.Any(d => d.PropertyName == ModuleConstants.K_Expiration_Date && d.DateTimeValue < DateTime.Now.AddDays(-days)));

                        var ids = await query.Select(p => new { CustomerOrderId = p.CustomerOrderId, PaymentId = p.Id }).ToListAsync();
                        if (ids.Count == 0)
                            continue;

                        var grupo = ids.GroupBy(i => i.CustomerOrderId).Select(s => new { id = s.Key, payments = s.Select(x => x.PaymentId).ToArray() }).ToDictionary(x => x.id);

                        var orders = await _orderService.GetByIdsAsync(ids.Select(i => i.CustomerOrderId).ToArray());
                        foreach (var order in orders)
                        {
                            order.Status = statusOrderOnOverdue;
                            var paymentsIds = grupo[order.Id].payments;
                            var payments = order.InPayments.Where(p => paymentsIds.Contains(p.Id));

                            foreach (var payment in payments)
                            {
                                payment.PaymentStatus = PaymentStatus.Voided;
                                payment.Comment += $"OVERDUE {Environment.NewLine} ";
                            }
                        }

                        await _orderService.SaveChangesAsync(orders);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex.ToString());
                        continue;
                    }
                }
            }

            _log.LogTrace($"Complete processing {nameof(BoletoJob)} job");
        }
    }
}
