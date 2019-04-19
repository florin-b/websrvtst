using LiteSFATestWebService.SAPWebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace LiteSFATestWebService
{
    public class OperatiiMacara
    {


        public String getCostMacara(string unitLog, string codAgent, string codClient, string listArt)
        {


            JavaScriptSerializer serializer = new JavaScriptSerializer();

            /*
            if (!unitLog.Contains("IS") && !unitLog.Contains("CJ") && !unitLog.Contains("BV"))
            {
                CostDescarcare costDescarcareEmpty = new CostDescarcare();
                costDescarcareEmpty.sePermite = false;

                List<ArticolDescarcare> listArticoleEmpty = new List<ArticolDescarcare>();

                costDescarcareEmpty.articoleDescarcare = listArticoleEmpty;

                return new JavaScriptSerializer().Serialize(costDescarcareEmpty);

            }
            */

            List<ArticolCalculDesc> listArtCmd = serializer.Deserialize<List<ArticolCalculDesc>>(listArt);


            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZNrPaleti inParam = new SAPWebServices.ZNrPaleti();
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            SAPWebServices.ZstNrPaleti[] nrpal = new ZstNrPaleti[listArtCmd.Count];
            string tempDepoz = "";
            for (int i = 0; i < listArtCmd.Count; i++)
            {
                nrpal[i] = new ZstNrPaleti();

                string codArt = listArtCmd[i].cod;
                if (listArtCmd[i].cod.Length == 8)
                    codArt = "0000000000" + listArtCmd[i].cod;


                nrpal[i].Matnr = codArt;
                nrpal[i].Meins = listArtCmd[i].um;
                nrpal[i].Menge = Convert.ToDecimal(listArtCmd[i].cant);

                tempDepoz = listArtCmd[i].depoz;

                //if (tempDepoz.Equals("DESC"))
                //    tempDepoz = getDepartArticol(connection, codArt) + "V1";

                nrpal[i].Lgort = tempDepoz;

            }

            connection.Close();

            inParam.ItTable = nrpal;
            inParam.IpWerks = unitLog;
            inParam.IpPernr = codAgent;
            inParam.IpKunnr = codClient;


            SAPWebServices.ZNrPaletiResponse resp = webService.ZNrPaleti(inParam);

            SAPWebServices.ZstEtPaleti[] valPaleti = resp.EtValpal;


            CostDescarcare costDescarcare = new CostDescarcare();
            costDescarcare.sePermite = !resp.EpMacara.Equals("X");

            

            List<ArticolDescarcare> listArticole = new List<ArticolDescarcare>();

            for (int i = 0; i < valPaleti.Length; i++)
            {
                ArticolDescarcare articol = new ArticolDescarcare();
                articol.cod = valPaleti[i].Matnr;
                articol.depart = valPaleti[i].Spart.Equals("04") ? "041" : valPaleti[i].Spart;
                articol.valoare = valPaleti[i].Valpal.Trim();
                articol.cantitate = valPaleti[i].Nrpal.ToString();
                articol.valoareMin = valPaleti[i].Valmin.Trim();
                listArticole.Add(articol);
            }


            costDescarcare.articoleDescarcare = listArticole;

            return new JavaScriptSerializer().Serialize(costDescarcare);



            

        }




        private string getDepartArticol(OracleConnection connection, string codArticol)
        {
            string depart = "";

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select spart from articole where cod = :codArticol ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codArticol = oReader.GetString(0).Substring(0,2);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return depart;
        }



    }
}