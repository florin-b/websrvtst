using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;
using LiteSFATestWebService.SAPWebServices;
using System.Globalization;
using LiteSFATestWebService.General;

namespace LiteSFATestWebService
{


    public class OperatiiRetur
    {


        public string getListDocumenteSalvate(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {


            if ((tipUser.Equals("SD") && (stare.Equals("1") || stare.Equals("3")) || tipUser.Contains("AV")))
            {
                return getListDocumenteSalvateToDb(codAgent, filiala, tipUser, depart, interval, stare);
            }
            else
            {
                return getListDocumenteSAP(codAgent, filiala, tipUser, depart, interval, stare);
            }



        }



        private string getListDocumenteSalvateToDb(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {


            string serializedListComenzi = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            string query = "";

            string condData = "";

            if (interval == "0") //astazi
            {
                string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                string dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and a.datacreare >= '" + dateInterval + "' ";
            }


            string stareComanda = "";

            if (tipUser.Equals("SD"))
                stareComanda = " and a.statusAprob = " + stare;

            try
            {

                if (tipUser.Equals("SD"))
                {
                    query = " select a.id, a.nrdocument, a.numeclient, to_char(to_date(a.datacreare,'yyyymmdd')),  a.statusaprob , b.nume from sapprd.zreturhead a, agenti b where " +
                            " a.codagent = b.cod and b.filiala =:filiala and b.divizie like '" + depart + "%' " + condData + stareComanda + " order by a.id ";
                }
                else if (tipUser.Contains("AV"))
                {
                    query = " select a.id, a.nrdocument, a.numeclient, to_char(to_date(a.datacreare,'yyyymmdd')),  a.statusaprob, ' '  from sapprd.zreturhead a, agenti b where " +
                            " a.codagent = b.cod and a.codagent =:codagent and b.filiala =:filiala " + condData + " order by a.id ";
                }


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = query;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codagent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codAgent;
                }



                oReader = cmd.ExecuteReader();

                ComandaReturAfis retur = null;
                List<ComandaReturAfis> listComenzi = new List<ComandaReturAfis>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retur = new ComandaReturAfis();
                        retur.id = oReader.GetInt64(0).ToString();
                        retur.nrDocument = oReader.GetString(1);
                        retur.numeClient = oReader.GetString(2);
                        retur.dataCreare = oReader.GetString(3);
                        retur.status = oReader.GetString(4);

                        if (tipUser.Contains("AV") && retur.status.Equals("2"))
                            retur.status = getStareAlocareBorderou(retur.nrDocument);


                        retur.numeAgent = oReader.GetString(5);
                        listComenzi.Add(retur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedListComenzi = serializer.Serialize(listComenzi);

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


            return serializedListComenzi;
        }




        private string getStareAlocareBorderou(string idComanda)
        {



            string stareBorderou = "-1";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct decode(a1.vbeln,'','30','31') borderou from sapprd.vbak k, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1, sapprd.adrc c, " +
                                  " sapprd.vbpa m2, sapprd.adrc c1 where k.mandt = '900' and k.audat >= to_char(sysdate-45,'yyyymmyy') and k.auart in ('ZRI', 'ZRS') and k.mandt = a.mandt(+) " +
                                  " and k.vbeln = a.vbelv(+) and a.vbtyp_v(+) = 'H' and a.vbtyp_n(+) = 'T' and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) " +
                                  " and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' and a.mandt = l.mandt and a.vbeln = l.vbeln and k.mandt = m.mandt and k.vbeln = m.vbeln " +
                                  " and m.parvw in ('VE', 'ZC') and k.mandt = m1.mandt and k.vbeln = m1.vbeln and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber " +
                                  " and k.mandt = m2.mandt and k.vbeln = m2.vbeln and m2.parvw = 'AP' and m2.mandt = c1.client  and m2.adrnr = c1.addrnumber and k.vgbel =:idComanda ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    stareBorderou = oReader.GetString(0);
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

            return stareBorderou;



        }


        public string opereazaComanda(string idComanda, string tipOperatie)
        {
            string retVal = "";

           

            //aprobare
            if (tipOperatie.Equals("2"))
            {
                string dateComanda = getArticoleDocumentSalvat(idComanda);
                string stareCmdSap = saveComandaReturToWS(dateComanda);

                if (stareCmdSap.Equals("0"))
                    retVal = schimbaStareComanda(idComanda, tipOperatie);
                else
                    retVal = stareCmdSap;

            }

            //respingere
            if (tipOperatie.Equals("3"))
            {
                retVal = schimbaStareComanda(idComanda, tipOperatie);
            }


            return retVal;
        }


        private string schimbaStareComanda(string idComanda, string tipOperatie)
        {

            string retVal = "";

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToTestEnvironment();


            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " update sapprd.zreturhead set statusaprob =:stare where id =:idcmd ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idcmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int64.Parse(idComanda);

                cmd.Parameters.Add(":stare", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = tipOperatie;

                cmd.ExecuteNonQuery();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            retVal = "0";

            return retVal;
        }

        public string getListDocumenteSAP(string codAgent, string filiala, string tipUser, string depart, string interval, string stare)
        {


            string serializedListComenzi = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            string query = "";

            string condData = "";
            string dateInterval = "";

            if (interval == "0") //astazi
            {
                dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat = '" + dateInterval + "' ";
            }

            if (interval == "1") //ultimele 7 zile
            {
                dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat >= '" + dateInterval + "' ";
            }

            if (interval == "2") //ultimele 30 zile
            {
                dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                condData = " and k.audat >= '" + dateInterval + "' ";
            }


            try
            {


                string condAgent = "";
                if (!tipUser.Equals("SD"))
                    condAgent = " and ag.cod =:codAgent ";

                query = " select distinct ag.nume,  k.vgbel, c.name1 nume_client,  l.traty,  l.wadat data_livrare,   decode(a1.vbeln,'','30','31') ,  m.pernr " +
                        " from sapprd.vbak k, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1,  sapprd.adrc c,  sapprd.vbpa m2, sapprd.adrc c1, agenti ag " +
                        " where k.mandt = '900'  " + condData + " and k.auart in ('ZRI', 'ZRS')  and k.mandt = a.mandt(+)  and k.vbeln = a.vbelv(+)  and a.vbtyp_v(+) = 'H' " +
                        " and substr(ag.divizie,0,2)=:divizie and a.vbtyp_n(+) = 'T' and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' " + condAgent +
                        " and a.mandt = l.mandt and a.vbeln = l.vbeln and k.mandt = m.mandt and k.vbeln = m.vbeln and m.parvw in ('VE', 'ZC') and m.pernr = ag.cod and ag.filiala =:filiala " +
                        " and k.mandt = m1.mandt and k.vbeln = m1.vbeln and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber and k.mandt = m2.mandt and k.vbeln = m2.vbeln " +
                        " and m2.parvw = 'AP' and m2.mandt = c1.client and m2.adrnr = c1.addrnumber ";

                connection.ConnectionString = connectionString;
                connection.Open();
                cmd = connection.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filiala;

                cmd.Parameters.Add(":divizie", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = depart;

                if (!tipUser.Equals("SD"))
                {
                    cmd.Parameters.Add(":codagent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = codAgent;
                }

                oReader = cmd.ExecuteReader();

                ComandaReturAfis retur = null;
                List<ComandaReturAfis> listComenzi = new List<ComandaReturAfis>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        retur = new ComandaReturAfis();
                        retur.id = oReader.GetString(1);
                        retur.nrDocument = oReader.GetString(1);
                        retur.numeClient = oReader.GetString(2);
                        retur.dataCreare = " ";
                        retur.status = oReader.GetString(5);
                        retur.numeAgent = oReader.GetString(0);
                        listComenzi.Add(retur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedListComenzi = serializer.Serialize(listComenzi);

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


            return serializedListComenzi;


        }

        public string getArticoleDocumentSAP(string idComanda)
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

                cmd.CommandText = " select decode(length(p.matnr),18,substr(p.matnr,-8),p.matnr) , p.arktx, p.klmeng, p.vrkme, a1.vbeln,  l.traty, a1.vbeln borderou, c.name1, k.vgbel, " +
                                  " to_char(to_date(l.wadat,'yyyymmdd')) data_livrare,  c1.name1 pers_cont,  c1.tel_number tel_pc,  c.region, c.city1, c.street " +
                                  " from sapprd.vbak k, sapprd.vbap p, sapprd.vbfa a, sapprd.vbfa a1, sapprd.likp l, sapprd.vbpa m, sapprd.vbpa m1, sapprd.adrc c, sapprd.vbpa m2, " +
                                  " sapprd.adrc c1 where k.mandt = '900' and k.auart in ('ZRI', 'ZRS') and p.mandt = a.mandt(+) and p.vbeln = a.vbelv(+) " +
                                  " and p.posnr = a.posnv(+) and k.audat >= to_char(sysdate-45,'yyyymmyy') and a.vbtyp_v(+) = 'H' and a.vbtyp_n(+) = 'T' and k.mandt = p.mandt and k.vbeln = p.vbeln " +
                                  " and a.mandt = a1.mandt(+) and a.vbeln = a1.vbelv(+) and a1.vbtyp_v(+) = 'T' and a1.vbtyp_n(+) = '8' and a.mandt = l.mandt and a.vbeln = l.vbeln " +
                                  " and k.mandt = m.mandt and k.vbeln = m.vbeln and m.parvw in ('VE', 'ZC')  and k.mandt = m1.mandt and k.vbeln = m1.vbeln " +
                                  " and m1.parvw = 'WE' and m1.mandt = c.client and m1.adrnr = c.addrnumber and k.mandt = m2.mandt and k.vbeln = m2.vbeln and m2.parvw = 'AP' " +
                                  " and m2.mandt = c1.client and m2.adrnr = c1.addrnumber and k.vgbel =:idComanda ";


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ComandaRetur retur = null;

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                retur = new ComandaRetur();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        retur.dataLivrare = oReader.GetString(9);
                        retur.tipTransport = oReader.GetString(5);
                        retur.numePersContact = oReader.GetString(10);
                        retur.telPersContact = oReader.GetString(11);
                        retur.adresaCodJudet = oReader.GetString(12);
                        retur.adresaOras = oReader.GetString(13);
                        retur.adresaStrada = oReader.GetString(14);
                        retur.nrDocument = oReader.GetString(8);
                        retur.codAgent = "";
                        retur.tipAgent = "";
                        retur.motivRetur = "";

                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.cantitateRetur = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);
                        listArticole.Add(artRetur);

                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                String listaArticoleSer = serializer.Serialize(listArticole);

                retur.listaArticole = listaArticoleSer;
                serializedResult = serializer.Serialize(retur);

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



        public string getArticoleDocumentSalvat(string idComanda)
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

                cmd.CommandText = " select datalivrare, tiptransport, numeperscontact, telperscontact, codjudet, localitate, strada,  " +
                                  " nrdocument, codagent, tipagent, motivretur " +
                                  " from sapprd.zreturhead where id =:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ComandaRetur retur = null;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    {
                        retur = new ComandaRetur();
                        retur.dataLivrare = oReader.GetString(0);
                        retur.tipTransport = oReader.GetString(1);
                        retur.numePersContact = oReader.GetString(2);
                        retur.telPersContact = oReader.GetString(3);
                        retur.adresaCodJudet = oReader.GetString(4);
                        retur.adresaOras = oReader.GetString(5);
                        retur.adresaStrada = oReader.GetString(6);
                        retur.nrDocument = oReader.GetString(7);
                        retur.codAgent = oReader.GetString(8);
                        retur.tipAgent = oReader.GetString(9);
                        retur.motivRetur = oReader.GetString(10);

                    }

                }

                cmd.CommandText = " select decode(length(a.codarticol),18,substr(a.codarticol,-8),a.codarticol), b.nume, a.cantitate, a.um from sapprd.zreturdet a, articole b where a.id =:idComanda and a.codarticol = b.cod order by b.nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.cantitateRetur = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);
                        listArticole.Add(artRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                String listaArticoleSer = serializer.Serialize(listArticole);

                retur.listaArticole = listaArticoleSer;
                serializedResult = serializer.Serialize(retur);

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




        public string getDocumenteRetur(string codClient, string codDepartament, string unitLog, string tipDocument, string interval)
        {
            string serializedResult = "";


            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condPaleti = "";
            string condData = "";

            if (tipDocument == null || (tipDocument != null && tipDocument.Equals("PAL")))
                condPaleti = "and p.matkl in ('433', '433_1', '716', '626', '929_2', '515')";

            if (interval != null)
                condData = " and substr(k.fkdat,0,6) = '" + interval + "' ";

            try
            {

                if (codDepartament.Equals("11"))
                {
                    cmd.CommandText = " select k.vbeln, to_date(k.fkdat,'yyyymmdd') " +
                                      " from sapprd.vbrk k, sapprd.vbrp p, sapprd.vbpa a, sapprd.adrc c where k.mandt = p.mandt " +
                                      " and k.vbeln = p.vbeln  and k.mandt = '900'  and k.fkart in ('ZFM','ZFMC','ZFS','ZFSC','ZFPA') " +
                                      " and k.fksto <> 'X'  and k.fkdat >= to_char(sysdate-45,'yyyymmdd') " +
                                      condPaleti +
                                      " and k.mandt = a.mandt  and k.vbeln = a.vbeln  and a.parvw = 'WE' " +
                                      " and p.prctr =:unitLog  and a.mandt = c.client and a.adrnr = c.addrnumber " +
                                      condData +
                                      " and lower(c.name1) like lower('%" + codClient + "%')  order by fkdat ";




                }
                else
                {
                    cmd.CommandText = " select distinct k.vbeln, to_date(k.fkdat,'yyyymmdd') " +
                                      " from sapprd.vbrk k, sapprd.vbrp p where k.mandt = p.mandt and k.vbeln = p.vbeln and p.spart =:depart " +
                                      " and k.mandt = '900' and k.fkdat >= to_char(sysdate-145,'yyyymmdd') and k.fkart = 'ZFI' and k.fksto <> 'X' " +
                                      " and k.kunag =:codClient " + condPaleti + condData + " order by to_date(k.fkdat,'yyyymmdd') ";
                }

                

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                if (codDepartament.Equals("11"))
                {
                    cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = unitLog;
                }
                else
                {
                    cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = codClient;

                    cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = codDepartament;
                }

                oReader = cmd.ExecuteReader();

                DocumentRetur docRetur = null;
                List<DocumentRetur> listDocumente = new List<DocumentRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        docRetur = new DocumentRetur();
                        docRetur.numar = oReader.GetString(0);
                        docRetur.data = oReader.GetDateTime(1).ToString("dd-MMM-yyyy");
                        listDocumente.Add(docRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                DateReturClient dateRetur = new DateReturClient();
                dateRetur.listaDocumente = serializer.Serialize(listDocumente);
                dateRetur.adreseLivrare = getAdreseLivrareClient(codClient);
                dateRetur.persoaneContact = getPersoaneContact(codClient);

                serializedResult = serializer.Serialize(dateRetur);

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


        public string getArticoleRetur(string nrDocument, string tipDocument)
        {



            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            string condPaleti = "";

            if (tipDocument == null || (tipDocument != null && tipDocument.Equals("PAL")))
                condPaleti = "and p.matkl in ('433', '433_1', '716', '626', '929_2', '515')";

            try
            {

                cmd.CommandText = " select decode(length(matnr),18,substr(matnr,-8),matnr) codart, arktx,  sum(fkimg - returnate) cant, vrkme from " +
                                  " (select p.matnr, p.fkimg, p.arktx,p.vrkme, p.posnr, p.vbeln, ( select " +
                                  " nvl(sum(cp.KWMENG),0) from sapprd.vbap cp, sapprd.vbfa a, sapprd.vbak vk " +
                                  " where a.mandt = '900' and a.vbelv = p.vbeln and a.posnv = p.posnr and a.vbtyp_v = 'M' " +
                                  " and a.vbtyp_n = 'H' and a.mandt = vk.mandt and a.vbeln = vk.vbeln and vk.auart = 'ZRI' " +
                                  " and a.mandt = cp.mandt and a.vbeln = cp.vbeln and a.posnn = cp.posnr and cp.abgru = ' ') returnate " +
                                  " from sapprd.vbrp p where p.mandt = '900' and p.vbeln =:nrDoc " + condPaleti + "  ) where fkimg - returnate > 0 " +
                                  " group by matnr, arktx, vrkme order by codart ";




                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDoc", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                oReader = cmd.ExecuteReader();

                ArticolRetur artRetur = null;
                List<ArticolRetur> listArticole = new List<ArticolRetur>();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        artRetur = new ArticolRetur();
                        artRetur.cod = oReader.GetString(0);
                        artRetur.nume = oReader.GetString(1);
                        artRetur.cantitate = oReader.GetDouble(2).ToString();
                        artRetur.um = oReader.GetString(3);
                        listArticole.Add(artRetur);
                    }
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

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


        public string saveComandaReturToDB(String dateRetur)
        {

            OracleConnection connection = new OracleConnection();
            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            var serializer = new JavaScriptSerializer();

            OracleTransaction transaction = null;


            long idCmd = Convert.ToInt64(GeneralUtils.getCurrentMillis().ToString().Substring(0, 11));
            try
            {
                ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zreturhead(mandt, id, nrdocument, statuscmd, statusaprob, datacreare, datalivrare, tiptransport, codagent, tipagent, motivretur, " +
                               " numeperscontact, telperscontact, codadresa, codjudet, localitate, strada, codclient, numeclient, acelasi_transp) " +
                               " values ('900', :id, :nrdocument, :statuscmd, :statusaprob, :datacreare, :datalivrare, :tiptransport, :codagent, :tipagent, :motivretur, " +
                               " :numeperscontact, :telperscontact, :codadresa, :codjudet, :localitate, :strada, :codclient, :numeclient, :acelasi_transp) returning id into :id ";

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":id", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idCmd;

                cmd.Parameters.Add(":nrdocument", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = comanda.nrDocument;

                cmd.Parameters.Add(":statuscmd", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = "0";

                cmd.Parameters.Add(":statusaprob", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = "1";

                cmd.Parameters.Add(":datacreare", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = Utils.getCurrentDate();

                cmd.Parameters.Add(":datalivrare", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = comanda.dataLivrare;

                cmd.Parameters.Add(":tiptransport", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = comanda.tipTransport;

                cmd.Parameters.Add(":codagent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = comanda.codAgent;

                cmd.Parameters.Add(":tipagent", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = comanda.tipAgent;

                cmd.Parameters.Add(":motivretur", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = comanda.motivRetur;

                cmd.Parameters.Add(":numeperscontact", OracleType.NVarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = comanda.numePersContact == null ? " " : comanda.numePersContact + " ";

                cmd.Parameters.Add(":telperscontact", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = comanda.telPersContact == null ? " " : comanda.telPersContact + " " ;

                cmd.Parameters.Add(":codadresa", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = comanda.adresaCodAdresa;

                cmd.Parameters.Add(":codjudet", OracleType.NVarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = comanda.adresaCodJudet;

                cmd.Parameters.Add(":localitate", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = comanda.adresaOras;

                cmd.Parameters.Add(":strada", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[15].Value = comanda.adresaStrada + " " ;

                cmd.Parameters.Add(":codclient", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = comanda.codClient;

                cmd.Parameters.Add(":numeclient", OracleType.NVarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[17].Value = comanda.numeClient;

                string obsTransp = " ";
                if (comanda.transpBack != null && Boolean.Parse(comanda.transpBack))
                    obsTransp = "X";

                cmd.Parameters.Add(":acelasi_transp", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = obsTransp;

               // OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
               // idCmd.Direction = ParameterDirection.Output;
               // cmd.Parameters.Add(idCmd);




                cmd.ExecuteNonQuery();

                List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);

                string fullCodeArticol = "";
                for (int i = 0; i < listaArticole.Count; i++)
                {

                    query = " insert into sapprd.zreturdet(mandt, id, codarticol, cantitate, um) values " +
                            " ('900', :idcmd, :codarticol, :cantitate, :um)";

                    fullCodeArticol = listaArticole[i].cod;
                    if (fullCodeArticol.Length == 8)
                        fullCodeArticol = "0000000000" + fullCodeArticol;


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":idcmd", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = idCmd;

                    cmd.Parameters.Add(":codarticol", OracleType.NVarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = fullCodeArticol;

                    cmd.Parameters.Add(":cantitate", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = Double.Parse(listaArticole[i].cantitateRetur, CultureInfo.InvariantCulture);

                    cmd.Parameters.Add(":um", OracleType.NVarChar, 9).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = listaArticole[i].um;

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

            }
            catch (Exception ex)
            {

                if (transaction != null)
                    transaction.Rollback();

                ErrorHandling.sendErrorToMail(ex.ToString());

                return "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();

                if (transaction != null)
                    transaction.Dispose();

            }

            return "0";

        }


        private string saveComandaReturToWS(String dateRetur)
        {
            string response = "";
            var serializer = new JavaScriptSerializer();

            try
            {
                ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);
                List<ArticolRetur> listaArticole = serializer.Deserialize<List<ArticolRetur>>(comanda.listaArticole);

                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                SAPWebServices.ZretMarfa inParam = new ZretMarfa();
                inParam.VVbeln = comanda.nrDocument;
                inParam.VData = comanda.dataLivrare;
                inParam.VTraty = comanda.tipTransport;
                inParam.VPernr = comanda.codAgent;
                inParam.TipPers = comanda.tipAgent;
                inParam.VAugru = comanda.motivRetur;
                inParam.VPers = comanda.numePersContact;
                inParam.VAddrnumber = comanda.adresaCodAdresa;
                inParam.VTelef = comanda.telPersContact;
                inParam.VTranspzone = comanda.adresaCodJudet;
                inParam.VCity = comanda.adresaOras.Length < 25 ? comanda.adresaOras : comanda.adresaOras.Substring(0, 24);
                inParam.VStreet = comanda.adresaStrada;



                SAPWebServices.ZmaterialeRetur[] articoleRetur = new ZmaterialeRetur[listaArticole.Count];

                for (int i = 0; i < listaArticole.Count; i++)
                {
                    articoleRetur[i] = new ZmaterialeRetur();
                    articoleRetur[i].Matnr = listaArticole[i].cod.Length == 8 ? "0000000000" + listaArticole[i].cod : listaArticole[i].cod;
                    articoleRetur[i].Cant = Decimal.Parse(listaArticole[i].cantitateRetur, CultureInfo.InvariantCulture);
                    articoleRetur[i].Um = listaArticole[i].um;
                }

                inParam.ItMateriale = articoleRetur;

                SAPWebServices.ZretMarfaResponse responseRetur = new ZretMarfaResponse();
                responseRetur = webService.ZretMarfa(inParam);
                response = responseRetur.VOk.Equals("0") ? "0" : responseRetur.VMessage;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                response = ex.ToString();
            }

            return response;
        }



        public string saveComandaRetur(string dateRetur, string tipRetur)
        {

            string retVal = "";

            if (tipRetur == null || (tipRetur != null && tipRetur.Equals("PAL")))
                retVal = saveReturPaleti(dateRetur);
            else if (tipRetur.Equals("CMD"))
                retVal = saveReturComanda(dateRetur);

            return retVal;

        }

        private string saveReturComanda(string dateRetur)
        {

            string retVal = "";

            var serializer = new JavaScriptSerializer();
            ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

            if (comanda.tipAgent.Contains("AV"))
                retVal = saveComandaReturToDB(dateRetur);
            else
                retVal = saveComandaReturToWS(dateRetur);

            return retVal;
        }

        private string saveReturPaleti(string dateRetur)
        {

            string retVal = "";

            var serializer = new JavaScriptSerializer();
            ComandaRetur comanda = serializer.Deserialize<ComandaRetur>(dateRetur);

            if (comanda.tipAgent.Contains("AV"))
                retVal = saveComandaReturToDB(dateRetur);
            else
                retVal = saveComandaReturToWS(dateRetur);

            return retVal;
        }


        private string getAdreseLivrareClient(string codClient)
        {
            string serializedResult = " ";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select nvl(a.city1,' ') city1 , nvl(a.street,' ') street, " +
                                  " nvl(a.house_num1,' ') house_num, nvl(region,' '), a.addrnumber from sapprd.adrc a " +
                                  " where a.client = '900' and a.addrnumber =  " +
                                  " (select k.adrnr from sapprd.kna1 k where k.mandt = '900' and k.kunnr =:codClient) " +
                                  " union " +
                                  " select nvl(z.localitate,' '), nvl(z.adr_livrare,' ') , ' ', nvl(z.regio,' '), z.nr_crt from sapprd.zclient_adrese z " +
                                  " where kunnr =:codClient ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();


                List<AdresaLivrareClient> listaAdreseLivrare = new List<AdresaLivrareClient>();
                AdresaLivrareClient oAdresa = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oAdresa = new AdresaLivrareClient();
                        oAdresa.oras = oReader.GetString(0);
                        oAdresa.strada = oReader.GetString(1);
                        oAdresa.nrStrada = oReader.GetString(2);
                        oAdresa.codJudet = oReader.GetString(3);
                        oAdresa.codAdresa = oReader.GetString(4);
                        listaAdreseLivrare.Add(oAdresa);

                    }

                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaAdreseLivrare);

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



        private string getPersoaneContact(string codClient)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd.CommandText = " select namev, name1, telf1 from sapprd.knvk u where u.mandt = '900' and " +
                                  " (u.parnr, u.kunnr) in (select distinct p.parnr, p.kunnr from sapprd.knvp p where p.mandt = '900' " +
                                  " and p.kunnr =:codClient and parvw = 'AP')";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codClient;

                oReader = cmd.ExecuteReader();

                List<PersoanaContact> listContacte = new List<PersoanaContact>();
                PersoanaContact persoana = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        persoana = new PersoanaContact();
                        persoana.nume = oReader.GetString(0) + " " + oReader.GetString(1);
                        persoana.telefon = oReader.GetString(2);
                        listContacte.Add(persoana);
                    }
                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listContacte);

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



        public string getListClientiCV(string numeClient, string unitLog)
        {
            string serializedResult = "";





            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                OracleCommand cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();


                //de modificat intervalul!!!
                cmd.CommandText = " select  c.name1 from sapprd.vbrk k, sapprd.vbrp p, sapprd.vbpa a, sapprd.adrc c " +
                                  " where k.mandt = p.mandt and k.vbeln = p.vbeln and k.mandt = '900' and k.fkart in ('ZFM','ZFMC','ZFS','ZFSC','ZFPA') " +
                                  " and k.fksto <> 'X' and k.fkdat >= to_char(sysdate-45,'yyyymmdd') and p.matkl in ('433', '433_1', '716', '626', '929_2','515') " +
                                  " and k.mandt = a.mandt and k.vbeln = a.vbeln and a.parvw = 'WE' " +
                                  " and p.prctr =:unitLog and a.mandt = c.client and a.adrnr = c.addrnumber  and lower(c.name1) like lower('%" + numeClient.ToLower() + "%') order by fkdat";


                


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = unitLog;

                oReader = cmd.ExecuteReader();

                List<Client> listaClienti = new List<Client>();
                Client unClient = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unClient = new Client();
                        unClient.numeClient = oReader.GetString(0);
                        unClient.codClient = oReader.GetString(0);
                        unClient.tipClient = "0";
                        listaClienti.Add(unClient);
                    }
                }

                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaClienti);

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




    }
}

