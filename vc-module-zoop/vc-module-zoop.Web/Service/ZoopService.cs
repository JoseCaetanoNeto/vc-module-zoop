using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoop.Web
{
    public class ZoopService
    {
        const string urlbase = "https://api.zoop.ws";
        const string C_NewTansationEndPoint = "{urlbase}/v2/marketplaces/{marketplace_id}/transactions";

        const string C_CapturaTransationEndPoint = "{urlbase}/v2/marketplaces/{marketplace_id}/transactions/{transaction_id}/capture";
        const string C_WebHookEndPoid = "{urlbase}/v1/marketplaces/{marketplace_id}/webhooks";
        const string C_VoidTransactionCard = "{urlbase}/v2/marketplaces/{marketplace_id}/transactions/{transaction_id}/void";
        const string C_WebHook = "/api/payments/zoopwebhook/registerpayment";
        const string C_EnviarBoleto = "{urlbase}/v1/marketplaces/{marketplace_id}/boletos/{transaction_id}/emails";
        const string C_NewTansationBoletoEndPoint = "{urlbase}/v1/marketplaces/{marketplace_id}/transactions";
        const string C_NewBuyer = "{urlbase}/v1/marketplaces/{marketplace_id}/buyers";
        const string C_UpdateBuyer = "{urlbase}/v1/marketplaces/{marketplace_id}/buyers/";
        const string C_SearchBuyer = "{urlbase}/v1/marketplaces/{marketplace_id}/buyers/search?taxpayer_id=";

        readonly string m_Marketplace_id;
        readonly string m_applycation_id;

        public static readonly List<string> s_needEvents = new List<string> {
                //"invoice.created",
                //"invoice.overdue",
                //"invoice.paid",
                //"invoice.refunded",
                "transaction.pre_authorization.succeeded",
                "transaction.pre_authorized",
                "transaction.canceled",
                "transaction.charged_back",
                "transaction.commission.succeeded",
                "transaction.dispute.succeeded",
                "transaction.disputed",
                "transaction.failed",
                "transaction.reversed",
                "transaction.succeeded",
                "transaction.updated",
                "transaction.created"
            };

        public ZoopService(string pMarketplace_id, string pApplycation_id)
        {
            m_Marketplace_id = pMarketplace_id;
            m_applycation_id = pApplycation_id;
        }

        public ModelApi.TransactionOut NewCardTansation(ModelApi.TransactionIn pTransactionIn)
        {
            var transation = ConexoesApi.EfetuarChamadaApi<ModelApi.TransactionOut>(BuildUrl(C_NewTansationEndPoint), GetBasicAut(), "POST", pTransactionIn);
            return transation;
        }

        public ModelApi.TransactionOut CaptureCardTansacton(string pTransactionId, ModelApi.VoidCaptureTransactionIn input)
        {
            return ConexoesApi.EfetuarChamadaApi<ModelApi.TransactionOut>(BuildUrl(C_CapturaTransationEndPoint, pTransactionId), GetBasicAut(), "POST", input);
        }

        public ModelApi.TransactionOut VoidCardTansacton(string pTransactionId, ModelApi.VoidCaptureTransactionIn input)
        {
            return ConexoesApi.EfetuarChamadaApi<ModelApi.TransactionOut>(BuildUrl(C_VoidTransactionCard, pTransactionId), GetBasicAut(), "POST", input);
        }

        public ModelApi.TransactionBoletoOut NewBoletoTansation(ModelApi.TransactionBoletoIn pTransactionIn)
        {
            pTransactionIn.PaymentType = "boleto";
            var transation = ConexoesApi.EfetuarChamadaApi<ModelApi.TransactionBoletoOut>(BuildUrl(C_NewTansationBoletoEndPoint), GetBasicAut(), "POST", pTransactionIn);
            return transation;
        }

        public ModelApi.TransactionBoletoOut GetBoletoTansation(string pTransactionId)
        {
            var transation = ConexoesApi.EfetuarChamadaApi<ModelApi.TransactionBoletoOut>(BuildUrl(C_NewTansationBoletoEndPoint) + "/" + pTransactionId, GetBasicAut(), "GET");
            return transation;
        }

        public void SendMailBoletoTansation(string pTransactionId)
        {
            ConexoesApi.EfetuarChamadaApi<ModelApi.Generic>(BuildUrl(C_EnviarBoleto, pTransactionId), GetBasicAut(), "POST");
        }

        public ModelApi.BuyerOut UpdateBuyer(ModelApi.BuyerIn pBuyerIn)
        {
            var buyer = ConexoesApi.EfetuarChamadaApi<ModelApi.BuyerOut>(BuildUrl(C_SearchBuyer) + pBuyerIn.TaxpayerId, GetBasicAut(), "GET");
            if (buyer == null || (buyer.error != null && buyer.error.status_code != 0))
                return buyer;

            if (string.IsNullOrEmpty(buyer.Id))
                buyer = ConexoesApi.EfetuarChamadaApi<ModelApi.BuyerOut>(BuildUrl(C_NewBuyer), GetBasicAut(), "POST", pBuyerIn);
            else
                buyer = ConexoesApi.EfetuarChamadaApi<ModelApi.BuyerOut>(BuildUrl(C_UpdateBuyer) + buyer.Id, GetBasicAut(), "PUT", pBuyerIn);
            return buyer;
        }

        public void registerWebHook(string pURL)
        {

            if (string.IsNullOrEmpty(pURL))
                return;

            if (pURL.EndsWith("/"))
                pURL = pURL.Substring(0, pURL.Length - 1);

            string url = pURL + C_WebHook;



            var list = ConexoesApi.EfetuarChamadaApi<ModelApi.ListWebHookOut>(BuildUrl(C_WebHookEndPoid), GetBasicAut(), "GET", null);
            bool found = false;
            foreach (var item in list.Items)
            {
                if (item.Url == url)
                {
                    found = true;
                    for (int i = 0; i < s_needEvents.Count; i++)
                    {
                        if (!item.Events.Contains(s_needEvents[i]))
                        {
                            ConexoesApi.EfetuarChamadaApi<ModelApi.Generic>(BuildUrl(C_WebHookEndPoid) + "/" + item.Id, GetBasicAut(), "DELETE", null);
                            found = false;
                            break;
                        }
                    }
                    break;
                }
            }
            if (!found)
            {
                var input = new ModelApi.CreateWebHookIn() { Method = "POST", Url = url, Description = "WebhookPagamentos", Event = s_needEvents };
                ConexoesApi.EfetuarChamadaApi<ModelApi.CreateWebHookOut>(BuildUrl(C_WebHookEndPoid), GetBasicAut(), "POST", input);
            }
        }

        string Base64StringEncode(string originalString)
        {
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(originalString);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        string GetBasicAut()
        {
            return Base64StringEncode(m_applycation_id + ":" + m_Marketplace_id);
        }

        string BuildUrl(string url, string pTransactionId = "")
        {
            url = url.Replace("{urlbase}", urlbase).Replace("{marketplace_id}", m_Marketplace_id).Replace("{transaction_id}", pTransactionId);
            return url;
        }

    }
}
