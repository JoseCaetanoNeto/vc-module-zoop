using System.Threading.Tasks;
using Zoop.ModelApi;

namespace VirtoCommerce.Zoop.Web.Services
{
    public interface IZoopRegisterPaymentService
    {
        Task<string> CallbackPaymentAsync(string orderId, WebhookIn paymentParameters);
    }
}
