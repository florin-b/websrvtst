﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using LiteSFATestWebService.General;

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



        public static string getCodClient(OracleConnection connection, string numeClient)
        {
            string codClient = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;


            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod from clienti where upper(trim(nume)) =:numeClient ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":numeClient", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = numeClient.ToUpper().Trim();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codClient = oReader.GetString(0);
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
            }

            return codClient;
        }


        public static string getCodCategorieClient(string numeCategorie)
        {
            string codCategorie = "00";

            if (numeCategorie.Equals("Client final"))
                codCategorie = "01";
            else if (numeCategorie.Equals("Constructor general"))
                codCategorie = "02";
            else if (numeCategorie.Equals("Constructor special"))
                codCategorie = "03";
            else if (numeCategorie.Equals("Revanzator"))
                codCategorie = "04";
            else if (numeCategorie.Equals("Producator mobila"))
                codCategorie = "05";
            else if (numeCategorie.Equals("Debitor materiale lemnoase"))
                codCategorie = "06";

            return codCategorie;
        }


        public string getListClienti(string numeClient, string depart, string departAg, string unitLog, string codUser, string tipUserSap)
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

                if (!depart.Equals("00") && !depart.Equals("16"))
                {
                    condDepart1 = " and t.depart='" + depart + "'";
                    condDepart2 = " and p.spart = '" + depart + "'";
                }

                if (depart.Equals("16"))
                {
                    condDepart1 = " and t.depart in ('03','04','09') ";
                    condDepart2 = " and p.spart in ('03','04','09') ";
                }


                string exceptieClient = " and p.kunn2 ='" + unitLog + "'";

                if (unitLog.Substring(0, 2).Equals("BU"))
                    exceptieClient = " and p.kunn2 like 'BU%' ";

                //pentru DV si SSCM nu trebuie restrictie pe filiala
                if (unitLog.Equals("NN10") || (tipUserSap != null && (tipUserSap.Equals("SSCM") || tipUserSap.Equals("OIVPD") || tipUserSap.Equals("ASDL") || tipUserSap.Equals("CGED"))))
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

                    string cond1_Canal = "  t.canal = '10' and ";
                    string cond2_Canal = " and p.vtweg = '10' ";

                    if (tipUserSap != null && tipUserSap.Equals("SSCM"))
                    {
                        cond1_Canal = "";
                        cond2_Canal = "";
                    }

                    condClient = " and exists (select 1 from clie_tip t where  " + cond1_Canal + 
                                " t.cod_cli=c.cod " + condDepart1 + " ) " +
                                " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod " + cond2_Canal
                                + condDepart2 + " and p.parvw in ('ZA','ZS') " +
                                  exceptieClient + " ) ";



                    string condExtraClient = " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                             " and p.spart = '" + departAg+ "' and p.parvw in ('VE', 'ZC')  and p.pernr = '" + codUser + "' ) ";


                    if (tipUserSap!= null && tipUserSap.Equals("CVOB"))
                    {
                        condClient = "";
                        condExtraClient = " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                          " and p.parvw in ('VE', 'ZC')  and p.pernr = '" + codUser + "' ) ";
                    }

                    if (depart.Equals("16"))
                    {
                        condExtraClient = " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                          " and p.spart in ('03','04','09') and p.parvw in ('VE', 'ZC')  and p.pernr = '" + codUser + "' ) ";

                    }

                    if (codUser != null)
                    {
                        string tipUser = getTipUser(connection, codUser);

                        if (tipUser != "SD" && !tipUserSap.Contains("KA") && tipUserSap != null && !tipUserSap.Equals(Constants.tipSuperAv) && !tipUserSap.Equals(Constants.tipInfoAv) && !tipUserSap.Equals(Constants.tipSMR) && !tipUserSap.Equals(Constants.tipCVR) && !tipUserSap.Equals(Constants.tipSSCM) && !tipUserSap.Equals(Constants.tipCGED) && !tipUserSap.Equals(Constants.tipOIVPD))
                            condClient += condExtraClient;


                    }

                }



                cmd.CommandText = " select x.nume, x.cod, x.tip_pers, x.city1, x.street ||' '|| x.house_num1, x.region " +
                                  " from (select c.nume, c.cod, c.tip_pers, a.city1, a.street, a.house_num1, a.region from clienti c, sapprd.adrc a " +
                                  " where upper(c.nume) like upper('" + numeClient.Replace("'", "") + "%')  " +
                                  " and a.client = '900' and a.addrnumber = " + 
                                  " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr = c.cod) " +
                                  condClient +
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
                        unClient.numeClient = oReader.GetString(0).Trim();
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        unClient.localitate = oReader.GetString(3);
                        unClient.strada = oReader.GetString(4);
                        unClient.codJudet = oReader.GetString(5);

                        if (tipUserSap != null && tipUserSap.Equals(Constants.tipSuperAv))
                        {
                            unClient.agenti = getAgentiAlocati(connection, depart, unClient.codClient);
                        }
                        else if (tipUserSap != null && (tipUserSap.Equals(Constants.tipInfoAv) || tipUserSap.Equals(Constants.tipSMR) || tipUserSap.Equals(Constants.tipCVR) || tipUserSap.Equals(Constants.tipCGED)))
                        {
                            unClient.agenti = getAgentiAlocati(connection, "00", unClient.codClient);
                        }
                        else if (tipUserSap != null && (tipUserSap.Equals(Constants.tipOIVPD) || tipUserSap.Equals(Constants.tipSSCM)))
                        {
                            unClient.agenti = getUltimAgentComanda(connection, "00", unClient.codClient);
                        }
                        else
                        {
                            unClient.agenti = "@";
                        }

                        unClient.codAgent = "0";
                        unClient.numeAgent = "0";
                        unClient.termenPlata = getTermenPlataClient(connection, unClient.codClient);
                        unClient.clientBlocat = HelperClienti.isClientBlocat(connection, unClient.codClient);

                        unClient.diviziiClient = getDiviziiClient(connection, unClient.codClient, codUser);

                        if (depart.Equals("16"))
                            unClient.diviziiClient = HelperClienti.getDiviziiClientDep16(unClient.diviziiClient);

                            

                        string tipPlataContract = getTipPlataContract(connection, unClient.codClient);

                        if (tipPlataContract.Trim().Equals(String.Empty))
                        {
                            unClient.tipPlata = " ";
                            unClient.termenPlata = new List<string>() { "C000" };
                        }
                        else
                        {
                             unClient.tipPlata = tipPlataContract;
                        }

                        if (unClient.agenti.Contains("@"))
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

        private string getUltimAgentComanda(OracleConnection connection, string codDepart, string codClient)
        {
            string agenti = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            DateTime localDate = DateTime.Now.AddYears(-1);
            string selectedDate = localDate.Year.ToString() + localDate.Month.ToString("00") + localDate.Day.ToString("00");

            try
            {
              
                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.pernr, x.nume from ( " +
                                  " select distinct pernr, nume, max(tb.datac) datacom from sapprd.knvp y, agenti ag, sapprd.zcomhead_tableta tb where y.mandt = '900' " +
                                  " and y.kunnr = :codClient and y.parvw in ('VE','ZC') " +
                                  " and tb.mandt = '900' and tb.cod_client = y.kunnr and tb.datac >= :dataCom and tb.cod_agent = pernr " +
                                  " and ag.cod = pernr and ag.activ = 1 group by pernr, nume order by datacom desc) x where rownum = 1 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":dataCom", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = selectedDate;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    agenti = oReader.GetString(0) + "#" + oReader.GetString(1) + "@";
                }
                else
                    agenti = getAgentiAlocati(connection, codDepart, codClient);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return agenti;
        }


        private string getAgentiAlocati(OracleConnection connection, string codDepart, string codClient)
        {

            string agenti = "";
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string condDepart = " and vtweg = '10' and spart = '" + codDepart + "' ";

                if (codDepart.Equals("00"))
                    condDepart = "";

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct pernr, nume from sapprd.knvp y, agenti ag where y.mandt = '900' and y.kunnr = '" + codClient + 
                                  "' and y.parvw in ('VE','ZC') " + condDepart + " and ag.cod = pernr and ag.activ = 1 order by nume ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        agenti += oReader.GetString(0) + "#" + oReader.GetString(1) + "@";
                    }
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



            return agenti;

        }


         public string getListClientiCUI(string cuiClient, string codAgent)
        {
            List<BeanDatePersonale> listClientiCUI = new List<BeanDatePersonale>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select name1, kunnr, regio, ort01, stras from sapprd.kna1 where mandt = '900' and " +
                                  " TRANSLATE(stceg, '0' || TRANSLATE(stceg, '.0123456789', '.'), '0') = " +
                                  " TRANSLATE(:cui, '0' || TRANSLATE(:cui, '.0123456789', '.'), '0') " +
                                  " and ktokd in ('1000','1020','1030', 'OCAV', 'OCAZ') and sperr <> 'X' order by name1, regio, ort01 ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cui", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = cuiClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        BeanDatePersonale datePersonale = new BeanDatePersonale();
                        datePersonale.nume = oReader.GetString(0);
                        datePersonale.cnp = cuiClient;
                        datePersonale.codjudet = oReader.GetString(2);
                        datePersonale.localitate = oReader.GetString(3);
                        datePersonale.strada = oReader.GetString(4);

                        datePersonale.termenPlata = getTermenPlataClient(connection, datePersonale.cnp);
                        datePersonale.clientBlocat = HelperClienti.isClientBlocat(connection, datePersonale.cnp);
                        datePersonale.codClient = oReader.GetString(oReader.GetOrdinal("kunnr"));

                        string tipPlataContract = getTipPlataContract(connection, datePersonale.cnp);

                        if (tipPlataContract.Trim().Equals(String.Empty))
                        {
                            datePersonale.tipPlata = " ";
                            datePersonale.termenPlata = new List<string>() { "C000" };
                        }
                        else
                        {
                            datePersonale.tipPlata = tipPlataContract;
                        }

                        if (codAgent != null && datePersonale.codClient != null && !datePersonale.codClient.Equals("-1"))
                            datePersonale.divizii = getDiviziiClient(connection, datePersonale.codClient, codAgent);

                        listClientiCUI.Add(datePersonale);
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

            return new JavaScriptSerializer().Serialize(listClientiCUI);
        }


        public string getDateClientAnaf(string cui)
        {
            DateClientAnaf dateClientAnaf = new DateClientAnaf();
            dateClientAnaf.starePlatitorTva = new VerificaTva().verificaTVAService(cui);
            dateClientAnaf.listTipClient = getListTipClientPJ();

            return new JavaScriptSerializer().Serialize(dateClientAnaf);
        }

        private  List<TipClient> getListTipClientPJ()
        {
            List<TipClient> listTipClient = new List<TipClient>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct trim(tip_b2b), upper(trim(txt_tip)) from SAPPRD.zb2b_tipclient order by 1 ";

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        TipClient tipClient = new TipClient();
                        tipClient.codTip = oReader.GetString(0);
                        tipClient.numeTip = oReader.GetString(1);
                        listTipClient.Add(tipClient);
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

            return listTipClient;
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
            JavaScriptSerializer serializer = new JavaScriptSerializer();

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
                    if (!depart.Equals("00") && !depart.Equals("16"))
                        condDepS = " and p.spart = '" + depart + "'";

                    condClient2 = "  and p.vtweg = '10' " + condDepS + " ";
                }


                cmd = connection.CreateCommand();

                String condDepartTip = "";
                if (!depart.Equals("00") && !depart.Equals("16"))
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
                    if (!depart.Equals("00") && !depart.Equals("16"))
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
                    if (!depart.Equals("00") && !depart.Equals("16"))
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
                                      " and p.kunnr =:k " + condClient2 + " ) and u.zterm like 'C%' order by u.zterm ";


                  

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

                    if (depart.Equals("16"))
                        detaliiClient.divizii = HelperClienti.getDiviziiClientDep16(detaliiClient.divizii);

                    //contract activ

                    bool contractActiv = true;


                    if (!contractActiv)
                    {
                        detaliiClient.tipPlata = "";
                        detaliiClient.termenPlata = "C000";
                    }
                    else {

                        //limita de credit definita si tip plata contract
                        cmd = connection.CreateCommand();

                        cmd.CommandText = " select k.klimk, b.zwels  from sapprd.knkk k, sapprd.knb1 b where k.mandt='900' and b.mandt = k.mandt " +
                                          " and k.kunnr = :codClient and b.kunnr = k.kunnr ";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = codClient;

                        oReader = cmd.ExecuteReader();
                        if (oReader.HasRows)
                        {
                            oReader.Read();

                            if (oReader.GetDouble(0) > 1)
                            {
                                if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                                    detaliiClient.tipPlata = oReader.GetString(1);
                                else
                                    detaliiClient.tipPlata = "O";
                            }
                            else
                                detaliiClient.tipPlata = "";

                        }
                        else {
                            detaliiClient.tipPlata = "";
                            detaliiClient.termenPlata = "C000";
                        }


                    }



                    if (codClient.Equals("4110035342"))
                    {
                        detaliiClient.tipPlata = "O";
                        detaliiClient.termenPlata = "C000;C001;C005;C010;C015;C020;C030";
                        
                    }

                    detaliiClient.email = " ";
                    string cuiClient = " ";

                    cmd = connection.CreateCommand();

                    cmd.CommandText = " select nvl(adrs_mail,' '), nvl(cui,' ') from clienti where cod = :codClient ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    oReader = cmd.ExecuteReader();
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        detaliiClient.email = oReader.GetString(0);
                        cuiClient = oReader.GetString(1);
                    }

                    oReader.Close();
                    oReader.Dispose();

                    detaliiClient.errMsg = "";

                    if (cuiClient.Trim().Length > 0)
                    {
                        string stareClient = new VerificaTva().isPlatitorTva(cuiClient, codUser, null);
                        PlatitorTvaResponse starePlatitor = serializer.Deserialize<PlatitorTvaResponse>(stareClient);
                        if (starePlatitor.stareInregistrare != null && starePlatitor.stareInregistrare.ToLower().Contains("radiere"))
                            detaliiClient.errMsg = "Acest client este radiat.";

                    }

                }
                
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



        public static string getDiviziiClientCUI(OracleConnection connection, string codCui, string codAgent)
        {

            string diviziiClient = "";

            if (codAgent == null || codAgent.Trim().Equals(String.Empty))
                return diviziiClient;

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                string tipUser = getTipUser(connection, codAgent);
                string condAgent = " and p.pernr =:codAgent ";

                if (tipUser.Equals("SD"))
                    condAgent = " ";


                cmd.CommandText = " select distinct spart from sapprd.knvp p where p.mandt = '900' and p.kunnr = " +
                                  " (select k.cod from clienti k where k.tip2 in ('1000', 'OCAV', 'OCAZ') and k.tip_pers='PJ'  " +
                                  " and k.cui = TRANSLATE(:codCui, '0' || TRANSLATE(:codCui, '.0123456789', '.'), '0') and rownum = 1 )" +
                                    condAgent + " and p.vtweg = '10' and p.parvw in ('VE','ZC') " + 
                                  " union " +
                                  " select distinct spart from sapprd.knvv v where v.mandt = '900' and " + 
                                  " v.kunnr = (select k.cod from websap.clienti k where k.tip2 in ('1000', 'OCAV', 'OCAZ') and k.tip_pers = 'PJ' " +
                                  " and k.cui = TRANSLATE(:codCui, '0' || TRANSLATE(:codCui, '.0123456789', '.'), '0') and rownum = 1 ) " + 
                                  " and v.vtweg = '10' and v.kdgrp in ('01','20') " + 
                                  " and not exists(select 1 from sapprd.knvp p1 where p1.mandt = '900' and p1.kunnr = v.kunnr and p1.vtweg = v.vtweg and " + 
                                  " p1.spart = v.spart and p1.parvw in ('VE', 'ZC')) order by spart ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                


                cmd.Parameters.Add(":codCui", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCui;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();
                string divizie = "";

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        divizie = oReader.GetString(0);

                        if (divizie.Equals("04"))
                            divizie = "040;041";

                        diviziiClient += divizie + ";";
                        
                    }
                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                if (!diviziiClient.Contains("11"))
                    diviziiClient += "11;";

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return diviziiClient;


        }

        public static string getCodClientNominal(OracleConnection connection, string codCui)
        {
            string codClientNominal = "";

            List<BeanDatePersonale> dateClienti = new List<BeanDatePersonale>();

            if (codCui == null || codCui.Trim().Equals(String.Empty))
                return codClientNominal;

            OracleDataReader oReader = null;

            try
            {

                OracleCommand cmd = connection.CreateCommand();

                cmd.CommandText = " select kunnr from sapprd.kna1 where mandt = '900' and TRANSLATE(stceg, '0' || TRANSLATE(stceg, '.0123456789', '.'), '0') =" +
                    " TRANSLATE(:codCui, '0' || TRANSLATE(:codCui, '.0123456789', '.'), '0') and ktokd in ('1000', 'OCAV', 'OCAZ') ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(":codCui", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCui.ToUpper().Replace("RO","");

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        
                        codClientNominal = oReader.GetString(0);
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

            return codClientNominal;
        }


        public static string getDiviziiClientStatic(string  tipAngajat, string codDepart)
        {
            string diviziiClient = "";

            if (tipAngajat.Equals("AV") || tipAngajat.Equals("SD"))
            {
                if (codDepart.Equals("16"))
                    diviziiClient = "03;040;041;09;11;";
                else if (codDepart.Contains("04"))
                    diviziiClient = "040;041;11;";
                else
                    diviziiClient = codDepart + ";11;";
            }
            else
                diviziiClient = "01;02;03;040;041;05;06;07;08;09;11;";

            return diviziiClient;

        }


        public static string getDiviziiClient(string codClient, string codAgent)
        {
            string diviziiClient = "";

            OracleConnection connection = new OracleConnection();
            connection.ConnectionString = DatabaseConnections.ConnectToTestEnvironment();
            connection.Open();

            diviziiClient = getDiviziiClient(connection, codClient, codAgent);
            connection.Close();
            connection.Dispose();

            return diviziiClient;
        }


        public static string getDiviziiClient(OracleConnection connection, string codClient, string codAgent)
        {

            string diviziiClient = "";

            if (codAgent == null || codAgent.Trim().Equals(String.Empty))
                return "";

            OracleDataReader oReader = null;

            try
            {
                OracleCommand cmd = null;

                cmd = connection.CreateCommand();

                string tipUser = getTipUser(connection, codAgent);
                string condAgent = " and p.pernr =:codAgent ";

                if (tipUser.Equals("SD"))
                    condAgent = " ";


                cmd.CommandText = " select distinct spart from sapprd.knvp p where p.mandt = '900' and p.kunnr =:codClient " +
                    condAgent + " and p.vtweg = '10' and p.parvw in ('VE','ZC') " +
                  " union " +
                  " select distinct spart from sapprd.knvv v where v.mandt = '900' and " +
                  " v.kunnr =:codClient " +
                  " and v.vtweg = '10' and v.kdgrp in ('01','20') " +
                  " and not exists(select 1 from sapprd.knvp p1 where p1.mandt = '900' and p1.kunnr = v.kunnr and p1.vtweg = v.vtweg and " +
                  " p1.spart = v.spart and p1.parvw in ('VE', 'ZC')) order by spart ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();
                string divizie = "";

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        divizie = oReader.GetString(0).ToString();

                        if (divizie.Equals("04"))
                            divizie = "040;041";

                        diviziiClient += divizie + ";";
                    }
                }


                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                if (!diviziiClient.Contains("11"))
                    diviziiClient += "11;";

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " codClient: " + codClient + ",  codAgent: " + codAgent);
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
                                  " and p.vtweg = '10' and p.parvw in ('VE','ZC') ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":idComanda", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();
                string divizie = "";

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        divizie = oReader.GetString(0).ToString();

                        if (divizie.Equals("04"))
                            divizie = "040;041";

                        diviziiClient += divizie + ";";

                    }
                }

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();

                if (!diviziiClient.Contains("11"))
                    diviziiClient += "11;";

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
                string tipConsilier = "";

                if (tipUser.Equals("AV"))
                    sqlString = " select c.name1 nume, c.kunnr cod  from sapprd.kna1 c " +
                                " where c.ktokd = '1180' and c.sperr != 'X' " +
                                " and exists(select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr  and p.vtweg = '20' and " +
                                " p.parvw in ('ZA', 'ZS') and p.kunn2 =:filiala ) " +
                                " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr " +
                                " and p.vtweg = '20' and p.parvw in ('ZC') and p.pernr =:codAgent ) order by c.name1 ";
                else {

                        sqlString = " select c.name1 nume, c.kunnr cod from sapprd.kna1 c where c.ktokd = '1180' and c.sperr != 'X' " + 
                                    " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr and p.vtweg = '20' " +
                                    " and p.parvw in ('ZA', 'ZS') and p.kunn2 = :filiala ) " + 
                                    " and (exists(select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.kunnr and p.vtweg = '20' " + 
                                    " and p.parvw in ('ZC', 'VE') and p.pernr = :codAgent) " +
                                    " or exists (select 1 from websap.agenti a where a.tip in ('SMR', 'CVR', 'CVS') and a.cod = :codAgent )) " + 
                                    " order by c.name1 ";
                }

               

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codFiliala;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codUser;


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





        public string getDatePersonaleClient(string numeClient, string tipClient, string codAgent, string depart, string tipUserSap)
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

                string sqlString;

                if (tipClient.Equals("PF"))
                    sqlString = " select name1, stceg, regio, city1, street||' '||nr from sapprd.zinformclmag where mandt='900' " +
                                " and lower(name1) like lower('" + numeClient.Trim() + "%') and tip_cl ='PF' order by name1 ";
                else {

                    if (tipUserSap != null && tipUserSap.Equals("CVOB"))
                        sqlString = "select c.nume, c.cui cui, a.region, a.city1, a.street||' '||a.house_num1, c.cod codClient from websap.clienti c, sapprd.adrc a where " +
                                    " upper(c.nume) like upper('" + numeClient.Trim() + "%') and a.client = '900' " + 
                                    " and a.addrnumber = (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr = c.cod) and " + 
                                    " exists (select 1 from clie_tip t where t.canal = '10' and t.cod_cli = c.cod  ) " + 
                                    " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod and p.vtweg = '10' " + 
                                    " and p.parvw in ('VE', 'ZC') and p.pernr = '" + codAgent + "') order by c.nume" ; 
                    else
                        sqlString = " select z.numefirma, z.cui, z.judet, z.localitate, z.adresa||' '||z.nr, " +
                                    " nvl((select k.cod from clienti k where k.tip2 in ('1000', '1020', 'OCAV', 'OCAZ') and k.tip_pers='PJ' " +
                                    " and k.cui = TRANSLATE(z.cui, '0' || TRANSLATE(z.cui, '.0123456789', '.'), '0')),'-1') codclient " +
                                    " from sapprd.zverifcui z where z.mandt='900' and " +
                                    " lower(z.numefirma) like lower('" + numeClient.Trim() + "%') and length(trim(z.judet)) = 2 and rownum <= 30 order by z.numefirma ";
                }


                cmd.CommandText = sqlString;

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
                        datePersonale.divizii = "";

                        if (tipClient.Equals("PF"))
                        {
                            datePersonale.clientBlocat = false;
                            datePersonale.tipPlata = " ";
                            datePersonale.termenPlata = new List<string>() { "C000" };
                            datePersonale.cnp = "";
                        }
                        else {

                            datePersonale.termenPlata = getTermenPlataClient(connection, datePersonale.cnp);
                            datePersonale.clientBlocat = HelperClienti.isClientBlocat(connection, datePersonale.cnp);
                            datePersonale.codClient = oReader.GetString(oReader.GetOrdinal("codClient"));

                            string tipPlataContract = getTipPlataContract(connection, datePersonale.cnp);

                            if (tipPlataContract.Trim().Equals(String.Empty))
                            {
                                datePersonale.tipPlata = " ";
                                datePersonale.termenPlata = new List<string>() { "C000" };
                            }
                            else
                            {
                                 datePersonale.tipPlata = tipPlataContract;
                            }

                            if (codAgent != null && datePersonale.codClient != null && !datePersonale.codClient.Equals("-1"))
                                datePersonale.divizii = getDiviziiClient(connection, datePersonale.codClient, codAgent);

                            if (depart != null && depart.Equals("16"))
                                datePersonale.divizii = HelperClienti.getDiviziiClientDep16(datePersonale.divizii);

                        }


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

        public string cautaClientPF(string strClient, string criteriu)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<BeanDatePersonale> listDatePersonale = new List<BeanDatePersonale>();

            if (strClient == null || strClient.Trim().Equals(String.Empty))
                return serializer.Serialize(listDatePersonale);

            if (criteriu == null || criteriu.Trim().Equals(String.Empty))
                return serializer.Serialize(listDatePersonale);

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString;

                if (criteriu.ToLower().Equals("nume"))
                    sqlString = " select k.name1, k.sortl, k.regio, k.ort01, k.stras, k.kunnr from sapprd.kna1 k where k.mandt = '900' and " +
                                " upper(k.name1) like upper('" + strClient.Trim() + "%') ";
                else
                    sqlString = " select k.name1, k.sortl, k.regio, k.ort01, k.stras, k.kunnr from sapprd.kna1 k where k.mandt = '900' and " +
                                " k.sortl = :telefon ";

                cmd.CommandText = sqlString;

                if (!criteriu.ToLower().Equals("nume"))
                {
                    cmd.Parameters.Add(":telefon", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = strClient;
                }

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
                        datePersonale.divizii = "";
                        datePersonale.clientBlocat = false;
                        datePersonale.tipPlata = " ";
                        datePersonale.termenPlata = new List<string>() { "C000" };
                        datePersonale.codClient = oReader.GetString(5);
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

            return serializer.Serialize(listDatePersonale);

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


        public string getListClientiInstPublice(string numeClient, string unitLog, string tipUser, string tipClient)
        {

           

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Client> listClienti = new List<Client>();


            try
            {

                int i = 0;
                bool isCUI = int.TryParse(numeClient, out i);

                string restrClient = " and lower(c.nume) like lower('" + numeClient + "%') ";

                if (isCUI)
                    restrClient = " and (k.stceg like '" + numeClient + "%' or upper(k.stceg) like 'RO" + numeClient + "%')";


                string tipClientIP = "18";

                if (tipClient != null && tipClient.Equals("NONCONSTR"))
                    tipClientIP = "19";

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select distinct c.nume, c.cod, c.tip_pers, k.stceg, v.kdgrp, p.kunn2 from websap.clienti c,  sapprd.knvv v, sapprd.knvp p, sapprd.kna1 k " +
                                  " where c.cod = v.kunnr and v.mandt = '900' and v.vtweg = '20' " + 
                                  " and v.spart = '11' and v.kdgrp=:tipClient and v.mandt = p.mandt " +
                                  " and v.kunnr = p.kunnr and v.vtweg = p.vtweg and v.spart = p.spart and k.mandt='900' and k.kunnr = c.cod " + 
                                  " and p.parvw in ('ZA','ZS') " +  restrClient + " order by nume ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":tipClient", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = tipClientIP;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ClientIP unClient = new ClientIP();
                        unClient.numeClient = oReader.GetString(0) + " (" + oReader.GetString(5) + ")";
                        unClient.codClient = oReader.GetString(1);
                        unClient.tipClient = oReader.GetString(2);
                        unClient.codCUI = oReader.GetString(3);
                        unClient.tipClientIP = oReader.GetString(4);
                        unClient.filiala = oReader.GetString(5);

                        unClient.termenPlata = getTermenPlataInstPublic(connection, unClient.codClient);
                        unClient.clientBlocat = HelperClienti.isClientBlocat(connection, unClient.codClient);

                        string tipPlataContract;

                        if (tipClientIP.Equals("18"))
                            tipPlataContract = getTipPlataContractIP18(connection, unClient.codClient);
                        else
                            tipPlataContract = getTipPlataContractIP19(connection, unClient.codClient);

                        if (tipPlataContract.Trim().Equals(String.Empty))
                        {
                            unClient.tipPlata = " ";
                            unClient.termenPlata = new List<string>() { "C000" };
                        }
                        else
                        {
                            unClient.tipPlata = tipPlataContract;
                        }

                        listClienti.Add(unClient);
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


            return new JavaScriptSerializer().Serialize(listClienti);


        }


        private List<string> getTermenPlataInstPublic(OracleConnection connection, string codClient)
        {
            
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> termenPlata = new List<string>();
            termenPlata.Add("C010");

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '20'  and p.spart = '11' ) and u.zterm like 'C%' order by u.zterm ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
                    }
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


            return termenPlata;


        }

        public static string getCodClientCUI(OracleConnection connection, string codCUI)
        {
            string codClient = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod from clienti where cui=:codCui ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCui", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codCUI;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codClient = oReader.GetString(0);
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

            return codClient;
        }


        private List<string> getTermenPlataClient(OracleConnection connection, string codClient)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string localCodClient = codClient;

            if (HelperClienti.isConditiiCodNominal(codClient))
                localCodClient = getCodClientCUI(connection, codClient);

            List<string> termenPlata = new List<string>();
            termenPlata.Add("C010");

            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '10' ) and upper(u.zterm) like 'C%' order by u.zterm ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = localCodClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
                    }
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


            return termenPlata;


        }


        public string getTermenPlataClientDepart(string codAgent, string codClient)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<string> termenPlata = new List<string>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select u.zterm from sapprd.T052u u, sapprd.TVZBT t where u.mandt = '900' and  u.spras = '4' " +
                                  " and u.mandt = t.mandt and u.spras = t.spras and u.zterm = t.zterm  and u.zterm " +
                                  " <= (select max(p.zterm) from sapprd.knvv p where p.mandt = '900' " +
                                  " and p.kunnr = :codClient and p.vtweg = '20' and p.spart = (select divizie from agenti where activ = 1 and cod=:codAgent)) " +
                                  " and u.zterm != 'C000' and u.zterm like 'C%' order by u.zterm ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    termenPlata.Clear();
                    while (oReader.Read())
                    {
                        termenPlata.Add(oReader.GetString(0));
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


            return new JavaScriptSerializer().Serialize(termenPlata);


        }



        public static string getAgentAlocat(OracleConnection connection, OracleTransaction transaction,  string codDepart, string codClient, string codAgentInit)
        {

            string codAgent = "";
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (codDepart.Equals("11"))
                return codAgentInit;


            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select p.pernr from sapprd.knvp p where p.mandt = '900' and p.kunnr =:codClient  and " + 
                                  " p.vtweg = '10'  and substr(p.spart,0,2) =:codDepart and p.parvw in ('VE', 'ZC') ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codDepart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codDepart.Substring(0, 2);

                cmd.Transaction = transaction;
                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    {
                        codAgent = oReader.GetString(0);
                    }
                }
                else
                    codAgent = codAgentInit;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            ErrorHandling.sendErrorToMail("getAgentAlocat:" + " cod agent: " + codAgent + " cod depart " + codDepart + " cod init agent " + codAgentInit + " cod client : " + codClient);

            return codAgent;

        }



        public string getClientiAlocati(string codAgent, string filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientAlocat> listClientiAlocati = new List<ClientAlocat>();

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select c.nume, " +
                                  " nvl(max(decode(t.depart, '01', t.tip)), ' ') d01, nvl(max(decode(t.depart, '02', t.tip)), ' ') d02, " +
                                  " nvl(max(decode(t.depart, '03', t.tip)), ' ') d03, nvl(max(decode(t.depart, '04', t.tip)), ' ') d04, " +
                                  " nvl(max(decode(t.depart, '05', t.tip)), ' ') d05, nvl(max(decode(t.depart, '06', t.tip)), ' ') d06, " +
                                  " nvl(max(decode(t.depart, '07', t.tip)), ' ') d07, nvl(max(decode(t.depart, '08', t.tip)), ' ') d08, " +
                                  " nvl(max(decode(t.depart, '09', t.tip)), ' ') d09 " +
                                  " from clie_tip t, clienti c where t.cod_cli = c.cod and t.canal = '10' " +
                                  " and exists (select 1 from clie_tip t where t.canal = '10'  and t.cod_cli = c.cod  ) " +
                                  " and exists (select 1 from sapprd.knvp p where p.mandt = '900' and p.kunnr = c.cod  and p.vtweg = '10' " +
                                  " and p.parvw in ('ZA', 'ZS')  and p.kunn2 =:filiala )  and exists (select 1 from sapprd.knvp p where p.mandt = '900' " +
                                  " and p.kunnr = c.cod  and p.vtweg = '10'  and p.parvw in ('VE', 'ZC')  and p.pernr =:codAgent ) group by c.nume order by c.nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ClientAlocat client = new ClientAlocat();

                        client.numeClient = oReader.GetString(0);
                        client.tipClient01 = getTipClient(oReader.GetString(1));
                        client.tipClient02 = getTipClient(oReader.GetString(2));
                        client.tipClient03 = getTipClient(oReader.GetString(3));
                        client.tipClient04 = getTipClient(oReader.GetString(4));
                        client.tipClient05 = getTipClient(oReader.GetString(5));
                        client.tipClient06 = getTipClient(oReader.GetString(6));
                        client.tipClient07 = getTipClient(oReader.GetString(7));
                        client.tipClient08 = getTipClient(oReader.GetString(8));
                        client.tipClient09 = getTipClient(oReader.GetString(9));
                        listClientiAlocati.Add(client);
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

            return new JavaScriptSerializer().Serialize(listClientiAlocati);


        }

        public string getInfoCreditClient(string codClient)
        {

            CreditClient creditClient = new CreditClient();
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();
                string nowDate = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00"); 

                cmd = connection.CreateCommand();
                cmd.CommandText = " select distinct k.kunnr,k.klimk lcredit, " +
                  " k.skfor,k.ssobl, nvl((select s2.olikw+s2.ofakw from sapprd.s067 s2 where s2.mandt='900' and " +
                  " s2.kkber='1000' and s2.knkli=k.kunnr),0) olikw, nvl((select sum(s1.oeikw) from sapprd.s066 s1 " +
                  " where s1.mandt='900' and s1.kkber='1000' and spmon=:l  and s1.knkli=k.kunnr),0) oeikw,  k.crblb " +
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
                    creditClient.limitaCredit = oReader.GetFloat(1);
                    creditClient.restCredit = Math.Round(oReader.GetFloat(1) - (oReader.GetFloat(2) + oReader.GetFloat(3)) - (oReader.GetFloat(4) + oReader.GetFloat(5)),2);
                    creditClient.isBlocat = oReader.GetString(6).Equals("X");
                }


                if (creditClient.isBlocat)
                {
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
                            creditClient.motivBlocat = oReader.GetString(1).ToString();
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

            return new JavaScriptSerializer().Serialize(creditClient);

        }

        private string getTipPlataContract(OracleConnection connection, string codClient)
        {
            
            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            string localCodClient = codClient;

            if (HelperClienti.isConditiiCodNominal(codClient))
                localCodClient = getCodClientCUI(connection, codClient);

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = localCodClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }
                   
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }



            return tipPlata;

        }


        private string getTipPlataContractIP18(OracleConnection connection, string codClient)
        {

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and (exists (select 1 from sapprd.knvv v where v.mandt = '900' and v.kunnr = b.kunnr and v.vtweg = '20' and v.spart = '11' and v.kdgrp = '18') " +
                                  " or ( c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate )) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPlata;
        }


        private string getTipPlataContractIP19(OracleConnection connection, string codClient)
        {

            OracleDataReader oReader = null;
            OracleCommand cmd = new OracleCommand();
            string tipPlata = "";

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select k.klimk, b.zwels from sapprd.knkk k, sapprd.knb1 b, sapprd.kna1 c " +
                                  " where k.mandt = '900' and b.mandt = '900' and c.mandt = '900' and c.kunnr = k.kunnr " +
                                  " and k.kunnr = :codClient and b.kunnr = k.kunnr " +
                                  " and(exists(select * from sapprd.knvv v where v.mandt = '900' and v.kunnr = b.kunnr and v.vtweg = '20' and v.spart = '11' and v.kdgrp = '19') " +
                                  " or(c.ZDATAEXPCTR != '00000000' and to_date(c.ZDATAEXPCTR, 'yyyymmdd') >= sysdate)) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();
                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetDouble(0) > 1)
                    {
                        if (oReader.GetString(1) != null && oReader.GetString(1).Trim() != "")
                            tipPlata = oReader.GetString(1);
                        else
                            tipPlata = "O";
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + codClient);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPlata;
        }

        public string creeazaClientPJ(string dateClientPJ)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            RaspunsClientSap raspunsClient = new RaspunsClientSap();

            DateClientSap dateClientSap = serializer.Deserialize<DateClientSap>(dateClientPJ);


            DatePoligon datePoligon =  serializer.Deserialize<DatePoligon>(new OperatiiPoligoane().getDatePoligonLivrareDB(dateClientSap.coordonateAdresa));
            dateClientSap.filialaAsociata = datePoligon.filialaPrincipala;

            try {

                SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();
                SAPWebServices.ZcreateClientMinReq inParam = new SAPWebServices.ZcreateClientMinReq();

                webService.Credentials = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Timeout = 300000;

                inParam.IpClient = new SAPWebServices.ZclientMin();

                inParam.CodAv = dateClientSap.codAgent;
                inParam.IpClient.Fiscalcode = dateClientSap.cui;
                inParam.IpClient.Companyname = dateClientSap.numeCompanie;
                inParam.IpClient.Companyemail = dateClientSap.emailCompanie;
                inParam.IpClient.Companystreet = dateClientSap.strada;
                inParam.IpClient.Companynumber = dateClientSap.numarStrada;
                inParam.IpClient.Companycity = dateClientSap.localitate;
                inParam.IpClient.Companycounty = dateClientSap.judet;
                inParam.IpClient.Firstname = dateClientSap.prenumePersContact;
                inParam.IpClient.Lastname = dateClientSap.numePersContact;
                inParam.IpClient.Phonenumber = dateClientSap.telPersContact;
                inParam.IpClient.Comregnumber = dateClientSap.codJ;
                inParam.IpClient.Vatpayer = dateClientSap.platitorTVA;
                inParam.IpClient.Prctr = dateClientSap.filialaAsociata;
                inParam.IpClient.TipB2b = dateClientSap.tipClient;
                

                SAPWebServices.ZcreateClientMinReqResponse outParam = webService.ZcreateClientMinReq(inParam);

                if (outParam.EpReturncode.ToString().Equals("0"))
                {
                    raspunsClient.codClient = outParam.EpKunnr;
                    raspunsClient.msg = "";
                    raspunsClient.diviziiClient = getDiviziiClientStatic(dateClientSap.tipAngajat, dateClientSap.codDepart);

                } else
                {
                    raspunsClient.codClient = "";
                    raspunsClient.msg = outParam.EpMess;
                    ErrorHandling.sendErrorToMail("creeazaClientPJ: \n\n" + serializer.Serialize(dateClientPJ) + "\n\n" + serializer.Serialize(inParam) + "\n\n" +
                    serializer.Serialize(outParam));
                }

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " \n\n " + dateClientPJ);
            }

            return serializer.Serialize(raspunsClient);
        }


        public string creeazaClientPF(string dateClientPF)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            RaspunsClientSap raspunsClient = new RaspunsClientSap();

            DateClientSap dateClientSap = serializer.Deserialize<DateClientSap>(dateClientPF);


            DatePoligon datePoligon = serializer.Deserialize<DatePoligon>(new OperatiiPoligoane().getDatePoligonLivrareDB(dateClientSap.coordonateAdresa));
            dateClientSap.filialaAsociata = datePoligon.filialaPrincipala;

            try
            {

                SAPWebServices.ZTBL_WEBSERVICE webService = new SAPWebServices.ZTBL_WEBSERVICE();
                SAPWebServices.ZcreateClientMinReq inParam = new SAPWebServices.ZcreateClientMinReq();

                webService.Credentials = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Timeout = 300000;

                inParam.IpClient = new SAPWebServices.ZclientMin();

                inParam.CodAv = dateClientSap.codAgent;
                inParam.IpClient.Fiscalcode = " ";
                inParam.IpClient.Companyname = dateClientSap.numeCompanie;
                inParam.IpClient.Companyemail = dateClientSap.emailCompanie;
                inParam.IpClient.Companystreet = dateClientSap.strada;
                inParam.IpClient.Companynumber = " ";
                inParam.IpClient.Companycity = dateClientSap.localitate;
                inParam.IpClient.Companycounty = dateClientSap.judet;
                inParam.IpClient.Firstname = " ";
                inParam.IpClient.Lastname = dateClientSap.numeCompanie;
                inParam.IpClient.Phonenumber = dateClientSap.telPersContact;
                inParam.IpClient.Comregnumber = " ";
                inParam.IpClient.Vatpayer = " ";
                inParam.IpClient.Prctr = dateClientSap.filialaAsociata;
                inParam.IpClient.TipB2b = "PF";


                SAPWebServices.ZcreateClientMinReqResponse outParam = webService.ZcreateClientMinReq(inParam);

                if (outParam.EpReturncode.ToString().Equals("0"))
                {
                    raspunsClient.codClient = outParam.EpKunnr;
                    raspunsClient.msg = "";
                    raspunsClient.diviziiClient = getDiviziiClientStatic(dateClientSap.tipAngajat, dateClientSap.codDepart);
                }
                else
                {
                    raspunsClient.codClient = "";
                    raspunsClient.msg = outParam.EpMess;

                    ErrorHandling.sendErrorToMail("creeazaClientPF: \n\n" + serializer.Serialize(dateClientPF) + "\n\n" + serializer.Serialize(inParam) + "\n\n" +
                    serializer.Serialize(outParam));
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return serializer.Serialize(raspunsClient);
        }

        private static string getTipClient(string codTip)
        {
            string tipClient = codTip;

            if (codTip.Equals("01"))
                tipClient = "Final";

            else if (codTip.Equals("02"))
                tipClient = "Constr. general";

            else if (codTip.Equals("03"))
                tipClient = "Constr. special";

            else if (codTip.Equals("04"))
                tipClient = "Revanzator";

            else if (codTip.Equals("05"))
                tipClient = "Prod. mobila";

            else if (codTip.Equals("06"))
                tipClient = "Debit. mat. lemn.";

            else if (codTip.Equals("07"))
                tipClient = "Fraudator";

            else if (codTip.Equals("08"))
                tipClient = "Nespecificat";

            else if (codTip.Equals("09"))
                tipClient = "Extern UE";

            else if (codTip.Equals("10"))
                tipClient = "Extern non-UE";

            else if (codTip.Equals("11"))
                tipClient = "Desfiintat";

            return tipClient;
        }





    }
}