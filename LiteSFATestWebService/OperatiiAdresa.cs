using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Globalization;

namespace LiteSFATestWebService
{
    public class OperatiiAdresa
    {

        public String getLocalitatiJudet(String codJudet)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select upper(localitate) from sapprd.zlocalitati where bland=:codJudet order by localitate";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codJudet;


                oReader = cmd.ExecuteReader();

                List<string> listLocalitati = new List<string>();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        listLocalitati.Add(oReader.GetString(0));
                    }


                }





                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listLocalitati);

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

            return serializedResult;

        }




        public string getAdreseJudet(String codJudet)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct mc_street from sapprd.m_strta where region =:codJudet order by mc_street ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codJudet;


                oReader = cmd.ExecuteReader();

                List<string> listStrazi = new List<string>();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        listStrazi.Add(oReader.GetString(0));
                    }

                }

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();


                BeanAdreseJudet adreseJudet = new BeanAdreseJudet();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                adreseJudet.listLocalitati = getLocalitatiJudet(codJudet);
                adreseJudet.listStrazi = serializer.Serialize(listStrazi);

                serializedResult = serializer.Serialize(adreseJudet);

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

            return serializedResult;

        }



        public static void insereazaCoordonateAdresa(OracleConnection connection, String idComanda, String coordonate)
        {

            String[] coords = coordonate.Split('#');

            if (coords[0].Equals("0") || coords[0].Equals("0.0"))
                return;



            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " insert into sapprd.zcoordcomenzi(mandt, idcomanda, latitude, longitude) values ('900', :idComanda, :latitude, :longitude) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = coords[1];

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

            return;



        }

        public bool isAdresaValida(string codJudet, string localitate)
        {

            bool isValid = false;

            Adresa adresa = new Adresa();
            adresa.codJudet = codJudet;
            adresa.localitate = localitate;

            OracleConnection connection = null;

            try
            {
                connection = new OracleConnection();

                OracleDataReader oReader = null;

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zlocalitati where bland=:codJudet and lower(convert(localitate,'US7ASCII'))=:localitate ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codJudet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = adresa.codJudet;

                cmd.Parameters.Add(":localitate", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = adresa.localitate.ToLower();


                oReader = cmd.ExecuteReader();

                List<string> listStrazi = new List<string>();

                if (oReader.HasRows)
                {
                    isValid = true;
                }

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return isValid;
        }



    }
}