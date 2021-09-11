using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;

namespace Zoop.Web.Validation
{
    public class ZoopPaymentInValidator : AbstractValidator<PaymentIn>
    {
        public ZoopPaymentInValidator(IPaymentService pOrderService)
        {
            RuleFor(payment => payment).Custom((newPaymentRequest, context) =>
            {
                var paymentIn = pOrderService.GetByIdAsync(newPaymentRequest.Id).GetAwaiter().GetResult();
                if (paymentIn != null)
                {
                    if (paymentIn.Sum != newPaymentRequest.Sum && !string.IsNullOrEmpty(paymentIn.OuterId))
                    {
                        context.AddFailure("Pagamento já enviado para Gateway de pagamento não pode ser alterado valor!");
                    }
                }
            });
        }
    }
}
