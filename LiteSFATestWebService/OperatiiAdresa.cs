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

        private const double DIST_MIN_ADRESA = 0.2;

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

                cmd.CommandText = " select upper(localitate) from sapprd.zlocalitati where bland=:codJudet order by localitate ";

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




        public static void adaugaAdresaClient(OracleConnection connection, String idComanda, DateLivrare dateLivrare)
        {

            Adresa adresaComanda = new Adresa();


            try
            {

                ClientComanda clientComanda = OperatiiComenzi.getClientComanda(connection, idComanda);

                if (clientComanda.codClient != null)
                {
                    adresaComanda = OperatiiComenzi.getAdresaComanda(connection, idComanda, clientComanda.codAdresa);

                    if (dateLivrare.Strada == null || dateLivrare.Strada.Trim().Length == 0)
                        return;

                    List<Adresa> adreseClient = getAdreseClient(connection, clientComanda);


                    if (adresaComanda.latitude != null && !adresaInLista(clientComanda.codAdresa, adreseClient))
                        adaugaCodAdresa(connection, clientComanda, dateLivrare.coordonateGps);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


        }


        private static bool adresaInLista(String codAdresa, List<Adresa> adreseClient)
        {
            foreach (Adresa adresa in adreseClient)
            {
                if (adresa.codAdresa.Equals(codAdresa))
                    return true;

            }

            return false;
        }


        private static bool adresaExista(Adresa adresaComanda, List<Adresa> adreseClient)
        {


            foreach (Adresa adresa in adreseClient)
            {
                double distAdrese = distanceXtoY(Double.Parse(adresaComanda.latitude), Double.Parse(adresaComanda.longitude), Double.Parse(adresa.latitude), Double.Parse(adresa.longitude), "K");

                if (distAdrese < DIST_MIN_ADRESA)
                    return true;

            }

            return false;
        }



        private static List<Adresa> getAdreseClient(OracleConnection connection, ClientComanda clientComanda)
        {

            List<Adresa> adreseClient = new List<Adresa>();

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select  a.latitude, a.longitude, a.codadresa  from sapprd.zadreseclienti a, sapprd.adrc b " +
                                   " where a.mandt = '900' and b.client = '900' and a.codadresa = b.addrnumber and a.codclient =:codClient ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = clientComanda.codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Adresa adresa = new Adresa();
                        adresa.latitude = oReader.GetString(0);
                        adresa.longitude = oReader.GetString(1);
                        adresa.codAdresa = oReader.GetString(2);
                        adreseClient.Add(adresa);

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

            return adreseClient;
        }


        private static double distanceXtoY(double lat1, double lon1, double lat2, double lon2, string unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == "K")
            {
                dist = dist * 1.609344;
            }
            else if (unit == "N")
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }


        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad * 180 / Math.PI);
        }


        private static void adaugaCodAdresa(OracleConnection connection, ClientComanda clientComanda, String coordonate)
        {

            String[] coords = coordonate.Split('#');

            if (coords[0].Equals("0") || coords[0].Equals("0.0"))
                return;

            OracleCommand cmd = null;
            try
            {
                cmd = connection.CreateCommand();


                cmd.CommandText = " insert into sapprd.zadreseclienti(mandt, codclient, codadresa, latitude, longitude ) values ('900', :codClient, :codAdresa, :latitude, :longitude) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = clientComanda.codClient;

                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = clientComanda.codAdresa;

                cmd.Parameters.Add(":latitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = coords[0];

                cmd.Parameters.Add(":longitude", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = coords[1];

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



        public string getLocalitatiLivrareRapida()
        {

            string localitati = "Alba Iulia, Alexandria, Bacau, Bistrita, Botosani, Braila, Brasov, Bucuresti, Buzau, Calarasi, Cluj-Napoca, Constanta, Deva, " +
                    "Drobeta Turnu Severin, Focsani, Galati, Gheorghieni, Giurgiu, Iasi, Medias, Miercurea Ciuc, Odorheiu Secuiesc, Onesti, Petrosani, Piatra-Neamt, " +
                    "Pitesti, Ploiesti, Ramnicu-Sarat, Ramnicu-Valcea, Resita, Roman, Satu-Mare, Sfantu Gheorghe, Sibiu, Slobozia, Slatina, Suceava, Targoviste, " +
                    "Targu-Jiu, Targu-Mures, Tulcea, Zalau";



            return localitati;

        }


    }
}