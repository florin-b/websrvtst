
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace LiteSFATestWebService
{
    public class Test
    {

        public void testEtransport()
        {

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            string certificatePath1 = HttpRuntime.AppDomainAppPath + @"\801354486.cer";
            string certificatePath2 = HttpRuntime.AppDomainAppPath + @"\etransport_glc-it-fb.pfx";


            string pass = "changeit";


            X509Certificate cert1 = new X509Certificate(certificatePath1);
            X509Certificate cert2 = new X509Certificate(certificatePath2);


           // var client = new RestClient("https://api.com/oauth/v2/token");


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://etransport-test-flota-gps.anaf.ro/api/internal/rawMessages");

           


            //request.ClientCertificates.Add(cert1);
            request.ClientCertificates.Add(cert2);

            request.Method = "POST";
            request.ContentType = "application/json";

            string jsonData = "";

            request.ContentLength = jsonData.Length;

            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(jsonData);
            }

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string stockResponse = sr.ReadToEnd().Trim();


        }


    }
}
