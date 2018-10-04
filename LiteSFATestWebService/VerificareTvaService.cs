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

            string serviceUrl = "http://www.verificaretva.ro/api/apiv4.aspx?key=z1dvZijKepDykHGS&cui=" + cuiClient + "&data=" + AddressUtils.getCurrentDate_YY_MM_DD();

            System.Net.WebRequest req = System.Net.WebRequest.Create(serviceUrl);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            string jsonResponse = sr.ReadToEnd().Trim();
            StarePlatitorTva starePlatitor = new StarePlatitorTva();

            if (jsonResponse != null && !jsonResponse.ToLower().Contains("error"))
            {
                var serializer = new JavaScriptSerializer();
                starePlatitor = serializer.Deserialize<StarePlatitorTva>(jsonResponse);
            }
            else
            {
                starePlatitor.errMessage = jsonResponse;
            }

            return starePlatitor;


        }


        public string isPlatitorTva(string cuiClient)
        {

            OracleConnection connection = null;
            OracleDataReader oReader = null;
            OracleCommand cmd = null;

            PlatitorTvaResponse platitorResponse = new PlatitorTvaResponse();

            try
            {

                connection = DatabaseConnections.createTESTConnection();
                cmd = connection.CreateCommand();

                cmd.CommandText = " select data_update, tva, numefirma, nr_inmatric from sapprd.zverifcui where mandt = '900' and cui=:cui ";

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

                        if (oReader.GetString(1).Equals("0"))
                            platitorResponse.isPlatitor = false;
                        else
                            platitorResponse.isPlatitor = true;
                    }

                    if (!oReader.GetString(0).Equals(AddressUtils.getCurrentDate_YYDDMM()))
                    {
                        StarePlatitorTva starePlatitor = verificaTVAService(cuiClient);
                        updateTvaInfo(connection, starePlatitor);
                        platitorResponse = getPlatitorStatus(starePlatitor);

                    }

                }
                else
                {
                    StarePlatitorTva starePlatitor = verificaTVAService(cuiClient);
                    insertTvaInfo(connection, starePlatitor);
                    platitorResponse = getPlatitorStatus(starePlatitor);
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
            return serializer.Serialize(platitorResponse);
        }



        private void updateTvaInfo(OracleConnection connection, StarePlatitorTva starePlatitor)
        {

            if (starePlatitor.CUI == null)
                return;

            OracleCommand cmd = connection.CreateCommand();

            cmd.CommandText = " update sapprd.zverifcui set nr_inmatric=:nrInmatric, judet=:judet, numejudet=:numeJudet, " + 
                              " localitate=:localitate, tip_str=:tipStr, adresa=:adresa, nr=:nr, stare=:stare, tva=:tva, " + 
                              " tva_incs=:tvaIncs, data_tva=:dataTva, data_update=:dataUpdate where cui=:cui ";

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Clear();

            cmd.Parameters.Add(":nrInmatric", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = starePlatitor.NrInmatr;

            cmd.Parameters.Add(":judet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = getCodJudet(starePlatitor.NrInmatr.Trim());

            cmd.Parameters.Add(":numeJudet", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[2].Value = starePlatitor.Judet;

            cmd.Parameters.Add(":localitate", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[3].Value = starePlatitor.Localitate;

            cmd.Parameters.Add(":tipStr", OracleType.VarChar, 21).Direction = ParameterDirection.Input;
            cmd.Parameters[4].Value = starePlatitor.Tip == "" ? " " : starePlatitor.Tip;

            cmd.Parameters.Add(":adresa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[5].Value = starePlatitor.Adresa == "" ? " " : starePlatitor.Adresa;

            cmd.Parameters.Add(":nr", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[6].Value = starePlatitor.Nr == "" ? " " : starePlatitor.Nr;

            cmd.Parameters.Add(":stare", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
            cmd.Parameters[7].Value = starePlatitor.Stare == null ? " " : starePlatitor.Stare;

            cmd.Parameters.Add("tva", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[8].Value = starePlatitor.TVA == "False" ? "0" : "1";

            cmd.Parameters.Add(":tvaIncs", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[9].Value = starePlatitor.TVAIncasare == "False" ? "0" : "1";

            cmd.Parameters.Add(":dataTva", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[10].Value = starePlatitor.DataTVA == null || starePlatitor.DataTVA == "" ? " " : formatTvaDate(starePlatitor.DataTVA);

            cmd.Parameters.Add(":dataUpdate", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[11].Value = AddressUtils.getCurrentDate_YYDDMM();

            cmd.Parameters.Add(":cui", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[12].Value = starePlatitor.CUI;

            cmd.ExecuteNonQuery();

            cmd.Dispose();

        }




        private void insertTvaInfo(OracleConnection connection, StarePlatitorTva starePlatitor)
        {

            if (starePlatitor.CUI == null)
                return;

            OracleCommand cmd = connection.CreateCommand();

            cmd.CommandText = " insert into sapprd.zverifcui (mandt, cui_sap, cui, numefirma, nr_inmatric, judet, numejudet, localitate, tip_str, adresa, " +
                              " nr, stare, data_update, tva, tva_incs, data_tva, data_salv ) values  " +
                              " ('900', :cui_sap, :cui, :numefirma, :nr_inmatric, :judet, :numejudet, :localitate, :tip_str, :adresa, " +
                              " :nr, :stare, :data_update, :tva, :tva_incs, :data_tva, :data_salv )";

            cmd.Parameters.Add(":cui_sap", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = starePlatitor.CUI;

            cmd.Parameters.Add(":cui", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[1].Value = starePlatitor.CUI;

            cmd.Parameters.Add(":numefirma", OracleType.VarChar, 105).Direction = ParameterDirection.Input;
            cmd.Parameters[2].Value = starePlatitor.Nume;

            cmd.Parameters.Add(":nr_inmatric", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
            cmd.Parameters[3].Value = starePlatitor.NrInmatr;

            cmd.Parameters.Add(":judet", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
            cmd.Parameters[4].Value = getCodJudet(starePlatitor.NrInmatr.Trim()); 

            cmd.Parameters.Add(":numeJudet", OracleType.VarChar, 60).Direction = ParameterDirection.Input;
            cmd.Parameters[5].Value = starePlatitor.Judet;

            cmd.Parameters.Add(":localitate", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[6].Value = starePlatitor.Localitate;

            cmd.Parameters.Add(":tip_str", OracleType.VarChar, 21).Direction = ParameterDirection.Input;
            cmd.Parameters[7].Value = starePlatitor.Tip == "" ? " " : starePlatitor.Tip;

            cmd.Parameters.Add(":adresa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
            cmd.Parameters[8].Value = starePlatitor.Adresa == "" ? " " : starePlatitor.Adresa;

            cmd.Parameters.Add(":nr", OracleType.VarChar, 120).Direction = ParameterDirection.Input;
            cmd.Parameters[9].Value = starePlatitor.Nr == "" ? " " : starePlatitor.Nr;

            cmd.Parameters.Add(":stare", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
            cmd.Parameters[10].Value = starePlatitor.Stare == null ? " " : starePlatitor.Stare;

            cmd.Parameters.Add(":data_update", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[11].Value = AddressUtils.getCurrentDate_YYDDMM();

            cmd.Parameters.Add("tva", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[12].Value = starePlatitor.TVA == "False" ? "0" : "1";

            cmd.Parameters.Add(":tva_incs", OracleType.VarChar, 3).Direction = ParameterDirection.Input;
            cmd.Parameters[13].Value = starePlatitor.TVAIncasare == "False" ? "0" : "1";

            cmd.Parameters.Add(":data_tva", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[14].Value = starePlatitor.DataTVA == null || starePlatitor.DataTVA == "" ? " " : formatTvaDate(starePlatitor.DataTVA);

            cmd.Parameters.Add(":data_salv", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[15].Value = AddressUtils.getCurrentDate_YYMMDD();

            cmd.ExecuteNonQuery();

            cmd.Dispose();

        }


        private PlatitorTvaResponse getPlatitorStatus(StarePlatitorTva platitorTva)
        {

            PlatitorTvaResponse tvaResponse = new PlatitorTvaResponse();

            if (platitorTva.Raspuns != null && platitorTva.Raspuns.ToLower().Contains("valid"))
            {
                tvaResponse.isPlatitor = platitorTva.TVA.Equals("0") ? false : true;
                tvaResponse.numeClient = platitorTva.Nume;
                tvaResponse.nrInreg = platitorTva.NrInmatr;
            }
            else
            {
                tvaResponse.isPlatitor = false;
                tvaResponse.errMessage = platitorTva.errMessage;
            }

            return tvaResponse;

        }

        private string getCodJudet(string nrInmatr)
        {
            string codJudet = "00";

            if (nrInmatr != null && nrInmatr.Length > 3)
                codJudet = nrInmatr.Substring(1, 2);

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

    }
}