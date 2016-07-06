using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using System.Globalization;

namespace Flota
{
    public class OperatiiDocumente
    {

        public string getBorderouri(string codSofer, string dataStart, string dataStop)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select numarb, to_char(data_e) from borderouri a where  cod_sofer =:codSofer and trunc(data_e) between :dataStart and :dataStop order by data_e ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.Parameters.Add(":dataStart", OracleType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = dataStart;

                cmd.Parameters.Add(":dataStop", OracleType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = dataStop;

                oReader = cmd.ExecuteReader();

                Borderou borderou = null;
                List<Borderou> listBorderouri = new List<Borderou>();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        borderou = new Borderou();
                        borderou.cod = oReader.GetString(0);
                        borderou.dataEmitere = oReader.GetString(1);
                        listBorderouri.Add(borderou);
                    }
                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listBorderouri);


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



            return serializedResult;


            
        }





    }
}