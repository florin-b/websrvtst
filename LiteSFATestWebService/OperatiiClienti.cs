using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;

namespace LiteSFATestWebService
{
    public class OperatiiClienti
    {

        public OperatiiClienti()
        {
        }

        public string getNumeClient(string codClient)
        {
            string numeClient = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nume from clienti where cod =:codClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeClient = oReader.GetString(0);
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


            return numeClient;
        }



        public string getListClienti(string numeClient, string depart, string departAg, string unitLog, string codUser)
        {

            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string condClient = "";

            if (depart.Equals("040") || depart.Equals("041"))
                depart = "04";

            if (departAg.Equals("040") || departAg.Equals("041"))
                departAg = "04";

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condDepart1 = "", condDepart2 = "";

                if (!depart.Equals("00"))
                {
                    condDepart1 = " and t.depart='" + depart + "'";
                    condDepart2 = " and p.spart = '" + depart + "'";

                }


                string exceptieClient = " and p.kunn2 ='" + unitLog + "'";

                if (unitLog.Substring(0, 2).Equals("BU"))
                    exceptieClient = " and p.kunn2 like 'BU%' ";

                //pentru DV nu trebuie restrictie pe filiala
                if (unitLog.Equals("NN10"))
                    exceptieClient = "";



                //Doar clientii definiti pe department, filiala
                if (depart == "11")
                {
                    condClient = " and exists (select 1 from clie_tip t where t.canal = '20'  and t.cod_cli=c.cod " +
                                 " and t.depart='11' )  and exists (select 1 from clie_tip t where t.canal = '20' " +
                                 " and t.cod_cli=c.cod and t.depart='" + departAg + "' ) " +
                                 " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " +
                                 " and p.vtweg = '20' and p.spart = '11' and p.parvw in ('ZA','ZS') " + exceptieClient + ")";
                }
                else
                {
                    condClient = " and exists (select 1 from clie_tip t where t.canal = '10' " +
                                " and t.cod_cli=c.cod " + condDepart1 + " ) " +
                                " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " +
                                " and p.vtweg = '10' " + condDepart2 + " and p.parvw in ('ZA','ZS') " +
                                  exceptieClient + " ) ";


                    string condExtraClient = " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                             " and p.spart = '" + departAg+ "' and p.parvw in ('VE', 'ZC')  and p.pernr = '" + codUser + "' ) ";

                    if (codUser != null)
                    {
                        string tipUser = getTipUser(connection, codUser);

                        if (tipUser != "SD" && !tipUser.StartsWith("KA"))
                            condClient += condExtraClient;

                    }

                   
                }


                cmd.CommandText = " select x.nume, x.cod, x.tip_pers from (select c.nume, c.cod, c.tip_pers from clienti c " +
                                  " where upper(c.nume) like upper('" + numeClient.Replace("'", "") + "%')  " + condClient +
                                  "  ) x " +
                                  " where rownum<=50 order by x.nume ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                List<Client> listaClienti = new List<Client>();
                Client unClient = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        listaClienti.Add(unClient);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

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

