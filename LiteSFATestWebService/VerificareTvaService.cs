﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using LiteSFATestWebService.General;
using System.Web.Script.Serialization;
using System.Data.OracleClient;
using System.Data;


namespace LiteSFATestWebService
{
    public class VerificaTva
    {

        public StarePlatitorTva verificaTVAService(string cuiClient)
        {

            

            string serviceUrl = "https://www.verificaretva.ro/api/apiv5.aspx?key=z1dvZijKepDykHGS&cui=" + cuiClient.Replace("RO","") + "&data=" + AddressUtils.getCurrentDate_YY_MM_DD();

            ErrorHandling.sendErrorToMail(serviceUrl);

            System.Net.WebRequest req = System.Net.WebRequest.Create(serviceUrl);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();
            StarePlatitorTva starePlatitor = new StarePlatitorTva();

            if (jsonResponse != null && !jsonResponse.ToLower().Contains("error") && !jsonResponse.ToLower().Equals("invalid"))
            {
                var serializer = new JavaScriptSerializer();
                starePlatitor = serializer.Deserialize<StarePlatitorTva>(jsonResponse);

                if (!starePlatitor.Raspuns.ToLower().Equals("valid"))
                    starePlatitor.errMessage = jsonResponse;

            }
            else
            {
                starePlatitor.errMessage = jsonResponse;
            }

            return starePlatitor;


        }


