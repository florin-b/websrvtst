using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace DistributieTESTWebServices
{
    public class Test
    {

        public string eTranspService(string jsonData)
        {


            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            string certificatePath = HttpRuntime.AppDomainAppPath + @"\etransport_glc-it-fb.pfx";

            handler.ClientCertificates.Add(new X509Certificate2(certificatePath));
            

            var client = new HttpClient(handler);

            var dataToAuth = new StringContent(jsonData, Encoding.UTF8, "application/json");
           
            var request = client.PostAsync("https://etransport-flota-gps.anaf.ro/api/cnif/internal/rawMessages", dataToAuth).GetAwaiter().GetResult();

            var response = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return response;


        }

    }
}