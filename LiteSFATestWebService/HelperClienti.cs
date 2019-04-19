using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class HelperClienti
    {
        public static bool isClientInstitutiePublica(OracleConnection connection, string codClient)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            bool isInstPublica = false;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from   sapprd.knvv v, sapprd.knvp p where v.mandt = '900' and v.vtweg = '20' " +
                                  " and v.spart = '11' and v.kdgrp = '18' and v.mandt = p.mandt and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart " +
                                  " and p.parvw = 'ZA' and v.kunnr = :codClient ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                    isInstPublica = true;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return isInstPublica;

        }



    }
}