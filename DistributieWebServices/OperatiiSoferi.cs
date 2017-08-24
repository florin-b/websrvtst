using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieTESTWebServices
{
    public class OperatiiSoferi
    {

        public string getSoferi()
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Sofer> listSoferi = new List<Sofer>();
            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.fili, upper(a.nume), b.codTableta  from soferi a, sapprd.ztabletesoferi b where a.cod = b.codsofer " +
                                  " and b.stare = 1 order by a.fili,a.nume ";

                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Sofer sofer = new Sofer();
                        sofer.filiala = oReader.GetString(0);
                        sofer.nume = oReader.GetString(1);
                        sofer.codTableta = oReader.GetString(2);
                        listSoferi.Add(sofer);

                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listSoferi);

        }



        public static bool isSoferDTI(OracleConnection conn, String codSofer)
        {

            bool isDTI = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            cmd = conn.CreateCommand();

            cmd.CommandText = " select fili from soferi where cod =:codSofer ";

            cmd.Parameters.Clear();
            cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = codSofer;

            oReader = cmd.ExecuteReader();

            oReader.Read();

            if (oReader.GetString(0).Equals("GL90"))
                isDTI = true;

            DatabaseConnections.CloseConnections(oReader, cmd);

            return isDTI;
        }



    }
}