        public string isPlatitorTva_service(string cuiClient, string codAgent)
        {
            PlatitorTvaResponse platitorResponse = new PlatitorTvaResponse();

            StarePlatitorTva starePlatitor = verificaTVAService(cuiClient);
            platitorResponse = getPlatitorStatus(starePlatitor);

            OracleConnection connection = null;
            try
            {
                connection = DatabaseConnections.createTESTConnection();
                platitorResponse.diviziiClient = OperatiiClienti.getDiviziiClientCUI(connection, cuiClient, codAgent);
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(platitorResponse);

        }

        public string isPlatitorTva(string cuiClient, string codAgent)
        {

            PlatitorTvaResponse platitorResponse = new PlatitorTvaResponse();
            StarePlatitorTva starePlatitor = null;

            OracleConnection connection = null;
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            try
            {

                connection = DatabaseConnections.createTESTConnection();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select data_update, tva, numefirma, nr_inmatric, judet, localitate, adresa||' '||nr, radiat, numeJudet " + 
                                  " from sapprd.zverifcui where mandt = '900' and (cui=:cui or cui=replace(:cui,'RO',''))  ";

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":cui", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = cuiClient;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();

                    if (oReader.GetString(0).Equals(AddressUtils.getCurrentDate_YYDDMM()))
                    {
                        platitorResponse.numeClient = oReader.GetString(2);
                        platitorResponse.nrInreg = oReader.GetString(3);
                        platitorResponse.codJudet = oReader.GetString(4);

                        if (platitorResponse.codJudet.Equals("/-") || platitorResponse.codJudet.Equals("-"))
                            platitorResponse.codJudet = getCodJudet(oReader.GetString(4), oReader.GetString(oReader.GetOrdinal("numeJudet")));

                        platitorResponse.localitate = oReader.GetString(5);
                        platitorResponse.strada = oReader.GetString(6);
                        platitorResponse.stareInregistrare = oReader.GetString(7).Equals("X") ? "radiere" : " ";

                        if (oReader.GetString(1).Equals("0"))
                            platitorResponse.isPlatitor = false;
                        else
                            platitorResponse.isPlatitor = true;
                    }

                    if (!oReader.GetString(0).Equals(AddressUtils.getCurrentDate_YYDDMM()))
                    {
                        starePlatitor = verificaTVAService(cuiClient);
                        updateTvaInfo(connection, starePlatitor);
                        platitorResponse = getPlatitorStatus(starePlatitor);
                    }

                }
                else
                {
                    starePlatitor = verificaTVAService(cuiClient);
                    platitorResponse = getPlatitorStatus(starePlatitor);
                    insertTvaInfo(connection, starePlatitor);
                }

                platitorResponse.diviziiClient = OperatiiClienti.getDiviziiClientCUI(connection, cuiClient, codAgent);
                platitorResponse.codClientNominal = OperatiiClienti.getCodClientNominal(connection, cuiClient);


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

            return serializer.Serialize(platitorResponse);
        }



        private void updateTvaInfo(OracleConnection connection, StarePlatitorTva starePlatitor)
        {

            if (starePlatitor.CUI == null)
                return;

            OracleCommand cmd = connection.CreateCommand();

            cmd.CommandText = " update sapprd.zverifcui set nr_inmatric=:nrInmatric, judet=:judet, numejudet=:numeJudet, " + 
                              " localitate=:localitate, tip_str=:tipStr, adresa=:adresa, nr=:nr, stare=:stare, tva=:tva, " + 
                              " tva_incs=:tvaIncs, data_tva=:dataTva, data_update=:dataUpdate, radiat=:radiere where cui=:cui ";

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Clear();

            cmd.Parameters.Add(":nrInmatric", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = starePlatitor.NrInmatr;

            cmd.Parameters.Add(":judet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = getCodJudet(starePlatitor.NrInmatr.Trim(), starePlatitor.Judet);

            cmd.Parameters.Add(":numeJudet", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[2].Value = starePlatitor.Judet;

            cmd.Parameters.Add(":localitate", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[3].Value = starePlatitor.Localitate;

            cmd.Parameters.Add(":tipStr", OracleType.VarChar, 21).Direction = ParameterDirection.Input;
            cmd.Parameters[4].Value = starePlatitor.Tip == null || starePlatitor.Tip == "" ? " " : starePlatitor.Tip;

            cmd.Parameters.Add(":adresa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[5].Value = starePlatitor.Adresa == "" ? " " : starePlatitor.Adresa;

            cmd.Parameters.Add(":nr", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[6].Value = starePlatitor.Nr == null || starePlatitor.Nr.Length > 21 || starePlatitor.Nr == "" ? " " : starePlatitor.Nr;

            cmd.Parameters.Add(":stare", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
            cmd.Parameters[7].Value = starePlatitor.Stare == null ? " " : starePlatitor.Stare;

            cmd.Parameters.Add("tva", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[8].Value = starePlatitor.TVA == "False" ? "0" : "1";

            cmd.Parameters.Add(":tvaIncs", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[9].Value = starePlatitor.TVAIncasare == "False" ? "0" : "1";

            cmd.Parameters.Add(":dataTva", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[10].Value = starePlatitor.TVA_data == null || starePlatitor.TVA_data == "" ? " " : formatTvaDate(starePlatitor.TVA_data);

            cmd.Parameters.Add(":dataUpdate", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[11].Value = AddressUtils.getCurrentDate_YYDDMM();

            cmd.Parameters.Add(":radiere", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[12].Value = starePlatitor.StareInregistrare.ToLower().Contains("radiere") ? "X" : " ";

            cmd.Parameters.Add(":cui", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[13].Value = starePlatitor.CUI;

            cmd.ExecuteNonQuery();

            cmd.Dispose();

        }




        private void insertTvaInfo(OracleConnection connection, StarePlatitorTva starePlatitor)
        {

            if (starePlatitor.CUI == null)
                return;

            OracleCommand cmd = connection.CreateCommand();

            cmd.CommandText = " insert into sapprd.zverifcui (mandt, cui_sap, cui, numefirma, nr_inmatric, judet, numejudet, localitate, tip_str, adresa, " +
                              " nr, stare, data_update, tva, tva_incs, data_tva, data_salv, radiat ) values  " +
                              " ('900', :cui_sap, :cui, :numefirma, :nr_inmatric, :judet, :numejudet, :localitate, :tip_str, :adresa, " +
                              " :nr, :stare, :data_update, :tva, :tva_incs, :data_tva, :data_salv, :radiat )";

            cmd.Parameters.Add(":cui_sap", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = starePlatitor.CUI;

            cmd.Parameters.Add(":cui", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = starePlatitor.CUI;

            cmd.Parameters.Add(":numefirma", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
            cmd.Parameters[2].Value = Utils.removeDiacritics(starePlatitor.Nume);

            cmd.Parameters.Add(":nr_inmatric", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
            cmd.Parameters[3].Value = starePlatitor.NrInmatr;

            cmd.Parameters.Add(":judet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[4].Value = getCodJudet(starePlatitor.NrInmatr.Trim(),starePlatitor.Judet); 

            cmd.Parameters.Add(":numeJudet", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[5].Value = starePlatitor.Judet;

            cmd.Parameters.Add(":localitate", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[6].Value = starePlatitor.Localitate;

            cmd.Parameters.Add(":tip_str", OracleType.VarChar, 21).Direction = ParameterDirection.Input;
            cmd.Parameters[7].Value = starePlatitor.Tip == "" ? " " : starePlatitor.Tip;

            cmd.Parameters.Add(":adresa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[8].Value = starePlatitor.Adresa == "" ? " " : starePlatitor.Adresa;

            cmd.Parameters.Add(":nr", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[9].Value = starePlatitor.Nr == null || starePlatitor.Nr == "" ? " " : starePlatitor.Nr;

            cmd.Parameters.Add(":stare", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
            cmd.Parameters[10].Value = starePlatitor.Stare == null ? " " : starePlatitor.Stare;

            cmd.Parameters.Add(":data_update", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[11].Value = AddressUtils.getCurrentDate_YYDDMM();

            cmd.Parameters.Add("tva", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[12].Value = starePlatitor.TVA == "False" ? "0" : "1";

            cmd.Parameters.Add(":tva_incs", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[13].Value = starePlatitor.TVAIncasare == "False" ? "0" : "1";

            cmd.Parameters.Add(":data_tva", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[14].Value = starePlatitor.TVA_data == null || starePlatitor.TVA_data == "" ? " " : formatTvaDate(starePlatitor.TVA_data);

            cmd.Parameters.Add(":data_salv", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[15].Value = AddressUtils.getCurrentDate_YYMMDD();

            cmd.Parameters.Add(":radiat", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[16].Value = starePlatitor.StareInregistrare.ToLower().Contains("radiere") ? "X" : " ";

            cmd.ExecuteNonQuery();

            cmd.Dispose();

        }


        private PlatitorTvaResponse getPlatitorStatus(StarePlatitorTva platitorTva)
        {

            PlatitorTvaResponse tvaResponse = new PlatitorTvaResponse();

            if (platitorTva.Raspuns != null && platitorTva.Raspuns.ToLower().Contains("valid"))
            {
                tvaResponse.isPlatitor = platitorTva.TVA.Equals("0") || platitorTva.TVA.ToLower().Equals("false") ? false : true;
                tvaResponse.numeClient = platitorTva.Nume;
                tvaResponse.nrInreg = platitorTva.NrInmatr;
                tvaResponse.codJudet = getCodJudet(platitorTva.NrInmatr.Trim(), platitorTva.Judet);
                tvaResponse.localitate = platitorTva.Localitate;
                tvaResponse.strada = platitorTva.Adresa + " " + platitorTva.Nr;
                tvaResponse.stareInregistrare = platitorTva.StareInregistrare;
            }
            else
            {
                tvaResponse.isPlatitor = false;
                tvaResponse.errMessage = platitorTva.errMessage;
            }

            return tvaResponse;

        }

        private string getCodJudet(string nrInmatr, string numeJudet)
        {
            string codJudet = "00";

            if (nrInmatr != null && nrInmatr.Length > 3)
                codJudet = nrInmatr.Substring(1, 2);

            if (codJudet.Equals("00") || codJudet.Equals("/-"))
                codJudet = getCodJudetDenumire(numeJudet);

            return codJudet;
        }


        private string getCodJudetDenumire(string numeJudet)
        {

            string codJudet = "00";

            if (numeJudet.ToUpper().Contains("ALBA"))
                codJudet = "01";
            else if (numeJudet.ToUpper().Contains("ARAD"))
                codJudet = "02";
            else if (numeJudet.ToUpper().Contains("ARGES"))
                codJudet = "03";
            else if (numeJudet.ToUpper().Contains("BACAU"))
                codJudet = "04";
            else if (numeJudet.ToUpper().Contains("BIHOR"))
                codJudet = "05";
            else if (numeJudet.ToUpper().Contains("BISTRITA"))
                codJudet = "06";
            else if (numeJudet.ToUpper().Contains("BOTOSANI"))
                codJudet = "07";
            else if (numeJudet.ToUpper().Contains("BRAILA"))
                codJudet = "09";
            else if (numeJudet.ToUpper().Contains("BRASOV"))
                codJudet = "08";
            else if (numeJudet.ToUpper().Contains("BUCURESTI"))
                codJudet = "40";
            else if (numeJudet.ToUpper().Contains("BUZAU"))
                codJudet = "10";
            else if (numeJudet.ToUpper().Contains("CALARASI"))
                codJudet = "51";
            else if (numeJudet.ToUpper().Contains("SEVERIN"))
                codJudet = "11";
            else if (numeJudet.ToUpper().Contains("CLUJ"))
                codJudet = "12";
            else if (numeJudet.ToUpper().Contains("CONSTANTA"))
                codJudet = "13";
            else if (numeJudet.ToUpper().Contains("COVASNA"))
                codJudet = "14";
            else if (numeJudet.ToUpper().Contains("DAMBOVITA"))
                codJudet = "15";
            else if (numeJudet.ToUpper().Contains("DOLJ"))
                codJudet = "16";
            else if (numeJudet.ToUpper().Contains("GALATI"))
                codJudet = "17";
            else if (numeJudet.ToUpper().Contains("GIURGIU"))
                codJudet = "52";
            else if (numeJudet.ToUpper().Contains("GORJ"))
                codJudet = "18";
            else if (numeJudet.ToUpper().Contains("HARGHITA"))
                codJudet = "19";
            else if (numeJudet.ToUpper().Contains("HUNEDOARA"))
                codJudet = "20";
            else if (numeJudet.ToUpper().Contains("IALOMITA"))
                codJudet = "21";
            else if (numeJudet.ToUpper().Contains("IASI"))
                codJudet = "22";
            else if (numeJudet.ToUpper().Contains("ILFOV"))
                codJudet = "23";
            else if (numeJudet.ToUpper().Contains("MARAMURES"))
                codJudet = "24";
            else if (numeJudet.ToUpper().Contains("MEHEDINTI"))
                codJudet = "25";
            else if (numeJudet.ToUpper().Contains("MURES"))
                codJudet = "26";
            else if (numeJudet.ToUpper().Contains("NEAMT"))
                codJudet = "27";
            else if (numeJudet.ToUpper().Contains("OLT"))
                codJudet = "28";
            else if (numeJudet.ToUpper().Contains("PRAHOVA"))
                codJudet = "29";
            else if (numeJudet.ToUpper().Contains("SALAJ"))
                codJudet = "31";
            else if (numeJudet.ToUpper().Contains("SATU"))
                codJudet = "30";
            else if (numeJudet.ToUpper().Contains("SIBIU"))
                codJudet = "32";
            else if (numeJudet.ToUpper().Contains("SUCEAVA"))
                codJudet = "33";
            else if (numeJudet.ToUpper().Contains("TELEORMAN"))
                codJudet = "34";
            else if (numeJudet.ToUpper().Contains("TIMIS"))
                codJudet = "35";
            else if (numeJudet.ToUpper().Contains("TULCEA"))
                codJudet = "36";
            else if (numeJudet.ToUpper().Contains("VALCEA"))
                codJudet = "38";
            else if (numeJudet.ToUpper().Contains("VASLUI"))
                codJudet = "37";
            else if (numeJudet.ToUpper().Contains("VRANCEA"))
                codJudet = "39";

            return codJudet;
        }

        private string formatTvaDate(string tvaDate)
        {
            string localDate = tvaDate;

            if (tvaDate!= null && tvaDate.Contains("/"))
            {
                string[] tokDate = tvaDate.Split('/');
                localDate = tokDate[0] + tokDate[2] + tokDate[1];
            }

            return localDate;
        }

        public string testAnafService()
        {
            List<AnafData> listAnaf = new List<AnafData>();

            AnafData anaf = new AnafData();
            anaf.cui = "31157715";
            anaf.data = "2024-03-11";

            listAnaf.Add(anaf);

            anaf = new AnafData();
            anaf.cui = "14883114";
            anaf.data = "2024-03-11";

            listAnaf.Add(anaf);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://webservicesp.anaf.ro/PlatitorTvaRest/api/v8/ws/tva");

            string jsonData = new JavaScriptSerializer().Serialize(listAnaf);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = jsonData.Length;

            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(jsonData);
            }

            System.Net.WebResponse response = request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());

            string deliveryResponse = sr.ReadToEnd().Trim();

            return deliveryResponse;
        }

        class AnafData
        {
            public string cui;
            public string data;
        }

    }
}