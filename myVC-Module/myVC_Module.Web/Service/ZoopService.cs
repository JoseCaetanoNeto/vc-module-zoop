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

        readonly string m_Marketplace_id;
        readonly string m_applycation_id;


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

        public void registerWebHook(string pURL)
        {

            if (string.IsNullOrEmpty(pURL))
                return;
            
            if (pURL.EndsWith("/"))
                pURL = pURL.Substring(0, pURL.Length - 1);

            string url =  pURL + C_WebHook;
            
            var needEvents = new List<string> {
                "buyer.transaction.canceled",
                "buyer.transaction.charged_back",
                "buyer.transaction.commission.succeeded",
                "buyer.transaction.dispute.succeeded",
                "buyer.transaction.disputed",
                "buyer.transaction.failed",
                "buyer.transaction.reversed",
                "buyer.transaction.succeeded",
                "buyer.transaction.updated",
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
                "transfer.canceled",
                "transfer.created",
                "transfer.delayed",
                "transfer.failed",
                "transfer.succeeded",
                "transfer.updated"
            };

            var list = ConexoesApi.EfetuarChamadaApi<ModelApi.ListWebHookOut>(BuildUrl(C_WebHookEndPoid), GetBasicAut(), "GET", null);
            bool found = false;
            foreach (var item in list.Items)
            {
                if (item.Url == url)
                {
                    found = true;
                    for (int i = 0; i < needEvents.Count; i++)
                    {
                        if (!item.Events.Contains(needEvents[i]))
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
                var input = new ModelApi.CreateWebHookIn() { Method = "POST", Url = url, Description = "WebhookPagamentos", Event = needEvents };
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
