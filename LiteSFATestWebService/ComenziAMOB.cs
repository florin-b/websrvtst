using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService
{
    public class ComenziAMOB
    {
        public string getComenziAmob(string codAgent)
        {
            List<ComandaAMOBAfis> listComenzi = new List<ComandaAMOBAfis>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select id, id_amob,  to_char(to_date(datac,'yyyymmdd')) datac, valoare, nume_client from sapprd.zcom_amob_head " + 
                                  " where cod_agent = :codAgent and status = '0' order by id_amob ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ComandaAMOBAfis comanda = new ComandaAMOBAfis();
                        comanda.idComanda = oReader.GetDecimal(0).ToString();
                        comanda.idAmob = oReader.GetString(1);
                        comanda.dataCreare = oReader.GetString(2);
                        comanda.valoare = oReader.GetDouble(3).ToString();
                        comanda.numeClient = oReader.GetString(4);
                        listComenzi.Add(comanda);

                    }
                }

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listComenzi);

        }


        public string getArticoleComanda(string idComanda)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            List<ArticolAMOB> listArticole = new List<ArticolAMOB>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                double procentReducere = getProcentReducereComanda(connection, idComanda);

                cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod_art, a.depozit, a.cantitate, a.um, a.valoare, b.nume, " + 
                                  " b.umvanz10, b.grup_vz, 'A' from sapprd.zcom_amob_det a, articole b " + 
                                  " where  a.cod = b.cod and id = :idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                string codArticol = "";
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolAMOB articol = new ArticolAMOB();

                        codArticol = oReader.GetString(0);


                        articol.codArticol = codArticol;
                        articol.depozit = oReader.GetString(1);
                        articol.cantitate = oReader.GetDouble(2);
                        articol.um = oReader.GetString(3);
                        articol.pretUnitar = Math.Round((oReader.GetDouble(4) * (1 - procentReducere / 100)), 2);
                        articol.procentReducere = procentReducere;
                        articol.numeArticol = oReader.GetString(5);
                        articol.umVanz = oReader.GetString(6);
                        articol.depart = oReader.GetString(7);
                        articol.tipAB = oReader.GetString(8);

                        listArticole.Add(articol);

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

            return new JavaScriptSerializer().Serialize(listArticole);

        }



        private  double getProcentReducereComanda(OracleConnection connection, string idComanda)
        {
            double procent = 0;
            double valoareComanda = 0;
            double valoareArticole = 0;

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select min(a.valoare) valoare, sum(b.cantitate * b.valoare) articole from " + 
                                  " sapprd.zcom_amob_head a, sapprd.zcom_amob_det b where a.id = :idComanda and b.id = a.id  ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    valoareComanda = oReader.GetDouble(0);
                    valoareArticole = oReader.GetDouble(1);
                }

                procent = Math.Round((1 - valoareComanda / valoareArticole) * 100, 2);

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return procent;

        }


        public static void setStatusComanda(string idComanda, string status)
        {

            ErrorHandling.sendErrorToMail("Set status comanda Amob: " + idComanda + " , " + status);

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapprd.zcom_amob_head set status =:status where id =:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":status", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = status;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = idComanda;

                cmd.ExecuteNonQuery();

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


        }




    }
}