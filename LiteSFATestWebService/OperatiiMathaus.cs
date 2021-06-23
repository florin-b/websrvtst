
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

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select cod, nume, cod_hybris, nvl(cod_parinte,'') from sapprd.zcatmathaus_b order by cod ";
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



        public string getArticoleCategorie(string codCategorie, string filiala, string depart)
        {

            List<ArticolMathaus> listArticole;

            if (codCategorie.Equals("1"))
                listArticole = getArticoleLocal(depart);
            else
                listArticole = getArticoleWebService(codCategorie, depart, "");


            if (listArticole.Count > 0)
                addExtraData(listArticole, filiala);

            return new JavaScriptSerializer().Serialize(listArticole);
        }

        private List<ArticolMathaus> getArticoleLocal(string depart)
        {
            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            connection.ConnectionString = connectionString;
            connection.Open();

            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select cod, nume from articole where grup_vz=:depart and rownum<20 ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = depart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        ArticolMathaus articol = new ArticolMathaus();
                        articol.cod = oReader.GetString(0);
                        articol.nume = oReader.GetString(1);
                        articol.isLocal = true;
                        setDetaliiArticol(articol);
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


            return listArticole;
        }


        private List<ArticolMathaus> getArticoleWebService(string codCategorie, string depart, string urlService)
        {

            List<ArticolMathaus> listArticole = new List<ArticolMathaus>();

            string serviceUrl = "https://idx.arabesque.ro/solr/master_erp_Product_default/select?q=categoryCode_string_mv:" + codCategorie;

            if (urlService != null && !urlService.Equals(""))
                serviceUrl = urlService;

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.WebRequest request = System.Net.WebRequest.Create(serviceUrl);

            CredentialCache credential = new CredentialCache();
            credential.Add(new System.Uri(serviceUrl), "Basic", new System.Net.NetworkCredential("erpClient", "S3EjkNEm"));
            request.Credentials = credential;

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();

            int startResponse = jsonResponse.IndexOf("docs\":[") + 7;

            jsonResponse = jsonResponse.Substring(startResponse, jsonResponse.Length - startResponse - 1);

            string[] articole = Regex.Split(jsonResponse, "\"id\":");
            string divizieArt = "";

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
                        articol.adresaImg = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("image_l_string"))
                    {
                        articol.adresaImgMare = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("description_text_ro"))
                    {
                        articol.descriere = Regex.Replace(Regex.Split(data.Trim(), "\":\"")[1].Replace("\"", "").Replace("\\n", " ").Replace("\\t", " ").Replace("&nbsp;", " "), "<.*?>", String.Empty);
                    }

                    if (data.Contains("divizie_string"))
                    {
                        divizieArt = data.Split(':')[1].Replace("\"", "");
                    }

                }
                if (articol.nume != null && (divizieArt.Equals(depart) || divizieArt.Equals("11")))
                {
                    if (articol.descriere == null)
                        articol.descriere = " ";

                    articol.isLocal = false;
                    listArticole.Add(articol);
                }

            }

            return listArticole;
        }

        public string cautaArticoleMathaus(string codArticol, string tipCautare, string filiala, string depart) 
        {
            string serviceUrlCod = "https://idx.arabesque.ro/solr/master_erp_Product_default/select?q=code_string:";
            string serviceUrlNume = "https://idx.arabesque.ro/solr/master_erp_Product_default/select?q=name_text_ro:";
            string serviceUrl;
            List<ArticolMathaus> listArticole;

            if (tipCautare.Equals("c"))
                serviceUrl = serviceUrlCod + codArticol + "*";
            else
                serviceUrl = serviceUrlNume + codArticol;

            listArticole = getArticoleWebService("", depart, serviceUrl);

            if (listArticole.Count > 0)
                    addExtraData(listArticole, filiala);

            return new JavaScriptSerializer().Serialize(listArticole);

        }


        private void setDetaliiArticol(ArticolMathaus articol)
        {

            string serviceUrl = "https://wse1-sap-prod.arabesque.ro/solr/master_erp_Product_default/select?q=code_string:" + articol.cod;

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.WebRequest request = System.Net.WebRequest.Create(serviceUrl);

            CredentialCache credential = new CredentialCache();
            credential.Add(new System.Uri(serviceUrl), "Basic", new System.Net.NetworkCredential("erpClient", "S3EjkNEm"));
            request.Credentials = credential;

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();

            int startResponse = jsonResponse.IndexOf("docs\":[") + 7;

            jsonResponse = jsonResponse.Substring(startResponse, jsonResponse.Length - startResponse - 1);

            string[] articole = Regex.Split(jsonResponse, "\"id\":");

            foreach (string art in articole)
            {

                string[] artData = Regex.Split(art, "\",");

                foreach (string data in artData)
                {

                    if (data.Contains("image_m_string"))
                    {
                        articol.adresaImg = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("image_l_string"))
                    {
                        articol.adresaImgMare = "https" + Regex.Split(data, "https")[1].Replace("\"", "");
                    }

                    if (data.Contains("description_text_ro"))
                    {
                        articol.descriere = Regex.Replace(Regex.Split(data.Trim(), "\":\"")[1].Replace("\"", "").Replace("\\n", " ").Replace("\\t", " ").Replace("&nbsp;", " "), "<.*?>", String.Empty);
                    }

                }
            }

        }


        private void addExtraData(List<ArticolMathaus> listArticole, string filiala)
        {

            string listCodArt = "";
            string filialaGed = filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);

            string magMathaus = filiala;

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
                                  " (select nvl((select 1 from sapprd.mara m where m.mandt = '900' and m.matnr = a.cod and m.categ_mat in ('PA','AM')),-1) " + 
                                  " palet from dual) palet  , -1 stoc , categ_mat, " +
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
                                articol.pretUnitar = " ";

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


      


        public string getLivrariComanda(string strComanda)
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            ComandaMathaus comandaMathaus = serializer.Deserialize<ComandaMathaus>(strComanda);
            List<DateArticolMathaus> articole = comandaMathaus.deliveryEntryDataList;

            ComandaMathaus comanda = new ComandaMathaus();
            comanda.sellingPlant = comandaMathaus.sellingPlant;
            List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

            foreach (DateArticolMathaus dateArticol in articole)
            {
                DateArticolMathaus articol = new DateArticolMathaus();
                articol.productCode = "0000000000" + dateArticol.productCode;
                articol.quantity = dateArticol.quantity;
                articol.unit = dateArticol.unit;
                deliveryEntryDataList.Add(articol);

            }

            comanda.deliveryEntryDataList = deliveryEntryDataList;
            return callDeliveryService(serializer.Serialize(comanda));

        }


        private string callDeliveryService(string jsonData)
        {

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://wt1.arabesque.ro/arbsqintegration/optimiseDelivery");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = jsonData.Length;

            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("arbsqservice" + ":" + "arbsqservice"));
            request.Headers.Add("Authorization", "Basic " + encoded);

            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(jsonData);
            }

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            return sr.ReadToEnd().Trim();

        }

        public string getStocMathaus(string filiala, string codArticol, string um)
        {
            StockMathaus stockMathaus = new StockMathaus();

            stockMathaus.plant = filiala;
            List<StockEntryDataList> stockEntryDataList = new List<StockEntryDataList>();

            StockEntryDataList stockEntry = new StockEntryDataList();
            stockEntry.productCode = "0000000000" + codArticol;
            stockEntry.warehouse = "";
            stockEntry.availableQuantity = 0;
            stockEntryDataList.Add(stockEntry);
            stockMathaus.stockEntryDataList = stockEntryDataList;

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            StockMathaus stockResponse = serializer.Deserialize<StockMathaus>(callStockService(serializer.Serialize(stockMathaus)));

            ComandaMathaus comandaMathaus = new ComandaMathaus();
            comandaMathaus.sellingPlant = stockResponse.plant;

            List<DateArticolMathaus> deliveryEntryDataList = new List<DateArticolMathaus>();

            DateArticolMathaus dateArticol = new DateArticolMathaus();
            dateArticol.deliveryWarehouse = stockResponse.stockEntryDataList[0].warehouse;
            dateArticol.quantity = stockResponse.stockEntryDataList[0].availableQuantity;
            dateArticol.productCode = stockResponse.stockEntryDataList[0].productCode;
            dateArticol.unit = um;
            deliveryEntryDataList.Add(dateArticol);
            comandaMathaus.deliveryEntryDataList = deliveryEntryDataList;

            return serializer.Serialize(comandaMathaus);

        }

        private string callStockService(string jsonData)
        {

            System.Net.ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://wt1.arabesque.ro/arbsqintegration/getStocks");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = jsonData.Length;

            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("arbsqservice" + ":" + "arbsqservice"));
            request.Headers.Add("Authorization", "Basic " + encoded);

            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(jsonData);
            }

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            return sr.ReadToEnd().Trim();

        }

    }
}