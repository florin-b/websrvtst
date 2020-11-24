
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace LiteSFATestWebService
{


      


    public class OperatiiMathaus
    {



        public string getListaCategorii()
        {
            List<CategorieMathaus> listCategorii = new List<CategorieMathaus>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod, nume, cod_hybris, nvl(cod_parinte,'') from sapprd.zcatmathaus order by cod ";
                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        CategorieMathaus cat = new CategorieMathaus();
                        cat.cod = oReader.GetString(0);
                        cat.nume = oReader.GetString(1);
                        cat.codHybris = oReader.GetString(2);
                        cat.codParinte = oReader.GetString(3);
                        listCategorii.Add(cat);

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

            return new JavaScriptSerializer().Serialize(listCategorii);
        }



        public string getMagazinMathaus(string filiala)
        {

            string mMathaus = filiala;

            string serviceUrl = "https://pcm.arabesque.ro/ws410/rest/arabesqueplants/" + filiala  + "/arabesquezones/Z" + filiala;

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);

            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("wsorganizatie" + ":" + "uGwxpmxRK7e6k5fh"));
            request.Headers.Add("Authorization", "Basic " + encoded);

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string xmlResponse = sr.ReadToEnd().Trim();

            XDocument xdoc = XDocument.Parse(xmlResponse);
            XElement result = xdoc.Element("arabesquezone").Element("associatedPlants");
            XElement plant = result.Element("arabesquePlant");

            if (plant != null)
            {
                XAttribute plantName = plant.Attribute("name");
                mMathaus = plantName.Value.ToString();
            }

            return mMathaus;

        }



        public string getArticoleCategorie(string codCategorie, string filiala)
        {

            string serviceUrl = "https://idx1.arabesque.ro/solr/master_erp_Product_default/select?q=categoryCode_string_mv:" + codCategorie;

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.WebRequest request = System.Net.WebRequest.Create(serviceUrl);

            CredentialCache credential = new CredentialCache();
            credential.Add(new System.Uri(serviceUrl), "Basic", new System.Net.NetworkCredential("erpClient", "S3EjkNEm"));
            request.Credentials = credential;

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();
            

            int startResponse = jsonResponse.IndexOf("docs\":[") + 7;

            jsonResponse = jsonResponse.Substring(startResponse, jsonResponse.Length - startResponse - 1);

            string[] articole = Regex.Split(jsonResponse, "\"id\":");

            foreach (string art in articole)
            {

                string[] artData = Regex.Split(art, "\",");
                ArticolMathaus articol = new ArticolMathaus();

                foreach (string data in artData)
                {

                    if (data.Contains("code_string"))
                    {
                        articol.cod = data.Split(':')[1].Replace("\"", "");
                    }

                    if (data.Contains("name_text_ro"))
                    {
                        articol.nume = data.Split(':')[1].Replace("\"", "").ToUpper();
                    }

                    if (data.Contains("image_m_string"))
                    {
                        articol.adresaImg = "https" +  Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("image_l_string"))
                    {
                        articol.adresaImgMare = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("description_text_ro"))
                    {
                        articol.descriere = Regex.Replace(Regex.Split(data.Trim(), "\":\"")[1].Replace("\"", "").Replace("\\n", " ").Replace("\\t", " ").Replace("&nbsp;", " "), "<.*?>", String.Empty) ;
                    }

                }
                if (articol.nume != null)
                {
                    if (articol.descriere == null)
                        articol.descriere = " ";

                    listArticole.Add(articol);
                }

            }

            addExtraData(listArticole, filiala);

            return new JavaScriptSerializer().Serialize(listArticole);
        }



        private void addExtraData(List<ArticolMathaus> listArticole, string filiala)
        {

            string listCodArt = "";
            string filialaGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);

            string magMathaus = getMagazinMathaus(filiala);


            List<ArticolMathaus> eliminate = new List<ArticolMathaus>();

            foreach (ArticolMathaus articol in listArticole){
                if (listCodArt.Equals(""))
                    listCodArt = "'" + articol.cod + "'";
                else
                    listCodArt += ",'" + articol.cod + "'";

            }

            listCodArt = "(" + listCodArt + ")";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {

                cmd.CommandText = " select distinct a.cod, a.sintetic, b.cod_nivel1, a.umvanz10, a.umvanz, nvl(a.tip_mat, ' '),  b.cod nume_sint, " +
                                  " decode(a.grup_vz, ' ', '-1', a.grup_vz), decode(trim(a.dep_aprobare), '', '00', a.dep_aprobare)  dep_aprobare, " +
                                  " (select nvl((select 1 from sapprd.marm m where m.mandt = '900' " +
                                  " and m.matnr = a.cod and m.meinh = 'EPA'),-1) palet from dual) palet  , -1 stoc , categ_mat, " +
                                  " lungime, a.s_indicator from articole a, sintetice b, sapprd.marc c   where c.mandt = '900' and c.matnr = a.cod " +
                                  " and c.werks = :filiala and c.mmsta <> '01'  and a.sintetic = b.cod and a.cod != 'MAT GENERIC PROD' " +
                                  " and a.blocat <> '01' and a.cod in " + listCodArt + "   ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = filialaGed;


                oReader = cmd.ExecuteReader();
                string strCat;


                int ii = 0;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        foreach (ArticolMathaus articol in listArticole)
                        {
                            if (oReader.GetString(0).Equals(articol.cod))
                            {

                                string codArtBrut = articol.cod;
                                articol.cod = articol.cod.TrimStart('0');
                                articol.sintetic = oReader.GetString(1);
                                articol.nivel1 = oReader.GetString(2);
                                articol.umVanz10 = oReader.GetString(3);
                                articol.umVanz = oReader.GetString(7).Substring(0, 2).Equals("11") ? oReader.GetString(4) : oReader.GetString(3);
                                articol.tipAB = oReader.GetString(5);
                                articol.depart = oReader.GetString(7);
                                articol.departAprob = oReader.GetString(8);
                                articol.umPalet = oReader.GetInt32(9).ToString();
                                articol.stoc = oReader.GetDouble(10).ToString();

                                strCat = oReader.GetString(11);
                                if (strCat.ToUpper().Equals("AM") || strCat.ToUpper().Equals("PA"))
                                    strCat = "AM";
                                else
                                    strCat = " ";

                                articol.categorie = strCat;
                                articol.lungime = oReader.GetDouble(12).ToString();
                                articol.catMathaus = oReader.GetString(13).Equals("Y") ? "S" : " ";
                                articol.pretUnitar = "-1";
                                //articol.pretUnitar = getPret("4110010417", codArtBrut, "1","11", articol.umVanz, "GL20","SD", "MAV1", "00083206", "10", "GL10", "") ;

                                if (!magMathaus.Equals(filiala) && oReader.GetString(13).Equals("N"))
                                    eliminate.Add(articol);

                                break;

                            }

                            ii++;
                        }
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

            listArticole.RemoveAll(x => eliminate.Contains(x));

        }



        private byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }



        public string getPret(string client, string articol, string cantitate, string depart, string um, string ul, string tipUser, string depoz, string codUser, string canalDistrib, string filialaAlternativa, string filialaClp)
        {

            string retVal = "";
            SAPWebServicesPRD.ZTBL_WEBSERVICE webService = null;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string tipUserLocal;

            if (tipUser == null || (tipUser != null && tipUser.Trim().Length == 0))
            {
                tipUserLocal = Service1.getTipUser(codUser);
            }
            else
            {
                tipUserLocal = tipUser;
            }


            try
            {

                webService = new SAPWebServicesPRD.ZTBL_WEBSERVICE();
                SAPWebServicesPRD.ZgetPrice inParam = new SAPWebServicesPRD.ZgetPrice();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());

                webService.Credentials = nc;
                webService.Timeout = 300000;

                inParam.GvKunnr = client;
                inParam.GvMatnr = articol;
                inParam.GvSpart = depart;
                inParam.GvVrkme = um;
                inParam.GvWerks = ul;
                inParam.GvLgort = depoz;
                inParam.GvCant = Decimal.Parse(cantitate);
                inParam.GvCantSpecified = true;
                inParam.GvSite = " ";
                inParam.TipPers = tipUserLocal;
                inParam.Canal = canalDistrib;
                inParam.UlStoc = filialaClp != null ? filialaClp : " ";


                SAPWebServicesPRD.ZgetPriceResponse outParam = webService.ZgetPrice(inParam);

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
                string impachetare = outParam.Impachet.ToString() != "" ? outParam.Impachet.ToString() : " ";
                string pretGed = outParam.GvNetwrFtva.ToString();


                string extindere11 = outParam.ErrorCode.ToString();


                if (depart.Equals("11") && extindere11.Equals("1"))
                {
                    if (Service1.extindeClient(client).Equals("0"))
                    {
                        return getPret(client, articol, cantitate, depart, um, ul, tipUserLocal, depoz, codUser, canalDistrib, filialaAlternativa, filialaClp);
                    }
                    else
                    {
                        return "-1";
                    }
                }


                //---verificare cmp



                string filialaCmp = filialaAlternativa;

                if (depart.Equals("11"))
                {
                    if (filialaAlternativa.Equals("BV90"))
                        filialaCmp = "BV92";
                    else
                        filialaCmp = filialaAlternativa.Substring(0, 2) + "2" + filialaAlternativa.Substring(3, 1);
                }

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();


                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'999999.9999'),0) from sapprd.mbew y where " +
                                  " y.mandt='900' and y.matnr=:matnr  and y.bwkey = :unitLog ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":matnr", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":unitLog", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;



                oReader = cmd.ExecuteReader();
                double cmpArticol = 0;
                double procRedCmp = 0;
                if (oReader.HasRows)
                {
                    oReader.Read();
                    cmpArticol = Convert.ToDouble(oReader.GetString(0));
                    procRedCmp = 100;

                    cmpArticol = cmpArticol * (100 - procRedCmp) / 100;
                }

                //---sf. verificare cmp


                retVal = cantOut + "#" + pretOut + "#" + umOut + "#" + noDiscOut + "#" + codArtPromo + "#" +
                         cantArtPromo + "#" + pretArtPromo + "#" + umArtPromo + "#" + pretLista + "#";



                //descriere conditii pret
                string[] codReduceri = condPret.Split(';');
                string[] tokCod;



                condPret = "";
                for (int jj = 0; jj < codReduceri.Length; jj++)
                {

                    tokCod = codReduceri[jj].Split(':');


                    cmd = connection.CreateCommand();

                    //stoc la zi
                    cmd.CommandText = " SELECT vtext FROM SAPPRD.T685t r where mandt = '900' and spras = '4' " +
                                      " and r.kvewe = 'A' and r.kappl = 'V' and KSCHL=:codRed ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":codRed", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = tokCod[0];

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        condPret += oReader.GetString(0) + ":" + tokCod[1] + ";";
                    }
                    else
                    {
                        condPret += "Taxa verde:0,00;Pret net:0,00;TVA încasat:0,00;";
                    }


                }//sf. for

                retVal += condPret + "#";

                oReader.Close();
                oReader.Dispose();


                //

                //discounturi maxime
                string discMaxAV = "0", discMaxSD = "0", discMaxDV = "0", discMaxKA = "0";

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct nvl(a.discount,0) av, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='SD' and spart =:depart and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) sd, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='DV' and spart =:depart and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) dv, " +
                                  " nvl((select distinct discount from sapprd.zdisc_pers_sint where  functie='KA' and werks =:filiala and inactiv <> 'X' and matkl = c.cod),0) ka " +
                                  " from sapprd.zdisc_pers_sint a, articole b, sintetice c where  a.functie='AV' and a.spart =:depart and a.werks =:filiala " +
                                  " and b.sintetic = c.cod and inactiv <> 'X' and matkl = c.cod and b.cod =:cod ";




                cmd.Parameters.Clear();
                cmd.Parameters.Add(":cod", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = articol;

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = ul;

                cmd.Parameters.Add(":depart", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = depart;




                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    discMaxAV = oReader.GetDouble(0).ToString();
                    discMaxSD = oReader.GetDouble(1).ToString();
                    discMaxDV = oReader.GetDouble(2).ToString();
                    discMaxKA = oReader.GetDouble(3).ToString();
                }




                //sf. disc.

                //KA - la preturile promotionale nu se mai aplica alte discounturi
                if (noDiscOut.Equals("X"))
                {
                    discMaxKA = "0";
                }




                //pret mediu oras
                string pretMediu = "0";

                if (tipUserLocal.Equals("KA"))
                {
                    cmd.CommandText = " select pret_med, adaos_med, cant from sapprd.zpret_mediu_oras where matnr =:articol and pdl=:ul ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = articol;

                    cmd.Parameters.Add(":ul", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = ul;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        double dblPretMediu = oReader.GetDouble(0) / oReader.GetDouble(2) + (oReader.GetDouble(1) / oReader.GetDouble(2)) * 0.15;
                        pretMediu = dblPretMediu.ToString();

                    }

                }

                string istoricPret = " ";

               // if (canalDistrib.Equals("10"))
               //     istoricPret = getIstoricPret(connection, articol, client);


                retVal += discMaxAV + "#" + discMaxSD + "#" + discMaxDV + "#" +
                         Convert.ToInt32(Double.Parse(multiplu)).ToString() + "#" +
                         cantUmb + "#" + Umb + "#" + discMaxKA + "#" + cmpArticol.ToString() + "#" + pretMediu + "#" + impachetare + "#" + istoricPret + "#" + procRedCmp + "#" + pretGed + "#";

                

                if (pretOut.Equals("0.0"))
                    retVal = "-1";




            }
            catch (Exception ex)
            {
                string context = client + "\n " + articol + "\n " + cantitate + "\n " + depart + "\n " + um + "\n " + ul + "\n " + tipUser + "\n " + depoz + "\n " + codUser + "\n " + canalDistrib + "\n " + filialaAlternativa;
                ErrorHandling.sendErrorToMail(ex.ToString() + " context: " + context);
                retVal = "-1";
            }
            finally
            {
                webService.Dispose();
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            if (retVal.Contains("#"))
                return ((Double.Parse(retVal.Split('#')[1]) / Double.Parse(retVal.Split('#')[14])) * Double.Parse(retVal.Split('#')[13])).ToString() + " LEI";
            else
                return retVal;

        }

    }
}