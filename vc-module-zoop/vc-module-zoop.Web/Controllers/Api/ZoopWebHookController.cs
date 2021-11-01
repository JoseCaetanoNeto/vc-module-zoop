using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Zoop.Web.Services;
using Zoop.ModelApi;

namespace VirtoCommerce.Zoop.Web.Controllers.Api
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/payments/zoopwebhook")]
    public class ZoopWebHookController : Controller
    {
        private readonly IZoopRegisterPaymentService _ZoopRegisterPaymentService;

        public ZoopWebHookController(IZoopRegisterPaymentService zoopRegisterPaymentService)
        {
            _ZoopRegisterPaymentService = zoopRegisterPaymentService;
        }

        [HttpPost]
        [Route("registerpayment")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult> RegisterPayment([FromBody] WebhookIn gatewayPagamento)
        {
            object orderId = gatewayPagamento?.payload?.@object?["reference_id"];
            var result = await _ZoopRegisterPaymentService.CallbackPaymentAsync(System.Convert.ToString(orderId), gatewayPagamento);
            return !string.IsNullOrEmpty(result) ? Ok(result) : (ActionResult)NoContent();
        }
    }
}
