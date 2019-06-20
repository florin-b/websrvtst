﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using LiteSFATestWebService.SAPWebServices;


namespace LiteSFATestWebService
{
    public class OperatiiArticole
    {

        public OperatiiArticole()
        {
        }

        public enum EnumUnitMas { G, KG, TO };

        private static double GRAMS = 0.001;
        private static double TONES = 1000;


        public string getListArticoleFurnizor(string codArticol, string tip1, string tip2, string furnizor, string codDepart)
        {

            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string conditie = "";
            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                if (tip1 == "1") //cod
                {
                    if (tip2 == "1") //articol
                    {
                        conditie = " substr(b.cod,-8) like '" + codArticol + "%' ";
                    }

                    if (tip2 == "2") //sintetic
                    {
                        conditie = " b.sintetic like '" + codArticol + "%' ";
                    }

                }

                if (tip1 == "2")  //nume
                {
                    if (tip2 == "1") //articol
                    {
                        conditie = " upper(b.nume) like upper('" + codArticol + "%') ";
                    }

                    if (tip2 == "2") //sintetic
                    {
                        conditie = " upper(c.nume) like upper('" + codArticol + "%') ";
                    }

                }



                string conditieDepart = "";
                if (!codDepart.Equals("00"))
                    conditieDepart = " b.grup_vz =:depart and ";

                if (codDepart.StartsWith("04"))
                    conditieDepart = " substr(b.grup_vz,0,2) = substr(:depart,0,2) and ";

                cmd.CommandText = " select x.cod_art, x.nume, nvl(x.meins,'-') meins, x.umvanz10, x.sintetic, x.cod_nivel1, x.umvanz10, x.tip_mat, x.grup_vz, x.dep_aprobare, " +
                                  " x.palet, x.categ_mat, x.lungime " +
                                  " from (select decode(length(e.matnr),18,substr(e.matnr,-8),e.matnr) " +
                                  " cod_art, b.nume, e.meins, b.umvanz10, b.sintetic, c.cod_nivel1, nvl(b.tip_mat,' ') tip_mat, b.grup_vz, " +
                                  " decode(trim(b.dep_aprobare),'','00', b.dep_aprobare)  dep_aprobare, " +
                                  " (select nvl( " +
                                  " (select 1 from sapprd.marm m where m.mandt = '900' and m.matnr = b.cod and m.meinh = 'EPA'),-1) palet from dual) palet, " +
                                  " b.categ_mat, b.lungime " +
                                  " from sapprd.eina e, articole b, sintetice c where e.mandt = '900' and e.matnr = b.cod and   " +
                                  " c.cod = b.sintetic and e.lifnr=:furniz and  " + conditieDepart  + conditie + " ) x where rownum < 50 order by x.nume ";

              
                

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":furniz", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = furnizor;

                if (!codDepart.Equals("00"))
                {
                    cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codDepart;
                }


                List<ArticolCautare> listArticole = new List<ArticolCautare>();
                ArticolCautare articol;

                oReader = cmd.ExecuteReader();
                string umAprov = "";
                double stocArtStatic = -1;
                string strCat = "";
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCautare();

                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        umAprov = oReader.GetString(2);

                        if (umAprov.Equals("-"))
                            umAprov = oReader.GetString(3);

                        articol.umVanz = umAprov;
                        articol.stoc = stocArtStatic.ToString();
                        

                        articol.sintetic = oReader.GetString(4);
                        articol.nivel1 = oReader.GetString(5);
                        articol.umVanz10 = oReader.GetString(6);
                        articol.tipAB = oReader.GetString(7);
                        articol.depart = oReader.GetString(8);
                        articol.departAprob = oReader.GetString(9);
                        articol.umPalet = oReader.GetInt32(10).ToString();

                        strCat = oReader.GetString(11);

                        if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                            strCat = "AM";
                        else
                            strCat = " ";

                        articol.categorie = strCat;

                        articol.lungime = oReader.GetDouble(12).ToString();

                        listArticole.Add(articol);

                    }


                }

               

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }


            return serializedResult;


        }



        public string getListArticoleComplementare(string listaArticole)
        {

            string serializedResult = "";
            string sqlString = "";

            var serializer = new JavaScriptSerializer();

            List<ArticolCautare> articole = serializer.Deserialize<List<ArticolCautare>>(listaArticole);
            List<String> listSinteticCompl = null;
            List<String> listNivel1Compl = null;

            string coduriArticole = "";
            string codArticol;

            for (int i = 0; i < articole.Count; i++)
            {

                codArticol = articole[i].cod;

                if (articole[i].cod.Length >= 8)
                    codArticol = "0000000000" + articole[i].cod;

                if (coduriArticole.Equals(""))
                    coduriArticole = "'" + codArticol + "'";
                else
                    coduriArticole = coduriArticole + ",'" + codArticol + "'";
            }

            coduriArticole = "(" + coduriArticole + ")";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            OracleCommand cmd = connection.CreateCommand();

            List<ArticolCautare> listArtCompl = new List<ArticolCautare>();

            try
            {

                connection.ConnectionString = connectionString;
                connection.Open();


                sqlString = " select 'sint' tip, a.lista_mat_sec " +
                            " from sapprd.ZCOMPLEMENTAR a, articole b, sintetice c " +
                            " where b.sintetic = a.MATKL_PRINC and c.cod = a.MATKL_PRINC and b.cod in " + coduriArticole + " " +
                            " union  select 'niv1' tip, a.lista_mat_sec " +
                            " from sapprd.ZCOMPLEMENTAR a, articole b,  sintetice d " +
                            " where d.cod_nivel1 = a.NIV_PRINC and d.cod = b.sintetic and b.cod in " + coduriArticole + " ";

                cmd.CommandText = sqlString;
                cmd.CommandType = CommandType.Text;

                oReader = cmd.ExecuteReader();

                listSinteticCompl = new List<string>();
                listNivel1Compl = new List<string>();

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {

                        if (oReader.GetString(0).Equals("sint"))
                        {
                            listSinteticCompl.Add(oReader.GetString(1));
                        }
                        else
                        {
                            listNivel1Compl.Add(oReader.GetString(1).Replace(";", "','"));
                        }

                    }
                }


                string strSintetice = "", strNivel1 = "";

                for (int i = 0; i < listSinteticCompl.Count(); i++)
                {
                    if (strSintetice.Equals(""))
                        strSintetice = "'" + listSinteticCompl[i] + "'";
                    else
                        strSintetice = strSintetice + ",'" + listSinteticCompl[i] + "'";
                }

                if (strSintetice.Equals(""))
                    strSintetice = "''";

                strSintetice = "(" + strSintetice + ")";

                for (int i = 0; i < listNivel1Compl.Count(); i++)
                {
                    if (strNivel1.Equals(""))
                        strNivel1 = "'" + listNivel1Compl[i] + "'";
                    else
                        strNivel1 = strNivel1 + ",'" + listNivel1Compl[i] + "'";
                }


                if (strNivel1.Equals(""))
                    strNivel1 = "''";

                strNivel1 = "(" + strNivel1 + ")";


                sqlString = " select distinct nume, cod cod from sintetice where cod in " + strSintetice +
                            " union " +
                            " select distinct nume_nivel1, cod_nivel1 cod from sintetice where cod_nivel1 in " + strNivel1 + " order by cod ";

                ArticolCautare articolCompl;


                cmd.CommandText = sqlString;
                cmd.CommandType = CommandType.Text;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        articolCompl = new ArticolCautare();
                        articolCompl.nume = oReader.GetString(0);
                        articolCompl.cod = oReader.GetString(1);
                        listArtCompl.Add(articolCompl);
                    }
                }

                serializedResult = serializer.Serialize(listArtCompl);

                
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return serializedResult;
        }


        public string getListArticoleStatistic(string codClient, string filiala, string departament)
        {

            string serializedResult;
            List<ArticolCautare> listArticole = new List<ArticolCautare>();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select x.* from (select distinct decode(length(a.cod), 18, substr(a.cod, -8), a.cod) codart,a.nume, " +
                              " a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat, ' '),  b.cod nume_sint, " +
                              " a.grup_vz, decode(trim(a.dep_aprobare), '', '00', a.dep_aprobare)  dep_aprobare, " +
                              " (select nvl((select 1 from sapprd.marm m where m.mandt = '900' " +
                              " and m.matnr = a.cod and m.meinh = 'EPA'),-1) palet from dual) palet " +
                              " , -1 stoc , categ_mat, lungime, count(b.cod)  from articole a, sintetice b, sapprd.marc c, " +
                              " sapprd.zcomhead_tableta ht, sapprd.zcomdet_tableta dt " +
                              " where c.mandt = '900' and c.matnr = a.cod and c.werks =:filiala and c.mmsta <> '01' " +
                              " and a.sintetic = b.cod and upper(a.nume)not like '%TRANSPORT%' and a.blocat <> '01' and " +
                              " ht.datac >=:dataCautare and ht.cod_client =:codClient  and a.cod = dt.cod and a.grup_vz =:depart and " +
                              " ht.id = dt.id and ht.status = '2' and ht.status_aprov in (2, 15)  " +
                              " group by a.cod, a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, a.tip_mat, b.cod, a.grup_vz,a.dep_aprobare, " +
                              " categ_mat, lungime having count(b.cod) > 1  order by count(b.cod) desc, a.nume) x where rownum<=100 ";


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":dataCautare", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = getStatisticDate();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = departament;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = filiala;

                oReader = cmd.ExecuteReader();

                ArticolCautare articol;
                string strCat;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        articol = new ArticolCautare();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.sintetic = oReader.GetString(2);
                        articol.nivel1 = oReader.GetString(3);
                        articol.umVanz10 = oReader.GetString(4);
                        articol.umVanz = oReader.GetString(8).Substring(0, 2).Equals("11") ? oReader.GetString(5) : oReader.GetString(4);
                        articol.tipAB = oReader.GetString(6);
                        articol.depart = oReader.GetString(8).Substring(0, 2);
                        articol.departAprob = oReader.GetString(9);
                        articol.umPalet = oReader.GetInt32(10).ToString();
                        articol.stoc = oReader.GetDouble(11).ToString();

                        strCat = oReader.GetString(12);

                        if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                            strCat = "AM";
                        else
                            strCat = " ";

                        articol.categorie = strCat;
                        articol.lungime = oReader.GetDouble(13).ToString();

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


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listArticole);

            return serializedResult;

        }


        private string getStatisticDate()
        {
            return DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
        }


        public string getListArticoleDistributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament, string afisStoc, string tipUser, string codUser, string modulCautare)
        {

            string condExtraDepart = " ";
            List<ArticolCautare> listArticole = new List<ArticolCautare>();

            if (codUser != null && (modulCautare == null || !modulCautare.Equals("CLP")) && General.GeneralUtils.isFilialaExtensie02(filiala) && departament.Equals("02"))
            {
                string departUser =  Service1.getDepartAgent(codUser);
                string departExtra = Service1.getDepartExtra(codUser, departUser, filiala);

                if (departUser.Equals("01") && departExtra.Equals("02"))
                    condExtraDepart = " and x.sintetic in ('204', '204_01', '205', '236', '237', '229', '238', '240') ";
            }

            if (modulCautare != null && modulCautare.Equals("CLP") && !filiala.StartsWith("BU") && searchString.StartsWith("111"))
            {
                return new JavaScriptSerializer().Serialize(listArticole);
            }



            string serializedResult = "";
            string condCautare = "";
            string condDepart = " ";
            string condTabCodBare = "";
            string condLimit = " rownum < 300 ";
            string valStoc = "";

            if (!departament.Equals("00") && !departament.Equals("12") && departament.Length > 0)
            {
                if (Utils.isFilialaMica04(filiala, departament) && modulCautare != null && modulCautare.Equals("CLP"))
                    condDepart = " and ( substr(a.grup_vz,0,2) like '" + departament.Substring(0,2) + "%') ";
                else
                    condDepart = " and (a.grup_vz like '" + departament + "%') ";
            }
            else
            {
                if (departament.Equals("00") && modulCautare == null)
                    condDepart = " and a.grup_vz <> '11' ";
                else if (departament.Equals("12"))
                    condDepart = " and a.grup_vz in ('01','02') ";
            }

            if (tipCautare.Equals("C"))
            {
                if (tipArticol.Equals("A"))
                {
                    
                    condCautare = " ( lower(decode(length(a.cod),18,substr(a.cod,-8),a.cod)) like lower('" + searchString + "%') " +
                       " or d.cod_bare like '" + searchString + "%' ) and d.cod(+) = a.cod ";
                    condTabCodBare = ", artiscan d ";
                    
                }

                if (tipArticol.Equals("S"))
                {
                    condCautare = " a.sintetic in ('" + searchString + "') ";
                    condLimit = " 1 = 1";
                }

            }


            if (tipCautare.Equals("N"))
            {
                if (tipArticol.Equals("A"))
                    condCautare = "  upper(a.nume) like upper('" + searchString.ToUpper() + "%')";

                if (tipArticol.Equals("S"))
                    condCautare = "  upper(b.nume) like upper('" + searchString.ToUpper() + "%')";

            }

            if (afisStoc != null && afisStoc.Equals("1"))
            {
                valStoc = " , nvl ((select sum(stocne) from sapprd.zstoc_job where matnr=a.cod and werks=:filiala),-1) stoc ";
            }
            else
            {
                valStoc = " , -1 stoc ";
            }


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                if (filiala == null || (filiala != null && filiala.Equals("NN10")) || departament.Equals("00"))
                {
                    cmd.CommandText = " select x.* from ( " +
                                    " select distinct decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat,' '), " +
                                    " b.cod nume_sint,  " +
                                    " decode(a.grup_vz,' ','-1', a.grup_vz), decode(trim(a.dep_aprobare),'','00', a.dep_aprobare)  dep_aprobare, " +
                                    " (select nvl( " +
                                    " (select 1 from sapprd.marm m where m.mandt = '900' and m.matnr = a.cod and m.meinh = 'EPA'),-1) palet from dual) palet " +
                                    valStoc + ", categ_mat, lungime " +
                                    " from articole a, " +
                                    " sintetice b " + condTabCodBare + " where a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' and a.blocat <> '01' and " + condCautare + condDepart +
                                    " ) x  where  " + condLimit + " order by x.nume ";
                }
                else
                if (!departament.Equals("00") && !departament.Equals("12") && departament.Length > 0)
                {

                    string condFil = filiala;
                    if (searchString.StartsWith("111"))
                        condFil = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);

                    cmd.CommandText = " select x.* from ( " +
                                    " select distinct decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat,' '), " +
                                    " b.cod nume_sint,  " +
                                    " decode(a.grup_vz,' ','-1', a.grup_vz), decode(trim(a.dep_aprobare),'','00', a.dep_aprobare)  dep_aprobare, " +
                                    " (select nvl( " +
                                    " (select 1 from sapprd.marm m where m.mandt = '900' and m.matnr = a.cod and m.meinh = 'EPA'),-1) palet from dual) palet " +
                                    valStoc + ", categ_mat, lungime " +
                                    " from articole a, " +
                                    " sintetice b, sapprd.marc c " + condTabCodBare + " where c.mandt = '900' and c.matnr = a.cod and c.werks = '" + condFil + "' and c.mmsta <> '01' " +
                                    " and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' and a.blocat <> '01' and " + condCautare + condDepart +
                                    " ) x  where  " + condLimit + condExtraDepart + " order by x.nume ";



                }
                else// consilieri
                {
                    string filGed = "NN10";

                    if (filiala != null && filiala.Length > 0)
                        filGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);

                    cmd.CommandText = " select distinct decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart,a.nume, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat,' '), b.cod nume_sint, " +
                                      " decode(a.grup_vz,' ','-1', a.grup_vz), decode(trim(a.dep_aprobare),'','00', a.dep_aprobare), " +
                                      " (select nvl( " +
                                      " (select 1 from sapprd.marm m where m.mandt = '900' and m.matnr = a.cod and m.meinh = 'EPA'),-1) palet from dual) palet " +
                                      valStoc + ", categ_mat, lungime " +
                                      " from articole a, " +
                                      " sintetice b, sapprd.marc c " + condTabCodBare + " where c.mandt = '900' and c.matnr = a.cod and c.werks = '" + filGed + "' and c.mmsta <> '01'" +
                                      " and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' and a.blocat <> '01' and " + condCautare + condDepart + " and " + condLimit + "  order by a.nume ";

                    

                }

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                if (afisStoc != null && afisStoc.Equals("1"))
                {
                    cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = filiala;
                }


              

                oReader = cmd.ExecuteReader();

                string strCat;

               
                ArticolCautare articol;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCautare();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.sintetic = oReader.GetString(2);
                        articol.nivel1 = oReader.GetString(3);
                        articol.umVanz10 = oReader.GetString(4);
                        articol.umVanz = oReader.GetString(8).Substring(0, 2).Equals("11") ? oReader.GetString(5) : oReader.GetString(4);
                        articol.tipAB = oReader.GetString(6);
                        articol.depart = oReader.GetString(8);
                        articol.departAprob = oReader.GetString(9);
                        articol.umPalet = oReader.GetInt32(10).ToString();
                        articol.stoc = oReader.GetDouble(11).ToString();

                        strCat = oReader.GetString(12);

                        if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                            strCat = "AM";
                        else
                            strCat = " ";

                        articol.categorie = strCat;

                        articol.lungime = oReader.GetDouble(13).ToString();

                        


                        listArticole.Add(articol);

                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

           

            return serializedResult;
        }



        public string getListSinteticeDistributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament)
        {

            string serializedResult = "";
            string condCautare = "";




            if (tipCautare.Equals("C"))
            {
                if (tipArticol.Equals("S"))
                    condCautare = " cod in ('" + searchString + "') ";
            }


            if (tipCautare.Equals("N"))
            {
                if (tipArticol.Equals("S"))
                    condCautare = "  upper(nume) like upper('" + searchString.ToUpper() + "%')";
            }

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select cod, nume from sintetice where " + condCautare + " order by nume ";

                cmd.CommandType = CommandType.Text;

                oReader = cmd.ExecuteReader();

                List<ArticolCautare> listArticole = new List<ArticolCautare>();
                ArticolCautare articol;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCautare();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.sintetic = oReader.GetString(0);
                        articol.nivel1 = "";
                        articol.umVanz10 = "";
                        articol.umVanz = "";
                        articol.tipAB = "";
                        articol.depart = "";
                        articol.lungime = "0";
                        articol.stoc = "1";
                        listArticole.Add(articol);

                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

                
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return serializedResult;


        }



        public string getListNivel1Distributie(string searchString, string tipArticol, string tipCautare, string filiala, string departament)
        {

            string serializedResult = "";
            string condCautare = "";

            if (tipCautare.Equals("C"))
            {
                condCautare = " lower(cod_nivel1) in lower('" + searchString + "') ";
            }


            if (tipCautare.Equals("N"))
            {
                condCautare = "  upper(nume_nivel1) like upper('" + searchString.ToUpper() + "%') ";
            }

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select distinct cod_nivel1, nume_nivel1 from sintetice where " + condCautare + " order by nume_nivel1 ";


                cmd.CommandType = CommandType.Text;

                oReader = cmd.ExecuteReader();

                List<ArticolCautare> listArticole = new List<ArticolCautare>();
                ArticolCautare articol;
                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCautare();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.sintetic = oReader.GetString(0);
                        articol.nivel1 = "";
                        articol.umVanz10 = "";
                        articol.umVanz = "";
                        articol.tipAB = "";
                        articol.depart = "";
                        listArticole.Add(articol);

                    }

                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

                oReader.Close();
                oReader.Dispose();

                cmd.Dispose();
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




        public string getPretGed(string parametruPret)
        {
           

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ParametruPretGed paramPret = serializer.Deserialize<ParametruPretGed>(parametruPret);

            string serializedResult = "";
            
            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            string localUnitLog = "";

            PretArticolGed pretArticolGed = new PretArticolGed();

            try
            {

                webService = new SAPWebServices.ZTBL_WEBSERVICE();
                SAPWebServices.ZgetPrice inParam = new SAPWebServices.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());

                webService.Credentials = nc;
                webService.Timeout = 300000;

                localUnitLog = paramPret.ul;

                //preturi ged doar pe ul 20
                if (paramPret.depart == "11")
                    localUnitLog = paramPret.ul.Substring(0, 2) + "2" + paramPret.ul.Substring(3, 1);

                inParam.GvKunnr = paramPret.client;
                inParam.GvMatnr = paramPret.articol;
                inParam.GvSpart = "11";
                inParam.GvVrkme = paramPret.um;
                inParam.GvWerks = localUnitLog;
                inParam.GvLgort = paramPret.depoz;
                inParam.GvCant = Decimal.Parse(paramPret.cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = Service1.isConsVanzSite(paramPret.codUser) ? "X" : " ";
                inParam.TipPers = "CV";
                inParam.Canal = "20";
                inParam.Mp = paramPret.metodaPlata;
                inParam.Dzterm = paramPret.termenPlata;
                inParam.Regio = paramPret.codJudet;
                inParam.City = paramPret.localitate;
                inParam.UlStoc = paramPret.filialaAlternativa;



                SAPWebServices.ZgetPriceResponse outParam = webService.ZgetPrice(inParam);

                string extindere11 = outParam.ErrorCode.ToString();

                if (extindere11.Equals("2"))
                {
                    if (Service1.extindeClient(paramPret.client).Equals("0"))
                    {
                        return getPretGed(parametruPret);
                    }
                    else
                    {
                        return "-1";
                    }
                }


                string pretOut = outParam.GvNetwr.ToString() != "" ? outParam.GvNetwr.ToString() : "-1";
                string umOut = outParam.GvVrkme.ToString() != "" ? outParam.GvVrkme.ToString() : "-1";
                string noDiscOut = outParam.GvNoDisc.ToString();

                string codArtPromo = outParam.GvMatnrFree.ToString() != "" ? outParam.GvMatnrFree.ToString() : "-1";
                string cantArtPromo = outParam.GvCantFree.ToString() != "" ? outParam.GvCantFree.ToString() : "-1";
                string pretArtPromo = outParam.GvNetwrFree.ToString() != "" ? outParam.GvNetwrFree.ToString() : "-1";
                string umArtPromo = outParam.GvVrkmeFree.ToString() != "" ? outParam.GvVrkmeFree.ToString() : "-1";
                string pretLista = outParam.GvNetwrList.ToString() != "" ? outParam.GvNetwrList.ToString() : "-1";
                string cantOut = outParam.GvCant.ToString() != "" ? outParam.GvCant.ToString() : "-1";
                string condPret = outParam.GvCond.ToString() != "" ? outParam.GvCond.ToString() : "-1";
                string multiplu = outParam.Multiplu.ToString() != "" ? outParam.Multiplu.ToString() : "-1";
                string cantUmb = outParam.OutCantUmb.ToString() != "" ? outParam.OutCantUmb.ToString() : "-1";
                string Umb = outParam.OutUmb.ToString() != "" ? outParam.OutUmb.ToString() : "-1";
                string procentTransport = outParam.ProcTrap.ToString();
                string impachetare = outParam.Impachet.ToString() != "" ? outParam.Impachet.ToString() : " ";
                string pretFaraTva = outParam.GvNetwrFtva.ToString();
                

                pretArticolGed.pret = pretOut;
                pretArticolGed.um = umOut;
                pretArticolGed.faraDiscount = noDiscOut;
                pretArticolGed.codArticolPromo = codArtPromo;
                pretArticolGed.cantitateArticolPromo = cantArtPromo;
                pretArticolGed.pretArticolPromo = pretArtPromo;
                pretArticolGed.umArticolPromo = umArtPromo;
                pretArticolGed.pretLista = pretLista;
                pretArticolGed.cantitate = cantOut;
                pretArticolGed.conditiiPret = condPret;
                pretArticolGed.multiplu = multiplu;
                pretArticolGed.cantitateUmBaza = cantUmb;
                pretArticolGed.umBaza = Umb;
                pretArticolGed.procTransport = procentTransport;
                pretArticolGed.impachetare = impachetare;
                pretArticolGed.pretFaraTva = ((outParam.GvNetwrFtva / outParam.GvCant) * outParam.Multiplu).ToString();
                pretArticolGed.valTrap = outParam.ValTrap.ToString();

                //pretArticolGed.valTrap = "150";

                pretArticolGed.errMsg = outParam.VMess;
                


                //---verificare cmp

                OracleConnection connection = new OracleConnection();
                OracleCommand cmd = new OracleCommand();
                OracleDataReader oReader = null;

                string filialaCmp = paramPret.filialaAlternativa;


                if (paramPret.filialaAlternativa.Equals("BV90"))
                    filialaCmp = "BV92";
                else
                    filialaCmp = paramPret.filialaAlternativa.Substring(0, 2) + "2" + paramPret.filialaAlternativa.Substring(3, 1);



                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog  ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = paramPret.articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;   //ul. ged



                oReader = cmd.ExecuteReader();
                double cmpArticol = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    cmpArticol = Convert.ToDouble(oReader.GetString(0));

                    // daca nu este in ged se cauta in distributie
                    if (cmpArticol == 0)
                    {
                        cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                          " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog ";

                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                        cmd.Parameters[0].Value = paramPret.articol;

                        cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                        cmd.Parameters[1].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);      //ul. distributie

                        oReader = cmd.ExecuteReader();

                        oReader.Read();
                        cmpArticol = Convert.ToDouble(oReader.GetString(0));

                    }


                }

                //---sf. verificare cmp



                //coeficient corectie
                cmd.CommandText = " select a.coef_corr from sapprd.zexc_coef_marja a, articole b where b.cod =:articol  and a.matkl = b.sintetic " +
                                  " and a.pdl =:ul and a.functie =:functie ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = paramPret.articol;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);      //ul. distributie

                cmd.Parameters.Add(":functie", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = paramPret.tipUser;




                oReader = cmd.ExecuteReader();
                oReader.Read();

                if (oReader.HasRows)
                    pretArticolGed.coefCorectie = oReader.GetDouble(0).ToString();
                else
                    pretArticolGed.coefCorectie = "0";
                //sf. coef corectie


                pretArticolGed.cmp = cmpArticol.ToString();

                pretArticolGed.procReducereCmp =  Preturi.getProcReducereCmp(connection, paramPret.articol).ToString();


                //descriere conditii pret
                string[] codReduceri = condPret.Split(';');
                string[] tokCod;


                //conditii pret, fara descriere, doar coduri
                condPret = "";

                for (int jj = 0; jj < codReduceri.Length; jj++)
                {

                    tokCod = codReduceri[jj].Split(':');

                    if (tokCod.Length > 1)
                    {
                        condPret += tokCod[0] + ":" + tokCod[1] + ";";
                    }


                }//sf. for               

                pretArticolGed.conditiiPret = condPret;

               



                //discounturi maxime
                string discMaxAV = "0", discMaxSD = "0", discMaxDV = "0", discMaxKA = "0";


                pretArticolGed.discMaxAV = discMaxAV;
                pretArticolGed.discMaxSD = discMaxSD;
                pretArticolGed.discMaxDV = discMaxDV;
                pretArticolGed.discMaxKA = discMaxKA;

                //sf. disc

                //pret si adaos mediu
                string pretMediu = "0";
                string adaosMediu = "0";
                string unitMasPretMediu = "0";


                if (paramPret.depart.Equals("11"))
                {



                    double dMarjaBruta = (Double.Parse(pretArticolGed.pret) / Double.Parse(pretArticolGed.cantitate)) - Double.Parse(pretArticolGed.cmp) * 1.19;
                    double dPretMediu = (Double.Parse(pretArticolGed.pret) / Double.Parse(pretArticolGed.cantitate)) - dMarjaBruta * 0.19;
                    double dMarjaMedie = dPretMediu - Double.Parse(pretArticolGed.cmp) * 1.19;


                    pretMediu = dPretMediu.ToString();
                    adaosMediu = dMarjaMedie.ToString();
                    unitMasPretMediu = pretArticolGed.um;




                }
                else
                {


                    pretMediu = ((Double.Parse(pretOut) / 1.19) / Double.Parse(cantOut)).ToString();


                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select to_char(pret_med/cant, '99990.999')  , to_char(adaos_med/cant,'99990.999')  , um from sapprd.zpret_mediu_oras r where mandt = '900' and pdl =:unitLog " +
                                      " and matnr=:articol ";


                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = paramPret.ul.Substring(0, 2) + "1" + paramPret.ul.Substring(3, 1);

                    cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = paramPret.articol;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        pretMediu = oReader.GetString(0).Trim();
                        adaosMediu = oReader.GetString(1).Trim();
                        unitMasPretMediu = oReader.GetString(2);
                    }

                }


                string istoricPret = " ";
                if (paramPret.codClientParavan != null && paramPret.codClientParavan.Trim().Length > 0 )
                    istoricPret = new Preturi().getIstoricPret(connection,  paramPret.articol, paramPret.codClientParavan);


                DatabaseConnections.CloseConnections(oReader, cmd, connection);


                pretArticolGed.pretMediu = pretMediu;
                pretArticolGed.adaosMediu = adaosMediu;
                pretArticolGed.umPretMediu = unitMasPretMediu;
                pretArticolGed.istoricPret = istoricPret;


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                serializedResult = "-1";
            }
            finally
            {
                webService.Dispose();
            }

            serializedResult = serializer.Serialize(pretArticolGed);

            return serializedResult;

        }



        public string getGreutateArticol(string codArticol, string unitMasArticol, EnumUnitMas unitMasRezultat)
        {

            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select a.brgew, a.gewei, b.umren, b.umrez, a.meins from sapprd.mara a, sapprd.marm b where a.mandt = '900' and b.mandt = '900' " +
                                  " and a.matnr =:codArticol and a.matnr = b.matnr and b.meinh =:unitMas ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":unitMas", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = unitMasArticol;

                oReader = cmd.ExecuteReader();

                GreutateArticol greutateArticol = new GreutateArticol();
                double greutateKG = 0.0;

                if (oReader.HasRows)
                {
                    oReader.Read();
                    greutateArticol = new GreutateArticol();
                    greutateKG = oReader.GetDouble(0) * (oReader.GetDouble(3) / oReader.GetDouble(2));
                    greutateArticol.greutate = greutateKG;
                    greutateArticol.umCantitate = oReader.GetString(4);

                    if (oReader.GetString(1).ToUpper().Equals("G"))
                        greutateKG = greutateKG * GRAMS;

                    if (oReader.GetString(1).ToUpper().Equals("TO"))
                        greutateKG = greutateKG / TONES;

                    greutateArticol.um = oReader.GetString(1);

                }

                if (oReader != null)
                {
                    oReader.Close();
                    oReader.Dispose();
                }

                cmd.Dispose();

                if (unitMasRezultat == EnumUnitMas.G)
                    greutateArticol.greutate = greutateKG / GRAMS;

                if (unitMasRezultat == EnumUnitMas.TO)
                    greutateArticol.greutate = greutateKG * TONES;

                greutateArticol.codArticol = codArticol;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(greutateArticol);

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



        public static bool isArtPermBV90(string codArticol, string filiala)
        {
            bool isPermited = false;




            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.matkl from sapprd.zsint_bv a, articole b where b.cod =:codArticol and a.ul_stoc=:ulStoc and b.sintetic = a.matkl ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                if (codArticol.Length == 8)
                    codArticol = "0000000000" + codArticol;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":ulStoc", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filiala;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    isPermited = true;
                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                isPermited = false;
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }



            return isPermited;

        }

        public static string getDivizieArticolBV90(string codArticol)
        {
            string divizie = "00";


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.aprobare from sapprd.zsint_bv a, articole b where b.cod =:codArticol and b.sintetic = a.matkl ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                if (codArticol.Length == 8)
                    codArticol = "0000000000" + codArticol;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    divizie = oReader.GetString(0).Trim();
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


            return divizie;
        }



        public string getStocArticole(string listArticole)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<ArticolStoc> listArticoleStoc = serializer.Deserialize<List<ArticolStoc>>(listArticole);

            List<ArticolStoc> resultList = new List<ArticolStoc>();


            foreach (ArticolStoc articol in listArticoleStoc)
            {
                string[] str = new Service1().getStocDepozit(articol.cod, articol.unitLog, articol.depozit, articol.depart).Split('#');
                ArticolStoc resultArt = new ArticolStoc();
                resultArt.cod = getCleanCodArt(articol.cod);
                resultArt.depozit = articol.depozit;
                resultArt.stoc = Double.Parse(str[0]);
                resultList.Add(resultArt);
            }

            string retVal = serializer.Serialize(resultList);


            return retVal;

        }

        public string getCodBare(string codArticol)
        {
            string coduriBare = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod_bare, um from artiscan where cod =:codArticol order by cod_bare ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                if (codArticol.Length == 8)
                    codArticol = "0000000000" + codArticol;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (coduriBare.Equals(""))
                            coduriBare = oReader.GetString(0).Trim() + " /" + oReader.GetString(1);
                        else
                            coduriBare += "," + oReader.GetString(0).Trim() + " /" + oReader.GetString(1);
                    }

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

            return coduriBare;


        }


        public string getStocCustodie(string codArticol, string codClient, string filiala)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            double stocArt = 0;
            string umArt = "BUC";
            string stocResponse = "";

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select sum(kulab), 'BUC' um_art from " + 
                                 " (select k.matnr, k.kulab from sapprd.MSKU k " + 
                                 " where k.mandt = '900' and k.kunnr = :codClient and sobkz = 'W' and kulab > 0 and matnr = :codArticol " + 
                                 " union all " +
                                 " select d.matnr, -1 * (d.lfimg)from sapprd.lips d, sapprd.vbup p, sapprd.likp l " +
                                 " where d.mandt = '900' and d.sobkz = 'W' and d.matnr = :codArticol and (d.werks =:filiala or d.werks =:filiala2) " + 
                                 " and d.mandt = p.mandt and d.vbeln = p.vbeln and d.posnr = p.posnr and d.mandt = l.mandt and d.vbeln = l.vbeln " +
                                 " and l.kunnr =:codClient and p.WBSTA <> 'C')group by matnr ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codArticol;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codArticol;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = getFilialaCustodie(filiala);

                cmd.Parameters.Add(":filiala2", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = getFilialaCustodie2(filiala);

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = codClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stocArt = oReader.GetDouble(0);
                    umArt = oReader.GetString(1);
                }

            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                stocResponse = "-1";
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            stocResponse = stocArt.ToString()  + "#" + umArt + "#1#";

            return stocResponse;

        }

        private static string getFilialaCustodie(string filiala)
        {
            return filiala.Substring(0, 2) + "11" ;
        }

        private static string getFilialaCustodie2(string filiala)
        {
            return filiala.Substring(0, 2) + "12";
        }


        private string getCleanCodArt(string codArt)
        {
            if (codArt.Length > 8)
                return codArt.Substring(10, 8);
            else
                return codArt;
        }




        public static bool isArticolExceptie02(OracleConnection connection, string codArticol)
        {

            bool isArticolExceptie = false;
       
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {


                cmd = connection.CreateCommand();

                cmd.CommandText = "select 1 from articole a, sapprd.zcant_02 b where a.cod = :codArticol and a.sintetic = b.matkl";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                        isArticolExceptie = true;
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


            return isArticolExceptie;
        }



    }
}