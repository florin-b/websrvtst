using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace LiteSFATestWebService
{
    public class OperatiiSuplimentare
    {

        public void saveTonaj(string JSONComanda, string JSONDateLivrare)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ComandaVanzare comandaVanzare = serializer.Deserialize<ComandaVanzare>(JSONComanda);
            DateLivrare dateLivrare = serializer.Deserialize<DateLivrare>(JSONDateLivrare);


            if (dateLivrare.addrNumber.Length < 5 || dateLivrare.tonaj == null || dateLivrare.tonaj.Equals("-1"))
                return;

            string query;

            OracleConnection connection = new OracleConnection();


            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            string nowDate = Utils.getCurrentDate();
            string nowTime = Utils.getCurrentTime();

            connection.ConnectionString = connectionString;
            connection.Open();


            try
            {

                OracleCommand cmd = connection.CreateCommand();

                if (!recordTonajExists(connection, comandaVanzare.codClient, dateLivrare.addrNumber))
                {
                    query = " insert into sapprd.ztonajclient(mandt,kunnr,adrnr,greutate,gewei) " +
                            " values ('900', :kunnr, :adrnr, :greutate, 'TO')";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":kunnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = comandaVanzare.codClient;

                    cmd.Parameters.Add(":adrnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = dateLivrare.addrNumber;

                    cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = dateLivrare.tonaj;

                    cmd.ExecuteNonQuery();




                }


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

            return;
        }



        public static void saveTonajComanda(OracleConnection connection, string idComanda, string tonaj)
        {

            if (tonaj == null || tonaj.Equals("-1"))
                return;


            OracleCommand cmd = null;

            try
            {
                cmd = connection.CreateCommand();

                string query = " insert into sapprd.ztonajcomanda(mandt, idComanda, greutate, gewei) " +
                               " values ('900', :idComanda , :greutate, :gewei)";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tonaj;

                cmd.Parameters.Add(":gewei", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = "TO";

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



        private bool recordTonajExists(OracleConnection connection, string codClient, string addrNumber)
        {

            bool exists = false;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select count(*) from sapprd.ztonajclient where kunnr = :codClient and adrnr = :codAdresa";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = addrNumber;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.GetInt32(0) > 0)
                    exists = true;
                else
                    exists = false;

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                exists = false;
            }


            return exists;
        }



        public void savePrelucrare04(OracleConnection connection, String idComanda, string tipPrelucrare)
        {



            if (tipPrelucrare == null || tipPrelucrare.Equals("-1"))
                return;

            string query = "";

            try
            {

                OracleCommand cmd = connection.CreateCommand();



                query = " insert into sapprd.zprelucrare04(mandt, idComanda, prelucrare) " +
                        " values ('900', :idComanda , :prelucrare)";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.Parameters.Add(":prelucrare", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipPrelucrare.ToUpper();

                cmd.ExecuteNonQuery();





            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return;
        }








    }
}