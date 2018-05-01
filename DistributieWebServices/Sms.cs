using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DistributieTESTWebServices.SMSService;
using System.Globalization;
using System.Data.OracleClient;
using System.Data;

namespace DistributieTESTWebServices
{
    public class Sms
    {

        private string filialaDocument = "";

        public void sendSMS(List<NotificareClient> listNotificari, String nrDocument)
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


                        string statusLink = getStatusLink(nrDocument, notificare.codClient);


                        filialaDocument = "GL10";

                        if (filialaDocument.Trim() == "GL10")
                        {

                        }


                            //smsService.sendSession(sessionId, notificare.nrTelefon, mesaj, dateTime, "", 0);

                            ErrorHandling.sendErrorToMail(mesaj + " , date comanda: " + notificare, "SMS Soferi");


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

        public void logSmsStatus(OracleConnection connection, NotificareClient notificare, string status)
        {


            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " insert into sapprd.zlogsmsclienti(mandt, datac, orac, codclient, telefon, msgstatus) values ('900', " +
                                  " to_char(sysdate,'yyyymmdd'), to_char(sysdate,'hh24mi'), :codclient, :telefon, :status) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = notificare.codClient;

                cmd.Parameters.Add(":telefon", OracleType.VarChar, 45).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = notificare.nrTelefon;

                cmd.Parameters.Add(":status", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = status;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }


        }

        public string getStatusLink(string nrDocument, string codClient)
        {

            string statusLink = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select i.latitude, i.longitude, b.fili from gps_masini g, borderouri b, gps_index i where " +
                                  " replace(g.nr_masina,'-','')=  replace(b.masina,'-','') and " +
                                  " b.numarb=:nrBord and b.sttrg in ('2','4','6') and g.id = i.device_id ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                double latMasina = 0.0;
                double longMasina = 0.0;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    latMasina = oReader.GetDouble(0);
                    longMasina = oReader.GetDouble(1);
                    filialaDocument = "filiala = " + oReader.GetString(2);
                }



                cmd.CommandText = " select ad.latitude, ad.longitude, c.nume from sapprd.zadreseclienti ad, " +
                                  " sapprd.zdocumentebord z, clienti c  where z.nr_bord=:nrBord and c.cod = z.cod " +
                                  " and z.cod = ad.codclient and z.adresa = ad.codadresa and z.cod=:codClient ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;


                oReader = cmd.ExecuteReader();

                double latClient = 0.0;
                double longClient = 0.0;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    latClient = Double.Parse(oReader.GetString(0));
                    longClient = Double.Parse(oReader.GetString(1));
                }


                if (latMasina > 0 && latClient > 0)
                {
                    //statusLink = " Detalii la http://10.1.5.28:8080/inf/stat?p=" + nrDocument + "-" + codClient;
                    statusLink = " Detalii la https://46.97.20.107:8443/inf/stat?p=" + nrDocument + "-" + codClient;

                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }





            return statusLink;
        }

        public string getRoCurrentDate()
        {
            CultureInfo ci = new CultureInfo("ro-RO");
            return DateTime.Now.ToString("M", ci).ToLower();

        }




    


        public string sendSMS(String nrTel)
        {

            string resString = " ";

            System.Net.ServicePointManager.Expect100Continue = false;

            try
            {

                SMSService.SMSServiceService smsService = new SMSServiceService();
                string sessionId = smsService.openSession("arabesque2", "arbsq123");



                DateTime dateTime = new DateTime();


                string mesaj = "Comanda dumneavoastra din 17 sept., depart. chimice, "+
                                " se va livra astazi, 18 sept. Detalii la http://10.1.5.28:8080/inf/stat?p=0001718461-4110075331 . Va multumim!";

                resString = smsService.sendSession(sessionId, nrTel, mesaj, dateTime, "", 0);

               // ErrorHandling.sendErrorToMail(mesaj + " , date comanda: " + nrTel, "SMS Soferi");




                smsService.closeSession(sessionId);
            }
            catch (Exception ex)
            {
                resString = ex.ToString();
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return resString;
        }







    }
}