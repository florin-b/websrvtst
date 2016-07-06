using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace LiteSFATestWebService
{
    public class OperatiiDocumente
    {
        public string getDocumente(String depart)
        {

            string documente = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condDepart = "";
                if (!depart.Equals("TOAT"))
                    condDepart = " where depart=:depart ";

                cmd.CommandText = " select document from sapprd.zdocprod " +condDepart ;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (!depart.Equals("TOAT"))
                {
                    cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = depart;
                }

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                        if (documente == "")
                            documente = oReader.GetString(0);
                        else
                            documente += "#" + oReader.GetString(0);
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
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return documente;

        }




       



    }
}