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

       
        public static void saveTonajAdresa(OracleConnection connection, string codClient, string codAdresa, string tonaj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();


            if (codAdresa.Length < 5 || tonaj == null || tonaj.Equals("-1"))
                return;

            string query;


            string nowDate = Utils.getCurrentDate();
            string nowTime = Utils.getCurrentTime();

            OracleCommand cmd = null;

            try
            {

                 cmd = connection.CreateCommand();

                if (!recordTonajExists(connection, codClient, codAdresa))
                {
                    query = " insert into sapprd.ztonajclient(mandt,kunnr,adrnr,greutate,gewei) " +
                            " values ('900', :kunnr, :adrnr, :greutate, 'TO')";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":kunnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    cmd.Parameters.Add(":adrnr", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAdresa;

                    cmd.Parameters.Add(":greutate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = tonaj;

                    cmd.ExecuteNonQuery();




                }


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

            return;
        }



        public static void saveTonajComanda(OracleConnection connection, string idComanda, string tonaj)
        {

            if (tonaj == null || tonaj.Equals("-1") || tonaj.Trim().Equals(""))
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



        private static bool recordTonajExists(OracleConnection connection, string codClient, string addrNumber)
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


            if (tipPrelucrare == null || tipPrelucrare.Equals("-1") || tipPrelucrare.Trim().Equals(""))
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



        public static void saveClpComanda(OracleConnection connection, string idComanda, string nrDocumentClp)
        {

            

            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            try
            {

                cmd = connection.CreateCommand();

                string query = " select nrcmdsap, cod_agent, com_referinta from sapprd.zcomhead_tableta where mandt = '900' and id=:idComanda ";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                List<string> listStrazi = new List<string>();
                string nrCmdSap = "", codAgent = "", cmdReferinta = "";
                if (oReader.HasRows)
                {

                    oReader.Read();
                    nrCmdSap = oReader.GetString(0);
                    codAgent = oReader.GetString(1);
                    cmdReferinta = oReader.GetString(2);


                    cmd.CommandText = " update sapprd.zclp_inchise set vbeln =:comandaSap, erdat=:data, erzet=:ora where mandt='900' and ebeln =:documentClp and vbeln =:comReferinta ";
                                  
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":comandaSap", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrCmdSap;

                    cmd.Parameters.Add(":documentClp", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = nrDocumentClp;

                    cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = General.GeneralUtils.getCurrentDate();

                    cmd.Parameters.Add(":ora", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = General.GeneralUtils.getCurrentTime();

                    cmd.Parameters.Add(":comReferinta", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = cmdReferinta;

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        query = " insert into sapprd.zclp_inchise(mandt, ebeln, vbeln, erdat, erzet, uname) values " +
                                " ('900', :documentClp , :comandaSap, :data, :ora, :codAgent)";


                        

                        cmd.CommandText = query;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":documentClp", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrDocumentClp;

                        cmd.Parameters.Add(":comandaSap", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = nrCmdSap;

                        cmd.Parameters.Add(":data", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                        cmd.Parameters[2].Value = General.GeneralUtils.getCurrentDate();

                        cmd.Parameters.Add(":ora", OracleType.NVarChar, 18).Direction = ParameterDirection.Input;
                        cmd.Parameters[3].Value = General.GeneralUtils.getCurrentTime();

                        cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 36).Direction = ParameterDirection.Input;
                        cmd.Parameters[4].Value = codAgent;

                        cmd.ExecuteNonQuery();
                    }

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



        }


        public static void saveTonajAdresa(OracleConnection connection, string idComanda,  string tonaj)
        {


            if (tonaj == null || tonaj.Equals("-1"))
                return;

            ClientComanda clientComanda = OperatiiComenzi.getClientComanda(connection, idComanda);

            saveTonajAdresa(connection, clientComanda.codClient, clientComanda.codAdresa, tonaj);

        }





        public static void saveDatePersonale(OracleConnection connection, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {

            OracleCommand cmd = null;

            try
            {

                if (comandaVanzare.cnpClient.Length == 0)
                    return;

                if (existaDatePersonale(connection, dateLivrare, comandaVanzare))
                {
                    return;
                }


                cmd = connection.CreateCommand();


                string query = " insert into sapprd.zinformclmag(mandt, cod_cl, stceg, name1, street, city1, country, regio, tip_cl) " +
                               " values ('900', '0000000000', :stceg, :name1, :street, :city1, 'RO',:regio, :tip_cl) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":stceg", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                cmd.Parameters.Add(":name1", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = comandaVanzare.numeClient;

                cmd.Parameters.Add(":street", OracleType.NVarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dateLivrare.Strada;

                cmd.Parameters.Add(":city1", OracleType.NVarChar, 120).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = dateLivrare.Oras;

                cmd.Parameters.Add(":regio", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = dateLivrare.codJudet;

                string tipClient;

                if (comandaVanzare.cnpClient.Trim().Length == 13)
                    tipClient = "PF";
                else
                    tipClient = "PJ";

                cmd.Parameters.Add(":tip_cl", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = tipClient;

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



        public static bool existaDatePersonale(OracleConnection connection, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {
            bool existaDatePers = false;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = connection.CreateCommand();

                string query = " select count(*) from sapprd.zinformclmag where mandt='900' and  stceg = :cnp ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cnp", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.GetInt32(0) == 0)
                    existaDatePers = false;
                else
                {
                    query = " select upper(name1), regio, upper(city1), upper(street) from sapprd.zinformclmag where mandt='900' and  stceg =:cnp  ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cnp", OracleType.NVarChar, 60).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = comandaVanzare.cnpClient;

                    oReader = cmd.ExecuteReader();


                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (!oReader.GetString(0).Equals(comandaVanzare.numeClient.ToUpper()))
                            {
                                existaDatePers = true;
                                break;
                            }
                            else
                            {
                                if (oReader.GetString(1).Equals(dateLivrare.codJudet) && oReader.GetString(2).Equals(dateLivrare.Oras.ToUpper()) && oReader.GetString(3).Equals(dateLivrare.Strada.ToUpper()))
                                {
                                    existaDatePers = true;
                                    break;
                                }
                            }
                        }
                    }

                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                existaDatePers = false;
            }



            return existaDatePers;
        }



    }
}