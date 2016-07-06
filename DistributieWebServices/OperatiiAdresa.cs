using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;

namespace DistributieTESTWebServices
{
    public class OperatiiAdresa
    {
        public string getRouteBounds(string codAdresa, string nrDocument)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            RouteBounds routeBonds = new RouteBounds();
            BeanAdresa adresa = new BeanAdresa();
            LatLng pozMasina = new LatLng();

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select country, region, city1, street, house_num1 from sapprd.adrc where addrnumber =:codAdresa and client = 900 ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    adresa.country = oReader.GetString(0);
                    adresa.region = oReader.GetString(1);
                    adresa.city = oReader.GetString(2);
                    adresa.streetName = oReader.GetString(3);
                    adresa.streetNo = oReader.GetString(4);
                }

                cmd.CommandText = " select a.latitude, a.longitude from gps_index a, gps_masini b, borderouri c where c.numarb =:nrDocument and " +
                                  " b.nr_masina = replace(c.masina,'-','') and a.device_id = b.id ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    pozMasina.latitude = oReader.GetDouble(0);
                    pozMasina.longitude = oReader.GetDouble(1);
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

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            routeBonds.adresaDest = adresa;
            routeBonds.pozMasina = pozMasina;

            return serializer.Serialize(routeBonds);


        }


    }
}