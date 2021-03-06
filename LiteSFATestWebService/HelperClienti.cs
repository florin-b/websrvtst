﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class HelperClienti
    {
        public static DateClientInstPublica getDateClientInstPublica(OracleConnection connection, string codClient)
        {
            DateClientInstPublica dateClient = new DateClientInstPublica();
            dateClient.isClientInstPublica = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select v.kdgrp from sapprd.knvv v, sapprd.knvp p where v.mandt = '900' and v.vtweg = '20' " +
                                  " and v.spart = '11' and v.kdgrp in ('18','19') and v.mandt = p.mandt and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart " +
                                  " and p.parvw = 'ZA' and v.kunnr = :codClient ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    dateClient.isClientInstPublica = true;

                    if (oReader.GetString(0).Equals("18"))
                        dateClient.tipClientInstPublica = "CONSTR";
                    else
                        dateClient.tipClientInstPublica = "NONCONSTR";
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

            return dateClient;

        }



    }
}