         public string getListMeseriasi(string numeClient, string unitLog){

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

                cmd.CommandText = " select x.nume, x.cod, x.tip_pers from (select c.nume, c.cod, c.tip_pers from websap.clienti c  where upper(c.nume) " + 
                                  " like upper('" + numeClient.Replace("'", "") + "%') " + 
                                  " and exists (select 1 from websap.clie_tip t where t.canal = '20'  and t.cod_cli=c.cod and tip in ('12','13','14','15','16','17') " + 
                                  " ) and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '20' " + 
                                  " and p.parvw in ('ZA','ZS')  and p.kunn2 ='" + unitLog + "' )  ) x  where rownum<=50 order by x.nume ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                List<Client> listaClienti = new List<Client>();
                Client unClient = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        listaClienti.Add(unClient);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

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

        public string getDetaliiClient(string codClient, string depart, string codUser)
        {

            string retVal = "-1";

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string adresa = "-";
            string exista = "";
            float limCredit = 0;
            float restCredit = 0;
            string condClient = "", condClient1 = "", condClient2 = "";

            if (depart.Equals("040") || depart.Equals("041"))
                depart = "04";


            try
            {

                DetaliiClient detaliiClient = new DetaliiClient();

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                if (depart == "11")
                {
                    condClient = " and canal = '20' ";
                    condClient1 = " and vtweg = '20' ";
                    condClient2 = "  and p.vtweg = '20' and p.spart = '11' ";
                }
                else
                {
                    condClient = " and canal = '10' ";
                    condClient1 = " and vtweg = '10' ";

                    string condDepS = "";
                    if (!depart.Equals("00"))
                        condDepS = " and p.spart = '" + depart + "'";

                    condClient2 = "  and p.vtweg = '10' " + condDepS + " ";
                }


                cmd = connection.CreateCommand();

                String condDepartTip = "";
                if (!depart.Equals("00"))
                {
                    condDepartTip = " and depart = '" + depart + "'";
                }


                cmd.CommandText = " select nvl(min(x.tip),0) exista from " +
                                  " (select tip from clie_tip where cod_cli=:codCl " + condClient + condDepartTip + "  ) x ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    exista = oReader.GetString(0);

                }

                if (exista.Equals("0"))  //clientul are restrictie pe departament
                {
                    retVal = "-1";
                    oReader.Close();
                    oReader.Dispose();
                }
                else
                {
                    //aflare adresa
                    string strada = "-", nrStr = "-", oras = "-", regiune = "-";

                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select a.city1, a.street, a.house_num1, a.region " +
                            " from sapprd.adrc a " +
                            " where a.client = '900' and a.addrnumber = " +
                            " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr =:k) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        oras = oReader.GetString(0);
                        strada = oReader.GetString(1);
                        nrStr = oReader.GetString(2);
                        regiune = oReader.GetString(3);

                        oReader.Close();
                        oReader.Dispose();

                    }

                    detaliiClient.oras = oras;
                    detaliiClient.strada = strada;
                    detaliiClient.nrStrada = nrStr;
                    detaliiClient.regiune = regiune;


                    adresa = oras + "!" + strada + "!" + nrStr + "!" + regiune;
                    retVal = adresa;

                    //limita de credit
                    DateTime cDate = DateTime.Now;
                    string year = cDate.Year.ToString();
                    string month = cDate.Month.ToString("00");
                    string day = cDate.Day.ToString("00");
                    string nowDate = year + month;
                    string clBlocat = " ";

                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select distinct k.kunnr,k.klimk lcredit, " +
                      " k.skfor,k.ssobl, nvl((select s2.olikw+s2.ofakw from sapprd.s067 s2 where s2.mandt='900' and " +
                      " s2.kkber='1000' and s2.knkli=k.kunnr),0) olikw, nvl((select sum(s1.oeikw) from sapprd.s066 s1 " +
                      " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb, " +
                      " nvl((select '1' from sapprd.lfa1 l where l.mandt = '900' and l.kunnr = k.kunnr),'-1') isFurnizor " +
                      " from sapprd.knkk k " +
                      " where k.mandt='900' and k.kkber='1000' and k.kunnr=:k ";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":l", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nowDate;

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        limCredit = oReader.GetFloat(1);
                        restCredit = oReader.GetFloat(1) - (oReader.GetFloat(2) + oReader.GetFloat(3)) - (oReader.GetFloat(4) + oReader.GetFloat(5));
                        clBlocat = oReader.GetString(6);
                        detaliiClient.isFurnizor = oReader.GetString(7).Equals("-1") ? false.ToString() : true.ToString();

                        oReader.Close();
                        oReader.Dispose();
                    }
                    else
                    {
                        limCredit = 0;
                        restCredit = 0;
                    }

                    detaliiClient.limitaCredit = limCredit;
                    detaliiClient.restCredit = restCredit;
                    detaliiClient.stare = clBlocat;

                    retVal += "#" + limCredit.ToString() + "#" + restCredit.ToString();


                    //peroana de contact

                    string condDepartPersCont = "";
                    if (!depart.Equals("00"))
                    {
                        condDepartPersCont = " and spart = '" + depart + "' ";
                    }

                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                        " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                        " and p.kunnr =:k and parvw = 'AP' " + condDepartPersCont + condClient1 + " )";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    string nume = "", tel = "";

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            nume += oReader.GetString(0) + " " + oReader.GetString(1) + ":";
                            tel += oReader.GetString(2) + ":";
                        }
                        oReader.Close();
                        oReader.Dispose();
                    }
                    //sf. pers.cont.

                    detaliiClient.persContact = nume;
                    detaliiClient.telefon = tel;
                    detaliiClient.tipClient = exista;

                    retVal += "#" + nume + "#" + tel + "#" + clBlocat + "#" + exista + "#";



                    //curs valutar
                    string cursDate = year + month + day;
                    cmd = connection.CreateCommand();

                    string condDepartCursV = "";
                    if (!depart.Equals("00"))
                    {
                        condDepartCursV = " and b.spart = '" + depart + "' ";
                    }

                    cmd.CommandText = " select x.ukurs from (select distinct a.ukurs, (100000000 - a.gdatu - 1) from sapprd.tcurr a, sapprd.knvv b where a.mandt='900' and " +
                                      " b.mandt='900' and b.kurst = a.kurst and b.kunnr =:k and fcurr = 'EUR' " +
                                      " and tcurr = 'RON' and (100000000 - a.gdatu - 1) <= " + cursDate + condDepartCursV +
                                      " order by 100000000 - a.gdatu - 1 desc ) x where rownum<2";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    string cursValut = "0";

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        cursValut = oReader.GetDouble(0).ToString();
                    }

                    retVal += cursValut + "#";

                    detaliiClient.cursValutar = cursValut;

                    oReader.Close();
                    oReader.Dispose();


                    //motivul blocarii
                    string strBlocare = " ";
                    if (clBlocat.Equals("X"))
                    {
                        cmd = connection.CreateCommand();
                        cmd.CommandText = " select k.grupp, c.gtext from sapprd.knkk k, sapprd.zt691c c " +
                                          " where k.mandt = '900' and c.mandt = '900' and k.kunnr =:k " +
                                          " and k.mandt = c.mandt and k.grupp = c.grupp ";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        oReader = cmd.ExecuteReader();
                        if (oReader.HasRows)
                        {
                            oReader.Read();
                            strBlocare = oReader.GetString(1).ToString();
                        }
                        else
                            strBlocare = " ";

                        oReader.Close();
                        oReader.Dispose();

                    }

                    detaliiClient.motivBlocare = strBlocare;

                    retVal += strBlocare + "#";

                    //filiala clientului
                    string filClnt = " ";
                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select p.kunn2 from sapprd.knvp p where p.mandt = '900' and p.kunnr =:k " +
                                      " and p.parvw = 'ZA' " + condClient2;


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        filClnt = oReader.GetString(0).ToString();
                    }
                    else
                        filClnt = " ";

                    detaliiClient.filiala = filClnt;

                    retVal += filClnt + "#";

                    oReader.Close();
                    oReader.Dispose();


                    //termen de plata
                    string termPlata = " ";
                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and " +
                                      " u.spras = '4' and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm " +
                                      " and u.zterm <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                      " and p.kunnr =:k " + condClient2 + " ) order by u.zterm ";



                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":k", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        termPlata = "";
                        while (oReader.Read())
                        {
                            termPlata += oReader.GetString(0).ToString() + ";";
                        }
                    }

                    detaliiClient.termenPlata = termPlata;

                    retVal += termPlata + "#";

                    detaliiClient.divizii = getDiviziiClient(connection, codClient, codUser);
                    oReader.Close();
                    oReader.Dispose();

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(detaliiClient);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = null;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return serializedResult;

        }

        public static string getDiviziiClient(OracleConnection connection, string codClient, string codAgent)
        {

            string diviziiClient = "";

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct spart from sapprd.knvp p where p.mandt = '900' and p.kunnr =:codClient " +
                                  " and p.pernr =:codAgent and p.vtweg = '10' and p.parvw in ('VE','ZC') ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        diviziiClient += oReader.GetString(0).ToString() + ";";
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return diviziiClient;


        }


        public static string getDiviziiClient(OracleConnection connection, string idComanda)
        {

            string diviziiClient = "";

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct spart from sapprd.knvp p, sapprd.zcomhead_tableta t where p.mandt = '900' and t.mandt = '900' " +
                                  " and t.id =:idComanda and p.kunnr = t.cod_client " +
                                  " and p.pernr = t.cod_agent and p.vtweg = '10' and p.parvw in ('VE','ZC') ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":idComanda", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        diviziiClient += oReader.GetString(0).ToString() + ";";
                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return diviziiClient;


        }



        public string getMeseriasi(string codFiliala, string tipUser, string codUser, string codDepart)
        {
            string serializedResult = "";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Client> listaClienti = new List<Client>();

            if (tipUser.Equals("AV") && !codDepart.Equals("08") && !codDepart.Equals("09"))
            {
                return serializer.Serialize(listaClienti);
            }


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString;

                if (tipUser.Equals("AV"))
                    sqlString = " select c.name1 nume, c.kunnr cod  from sapprd.kna1 c " +
                                " where c.ktokd = '1180' " +
                                " and exists(select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr  and p.vtweg = '20' and " +
                                " p.parvw in ('ZA', 'ZS') and p.kunn2 =:filiala ) " +
                                " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr " +
                                " and p.vtweg = '20' and p.parvw in ('ZC') and p.pernr =:codAgent ) order by c.name1 ";
                else
                    sqlString = " select c.name1 nume, c.kunnr cod from sapprd.kna1 c where  c.ktokd = '1180'  and exists " +
                               " (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr  and p.vtweg = '20' and p.parvw in ('ZA', 'ZS') and p.kunn2 =:filiala )  " +
                               " order by c.name1 ";


                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codFiliala;

                if (tipUser.Equals("AV"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codUser;
                }


                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Client unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = " ";

                        listaClienti.Add(unClient);
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


           
            serializedResult = serializer.Serialize(listaClienti);

            return serializedResult;
        }





        public string getDatePersonaleClient(string numeClient, string tipClient)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<BeanDatePersonale> listDatePersonale = new List<BeanDatePersonale>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

               

                string sqlString = " select name1, stceg, regio, city1, street from sapprd.zinformclmag where mandt='900' " +
                                   " and lower(name1) like lower('" + numeClient + "%') and tip_cl =:tipClient order by name1 ";


                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipClient", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipClient;


                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        BeanDatePersonale datePersonale = new BeanDatePersonale();

                        datePersonale.nume = oReader.GetString(0);
                        datePersonale.cnp = oReader.GetString(1);
                        datePersonale.codjudet = oReader.GetString(2);
                        datePersonale.localitate = oReader.GetString(3);
                        datePersonale.strada = oReader.GetString(4);

                        listDatePersonale.Add(datePersonale);

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

            return new JavaScriptSerializer().Serialize(listDatePersonale);

        }


        public static string getTipUser(OracleConnection connection, string codAgent)
        {
            string tipAgent = "NN";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select upper(substr(tip,0,2)) from agenti where cod =:codAgent ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent.Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipAgent = oReader.GetString(0);

                    if (tipAgent.Substring(0, 1).ToUpper().Equals("C"))
                        tipAgent = "CV";
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


            return tipAgent;
        }



    }
}