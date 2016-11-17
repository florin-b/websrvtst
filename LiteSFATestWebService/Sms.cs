using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LiteSFATestWebService.SMSService;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;


namespace LiteSFATestWebService
{
    public class Sms
    {

        public enum TipUser { SM, CVS };

        private string numeClient;
        private string codClient;


        public void setNumeClient(String numeClient)
        {
            this.numeClient = numeClient;
        }

        public void setCodClient(String codClient)
        {
            this.codClient = codClient;
        }

        public string getNumeClient()
        {
           
            if (numeClient == null)
            {
                OperatiiClienti opClienti = new OperatiiClienti();
                numeClient = opClienti.getNumeClient(codClient);
            }

            return numeClient;
        }

        public void sendSMS(TipUser tipUser, string filiala)
        {

            List<String> listTelefoane = getListNrTelefon(tipUser, filiala);
            System.Net.ServicePointManager.Expect100Continue = false;
            string smsMessage = getSmsMessage(tipUser);

            if (listTelefoane.Count > 0)
            {

                try
                {
                   
                    SMSService.SMSServiceService smsService = new SMSServiceService();
                    string sessionId = smsService.openSession("arabesque2", "arbsq123");

                    for (int i = 0; i < listTelefoane.Count; i++)
                    {
                        ErrorHandling.sendErrorToMail("mesaj sms to  " + listTelefoane[i] + " text " + smsMessage);
                       

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



        private string getSmsMessage(TipUser tipUser)
        {
            string smsMessage = "";

            if (tipUser == TipUser.SM || tipUser == TipUser.CVS)
            {
                smsMessage = "S-a creat o comanda cu marfa din MAV1 pentru clientul " + getNumeClient();
            }

            return smsMessage;
        }




        private List<string> getListNrTelefon(TipUser tipUser, String filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            List<String> listNrTel = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select nrtel from agenti where tip=:tipUser and filiala =:filiala and activ = 1 ";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipUser", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipUser;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (oReader.GetString(0).Trim().Length > 0)
                            listNrTel.Add(oReader.GetString(0));
                    }
                }

                oReader.Close();
                oReader.Dispose();



            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return listNrTel;
        }



    }
}