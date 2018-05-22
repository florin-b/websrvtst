using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Globalization;
using LiteSFATestWebService.SAPWebServices;

namespace LiteSFATestWebService
{
    public class OperatiiComenzi
    {

        public string getArticoleComanda(string nrComanda, string afisConditii, string tipUser)
        {




            string unitLog1 = "";
            string cmp = "";
            string serializedResult = "";

            //
            //afisCond:
            // 1 - pentru agent
            // 2 - pentru aprobare comanda SD
            // 3 - pentru aprobare comanda DV


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleCommand cmdInner = null;
            OracleDataReader oReader = null, oReader1 = null;
            OracleDataReader oReaderInner = null;
            int nrArt = 0;
            string infoPret = " , '0' info";

            String pretMediu = "0", adaosMediu = "0", unitMasPretMediu = "0";

            try
            {
                //antet comanda
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.mt, a.tip_plata, a.cantar, a.fact_red, a.city, a.region, a.ul, " +
                                  " decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, decode(a.obstra,'',' ',a.obstra) obstra, b.tip_pers from sapprd.zcomhead_tableta a, clienti b " +
                                  " where a.id=:idcmd and a.cod_client = b.cod ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader = cmd.ExecuteReader();

                ComandaAprobata comanda = new ComandaAprobata();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    unitLog1 = oReader.GetString(9);

                    comanda.persoanaContact = oReader.GetString(0);
                    comanda.telefon = oReader.GetString(1);
                    comanda.adresaLivrare = oReader.GetString(2);
                    comanda.tipTransport = oReader.GetString(3);
                    comanda.metodaPlata = oReader.GetString(4);
                    comanda.cantarire = oReader.GetString(5);
                    comanda.factRedSeparat = oReader.GetString(6);
                    comanda.oras = oReader.GetString(7);
                    comanda.codJudet = oReader.GetString(8);
                    comanda.unitLog = oReader.GetString(9);
                    comanda.termenPlata = oReader.GetString(10);
                    comanda.obsLivrare = oReader.GetString(11);
                    comanda.tipPersClient = oReader.GetString(12);

                }


                oReader.Close();
                oReader.Dispose();

                //articole comenzi

                if (afisConditii == "3" || afisConditii == "1") //3 = pentru DV se afiseaza si CMP, 1 = modif.cmd pt. a verifica art. sub cmp
                {
                    cmp = " , nvl(( select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),0) from sapprd.mbew y where " +
                                 " y.mandt='900' and y.matnr=a.cod  and y.bwkey = '" + unitLog1 + "'" +
                                 " ),0) cmp  ";
                }
                else
                {
                    cmp = ", '-1' cmp ";
                }

                string condTabKA = " , sapprd.zcomhead_tableta z ", condIdKA = " and a.id=:idcmd and z.id = a.id ", condOrderKA = " poz ";
                if (tipUser == "KA")
                {
                    // condTabKA = " , sapprd.zcomhead_tableta z ";
                    // condIdKA = " and z.parent_id=:idcmd and z.id = a.id ";
                    // condOrderKA = " depoz, poz ";
                }

                infoPret = ", nvl(a.inf_pret,'0') infopret";

                cmd.CommandText = " select a.status, decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod ,nvl(b.nume,' '), a.cantitate, decode(trim(a.depoz),'','0000',a.depoz) depoz, " +
                                  " a.valoare, a.um, a.procent, nvl(a.procent_fc,0) procent_fc, decode(trim(a.conditie),'','0',a.conditie) conditie " + cmp +
                                  " , nvl(a.disclient,0) disclient, nvl(a1.discount,0) av, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='SD' and spart =substr(c.cod_nivel1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='DV' and spart =substr(c.COD_NIVEL1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) dv, nvl(a.procent_aprob,0) procent_aprob, " +
                                  " decode(s.matkl,'','0','1') permitsubcmp, nvl(a.multiplu,1) multiplu, nvl(a.val_poz,0) " + infoPret +
                                  " ,nvl(a.cant_umb,0) cant_umb , nvl(a.umb,' ') umb, a.ul_stoc, z.depart, nvl(b.tip_mat,' ')  from sapprd.zcomdet_tableta a, sapprd.zdisc_pers_sint a1,  sintetice c," +
                                  " articole b, sapprd.zpretsubcmp s " + condTabKA + " where a.cod = b.cod(+) " + condIdKA + " and " +
                                  " a1.inactiv(+) <> 'X' and a1.functie(+)='AV' and a1.spart(+)=substr(c.COD_NIVEL1,2,2) and a1.werks(+) ='" + unitLog1 + "' " +
                                  " and b.sintetic = c.cod(+) and a1.matkl(+) = c.cod " +
                                  " and s.mandt(+)='900' and s.matkl(+) = c.cod order by " + condOrderKA;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader1 = cmd.ExecuteReader();
                String unitLogAlt = "NN10", depart = "00", tipMat = "", lnumeArt = "", lDepoz = "";

                ArticolComandaRap articol = null;
                List<ArticolComandaRap> listArticole = new List<ArticolComandaRap>();

                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {
                        depart = oReader1.GetString(23).Trim() != "" ? oReader1.GetString(23) : "00";
                        tipMat = oReader1.GetString(24);
                        lnumeArt = oReader1.GetString(2).Replace("#", "");
                        lDepoz = oReader1.GetString(4);

                        if (oReader1.GetString(22).Equals("BV90") && depart.Equals("02"))
                        {
                            // lDepoz = "02T1";
                        }

                        if (oReader1.GetString(22).Equals("BV90") && depart.Equals("05"))
                        {
                            lDepoz = "BV90";
                        }


                        if (tipMat.Trim() != "")
                            lnumeArt += " (" + tipMat + ")";

                        if (oReader1.GetString(1).Equals("00000000"))
                            lnumeArt = "Taxa verde";


                        articol = new ArticolComandaRap();
                        articol.status = oReader1.GetString(0);
                        articol.codArticol = oReader1.GetString(1);
                        articol.numeArticol = lnumeArt;
                        articol.cantitate = oReader1.GetFloat(3);
                        articol.depozit = lDepoz;
                        articol.pretUnit = oReader1.GetFloat(5);
                        articol.um = oReader1.GetString(6);
                        articol.procent = oReader1.GetFloat(7);
                        articol.procentFact = oReader1.GetFloat(8);
                        articol.conditie = oReader1.GetString(9).Equals("X") ? true : false;
                        articol.cmp = Double.Parse(oReader1.GetString(10), CultureInfo.InvariantCulture);
                        articol.discClient = oReader1.GetFloat(11);
                        articol.discountAg = oReader1.GetFloat(12);
                        articol.discountSd = oReader1.GetFloat(13);
                        articol.discountDv = oReader1.GetFloat(14);
                        articol.procAprob = oReader1.GetFloat(15);
                        articol.permitSubCmp = oReader1.GetString(16);
                        articol.multiplu = oReader1.GetFloat(17);
                        articol.pret = oReader1.GetFloat(18);
                        articol.infoArticol = oReader1.GetString(19);
                        articol.cantUmb = oReader1.GetFloat(20).ToString();
                        articol.Umb = oReader1.GetString(21);
                        articol.depart = oReader1.GetString(22);
                        articol.tipArt = oReader1.GetString(23);

                        //preturi medii
                        if ((tipUser.Equals("CV") || tipUser.Equals("SM")) && afisConditii.Equals("1") && !articol.depart.Equals("11"))
                        {

                            cmdInner = new OracleCommand();

                            cmdInner = connection.CreateCommand();
                            cmdInner.CommandText = " select to_char(pret_med/cant, '99990.999')  , to_char(adaos_med/cant,'99990.999')  , um from sapprd.zpret_mediu_oras r where mandt = '900' and pdl =:unitLog " +
                                              " and matnr=:articol ";


                            cmdInner.Parameters.Clear();
                            cmdInner.Parameters.Add(":unitLog", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[0].Value = unitLog1.Substring(0, 2) + "1" + unitLog1.Substring(3, 1);

                            cmdInner.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                            cmdInner.Parameters[1].Value = articol.codArticol.Length == 8 ? "0000000000" + articol.codArticol : articol.codArticol;

                            oReaderInner = cmdInner.ExecuteReader();

                            if (oReaderInner.HasRows)
                            {
                                oReaderInner.Read();
                                pretMediu = oReaderInner.GetString(0).Trim();
                                adaosMediu = oReaderInner.GetString(1).Trim();
                                unitMasPretMediu = oReaderInner.GetString(2);
                            }
                            else
                            {
                                pretMediu = adaosMediu = unitMasPretMediu = "0";
                            }

                            articol.pretMediu = pretMediu;
                            articol.adaosMediu = adaosMediu;
                            articol.unitMasPretMediu = unitMasPretMediu;

                        }


                        listArticole.Add(articol);

                        unitLogAlt = oReader1.GetString(22).Trim() != "" ? oReader1.GetString(22) : "NN10";

                        nrArt++;
                    }

                }

                comanda.filialaAlternativa = unitLogAlt;

                oReaderInner.Close();
                oReaderInner.Dispose();

                oReader1.Close();
                oReader1.Dispose();

                //articole conditii


                //antet
                string condAfis = "";
                int IdCndArt = -1;

                if (afisConditii == "1")
                    condAfis = " cmdref=:idcmd ";

                if (afisConditii == "2" || afisConditii == "3")
                    condAfis = " cmdmodif=:idcmd ";

                cmd.CommandText = " select id, condcalit, nrfact, nvl(observatii,' ') observatii from sapprd.zcondheadtableta where " + condAfis;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrComanda);

