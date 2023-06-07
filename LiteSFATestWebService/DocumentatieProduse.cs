using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;



namespace LiteSFATestWebService
{
    public class DocumentatieProduse
    {

        public string testDocService()
        {

            string result = "";

            try
            {

                string urlDeliveryService = "http://10.1.3.72:8080/documente/documente/existaDocumenteArticole";

                string articole = "10400012,10400013,10402736";

                System.Net.ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlDeliveryService);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = articole.Length;

                using (Stream webStream = request.GetRequestStream())
                using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(articole);
                }
                
                System.Net.WebResponse response = request.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

                string deliveryResponse = sr.ReadToEnd().Trim();

                result = deliveryResponse;
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("testDocService: " + ex.ToString());
            }

            return result;

        }



    }
}