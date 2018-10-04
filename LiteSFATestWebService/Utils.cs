using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class Utils
    {

        public static string getFilialaGed(string filiala)
        {
            return filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);
        }


        public static string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }


        public static string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            mTime = hour + minute + sec;
            return mTime;
        }

        public static String getCurrentMonth()
        {
            DateTime cDate = DateTime.Now;
            string month = cDate.Month.ToString("00");
            return month;
        }


        public static String getCurrentYear()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            return year;
        }



        public static string getDescTipTransport(string codTransport)
        {
            
            string tipTransport = "";

            if (codTransport.Equals("TCLI"))
            {
                tipTransport = "client";
            }
            if (codTransport.Equals("TRAP"))
            {
                tipTransport = "Arabesque";
            }
            if (codTransport.Equals("TERT"))
            {
                tipTransport = "terti";
            }

            return tipTransport;

        }


        public static string getYearFromStrDate(string strDate)
        {
            if (strDate == null || strDate.Trim().Length == 0)
                return getCurrentYear();

            return strDate.Substring(0, 4);

        }


        public static string getMonthFromStrDate(string strDate)
        {
            if (strDate == null || strDate.Trim().Length == 0)
                return getCurrentMonth();

            return strDate.Substring(4, 2);
        }


        public static List<string> getMail(Sms.TipUser[] tipUser, String filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;
            List<String> listMails = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();


                foreach (Sms.TipUser tip in tipUser)
                {

                    cmd.CommandText = " select mail from sapprd.zdest_mail where funct=:tipUser and prctr =:filiala ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":tipUser", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = tip;

                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = filiala;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (oReader.GetString(0).Trim().Length > 0)
                                listMails.Add(oReader.GetString(0));
                        }
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

            return listMails;
        }

        public static string checkROCnpClient(string cnpClient)
        {

            if (cnpClient == null)
                return " ";

            string localCnp = cnpClient;

            if (cnpClient.ToUpper().StartsWith("RORO"))
                localCnp = cnpClient.ToUpper().Replace("RORO", "RO");

            return localCnp;
        }



        public static bool isFilialaMica04(string filiala, string departament)
        {
            if (filiala == null)
                return false;

            if (departament.StartsWith("04") && (filiala.Equals("HD10") || filiala.Equals("VN10") || filiala.Equals("BU12") || filiala.Equals("BZ10") || filiala.Equals("MS10") || filiala.Equals("BC10")))
                return true;

            return false;


        }


        public static string getTipConsilier(OracleConnection connection, string codConsilier)
        {
            string tipConsilier = "NN";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select tip from agenti where cod =:codAgent and upper(tip) like 'C%' ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codConsilier.Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipConsilier = oReader.GetString(0).Trim();
                    
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


            return tipConsilier;
        }



        public static string getDepartArticol(OracleConnection connection, string codArticol)
        {
            string departament = "00";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (codArticol.Length == 8)
                codArticol = "0000000000" + codArticol;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select grup_vz from articole where cod =:codArt ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    departament = oReader.GetString(0).Trim();

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


            return departament;
        }



        public static string getNumeClient(OracleConnection connection, string codClient)
        {
            string numeClient = "Nedefinit";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nume from clienti where cod =:codClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeClient = oReader.GetString(0).Trim();
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


            return numeClient;
        }

        public static string formatDateToSap(string date)
        {
            string[] toksDate = date.Split('.');
            return toksDate[2] + toksDate[1] + toksDate[0];

        }



    }
}