                oReader1 = cmd.ExecuteReader();

                ConditiiComanda conditiiComanda = new ConditiiComanda();
                ArticolComanda articolConditie = null;
                List<ArticolComanda> listArticoleConditii = new List<ArticolComanda>();

                if (oReader1.HasRows)
                {
                    oReader1.Read();
                    IdCndArt = oReader1.GetInt32(0);

                    conditiiComanda.condCalitative = oReader.GetFloat(1).ToString();
                    conditiiComanda.nrFacturi = oReader1.GetInt32(2).ToString();
                    conditiiComanda.observatii = oReader1.GetString(3);
                }

                oReader1.Close();

                string condId = " and a.id = " + IdCndArt;


                //articole conditii
                if (IdCndArt != -1)
                {
                    cmd.CommandText = " select  decode(length(a.codart),18,substr(a.codart,-8),a.codart) codart ,b.nume, a.cant," +
                                      " a.um,a.valoare, nvl(multiplu,1)  " +
                                      " from sapprd.zconddettableta a, " +
                                      " articole b where  a.codart = b.cod " + condId + " order by poz ";


                    cmd.Parameters.Clear();
                    oReader1 = cmd.ExecuteReader();


                    if (oReader1.HasRows)
                    {
                        while (oReader1.Read())
                        {
                            articolConditie = new ArticolComanda();
                            articolConditie.codArticol = oReader1.GetString(0);
                            articolConditie.numeArticol = oReader1.GetString(1);
                            articolConditie.cantitate = oReader1.GetFloat(2);
                            articolConditie.um = oReader1.GetString(3);
                            articolConditie.pretUnit = oReader1.GetDouble(4);
                            articolConditie.multiplu = oReader1.GetDouble(5);
                            listArticoleConditii.Add(articolConditie);

                        }

                    }

                    oReader1.Close();
                    oReader1.Dispose();

                }


