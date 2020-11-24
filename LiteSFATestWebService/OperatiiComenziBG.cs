using LiteSFATestWebService.General;
using LiteSFATestWebService.SAPWSBudmax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService
{
    public class OperatiiComenziBG
    {

        public void testConnection()
        {

            List<string> numeAgenti = new List<string>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {


                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select * from agenti order by nume ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    numeAgenti.Add(oReader.GetString(1));
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


        }



        public List<Comanda> getListComenzi(string filiala, string codUser, string tipCmd, string tipUser, string depart, string interval, int restrictii, string codClient)
        {
            
            string retVal = "";
            string tipComanda = "";
            string selCmd = "";
            string tabDV = "", condDV = "";
            string condData = "", condRestr = "", condClient = "", sqlString = "", pondereB_30 = "0";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand(), cmd1 = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;

            List<Comanda> listComenzi = new List<Comanda>();

            try
            {
                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string condHome = "";
                string tabelaHome = "";
                string condComenziEmise1 = "";
                string tabelaComenziEmise = "";
                string condComenziEmise2 = "";


                //lista comenzi


                if (tipCmd == "2") //doar comenzile pentru aprobare 
                {

                   

                    if (tipUser.Equals("DV"))
                    {

                        string localDepart = depart;
                        
                        condHome = "  ";
                        tabelaHome = "";
                        
                        selCmd = " and a.accept2 = 'X' and a.ora_accept2 = '000000' and decode(decode(a.accept1,'X',a.ora_accept1,'1'),'000000',1,0)=0  ";
                        condDV = " and a.depart = '" + localDepart + "' ";
                        tipComanda = " and a.status_aprov in ('1','6','21') and a.status in ('2','11') " + selCmd;

                    }

                }
               

              

                string condDepart = " and cl.depart = a.depart ";


                if (tipUser == "KA")
                    condDepart = " and cl.depart = a.depart ";



                
                //restul (agenti, ka, directori)
              
                {

                    string condCanal = " and substr(a.ul,3,1) = substr(cl.canal(+),1,1) ";
                  

                    sqlString = " select distinct a.id, nvl(b.nume,'-') nume1, to_char(to_date(a.datac,'yyyymmdd')) datac1, a.valoare,a.status,  a.cod_client, " +
                                " decode(a.nrcmdsap,' ','-1',a.nrcmdsap) cmdsap, nvl(a.status_aprov,-1) status_aprov, a.fact_red,  a.ul, a.accept1, a.accept2, " +
                                " nvl((select  cl.tip from clie_tip cl where cl.depart = a.depart  " + condCanal + "   and cl.cod_cli = a.cod_client),' ') tip" +
                                " , ag.nume, decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, a.datac, nvl(a.docin,-1) docin1, " +
                                " nvl(a.adr_noua,-1) adr_noua1, a.city ||', '|| a.adr_livrare adr_livrare1, ag.divizie, a.nume_client, a.depart, " +
                                " a.aprob_cv_necesar , a.aprob_cv_realiz, ' ' cod_client_generic_ged, ' ' conditii, nvl(ag.nrtel,'-1') telAgent, a.client_raft " +
                                " from sapdev.zcomhead_tableta a, " +
                                " clienti b, agenti ag, clie_tip cl " + tabelaComenziEmise + tabelaHome + tabDV +
                                " where a.cod_client=b.cod and ag.cod = a.cod_agent " + tipComanda + condDV + condData + condRestr + condClient + condDepart + condHome +
                                "  and cl.cod_cli = a.cod_client  " + condComenziEmise1 + condComenziEmise2 + " order by id ";




                    //pentru aprobare se afiseaza doar ultimile 10 comenzi
                    if (tipCmd.Equals("2") && !depart.Equals("04"))
                    {
                        sqlString = " select x.id, x.nume1, to_char(to_date(x.datac,'yyyymmdd')), x.valoare, x.status, x.cod_client, x.cmdsap, x.status_aprov, x.fact_red, x.ul, x.accept1, x.accept2, x.tip, x.nume, x.pmnttrms, x.datac, x.docin1, " +
                                    " x.adr_noua1, x.adr_livrare1, x.divizie, x.nume_client, x.depart, x.aprob_cv_necesar, x.aprob_cv_realiz,x.cod_client_generic_ged,x.conditii, x.telAgent, x.client_raft " +
                                    " from ( " + sqlString + " ) x where rownum<=15 ";
                    }



                }

                

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();




                oReader = cmd.ExecuteReader();
                string cursValut = "0";
                string strNumeClient = " ";


              


                if (oReader.HasRows)
                {

                    Comanda comanda;

                    while (oReader.Read())
                    {
                        strNumeClient = oReader.GetString(1);


                        if (oReader.GetString(20).Trim().Length > 2)
                            strNumeClient = oReader.GetString(20);


                        comanda = new Comanda();
                        comanda.idComanda = oReader.GetInt32(0).ToString();
                        comanda.numeClient = strNumeClient;
                        comanda.dataComanda = oReader.GetString(2);
                        comanda.sumaComanda = oReader.GetFloat(3).ToString();

                        comanda.stareComanda = oReader.GetString(7);
                        comanda.codClient = oReader.GetString(5);
                        comanda.cmdSap = oReader.GetString(6);
                        comanda.factRed = oReader.GetString(8);
                        comanda.filiala = oReader.GetString(9);
                        comanda.accept1 = oReader.GetString(10);
                        comanda.accept2 = oReader.GetString(11);
                        comanda.tipClient = oReader.GetString(12);
                        comanda.numeAgent = oReader.GetString(13);
                        comanda.termenPlata = oReader.GetString(14);
                        comanda.docInsotitor = oReader.GetString(16);
                        comanda.adresaLivrare = oReader.GetString(18);
                        comanda.divizieAgent = oReader.GetString(19);
                        comanda.divizieComanda = oReader.GetString(21);
                        comanda.aprobariNecesare = oReader.GetString(22);
                        comanda.aprobariPrimite = oReader.GetString(23);
                        comanda.codClientGenericGed = oReader.GetString(24);
                        comanda.conditiiImpuse = oReader.GetString(25);
                        comanda.telAgent = oReader.GetString(26);
                        comanda.monedaComanda = "BGN";
                        comanda.monedaTVA = "BGN";

                        if (tipCmd.Equals("4"))
                            //comanda.avans = oReader.GetDouble(28).ToString();
                            comanda.avans = "25";

                        else
                            comanda.avans = "0";

                        double comandaCuTva = oReader.GetFloat(3);
                        string canalDistrib = "20";
                        if (oReader.GetString(9).Substring(2, 1).Equals("1"))
                        {
                            comandaCuTva = oReader.GetFloat(3) * 1.19;
                            canalDistrib = "10";
                        }

                        comanda.sumaTVA = comandaCuTva.ToString();
                        comanda.canalDistrib = canalDistrib;
                        comanda.clientRaft = oReader.GetString(27) == null ? " " : oReader.GetString(27);

                        if (tipUser == "DV")
                        {
                            //curs valutar
                            cmd1 = connection.CreateCommand();
                            cmd1.CommandText = " select x.ukurs from (select distinct a.ukurs, (100000000 - a.gdatu - 1) " +
                                              " from sapdev.tcurr a, sapdev.knvv b where a.mandt='900' and " +
                                              " b.mandt='900' and b.kurst = a.kurst and b.kunnr =:codClient " +
                                              " and fcurr = 'EUR' and tcurr = 'BGN' and (100000000 - a.gdatu - 1) <=:dataCurs " +
                                              " and b.spart =:depart order by 100000000 - a.gdatu - 1 desc ) x where rownum<2 ";

                            cmd1.Parameters.Clear();

                            cmd1.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                            cmd1.Parameters[0].Value = oReader.GetString(5);

                            cmd1.Parameters.Add(":dataCurs", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                            cmd1.Parameters[1].Value = oReader.GetString(15);

                            cmd1.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                            cmd1.Parameters[2].Value = depart;


                            oReader1 = cmd1.ExecuteReader();
                            if (oReader1.HasRows)
                            {
                                oReader1.Read();
                                cursValut = oReader1.GetDouble(0).ToString();
                            }
                            else
                            {
                                cursValut = "0";
                            }
                            //sf. curs



                        }//sd. tip DV





                        String adresaNoua = "-1";
                        if (oReader.GetString(17).Equals("X"))
                            adresaNoua = oReader.GetString(18);

                        retVal += "#" + cursValut + "#" + oReader.GetString(16) + "#" + adresaNoua + "#" + oReader.GetString(19) + "#" + pondereB_30 + "@@";
                        comanda.cursValutar = cursValut;
                        comanda.adresaNoua = adresaNoua;
                        comanda.pondere30 = pondereB_30;


                        listComenzi.Add(comanda);
                    }





                }




                if (oReader1 != null)
                {
                    oReader1.Close();
                    oReader1.Dispose();
                }


               

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                cmd.Dispose();
                cmd1.Dispose();
                connection.Close();
                connection.Dispose();

            }





            return listComenzi;

        }


        public string getArticoleComandaVanzare(string nrCmd, string afisCond, string tipUser, string departament)
        {


            string serializedResult = "", retVal = "";
            string unitLog1 = "", termenPlata = "", obsLivrare = "", tipPersClient = "";
            string cmp = "";

            string conditieDepart = "";

            string condArticole111 = " ";
                                     

            if ((tipUser.Equals("DV") || tipUser.Equals("DD")) && !departament.Equals("00") && !departament.Equals("11"))
                conditieDepart = " and (b.spart = '" + departament + "' or b.spart = '11') " + condArticole111;
            //else if ((tipUser.Equals("DV") || tipUser.Equals("DD")) && (departament.Equals("00") || departament.Equals("11")))
            //    conditieDepart = " and a.cod like '0000000000111%' ";



            string condBlocAprov = " ,'-1' blocAprov ";

            if (tipUser.Equals("DV") || tipUser.Equals("DD"))
            {
                condBlocAprov = " ,nvl((select '1' from sapdev.mara m where m.mandt = '900' and m.matnr = a.cod and m.mstae = 'Z1'),'-1') blocAprov ";
            }


            string istoricPret = " ,a.istoric_pret istoricPret ";
            string vechime = ", a.vechime ";

            //
            //afisCond:
            // 1 - pentru agent
            // 2 - pentru aprobare comanda SD
            // 3 - pentru aprobare comanda DV


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null, oReader1 = null;
            OracleCommand cmdInner = null;
            OracleDataReader oReaderInner = null;
            int nrArt = 0;
            string varArtCmd = "", infoPret = " , '0' info";
            String pretMediu = "0", adaosMediu = "0", unitMasPretMediu = "0";
            String coefCorectie = "0";

            DateLivrareCmd dateLivrare = new DateLivrareCmd();
            List<ArticolComandaRap> listArticole = new List<ArticolComandaRap>();
            Conditii conditii = new Conditii();

            try
            {
                //antet comanda
                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.pers_contact, a.telefon, a.adr_livrare, a.mt, a.tip_plata, a.cantar, a.fact_red, a.city, a.region, a.ul, " +
                                  " decode(a.pmnttrms,'',' ',a.pmnttrms) pmnttrms, decode(a.obstra,'',' ',a.obstra) obstra, b.tip_pers, a.email, a.obsplata, a.ketdat, " +
                                  " a.adr_livrare_d, a.city_d, a.region_d, a.macara, a.nume_client, a.stceg, a.id_obiectiv, a.adresa_obiectiv, " +
                                  " nvl((select latitude||','||longitude from sapdev.zcoordcomenzi where idcomanda = a.id),'0,0') coord, " +
                                  " 0 tonaj, nvl(client_raft,' '), a.meserias, a.fact_palet_separat " +
                                  " from sapdev.zcomhead_tableta a, clienti b " +
                                  " where a.id=:idcmd and a.cod_client = b.cod ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();


                if (oReader.HasRows)
                {
                    oReader.Read();

                    unitLog1 = oReader.GetString(9);
                    termenPlata = oReader.GetString(10);
                    obsLivrare = oReader.GetString(11);
                    tipPersClient = oReader.GetString(12);

                    dateLivrare.persContact = oReader.GetString(0);
                    dateLivrare.nrTel = oReader.GetString(1);
                    dateLivrare.dateLivrare = oReader.GetString(2);
                    dateLivrare.Transport = oReader.GetString(3);
                    dateLivrare.tipPlata = oReader.GetString(4);
                    dateLivrare.Cantar = oReader.GetString(5);
                    dateLivrare.factRed = oReader.GetString(6);
                    dateLivrare.redSeparat = oReader.GetString(6);
                    dateLivrare.Oras = oReader.GetString(7);
                    dateLivrare.codJudet = oReader.GetString(8);
                    dateLivrare.unitLog = oReader.GetString(9);
                    dateLivrare.termenPlata = oReader.GetString(10);
                    dateLivrare.obsLivrare = oReader.GetString(11);
                    dateLivrare.tipPersClient = oReader.GetString(12);
                    dateLivrare.mail = oReader.GetString(13);
                    dateLivrare.obsPlata = oReader.GetString(14);
                    dateLivrare.dataLivrare = GeneralUtils.formatStrDate(oReader.GetString(15));
                    dateLivrare.adresaD = oReader.GetString(16);
                    dateLivrare.orasD = oReader.GetString(17);
                    dateLivrare.codJudetD = oReader.GetString(18);
                    dateLivrare.macara = oReader.GetString(19);
                    dateLivrare.numeClient = oReader.GetString(20);
                    dateLivrare.cnpClient = oReader.GetString(21);
                    dateLivrare.idObiectiv = oReader.GetInt32(22).ToString();
                    dateLivrare.isAdresaObiectiv = oReader.GetString(23).Equals("X") ? true : false;
                    dateLivrare.coordonateGps = oReader.GetString(24);
                    dateLivrare.tonaj = oReader.GetDouble(25).ToString();
                    dateLivrare.clientRaft = oReader.GetString(26).ToString();
                    dateLivrare.meserias = oReader.GetString(27);
                    dateLivrare.factPaletiSeparat = oReader.GetString(28).Equals("X") ? true : false;


                }


                oReader.Close();
                oReader.Dispose();

               



                string condTabKA = " , sapdev.zcomhead_tableta z ", condIdKA = " and a.id=:idcmd and z.id = a.id ", condOrderKA = " trim(poz) ";
               



                infoPret = ", nvl(a.inf_pret,'0') infopret";


               

                cmd.CommandText = " select a.status, decode(length(a.cod),18,substr(a.cod,-8),a.cod) codart ,nvl(b.nume,' '), a.cantitate, decode(trim(a.depoz),'','0000',a.depoz) depoz, " +
                                  " a.valoare, a.um, a.procent, nvl(a.procent_fc,0) procent_fc, decode(trim(a.conditie),'','0',a.conditie) conditie, '-1' cmp " + 
                                  " , nvl(a.disclient,0) disclient, nvl(a1.discount,0) av, " +
                                  " nvl((select distinct discount from sapdev.zdisc_pers_sint where  functie='SD' and spart =substr(c.cod_nivel1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapdev.zdisc_pers_sint where  functie='DV' and spart =substr(c.COD_NIVEL1,2,2) " +
                                  " and werks ='" + unitLog1 + "' and inactiv <> 'X' and matkl = c.cod),0) dv, nvl(a.procent_aprob,0) procent_aprob, " +
                                  " decode(s.matkl,'','0','1') permitsubcmp, nvl(a.multiplu,1) multiplu, nvl(a.val_poz,0) " + infoPret +
                                  " ,nvl(a.cant_umb,0) cant_umb , nvl(a.umb,' ') umb, a.ul_stoc, z.depart, nvl(b.tip_mat,' '), nvl(a.ponderat,'-1'), nvl(b.spart,' '), " +
                                  " decode(trim(z.depart),'','00', z.depart)  dep_aprobare " + condBlocAprov + istoricPret + vechime +
                                  " from sapdev.zcomdet_tableta a, sapdev.zdisc_pers_sint a1,  sintetice c," +
                                  " articole b, sapdev.zpretsubcmp s " + condTabKA + " where a.cod = b.cod(+) " + condIdKA + " and " +
                                  " a1.inactiv(+) <> 'X' and a1.functie(+)='AV' and a1.spart(+)=substr(c.COD_NIVEL1,2,2) and a1.werks(+) ='" + unitLog1 + "' " +
                                  " and b.sintetic = c.cod(+) and a1.matkl(+) = c.cod " + conditieDepart +
                                  " and s.mandt(+)='900' and s.matkl(+) = c.cod order by " + condOrderKA;





                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader1 = cmd.ExecuteReader();
                String unitLogAlt = "NN10", depart = "00", tipMat = "", lnumeArt = "", lDepoz = "";
                String blocAprov = "";

                ArticolComandaRap articol;
                if (oReader1.HasRows)
                {
                    while (oReader1.Read())
                    {
                        depart = oReader1.GetString(23).Trim() != "" ? oReader1.GetString(23) : "00";
                        tipMat = oReader1.GetString(24);
                        blocAprov = oReader1.GetString(28);

                        lnumeArt = oReader1.GetString(2).Replace("#", "");
                        lDepoz = oReader1.GetString(4);

                        if (tipMat.Trim() != "")
                            lnumeArt += " (" + tipMat + ")";

                        if (!blocAprov.Equals("-1"))
                            lnumeArt += " (Bloc.aprov.)";

                        if (oReader1.GetString(1).Equals("00000000"))
                            lnumeArt = "Taxa verde";

                        unitLogAlt = oReader1.GetString(22).Trim() != "" ? oReader1.GetString(22) : "NN10";

                        articol = new ArticolComandaRap();
                        articol.status = oReader1.GetString(0);
                        articol.codArticol = oReader1.GetString(1);
                        articol.numeArticol = lnumeArt;
                        articol.cantitate = oReader1.GetDouble(3);
                        articol.depozit = lDepoz;
                        articol.pretUnit = oReader1.GetDouble(5);
                        articol.um = oReader1.GetString(6);
                        articol.procent = oReader1.GetDouble(7);
                        articol.procentFact = oReader1.GetDouble(8);
                        articol.conditie = false;

                        double valCmp = OperatiiPreturiBG.getCmp(connection, afisCond, unitLogAlt, articol.codArticol, unitLog1);

                        articol.cmp = getTipValoare(valCmp, dateLivrare.unitLog, tipUser);

                        articol.discClient = oReader1.GetDouble(11);
                        articol.discountAg = oReader1.GetDouble(12);
                        articol.discountSd = oReader1.GetDouble(13);
                        articol.discountDv = oReader1.GetDouble(14);
                        articol.procAprob = oReader1.GetDouble(15);
                        articol.permitSubCmp = oReader1.GetString(16);
                        articol.multiplu = oReader1.GetDouble(17);
                        articol.pret = oReader1.GetDouble(18);
                        articol.infoArticol = oReader1.GetString(19);
                        articol.cantUmb = oReader1.GetDouble(20).ToString();
                        articol.Umb = oReader1.GetString(21);
                        articol.unitLogAlt = unitLogAlt;
                        articol.depart = depart;
                        articol.tipArt = tipMat;
                        articol.ponderare = Int32.Parse(oReader1.GetString(25).Trim().Equals("") ? "-1" : oReader1.GetString(25));
                        articol.departSintetic = oReader1.GetString(26);
                        //articol.departAprob = oReader1.GetString(27);
                        articol.departAprob = depart;
                        articol.istoricPret = GeneralUtils.formatIstoricPret(oReader1.GetString(29));
                        articol.vechime = oReader1.GetDouble(30).ToString();
                        articol.moneda = "BGN";
                        articol.valTransport = "0";
                        articol.procTransport = "0";

                        //verificare factori conversie


                        double factorConversie = 1.0;
                        if (!articol.um.Equals(articol.Umb))
                        {
                            String[] convArray = getArtFactConvUM(connection, articol.codArticol.Length == 8 ? "0000000000" + articol.codArticol : articol.codArticol, articol.um).Split('#');
                            factorConversie = Double.Parse(convArray[0]) / Double.Parse(convArray[1]);

                        }


                        articol.pretMediu = pretMediu;
                        articol.adaosMediu = adaosMediu;
                        articol.unitMasPretMediu = unitMasPretMediu;
                        articol.coefCorectie = coefCorectie;


                        listArticole.Add(articol);

                        nrArt++;
                    }


                }

                if (oReaderInner != null)
                {
                    oReaderInner.Close();
                    oReaderInner.Dispose();
                }

                if (oReader1 != null)
                {
                    oReader1.Close();
                    oReader1.Dispose();
                }

                //articole conditii


                //antet

                int IdCndArt = -1;


                cmd.CommandText = "select id, condcalit, nrfact, nvl(observatii,' ') observatii from sapdev.zcondheadtableta where cmdref =:idcmd ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrCmd;

                oReader1 = cmd.ExecuteReader();


                ConditiiHeader conditiiHeader = new ConditiiHeader();
                ConditiiArticole conditiiArticole;

                List<ConditiiArticole> listArtCond = new List<ConditiiArticole>();

                if (oReader1.HasRows)
                {
                    oReader1.Read();
                    IdCndArt = oReader1.GetInt32(0);
                    retVal += oReader1.GetFloat(1) + "#" + oReader1.GetInt32(2) + "#" + oReader1.GetString(3) + "@@";

                    conditiiHeader.id = IdCndArt;
                    conditiiHeader.conditiiCalit = oReader1.GetDouble(1);
                    conditiiHeader.nrFact = oReader1.GetInt32(2);
                    conditiiHeader.observatii = oReader1.GetString(3);

                }

                conditii.header = conditiiHeader;

                oReader1.Close();




            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                if (cmdInner != null)
                    cmdInner.Dispose();

                connection.Close();
                connection.Dispose();
            }

            if (tipUser.Equals("DV") && departament.Equals("01"))
                HelperComenzi.setMarjaCantPal(listArticole, dateLivrare);

            ArticolComandaAfis articoleComanda = new ArticolComandaAfis();

            articoleComanda.dateLivrare = dateLivrare;
            articoleComanda.articoleComanda = listArticole;
            articoleComanda.conditii = conditii;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(articoleComanda);


            return serializedResult;

        }


        public string getArtFactConvUM(OracleConnection connection, string codArt, string unitMas)
        {
            string retVal = "1#1";

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select umrez, umren from sapdev.marm where mandt = '900' and matnr =:codArt and meinh=:unitMas ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                cmd.Parameters.Add(":unitMas", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = unitMas;


                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    retVal = oReader.GetInt32(0).ToString() + "#" + oReader.GetInt32(1);
                }



                oReader.Close();
                oReader.Dispose();
                cmd.Dispose();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }
            return retVal;
        }

        private double getTipValoare(double valoare, string unitLog, string tipUser)
        {
            bool canalDistrib20 = unitLog.Substring(2, 1).Equals("2") ? true : false;

            bool canalDistrib40 = unitLog.Substring(2, 1).Equals("4") ? true : false;

            if (tipUser.Equals("DV") && (canalDistrib20 || canalDistrib40))
                return valoare * 1.19;
            else
                return valoare;
        }


        public static bool isComandaBG(string idComanda)
        {
            bool isComandaBG = false;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapdev.zcomhead_tableta where id =:idCmd ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    isComandaBG = true;
                }

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
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return isComandaBG;
        }



        public string opereazaComandaSap(string idComanda, string codUser, string tipOperatie, string codRespingere)
        {

            ErrorHandling.sendErrorToMail("opereazaComandaSap BG" + idComanda + "\n" +  codUser + "\n" + tipOperatie + "\n" + codRespingere);

            string retVal = "-1";

            string codOperatie = "-1";

            if (tipOperatie.Equals("APROBARE"))
                codOperatie = "2";
            else if (tipOperatie.Equals("RESPINGERE"))
                codOperatie = "3";

            SAPWSBudmax.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWSBudmax.ZstareComanda inParam = new SAPWSBudmax.ZstareComanda();

            inParam.NrCom = idComanda;
            inParam.Stare = codOperatie;
            inParam.PernrCh = codUser;

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(getUser(), getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWSBudmax.ZstareComandaResponse outParam = webService.ZstareComanda(inParam);
            string response = outParam.VOk;

            if (response.Equals("0"))
            {

                if (tipOperatie.Equals("RESPINGERE"))
                    setMotivRespingere(idComanda, codRespingere);
                else if (tipOperatie.Equals("APROBARE"))
                    updateAcceptTime(idComanda);

               retVal = "Operatie reusita BG.";

            }
            else
            {
                retVal = outParam.VMess;
            }

            return retVal;
        }


        private void updateAcceptTime(string idComanda) {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " update sapdev.zcomhead_tableta set ora_accept2 = (select to_char(systimestamp, 'hh24mmss') from dual) " +
                                  " where id =:idCmd ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idCmd", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idComanda;
                cmd.ExecuteNonQuery();

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }


        }

    public void setMotivRespingere(string idComanda, string codRespingere)
        {
            OracleCommand cmd = new OracleCommand();
            OracleConnection connection = new OracleConnection();

            try
            {

                string connectionString = DatabaseConnections.ConnectToBGTest();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();
                cmd.CommandText = " update sapdev.zcomhead_tableta set abgru =:codResp where id=:cmd ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codResp", OracleType.NVarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codRespingere;

                cmd.Parameters.Add(":cmd", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = idComanda;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }
        }





        private string getUser()
        {
            return "USER_RFC";
        }

        private string getPass()
        {
            return "2rfc7tes3";
        }

    }
}