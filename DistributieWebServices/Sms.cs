using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DistributieTESTWebServices.SMSService;
using System.Globalization;

namespace DistributieTESTWebServices
{
    public class Sms
    {

        public void sendSMS(List<NotificareClient> listNotificari)
        {


            System.Net.ServicePointManager.Expect100Continue = false;


            if (listNotificari.Count > 0)
            {

                try
                {
                    DateTime dateTime = new DateTime();
                    SMSService.SMSServiceService smsService = new SMSServiceService();
                    string sessionId = smsService.openSession("arabesque2", "arbsq123");


                    foreach (NotificareClient notificare in listNotificari)
                    {

                        string textComanda = "Comanda";
                        string prep = "va";


                        if (notificare.dateComanda.departament.Trim().Length == 0)
                            continue;

                        if (notificare.dateComanda.emitere.Trim().Length == 0)
                            continue;

                        if (notificare.dateComanda.emitere.Contains(","))
                        {
                            textComanda = "Comenzile";
                            prep = "vor";
                        }

                        string textDepartament = "departamentul";
                        if (notificare.dateComanda.departament.Contains(","))
                            textDepartament = "departamentele";



                        string mesaj = textComanda + " dumneavoastra din " + notificare.dateComanda.emitere.ToLower() + "din " + textDepartament + " " + notificare.dateComanda.departament +
                                                     " se " + prep + " livra astazi, " + getRoCurrentDate() + ". Va multumim!";

                        //smsService.sendSession(sessionId, notificare.nrTelefon, mesaj, dateTime, "", 0);


                    }

                    smsService.closeSession(sessionId);
                }
                catch (Exception ex)
                {
                    ErrorHandling.sendErrorToMail(ex.ToString());
                }
            }

            return;
        }




        public string getRoCurrentDate()
        {
            CultureInfo ci = new CultureInfo("ro-RO");
            return DateTime.Now.ToString("M", ci).ToLower();

        }



    }
}