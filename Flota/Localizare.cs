using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;

namespace Flota
{
    public class Localizare
    {

        public string getPozitieCurenta(string soferi)
        {
            string serializedResult = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Sofer> listaSoferi = serializer.Deserialize<List<Sofer>>(soferi);

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string strListSoferi = "";

            foreach (Sofer s in listaSoferi)
            {
                if (strListSoferi.Equals(""))
                    strListSoferi += "'" + s.cod + "'";
                else
                    strListSoferi = "," + "'" + s.cod + "'";
            }

            strListSoferi = "(" + strListSoferi + ")";

            string dataCurenta = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                string query = "";

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                query = " select distinct codsofer,g.id,  d.latitude, d.longitude,  to_char(d.record_time, 'dd-mon-yyyy hh24:mi:ss')  from sapprd.zevenimentsofer a, " +
                               " borderouri b, gps_masini g, gps_date d where a.codsofer in " + strListSoferi + " and a.data = '" + dataCurenta + "' " +
                               " and a.document = b.numarb and replace(b.masina, '-') = g.nr_masina and d.device_id = g.id and " +
                               " to_char(d.record_time, 'dd-mon-yyyy hh24:mi:ss') =  (select to_char(max(h.record_time), 'dd-mon-yyyy hh24:mi:ss') " +
                               " from gps_date h where h.device_id = d.device_id) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                PozitieActualaSofer pozitie;
                List<PozitieActualaSofer> listaPozitii = new List<PozitieActualaSofer>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        pozitie = new PozitieActualaSofer();
                        pozitie.codSofer = oReader.GetString(0);
                        pozitie.latitudine = oReader.GetDouble(2).ToString().Replace(",", ".");
                        pozitie.longitudine = oReader.GetDouble(3).ToString().Replace(",", ".");
                        pozitie.data = oReader.GetString(4);
                        listaPozitii.Add(pozitie);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                serializedResult = serializer.Serialize(listaPozitii);

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


    }
}