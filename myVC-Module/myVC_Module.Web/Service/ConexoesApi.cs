using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Zoop.Web
{
    public static class ConexoesApi
    {

        /// <summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pMethod">POST/GET</param>
        /// <param name="pUrl"></param>
        /// <param name="pObjetoEntrada"></param>
        /// <param name="pBasicAuthorization"></param>
        /// <returns>Efetua chamada POST/GET para uma determinada API</returns>
        /// </summary>
        /// <param name="pUrl"></param><param name="pBasicAuthorization"></param><param name="pMethod">POST/GET</param><param name="pObjetoEntrada"></param>
        public static T EfetuarChamadaApi<T>(string pUrl, string pBasicAuthorization, string pMethod, object pObjetoEntrada = null)
        {
            string objEntradaSerializado = string.Empty;
            if (pObjetoEntrada != null)
                objEntradaSerializado = JsonConvert.SerializeObject(pObjetoEntrada);

            using (MyWebClient client = new MyWebClient())
            {
                T objRetorno = default(T);
                try
                {

                    // Seta o certificado como válido em caso de utilizar SSL
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    client.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");

                    if (!string.IsNullOrEmpty(pBasicAuthorization))
                        client.Headers.Add("Authorization", string.Format("Basic {0}", pBasicAuthorization));
                    
                    string objRetornoSerializado;
                    if (pMethod == "GET")
                        objRetornoSerializado = client.DownloadString(pUrl);
                    else 
                        objRetornoSerializado = client.UploadString(pUrl, pMethod, objEntradaSerializado);

                    objRetorno = JsonConvert.DeserializeObject<T>(objRetornoSerializado);
                }
                catch (WebException ex)
                {
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            throw;
                        case WebExceptionStatus.ProtocolError:
                            HttpWebResponse response = (HttpWebResponse)ex.Response;
                            if (response != null)
                            {
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.GatewayTimeout:
                                    case HttpStatusCode.RequestTimeout:
                                        throw;
                                    case HttpStatusCode.Conflict:
                                    case HttpStatusCode.BadRequest:
                                    case HttpStatusCode.Unauthorized:
                                    case HttpStatusCode.PaymentRequired:
                                    case HttpStatusCode.Forbidden:
                                    case HttpStatusCode.NotFound:
                                    case HttpStatusCode.InternalServerError:
                                    case HttpStatusCode.BadGateway:
                                        using (StreamReader s = new StreamReader(ex.Response.GetResponseStream()))
                                        {
                                            string error = s.ReadToEnd();

                                            return JsonConvert.DeserializeObject<T>(error);
                                        }
                                }

                                if ((int)response.StatusCode == 422)
                                {
                                    using (StreamReader s = new StreamReader(ex.Response.GetResponseStream()))
                                    {
                                        string error = s.ReadToEnd();

                                        return JsonConvert.DeserializeObject<T>(error);
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                throw;
                            }
                    }

                }

                return objRetorno;
            }

        }

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }
    }
}
