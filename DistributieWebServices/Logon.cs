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
    public class Logon
    {
        public string userLogon(string userId, string userPass, string ipAdr)
        {



            string serializedResult = "";

            OracleConnection connection = null;
            OracleCommand cmd = new OracleCommand();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection = new OracleConnection();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = "web_pkg.wlogin";
                cmd.CommandType = CommandType.StoredProcedure;

                OracleParameter user = new OracleParameter("usname", OracleType.VarChar);
                user.Direction = ParameterDirection.Input;
                user.Value = userId;
                cmd.Parameters.Add(user);

                OracleParameter pass = new OracleParameter("uspass", OracleType.VarChar);
                pass.Direction = ParameterDirection.Input;
                pass.Value = userPass;
                cmd.Parameters.Add(pass);

                OracleParameter resp = new OracleParameter("x", OracleType.Number);
                resp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(resp);

                OracleParameter depart = new OracleParameter("z", OracleType.VarChar, 5);
                depart.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(depart);

                OracleParameter comp = new OracleParameter("w", OracleType.VarChar, 12);
                comp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(comp);

                OracleParameter tipAcces = new OracleParameter("k", OracleType.Number, 2);
                tipAcces.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(tipAcces);

                OracleParameter ipAddr = new OracleParameter("ipAddr", OracleType.VarChar, 15);
                ipAddr.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(ipAddr);

                OracleParameter idAg = new OracleParameter("agentID", OracleType.Number, 5);
                idAg.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idAg);

                OracleParameter userName = new OracleParameter("numeUser", OracleType.VarChar, 128);
                userName.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(userName);

                OracleParameter usrId = new OracleParameter("userID", OracleType.Number, 5);
                usrId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(usrId);

                cmd.ExecuteNonQuery();

                User userInfo = new User();
                userInfo.status = resp.Value.ToString();
                userInfo.nume = userName.Value.ToString();
                userInfo.id = idAg.Value.ToString();
                userInfo.filiala = comp.Value.ToString();
                userInfo.departament = depart.Value.ToString();
                userInfo.tipAcces = tipAcces.Value.ToString();
                userInfo.initStatus = OperatiiEvenimente.getInitStatus(userInfo.id.PadLeft(userInfo.id.Length + 8 - userInfo.id.Length, '0'));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(userInfo);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " userid = " + userId);
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return serializedResult;
        }


        public string getCodSofer(string codTableta)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.codsofer, b.nume, b.fili from sapprd.ztabletesoferi a, soferi b where a.codtableta=:codTableta and a.stare = '1' " +
                                  " and a.codsofer = b.cod ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codTableta", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codTableta;

                oReader = cmd.ExecuteReader();

                User userInfo = new User();
                if (oReader.HasRows)
                {
                    oReader.Read();
                    userInfo.nume = oReader.GetString(1);
                    userInfo.id = oReader.GetString(0);
                    userInfo.filiala = oReader.GetString(2);
                    userInfo.initStatus = OperatiiEvenimente.getInitStatus(userInfo.id.PadLeft(userInfo.id.Length + 8 - userInfo.id.Length, '0'));
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(userInfo);

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