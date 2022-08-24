using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;
using System.Web.Script.Serialization;
using LiteSFATestWebService.General;
using System.Globalization;

namespace LiteSFATestWebService
{
    public class OperatiiCLP
    {



        private string saveCmdClp(OracleConnection connection, AntetComandaCLP antetComanda, List<ArticolComandaCLP> articole, string filiala, string codAgent, bool alertSD, string codSuperAgent)
        {


            OracleCommand cmd = connection.CreateCommand();
            OracleTransaction transaction = null;

            string query = " insert into sapprd.zclphead(mandt, id, cod_client, cod_agent, ul, ul_dest, depart, status, nrcmdsap, datac, accept1, status_aprov, " +
                           " pers_contact, telefon, adr_livrare, city, region, ketdat, dl, tip_plata, mt, depoz_dest, val_comanda, obs, furn_prod, cod_agent2, " +
                           " fasonate, name1, felmarfa, masa, tipcamion, tipinc) values ('900', pk_clp.nextval, :codCl, :codAg, :ul, :ulDest, :depart, :status, :nrcmdsap , " +
                           " :datac, :accept1, :status_aprov, :perscont, :tel, :adr, :city, :region, :ketdat, ' ', :tipPlata, :tipTransport, :depozDest, 0, " +
                           " :obs ,' ', :codAgent2,:fasonate, :nume, :felmarfa, :masa, :tipcamion, :tipinc) " +
                           " returning id into :id ";

            transaction = connection.BeginTransaction();
            cmd.Transaction = transaction;

            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Clear();

            cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = antetComanda.codClient;

            cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = codAgent;

            cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[2].Value = filiala;

            cmd.Parameters.Add(":ulDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[3].Value = antetComanda.codFilialaDest;

            cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
            cmd.Parameters[4].Value = articole[0].depart;

            cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[5].Value = "0";

            cmd.Parameters.Add(":nrcmdsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
            cmd.Parameters[6].Value = " ";

            cmd.Parameters.Add(":datac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
            cmd.Parameters[7].Value = getCurrentDate();

            cmd.Parameters.Add(":accept1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            string valSD = "1";
            if (alertSD)
                valSD = "X";
            cmd.Parameters[8].Value = valSD;

            string status_aprov = "1";
            if (alertSD || antetComanda.tipTransport == "TERT" || !Boolean.Parse(antetComanda.cmdFasonate))
                status_aprov = "1";


            cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[9].Value = status_aprov;

            cmd.Parameters.Add(":perscont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
            cmd.Parameters[10].Value = antetComanda.persCont;

            cmd.Parameters.Add(":tel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
            cmd.Parameters[11].Value = antetComanda.telefon;

            cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
            cmd.Parameters[12].Value = antetComanda.strada;

            cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
            cmd.Parameters[13].Value = antetComanda.localitate;

            string varRegion = antetComanda.codJudet;
            if (varRegion.Trim().Equals(""))
                varRegion = " ";
            cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[14].Value = varRegion;

            cmd.Parameters.Add(":ketdat", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
            string[] dataLivareClp = antetComanda.dataLivrare.Split('.');

            cmd.Parameters[15].Value = dataLivareClp[2] + dataLivareClp[1] + dataLivareClp[0];

            cmd.Parameters.Add(":tipPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[16].Value = antetComanda.tipPlata;

            cmd.Parameters.Add(":tipTransport", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[17].Value = antetComanda.tipTransport;

            cmd.Parameters.Add(":depozDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            cmd.Parameters[18].Value = antetComanda.depozDest.Replace("041", "04").Replace("040", "04");

            cmd.Parameters.Add(":codAgent2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[19].Value = antetComanda.selectedAgent;

            cmd.Parameters.Add(":fasonate", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
            string paramFasonate = " ";
            if (antetComanda.cmdFasonate.Equals("true"))
            {
                paramFasonate = "X";
            }
            cmd.Parameters[20].Value = paramFasonate;

            cmd.Parameters.Add(":nume", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
            cmd.Parameters[21].Value = antetComanda.numeClientCV;

            cmd.Parameters.Add(":obs", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
            cmd.Parameters[22].Value = antetComanda.observatiiCLP != null ? antetComanda.observatiiCLP : " ";

            cmd.Parameters.Add(":felmarfa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[23].Value = antetComanda.tipMarfa;

            cmd.Parameters.Add(":masa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[24].Value = antetComanda.masaMarfa;

            cmd.Parameters.Add(":tipcamion", OracleType.VarChar, 45).Direction = ParameterDirection.Input;
            cmd.Parameters[25].Value = antetComanda.tipCamion;

            cmd.Parameters.Add(":tipinc", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[26].Value = antetComanda.tipIncarcare;


            OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
            idCmd.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(idCmd);

            cmd.ExecuteNonQuery();


            int pozArt = 0;

            for (int i = 0; i < articole.Count; i++)
            {

                pozArt = (i + 1) * 10;


                string codArtClp = articole[i].cod;
                if (codArtClp.Length == 8)
                    codArtClp = "0000000000" + codArtClp;


                query = " insert into sapprd.zclpdet(mandt,id,poz,status,cod,cantitate,umb,depoz) " +
                        " values ('900'," + idCmd.Value + ",'" + pozArt + "','0','" + codArtClp + "',:cantArt, " +
                        "'" + articole[i].umBaza + "','" + articole[i].depozit + "' ) ";


                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cantArt", OracleType.Number, 13).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Double.Parse(articole[i].cantitate, CultureInfo.InvariantCulture);

                cmd.ExecuteNonQuery();


            }

            transaction.Commit();


            SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

            webServiceClp = new SapWsClp.ZCLP_WEBSERVICE();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webServiceClp.Credentials = nc;
            webServiceClp.Timeout = 1200000;

            SapWsClp.ZcreazaSto inParam = new SapWsClp.ZcreazaSto();
            inParam.VId = Convert.ToDecimal(idCmd.Value);

            SapWsClp.ZcreazaStoResponse outParam = webServiceClp.ZcreazaSto(inParam);

            string retVal = outParam.VOk.ToString() + " , " + outParam.VMess.ToString();

            webServiceClp.Dispose();


            //nu este nevoie de aprobare, se trimite mail de instiintare
            if (status_aprov.Equals("0"))
            {
                if (!Service1.isClpTransferIntreFiliale(idCmd.Value.ToString()))
                    Service1.sendAlertMailCreareClp(idCmd.Value.ToString());
            }
            //sf. alert


            OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), antetComanda.tonaj);
            new OperatiiSuplimentare().savePrelucrare04(connection, idCmd.Value.ToString(), antetComanda.prelucrare);

            if (codSuperAgent != "")
                OperatiiSuplimentare.saveComandaSuperAv(connection, codSuperAgent, idCmd.Value.ToString());

            return retVal;


        }



        public string saveNewClp(string comanda, string codAgent, string filiala, string depart, bool alertSD, string serData, string codSuperAgent)
        {

            ErrorHandling.sendErrorToMail("saveNewClp:" + comanda + "\n" +  codAgent + "\n" + filiala + "\n" + depart + "\n" + alertSD + "\n" + serData + "\n" + codSuperAgent);

            if (serData == null)
                return saveNewClp_oldversion(comanda, codAgent, filiala, depart, alertSD);
            else
                return saveNewClp_newversion(comanda, codAgent, filiala, depart, alertSD, serData, codSuperAgent);
        }



        public string saveNewClp_newversion(string comanda, string codAgent, string filiala, string depart, bool alertSD, string serData, string codSuperAgent)
        {



            string retVal = "-1";


            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ComandaCreataCLP comandaCLP = serializer.Deserialize<ComandaCreataCLP>(serData);
            List<ArticolComandaCLP> listArticole = serializer.Deserialize<List<ArticolComandaCLP>>(comandaCLP.listArticole);
            AntetComandaCLP antetComanda = serializer.Deserialize<AntetComandaCLP>(comandaCLP.antetComanda);

            if (antetComanda.tipPlata.Equals("E") && (antetComanda.codFilialaDest.StartsWith("BU") || filiala.StartsWith("BU")))
            {
                retVal = "-1, Plata in numerar nu este acceptata.";
                return retVal;    
            }

            OracleConnection connection = new OracleConnection();

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();

            string tempDepart = "";

            List<ArticolComandaCLP> tempList = new List<ArticolComandaCLP>();


            if (antetComanda.selectedAgent.Trim().Length != 0 && antetComanda.selectedAgent.Trim().Length != 1 && antetComanda.selectedAgent.Trim().Length != 8)
            {

                if (antetComanda.selectedAgent.Trim().Equals("Selectati un agent"))
                    antetComanda.selectedAgent = "0";
                else
                    antetComanda.selectedAgent = Utils.getCodAngajat(connection, antetComanda.selectedAgent, filiala);

            }

            for (int i = 0; i < listArticole.Count; i++)
            {

                if (!tempDepart.Equals(listArticole[i].depart) && !tempDepart.Equals(""))
                {
                    retVal = saveCmdClp(connection, antetComanda, tempList, filiala, codAgent, alertSD, codSuperAgent);
                    tempList.Clear();
                }


                ArticolComandaCLP articol = new ArticolComandaCLP();
                articol.cod = listArticole[i].cod;
                articol.cantitate = listArticole[i].cantitate;
                articol.umBaza = listArticole[i].umBaza;
                articol.depozit = listArticole[i].depozit;
                articol.depart = listArticole[i].depart;
                tempList.Add(articol);


                tempDepart = listArticole[i].depart;


            }



            retVal = saveCmdClp(connection, antetComanda, tempList, filiala, codAgent, alertSD, codSuperAgent);

            connection.Close();
            connection.Dispose();


            return retVal;
        }



        public string saveNewClp_oldversion(string comanda, string codAgent, string filiala, string depart, bool alertSD)
        {

            string retVal = "-1";


            OracleConnection connection = new OracleConnection();
            OracleTransaction transaction = null;

            try
            {

                string[] mainTokenClp = comanda.Split('@');
                string[] antetClpToken = mainTokenClp[0].Split('#');

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zclphead(mandt, id, cod_client, cod_agent, ul, ul_dest, depart, status, nrcmdsap, datac, accept1, status_aprov, " +
                               " pers_contact, telefon, adr_livrare, city, region, ketdat, dl, tip_plata, mt, depoz_dest, val_comanda, obs, furn_prod, cod_agent2, " +
                               " fasonate, name1, felmarfa, masa, tipcamion, tipinc) values ('900', pk_clp.nextval, :codCl, :codAg, :ul, :ulDest, :depart, :status, :nrcmdsap , " +
                               " :datac, :accept1, :status_aprov, :perscont, :tel, :adr, :city, :region, :ketdat, ' ', :tipPlata, :tipTransport, :depozDest, 0, " +
                               ":obs ,' ', :codAgent2,:fasonate, :nume, :felmarfa, :masa,:tipcamion,:tipinc) " +
                               " returning id into :id ";

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = antetClpToken[0];

                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = filiala;

                cmd.Parameters.Add(":ulDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = antetClpToken[6];

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = depart;

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "0";

                cmd.Parameters.Add(":nrcmdsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = " ";

                cmd.Parameters.Add(":datac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = getCurrentDate();

                cmd.Parameters.Add(":accept1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                string valSD = "1";
                if (alertSD)
                    valSD = "X";
                cmd.Parameters[8].Value = valSD;

                string status_aprov = "1";
                if (alertSD || antetClpToken[9] == "TERT" || antetClpToken[12] == "false")
                    status_aprov = "1";

                cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = status_aprov;

                cmd.Parameters.Add(":perscont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = antetClpToken[4];

                cmd.Parameters.Add(":tel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = antetClpToken[5];

                cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = antetClpToken[3];

                cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = antetClpToken[2];

                string varRegion = antetClpToken[1];
                if (varRegion.Trim().Equals(""))
                    varRegion = " ";
                cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = varRegion;

                cmd.Parameters.Add(":ketdat", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                string[] dataLivareClp = antetClpToken[7].Split('.');

                cmd.Parameters[15].Value = dataLivareClp[2] + dataLivareClp[1] + dataLivareClp[0];

                cmd.Parameters.Add(":tipPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = antetClpToken[8];

                cmd.Parameters.Add(":tipTransport", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[17].Value = antetClpToken[9];

                cmd.Parameters.Add(":depozDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = antetClpToken[10];

                cmd.Parameters.Add(":codAgent2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[19].Value = antetClpToken[11];

                cmd.Parameters.Add(":fasonate", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                string paramFasonate = " ";
                if (antetClpToken[12] == "true")
                {
                    paramFasonate = "X";
                }
                cmd.Parameters[20].Value = paramFasonate;

                cmd.Parameters.Add(":nume", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[21].Value = antetClpToken[13];

                cmd.Parameters.Add(":obs", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[22].Value = antetClpToken[14].Length > 0 ? antetClpToken[14] : " ";

                cmd.Parameters.Add(":felmarfa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[23].Value = antetClpToken[15];

                cmd.Parameters.Add(":masa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[24].Value = antetClpToken[16];

                cmd.Parameters.Add(":tipcamion", OracleType.VarChar, 45).Direction = ParameterDirection.Input;
                cmd.Parameters[25].Value = antetClpToken[17];

                cmd.Parameters.Add(":tipinc", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[26].Value = antetClpToken[18];


                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();




                //inserare articole comanda
                string[] articoleCmdTokenClp;



                int artLen = mainTokenClp.Length - 2;
                int pozArt = 0;
                string tokenArtClp = "";
                for (int i = 0; i < artLen; i++)
                {

                    pozArt = (i + 1) * 10;


                    tokenArtClp = mainTokenClp[i + 1];
                    articoleCmdTokenClp = tokenArtClp.Split('#');
                    string codArtClp = articoleCmdTokenClp[0];
                    if (codArtClp.Length == 8)
                        codArtClp = "0000000000" + codArtClp;


                    query = " insert into sapprd.zclpdet(mandt,id,poz,status,cod,cantitate,umb,depoz) " +
                            " values ('900'," + idCmd.Value + ",'" + pozArt + "','0','" + codArtClp + "',:cantArt, " +
                            "'" + articoleCmdTokenClp[2] + "','" + articoleCmdTokenClp[3] + "' ) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cantArt", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Double.Parse(articoleCmdTokenClp[1]);

                    cmd.ExecuteNonQuery();


                }

                transaction.Commit();



                SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                webServiceClp = new SapWsClp.ZCLP_WEBSERVICE();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webServiceClp.Credentials = nc;
                webServiceClp.Timeout = 300000;

                SapWsClp.ZcreazaSto inParam = new SapWsClp.ZcreazaSto();
                inParam.VId = Convert.ToDecimal(idCmd.Value);

                SapWsClp.ZcreazaStoResponse outParam = webServiceClp.ZcreazaSto(inParam);

                retVal = outParam.VOk.ToString() + " , " + outParam.VMess.ToString();

                webServiceClp.Dispose();


                //nu este nevoie de aprobare, se trimite mail de instiintare
                if (status_aprov.Equals("0"))
                {
                    if (!Service1.isClpTransferIntreFiliale(idCmd.Value.ToString()))
                        Service1.sendAlertMailCreareClp(idCmd.Value.ToString());
                }
                //sf. alert


                OperatiiSuplimentare.saveTonajComanda(connection, idCmd.Value.ToString(), antetClpToken[19]);

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString() + " " + comanda);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }




            return retVal;



        }




        public string getListArtClpJSON(string nrCmd)
        {


            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            ComandaCLP comandaCLP = new ComandaCLP(); ;
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                //date livrare comanda
                cmd.CommandText = " select pers_contact,telefon,adr_livrare,city,region,decode(ketdat,' ',' ',to_char(to_date(ketdat,'yyyymmdd'))), felmarfa, masa, tipcamion, tipinc, " +
                                  " tip_plata, mt, " +
                                  " nvl((select  k.acc_ofc from sapprd.ekko k where k.mandt = '900' and k.ebeln = nrcmdsap ),' '), " +
                                  " (select count(*) from sapprd.ekbe e where e.mandt = '900' and e.ebeln = nrcmdsap " +
                                  " and ( e.bewtp <> 'L' or (e.bewtp = 'L' and exists (select * from sapprd.vttp p where p.mandt = '900' and p.vbeln = e.belnr)))), status_aprov, " +
                                  " nvl(val_comanda,0), nvl(obs,' '), nvl(val_transp,0), nvl(proc_transp,0),  " +
                                  " nvl((select  k.acc_dz from sapprd.ekko k where k.mandt = '900' and k.ebeln = nrcmdsap ),' ') acc_dz, " +
                                  " nvl((select i.dincarc from sapprd.zcom m, sapprd.zcomdti i where m.mandt = '900' and m.docn = nrcmdsap " +
                                  " and m.mandt = i.mandt and m.nrcom = i.nr  and rownum = 1),' ') data_inc, " +
                                  " nvl((select to_date(o.aedat,'yyyymmdd') || ',' || max(p.plifz) zile_livr " +
                                  " from sapprd.ekko o, sapprd.ekpo p where o.ebeln = nrcmdsap " +
                                  " and o.mandt = '900' and o.mandt = p.mandt and o.ebeln = p.ebeln and p.loekz <> 'L' group by o.aedat ),'-1') max_livrare, " +
                                  " nvl((select z.nrcom nr_ct from sapprd.zcom z where z.mandt = '900' and z.docn = nrcmdsap and rownum = 1),'-1') nrct " +
                                  " from sapprd.zclphead where id=:idcmd ";

                
               

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();

                DateLivrareCLP dateLivrare = new DateLivrareCLP();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    dateLivrare.persContact = oReader.GetString(0);
                    dateLivrare.telefon = oReader.GetString(1);
                    dateLivrare.adrLivrare = oReader.GetString(2);
                    dateLivrare.oras = oReader.GetString(3);
                    dateLivrare.codJudet = oReader.GetString(4);

                    /*
                    if (!oReader.GetString(21).Equals("-1"))
                        dateLivrare.data = GeneralUtils.addDays(oReader.GetString(21).Split(',')[0], Int32.Parse(oReader.GetString(21).Split(',')[1]));
                    else
                        dateLivrare.data = " ";
                    */

                    dateLivrare.data = oReader.GetString(5);

                    dateLivrare.tipMarfa = oReader.GetString(6);
                    dateLivrare.masa = oReader.GetString(7);
                    dateLivrare.tipCamion = oReader.GetString(8);
                    dateLivrare.tipIncarcare = oReader.GetString(9);
                    dateLivrare.tipPlata = oReader.GetString(10);
                    dateLivrare.mijlocTransport = oReader.GetString(11);
                    dateLivrare.aprobatOC = oReader.GetString(12);
                    dateLivrare.deSters = oReader.GetInt32(13).ToString();
                    dateLivrare.statusAprov = oReader.GetString(14);
                    dateLivrare.valComanda = oReader.GetDouble(15).ToString();
                    dateLivrare.obsComanda = oReader.GetString(16);
                    dateLivrare.valTransp = oReader.GetDouble(17).ToString();
                    dateLivrare.procTransp = oReader.GetDouble(18).ToString();
                    dateLivrare.acceptDV = oReader.GetString(19);
                    dateLivrare.dataIncarcare = oReader.GetString(20).Trim();
                    dateLivrare.nrCT = oReader.GetString(22).Equals("-1") ? " " : oReader.GetString(22);
                   


                }

                //sf. date livrare



                //articole comanda
                cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod ,b.nume,a.cantitate,a.umb,a.depoz,a.status from sapprd.zclpdet a, articole b " +
                                  " where a.cod = b.cod and id=:idcmd ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd);

                oReader = cmd.ExecuteReader();

                ArticolCLP articol;
                List<ArticolCLP> listArticole = new List<ArticolCLP>();


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCLP();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.cantitate = oReader.GetDouble(2).ToString();
                        articol.umBaza = oReader.GetString(3);
                        articol.depozit = oReader.GetString(4);
                        articol.status = oReader.GetString(5);
                        listArticole.Add(articol);

                    }
                }


                comandaCLP.dateLivrare = dateLivrare;
                comandaCLP.articole = listArticole;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return GeneralUtils.serializeObject(comandaCLP);

        }


       

        public string getCLPComanda(string dateComanda)
        {
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            List<ClpComanda> listClp = new List<ClpComanda>();

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            DateComanda comanda = serializer.Deserialize<DateComanda>(dateComanda);

            List<string> listArticole = serializer.Deserialize<List<string>>(comanda.listArticole.ToString());

            string strArticole= "";

            foreach (string art in listArticole)
            {
                string lart = art;
                if (art.Length == 8)
                    lart = "0000000000" + art;

                if (strArticole.Length == 0)
                    strArticole = "'" + lart + "'";
                else
                    strArticole += ",'" + lart + "'";

            }

            strArticole = "(" + strArticole + ")";


            

            try
            {
               

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                string sqlString = " select distinct o.ebeln, to_char(to_date(o.aedat,'yyyymmdd'),'DD-MON-YYYY'), decode(o.bsart, 'UB',o.reswk,(select name1 from sapprd.lfa1 l where l.mandt = '900' and l.lifnr = o.lifnr)) Furnizor " + 
                                   " from sapprd.ekko o, sapprd.ekpo p, sapprd.ekbe e where o.mandt = '900' and o.ebeln = p.ebeln " + 
                                   " and p.mandt = e.mandt and p.ebeln = e.ebeln and p.ebelp = e.ebelp and e.bwart = '101' " +
                                   " and o.aedat >=:dataStart and o.COD_CLIENT =:codClient " + 
                                   " and p.matnr in " + strArticole +
                                   " and p.lgort = 'DESC' and p.werks =:filiala " +
                                   " and o.cod_av =:codAgent " +
                                   " and not exists(select * from sapprd.ekbe b where b.mandt = '900' and b.ebeln = p.ebeln and b.ebelp = p.ebelp and b.bwart = '102' " + 
                                   " and b.lfbnr = e.belnr and b.lfpos = e.buzei and b.lfgja = e.gjahr) ";


                string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();


                cmd.Parameters.Add(":dataStart", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = dateInterval;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = comanda.codClient;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = comanda.filiala;

                cmd.Parameters.Add(":codAgent", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = comanda.codAgent;

                oReader = cmd.ExecuteReader();
               

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        ClpComanda clp = new ClpComanda();
                        clp.nrDocument = oReader.GetString(0);
                        clp.data = oReader.GetString(1);
                        clp.tip = oReader.GetString(2);
                        listClp.Add(clp);
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

            return GeneralUtils.serializeObject(listClp);

        }


        public string getListDlExpirate(string filiala, string depart, string tipUser, string codUser)
        {
            string serializedResult = "";


            string sqlString = "";
            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                sqlString = " select  a.id,  b.nume nume_client, to_char(to_date(a.datac,'yyyymmdd')) data_creare, " +
                            " to_char(to_date(a.ketdat, 'yyyymmdd'),'dd.mm.yyyy') data_livrare, " +
                            " a.nrcmdsap, " +
                            " (select name1 from sapprd.lfa1 where mandt = '900' and lifnr = a.lifnr) furnizor " +
                            " from sapprd.zcomhead_tableta a, clienti b where a.status = 2 and a.status_aprov = 2 and " +
                            " a.ketdat = to_char(sysdate,'yyyymmdd') and a.lifnr != ' ' and a.cod_agent =:codAgent and a.cod_client = b.cod " +
                            " and not exists (select 1 from sapprd.zdl_finalizate where mandt='900' and id_comanda = a.id) ";
               

                cmd.CommandText = sqlString;
                cmd.Parameters.Clear();

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codAgent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codUser;

                oReader = cmd.ExecuteReader();

                List<DLExpirat> listaDocumenteDl = new List<DLExpirat>();
                DLExpirat unDocumentDL = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        unDocumentDL = new DLExpirat();
                        unDocumentDL.nrDocument = oReader.GetInt32(0).ToString();
                        unDocumentDL.numeClient = oReader.GetString(1);
                        unDocumentDL.dataDocument = oReader.GetString(2).ToString();
                        unDocumentDL.dataLivrare = oReader.GetString(3).ToString();
                        unDocumentDL.nrDocumentSap = oReader.GetString(4);
                        unDocumentDL.furnizor = oReader.GetString(5);
                        listaDocumenteDl.Add(unDocumentDL);

                    }

                }


                oReader.Close();
                oReader.Dispose();


                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaDocumenteDl);


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


        public string getArticoleDLExpirat(string idComanda)
        {


            List<ArticolCLP> listArticole = new List<ArticolCLP>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod) cod ,b.nume,a.cantitate,a.umb,a.depoz from sapprd.zcomdet_tableta a, articole b " +
                                  " where a.cod = b.cod and id=:idcmd ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                oReader = cmd.ExecuteReader();

                ArticolCLP articol;


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ArticolCLP();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.cantitate = oReader.GetDouble(2).ToString();
                        articol.umBaza = oReader.GetString(3);
                        articol.depozit = oReader.GetString(4);
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
                cmd.Dispose();
                connection.Close();
                connection.Dispose();

            }

            return GeneralUtils.serializeObject(listArticole);

        }


        public string setDLFinalizata(string idComanda, string codAgent)
        {
            string retVal = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " insert into sapprd.zdl_finalizate (mandt, id_comanda, cod_agent, datac) " +
                               " values ('900', :idComanda, :codagent, to_char(sysdate,'yyyymmdd')) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(idComanda);

                cmd.Parameters.Add(":codagent", OracleType.NVarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;

        }



        public string setDLDataLivrare(string idComanda, string dataLivrare)
        {
            string retVal = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                cmd = connection.CreateCommand();

                connection.ConnectionString = connectionString;
                connection.Open();

                string query = " update sapprd.zcomhead_tableta set ketdat=:dataLivrare where id=:idComanda ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":dataLivrare", OracleType.NVarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = dataLivrare;

                cmd.Parameters.Add(":idComanda", OracleType.Number, 11).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = Int32.Parse(idComanda);

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                retVal = "-1";
            }
            finally
            {
                DatabaseConnections.CloseConnections(cmd, connection);
            }

            return retVal;

        }


        private string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }


    }




}