using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Globalization;

namespace DistributieTESTWebServices
{

    [WebService(Namespace = "http://distributieTest.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class Service1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            System.Threading.Thread.Sleep(2000);
            return "Hello World from ASP.Net";
        }


        [WebMethod]
        public string getBorderouri(string codSofer, string interval, string tip)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouri(codSofer, interval, tip);
        }


        [WebMethod]
        public string getEvenimenteBorderouri(string codSofer, string interval)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimenteBorderouri(codSofer, interval);
        }


        [WebMethod]
        public string getFacturiBorderou(string nrBorderou, string tipBorderou)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getFacturiBorderou(nrBorderou, tipBorderou);
        }


        [WebMethod]
        public string getArticoleBorderou(string nrBorderou, string codClient, string codAdresa)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getArticoleBorderou(nrBorderou, codClient, codAdresa);
        }


        [WebMethod]
        public string getEvenimenteBorderou(string nrBorderou)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimenteBorderou(nrBorderou);
        }


        [WebMethod]
        public string saveNewEvent(string serializedEvent, string serializedEtape)
        {

            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            if (serializedEtape != null && serializedEtape != "")
                eveniment.saveOrdineEtape(serializedEtape);

            return eveniment.saveNewEvent(serializedEvent);
        }

        [WebMethod]
        public void sendSmsAlerts(string nrDocument)
        {

            new OperatiiEvenimente().sendSmsAlerts(null, nrDocument);
        }

       


        [WebMethod]
        public string getRoDate()
        {
            CultureInfo ci = new CultureInfo("ro-RO");
            return DateTime.Now.ToString("M", ci).ToLower();
           
        }


        [WebMethod]
        public string saveStop(string idEveniment, string codSofer, string codBorderou, string codEveniment)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.saveNewStop(idEveniment, codSofer, codBorderou, codEveniment);
        }


        [WebMethod]
        public string saveEvenimentStopIncarcare(string document, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.saveEvenimentStopIncarcare(document, codSofer);
        }

        [WebMethod]
        public string getEvenimentStopIncarcare(string document, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getEvenimentStopIncarcare(document, codSofer);
        }


        [WebMethod]
        public string cancelEvent(string tipEveniment, string nrDocument, string codClient, string codSofer)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.cancelEvent(tipEveniment, nrDocument, codClient, codSofer);
        }

        [WebMethod]
        public string getPozitieCurenta(string nrBorderou)
        {
            OperatiiEvenimente eveniment = new OperatiiEvenimente();
            return eveniment.getPozitieCurenta(nrBorderou);
        }

        [WebMethod]
        public string getFmsTestData(string fmsMAC)
        {
            string fmsData = "";
            return fmsData;
        }


        [WebMethod]
        public string getDocEvents(string nrDoc, string tipEv)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getDocEvents(nrDoc, tipEv);
        }


        [WebMethod]
        public string userLogon(string userId, string userPass, string ipAdr)
        {
            Logon logon = new Logon();
            return logon.userLogon(userId, userPass, ipAdr);
        }

        [WebMethod]
        public string getCodSofer(string codTableta)
        {
            Logon logon = new Logon();
            return logon.getCodSofer(codTableta);
        }

        [WebMethod]
        public string getSoferi()
        {
            OperatiiSoferi opSoferi = new OperatiiSoferi();
            return opSoferi.getSoferi();
        }



        [WebMethod]
        public string getRouteBounds(string codAdresa, string nrDocument)
        {
            OperatiiAdresa opAdresa = new OperatiiAdresa();
            return opAdresa.getRouteBounds(codAdresa, nrDocument);
        }

        private string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }



        private string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            mTime = cDate.Hour.ToString("00") + cDate.Minute.ToString("00") + cDate.Second.ToString("00");
            return mTime;
        }



        [WebMethod]
        public string GetLocationAddress(string lat, string lng)
        {
            string currentAddress = "";
            HttpWebRequest request = default(HttpWebRequest);
            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            string json = null;

            try
            {

                request = (HttpWebRequest)WebRequest.Create("http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + ", " + lng + "&sensor=false");

                response = (HttpWebResponse)request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                json = reader.ReadToEnd();
                response.Close();

                if (json.Contains("ZERO_RESULTS"))
                {
                    currentAddress = "Adresa indisponibila";
                };
                if (json.Contains("formatted_address"))
                {

                    int start = json.IndexOf("formatted_address");
                    int end = json.IndexOf(", Romania");
                    string AddStart = json.Substring(start + 21);
                    string EndStart = json.Substring(end);
                    string FinalAddress = AddStart.Replace(EndStart, "");

                    currentAddress = FinalAddress;


                };
            }
            catch (Exception ex)
            {
                string Message = "Error: " + ex.ToString();
                currentAddress = "Adresa indisponibila";
            }
            finally
            {
                if ((response != null))
                    response.Close();
            }

            return currentAddress;

        }




        static private string GetDBConnectionString()
        {

            //TES
            //return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
            //        " (HOST = 10.1.3.89)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES))); " +
            //        " User Id = WEBSAP; Password = 2INTER7; ";

            //QAS
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.88)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = QAS.WORLD))); " +
                    " User Id = WEBSAP; Password = 2INTER7; ";



        }


        static private string GetDBConnectionString_prd()
        {

            //PRD
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.95)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = TABLET) )); " +
                    " User Id = WEBSAP; Password = 2INTER7;";



        }


        private void sendErrorToMail(string errMsg)
        {

            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("Distributie.WebService@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));
                message.Subject = "Distributie WebService Error";
                message.Body = errMsg;
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch (Exception)
            {

            }

        }




    }
}