                //sf. conditii

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string stringArticoleConditii = serializer.Serialize(listArticoleConditii);
                conditiiComanda.articole = stringArticoleConditii;

                string stringConditiiComanda = serializer.Serialize(conditiiComanda);
                string stringArticoleComanda = serializer.Serialize(listArticole);

                comanda.articole = stringArticoleComanda;
                comanda.conditiiComanda = stringConditiiComanda;

                serializedResult = serializer.Serialize(comanda);




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + "Nr cmd: " + nrComanda);
                ErrorHandling.sendErrorToMail(cmd.CommandText);
            }
            finally
            {
                cmd.Dispose();

                if (cmdInner != null)
                    cmdInner.Dispose();

                connection.Close();
                connection.Dispose();
            }


            return serializedResult;
        }



        public string getComenziDeschise(string codAgent)
        {

            string serializedResult = "";
            List<ComandaActiva> listComenzi = new List<ComandaActiva>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;



            try
            {
                // string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                string connectionString = conectToProd();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select b.name1, c.valoare, c.city, nvl(c.adr_livrare,' '),  c.nrcmdsap, c.cod_client, b.nr_bord, a.masina, c.region, d.nume, " +
                                 " nvl(d.telefon,'-') telefon, a.sttrg  " +
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.zcomhead_tableta c, soferi d" +
                                 "  where a.sttrg in ( 2, 3, 4, 6, 7) and b.cod_av=:codAgent and a.numarb = b.nr_bord and c.id = b.idcomanda and d.cod = a.cod_sofer " +
                                 " union " +
                                 " select b.name1 ||' (CLP)' name1, 0 valoare, c.city, nvl(c.adr_livrare, ' '),  c.nrcmdsap, c.cod_client, b.nr_bord, a.masina, c.region, d.nume, " +
                                 " nvl(d.telefon, '-') telefon, a.sttrg " +
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.zclphead c, soferi d " +
                                 " where a.sttrg in (2, 3, 4, 6, 7) and b.cod_av = :codAgent and a.numarb = b.nr_bord and c.id = b.iddlclp and d.cod = a.cod_sofer " +
                                 " union " +
                                 " select distinct b.name1 ||' (ONLINE)' name1, k.netwr valoare, a.city1, nvl(a.street,' '), " +
                                 " k.vbeln nrcmdsap, k.kunnr cod_client, b.nr_bord, a.masina, a.region, d.nume, " + 
                                 " nvl(d.telefon, '-') telefon, a.sttrg " + 
                                 " from borderouri a, sapprd.zdocumentesms b, sapprd.vbak k, soferi d, sapprd.vbfa f, sapprd.vbpa p, sapprd.adrc a " +
                                 " where a.sttrg in (2, 3, 4, 6, 7) and b.cod_av =:codAgent and a.numarb = b.nr_bord and f.mandt = '900' and f.vbeln = b.nr_doc and d.cod = a.cod_sofer " + 
                                 " and k.mandt = '900' and k.vbeln = f.vbelv and f.vbtyp_v = 'C' and f.vbtyp_n = 'J' " + 
                                 " and k.auart in ('ZTAH','ZEPH') and k.mandt = p.mandt and k.vbeln = p.vbeln and p.parvw = 'WE' and p.mandt = a.client and p.adrnr = a.addrnumber" +
                                 " order by name1 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codAgent;

                oReader = cmd.ExecuteReader();

                ComandaActiva comanda = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        comanda = new ComandaActiva();
                        comanda.numeClient = oReader.GetString(0);
                        comanda.valoare = oReader.GetDouble(1).ToString();
                        comanda.localitate = oReader.GetString(2);
                        comanda.strada = oReader.GetString(3);
                        comanda.idCmdSap = oReader.GetString(4);
                        comanda.codClient = oReader.GetString(5);
                        comanda.codBorderou = oReader.GetString(6);
                        comanda.nrMasina = oReader.GetString(7);
                        comanda.codJudet = oReader.GetString(8);
                        comanda.numeSofer = oReader.GetString(9);
                        comanda.telSofer = oReader.GetString(10);
                        comanda.stareComanda = oReader.GetString(11);
                        listComenzi.Add(comanda);

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
                serializedResult = "-1";
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listComenzi);


            return serializedResult;


        }


        public string getClientiBorderou(string codBorderou)
        {
            string serializedResult = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientBorderou> listClienti = new List<ClientBorderou>();

            try
            {
                // string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                string connectionString = conectToProd();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select b.cod_client, b.poz,  nvl(a.ora,' ')  from sapprd.zevenimentsofer a, sapprd.zdocumentesms b where  b.nr_bord =:codBorderou " +
                                  " and a.document(+) = b.nr_bord and a.eveniment(+) = 'S' " +
                                  " and a.client(+) = b.cod_client order by b.poz ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codBorderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codBorderou;

                oReader = cmd.ExecuteReader();

                ClientBorderou client = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        client = new ClientBorderou();
                        client.codClient = oReader.GetString(0);
                        client.pozitie = oReader.GetString(1);
                        client.dataEveniment = oReader.GetString(2);
                        listClienti.Add(client);

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
                serializedResult = "-1";
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listClienti);

            return serializedResult;


        }



        public string getPozitieMasina(string nrMasina)
        {



            string coords = "0#0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            List<ClientBorderou> listClienti = new List<ClientBorderou>();

            try
            {


                string connectionString = conectToProd();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(latitude,0), nvl(longitude,0) from gps_index where device_id = ( select id from gps_masini where nr_masina = replace(:nrMasina, '-', '')) ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrMasina", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina;

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();
                    coords = oReader.GetDouble(0).ToString() + "#" + oReader.GetDouble(1).ToString();
                }


                oReader.Close();
                oReader.Dispose();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                coords = "-1";
            }
            finally
            {

                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return coords;
        }




        public static ClientComanda getClientComanda(OracleConnection connection, string idComanda)
        {
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            ClientComanda clientComanda = new ClientComanda();

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select a.cod_client, a.nrcmdsap, b.adrnr from sapprd.zcomhead_tableta a, sapprd.vbpa b, clienti c " + 
                                   " where a.id=:idComanda and b.vbeln = a.nrcmdsap and b.parvw = 'WE' and b.mandt = '900' and c.cod = a.cod_client and c.tip_pers = 'PJ' ";

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    clientComanda.codClient = oReader.GetString(0);
                    clientComanda.idComandaSap = oReader.GetString(1);
                    clientComanda.codAdresa = oReader.GetString(2);

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

            return clientComanda;
        }



        public static Adresa getAdresaComanda(OracleConnection connection, string idComanda, string codAdresa)
        {

            Adresa adresa = new Adresa();

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                string sqlString = " select latitude, longitude from sapprd.zcoordcomenzi where idcomanda =:idComanda ";


                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    adresa.latitude = oReader.GetString(0);
                    adresa.longitude = oReader.GetString(1);

                }



            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return adresa;

        }



        public static string respingeComanda(OracleConnection connection, String nrCmd, String codUser, String codRespingere)
        {

            string retVal = " ";
            
            SAPWebServices.ZTBL_WEBSERVICE webService = null;
            webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZstareComanda inParam = new SAPWebServices.ZstareComanda();
            inParam.NrCom = nrCmd;
            inParam.Stare = "3";
            inParam.PernrCh = codUser;

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWebServices.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);

            string response = outParam.VOk;

            OracleCommand cmd = new OracleCommand();

            cmd = connection.CreateCommand();

            cmd.CommandText = " update sapprd.zcomhead_tableta set abgru = '" + codRespingere + "' where id=:cmd ";

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Clear();
            cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = nrCmd;

            cmd.ExecuteNonQuery();

            cmd.Dispose();
          


            if (response.Equals("0"))
            {
                retVal = "Comanda respinsa.";
            }
            else
            {
                retVal = outParam.VMess;
            }

            webService.Dispose();


            return retVal;

        }



        private static string conectToProd()
        {
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                             " (HOST = 10.1.3.95)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = TABLET))); " +
                             " User Id = WEBSAP; Password = 2INTER7;";


        }


    }
}