﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class HelperComenzi
    {

        public static string[] listSinteticeCant = { "200", "201", "202", "204", "205", "206", "207", "236", "237", "238", "240", "204_01", "204_02" };
        private static string[] listSinteticePal = { "100", "102", "103", "104", "105", "107", "142", "143" };


        public static void setMarjaCantPal(List<ArticolComandaRap> listArticole, DateLivrareCmd dateLivrare)
        {

            double marjaBrutaPalVal = 0;
            double marjaBrutaCantVal = 0;
            double marjaBrutaPalProc = 0;
            double marjaBrutaCantProc = 0;
            double totalLungimeCant = 0;
            double nrFoiPal = 0;
            double totalComanda = 0;

            foreach (ArticolComandaRap articol in listArticole)
            {

                if (Array.IndexOf(listSinteticeCant, articol.sintetic) >= 0)
                {
                    marjaBrutaCantVal += (articol.pretUnit - articol.cmp) * Double.Parse(articol.cantUmb);

                    if (articol.Umb.ToLower().Equals("rol"))
                        totalLungimeCant += 50 * Double.Parse(articol.cantUmb);
                    else if (articol.Umb.ToLower().Equals("m"))
                        totalLungimeCant += Double.Parse(articol.cantUmb);
                    else
                        totalLungimeCant += articol.lungime;
                }


                if (Array.IndexOf(listSinteticePal, articol.sintetic) >= 0)
                {
                    marjaBrutaPalVal += (articol.pretUnit - articol.cmp) * Double.Parse(articol.cantUmb);
                    nrFoiPal += Double.Parse(articol.cantUmb);
                }

                totalComanda += articol.pret;
            }


            if (totalComanda == 0)
            {
                marjaBrutaCantProc = 0;
                marjaBrutaPalProc = 0;
            }
            else
            {
                marjaBrutaCantProc = (marjaBrutaCantVal / totalComanda) * 100;
                marjaBrutaPalProc = (marjaBrutaPalVal / totalComanda) * 100;
            }

            dateLivrare.marjaBrutaCantVal = Math.Round(marjaBrutaCantVal, 2);
            dateLivrare.marjaBrutaCantProc = Math.Round(marjaBrutaCantProc, 2);

            dateLivrare.marjaBrutaPalVal = Math.Round(marjaBrutaPalVal, 2);
            dateLivrare.marjaBrutaPalProc = Math.Round(marjaBrutaPalProc, 2);

            if (nrFoiPal == 0)
                dateLivrare.mCantCmd = 0;
            else
                dateLivrare.mCantCmd = Math.Round(totalLungimeCant / nrFoiPal, 2);

            dateLivrare.mCant30 = 0;

        }

        public static string eliminaCodDepart(string numeArticol)
        {


            if (numeArticol == null || numeArticol.Trim().Length == 0)
                return " ";

            if (!numeArticol.ToLower().Contains("div."))
                return numeArticol;


            int posDiv = numeArticol.ToLower().IndexOf("div.");

            if (numeArticol.ToLower().Contains("-div."))
                posDiv = numeArticol.ToLower().IndexOf("-div.");


            return numeArticol.Substring(0, posDiv);

        }


        public static string getArtExcCherestea(OracleConnection connection, OracleTransaction transaction, List<ArticolComanda> listArticole, string codArticol)
        {
            string codExceptie = codArticol;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string sintCherestea = "374#373#372#368#349";
            Boolean isComandaCherestea = false;


            try
            {
                cmd = connection.CreateCommand();

                foreach (ArticolComanda articol in listArticole)
                {

                    cmd.CommandText = "select sintetic from articole where cod=:codArticol ";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = "0000000000" + articol.codArticol;

                    if (transaction != null)
                        cmd.Transaction = transaction;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (sintCherestea.Contains(oReader.GetString(0)))
                            {
                                isComandaCherestea = true;
                            }
                        }
                    }

                    if (isComandaCherestea)
                        break;
                }

                if (isComandaCherestea)
                {
                    if (codArticol.Equals("30101747"))
                        codExceptie = "30101924";
                    else if (codArticol.Equals("30101748"))
                        codExceptie = "30101925";
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


            return codExceptie;
        }


        public static bool isComandaDistribAprobata(string accept1, string oraAccept1, string accept2, string oraAccept2)
        {


            bool isAprob1 = true;
            bool isAprob2 = true;

            if (accept1.Equals("X"))
                isAprob1 = !oraAccept1.Equals("000000");


            if (accept2.Equals("X"))
                isAprob2 = !oraAccept2.Equals("000000");


            return isAprob1 && isAprob2;
        }

        public static string setTipPlata(string tipPlata)
        {
            if (tipPlata.Equals("R"))
                return "E1";

            return tipPlata;
        }


        public static void setLivrariArtACZC(OracleConnection connection, string nrComanda, ArticolComandaRap articol)
        {

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            if (articol.codArticol.StartsWith("30"))
            {
                articol.aczcDeLivrat = 0;
                articol.aczcLivrat = 0;
                return;
            }

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select nvl(sum(e.vmeng),0) qty_de_livrat from sapprd.vbbe e where " +
                                  " e.mandt = '900' and e.vbeln = :nrComanda and matnr = :codArticol ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrComanda", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrComanda;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = "0000000000" + articol.codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    articol.aczcDeLivrat = oReader.GetDouble(0);

                }
                else
                    articol.aczcDeLivrat = 0;



                cmd.CommandText = " select  nvl(sum(rfmng),0) qty_livr from sapprd.vbfa f, sapprd.vbap p " +
                                   " where f.mandt = '900' and f.vbelv = :nrComanda  and p.matnr = :codArticol " +
                                   " and f.vbtyp_v = 'C' and f.vbtyp_n = 'J' and f.mandt = p.mandt and f.vbelv = p.vbeln and f.posnv = p.posnr ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrComanda", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrComanda;

                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = "0000000000" + articol.codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    articol.aczcLivrat = oReader.GetDouble(0);

                }
                else
                    articol.aczcLivrat = 0;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrComanda + " , " + articol.codArticol);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

        }

        public static string getTipPrelucrare(OracleConnection connection, string nrCmd)
        {
            string tipPrelucrare = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select prelucrare from SAPPRD.zprelucrare04 where mandt = '900' and idComanda = :nrCmd ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrCmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd); ;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    tipPrelucrare = oReader.GetString(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrCmd);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return tipPrelucrare;
        }


        public static string getNrCmdClp(OracleConnection connection, string nrCmd)
        {
            string nrCmdClp = "";

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct vbeln from sapprd.vbfa f, sapprd.zcomhead_tableta b where f.mandt = '900' and b.mandt = '900' " +
                                  " and b.id =:idCmd and f.vbelv = b.nrcmdsap and f.vbtyp_v = 'C' and f.vbtyp_n = 'V' ";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":idcmd", OracleType.Int32, 20).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = Int32.Parse(nrCmd); ;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (nrCmdClp.Equals(String.Empty))
                            nrCmdClp = oReader.GetString(0);
                        else
                            nrCmdClp += ";" + oReader.GetString(0);
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , " + nrCmd);
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return nrCmdClp;
        }



        public static void tansformaCLPinCV(List<ArticolComanda> listArticole, DateLivrare dateLivrare)
        {

            if (dateLivrare.filialaCLP == null || dateLivrare.filialaCLP.Trim().Equals(String.Empty))
                return;

            string filialaClp = "";

            foreach (ArticolComanda articol in listArticole)
            {
                if (articol.filialaSite != null && !articol.filialaSite.Trim().Equals(String.Empty))
                {
                    filialaClp = articol.filialaSite;
                    break;
                }
            }

            if (filialaClp.Trim().Equals(String.Empty))
                return;

            string filialaComanda = Utils.isUnitLogGed(dateLivrare.unitLog) ? Utils.getFilialaGed(filialaClp) : filialaClp;

            dateLivrare.unitLog = filialaComanda;
            dateLivrare.filialaCLP = "";


        }




        public static bool isUlEquals(string ul1, string ul2)
        {
            string ulBrut1 = ul1.Substring(0, 2) + "X" + ul1.Substring(3, 1);

            string ulBrut2 = ul2.Substring(0, 2) + "X" + ul2.Substring(3, 1);

            return ulBrut1.Equals(ulBrut2);
        }


        public static string getDepartExtra(string divizie)
        {
            string depExtra = null;


            if (divizie.Equals("01"))
                depExtra = "'02'";

            else if (divizie.Equals("041"))
                if (depExtra == null)
                    depExtra = "'040'";
                else
                    depExtra += ",'040'";


            else if (divizie.Equals("040"))
                if (depExtra == null)
                    depExtra = "'041'";
                else
                    depExtra += ",'041'";


            else if (divizie.Equals("07"))
                if (depExtra == null)
                    depExtra = "'03','06'";
                else
                    depExtra += ",'03','06'";


            else if (divizie.Equals("03"))
                if (depExtra == null)
                    depExtra = "'07','06'";
                else
                    depExtra += ",'07','06'";


            else if (divizie.Equals("06"))
                if (depExtra == null)
                    depExtra = "'03','07'";
                else
                    depExtra += ",'03','07'";

            if (depExtra == null)
                depExtra = "('" + divizie + "')";
            else
                depExtra = "(" + depExtra + ",'" + divizie + "')";

            if (divizie.ToLower().Contains("extra"))
                depExtra = "(" +  getDepartIncrucisat(divizie) + ")";

            return depExtra;

        }

        public static string getDepartIncrucisat(string departIncrucisat)
        {

            string[] departExtra = departIncrucisat.Split(':')[1].Split(';');

            string strDeparts = "";

            foreach (string dep in departExtra)
            {
                if (dep == "")
                    continue;

                if (strDeparts == "")
                    strDeparts = "'" + dep + "'";
                else
                    strDeparts += ",'" + dep + "'";
            }

            return strDeparts;
        }

        public static string getCodArticolDescarcare(string codDepart)
        {
            string codArticol = "000000000000000000";

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select cod from articole where lower(nume) like '%serv%descarcare%' and grup_vz = :depart and lower(sintetic) like '%servtd%' ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":depart", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codDepart;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    codArticol = oReader.GetString(0);
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


            return codArticol;
        }

        public static Dictionary<string, string> getDictionarUmIso(List<DateArticolMathaus> listArticole)
        {

            Dictionary<string, string> dictionarUmIso = new Dictionary<string, string>();

            string umArt = "";
            string unitArt = "";
            foreach (DateArticolMathaus articol in listArticole)
            {

                unitArt = articol.unit;

                if (!articol.unit.Equals(articol.unit50))
                    unitArt = articol.unit50;

                if (umArt == "")
                    umArt = "'" + unitArt + "'";
                else
                    umArt += "," + "'" + unitArt + "'";
            }

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select distinct t.msehi, t.isocode from sapprd.t006 t where t.mandt = '900' and t.msehi in (" + umArt + ") ";
                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        dictionarUmIso.Add(oReader.GetString(0), oReader.GetString(1));
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


            return dictionarUmIso;

        }


        public static void convertUmFromIso(Dictionary<string, string> dictionarUmIso, List<DateArticolMathaus> listArticole)
        {
            foreach(DateArticolMathaus articol in listArticole)
            {
                if (dictionarUmIso.ContainsKey(articol.unit))
                    articol.unit = dictionarUmIso[articol.unit];
            }
        }

        public static string getOptiuneCamion(List<OptiuneCamion> listOptiuni, string tipCamion)
        {
            string optiune = "n/a";

            if (listOptiuni == null)
                return optiune;

            foreach(OptiuneCamion optCamion in listOptiuni)
            {
                if (optCamion.nume.ToLower().Equals(tipCamion.ToLower()))
                {
                    if (!optCamion.exista)
                        optiune = "n/a";
                    else
                        optiune = optCamion.selectat ? "y" : "n";
                            
                }
            }

            return optiune;
        }

        public static bool isComandaBV90(List<ArticolComanda> listArticole)
        {

            foreach (ArticolComanda articolComanda in listArticole)
            {
                if (articolComanda.filialaSite != null && articolComanda.filialaSite.Equals("BV90"))
                {
                    return true;
                }
            }

            return false;

        }

        public static bool isComandaCLP(DateLivrare dateLivrare)
        {

            if (dateLivrare.furnizorMarfa != null && dateLivrare.furnizorMarfa.Trim().Length == 10)
                return false;

            if (dateLivrare.filialaCLP == null)
                return false;

            return dateLivrare.filialaCLP.Trim().Length == 4;

        }


        public static double getCantitateUmb (string codArticol, double cantitate, string um)
        {

            double cantitateUmb = 0;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToTestEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select umrez numarator,umren numitor from sapprd.marm where mandt = '900' and matnr = :codArt and meinh = :um ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                cmd.Parameters.Add(":um", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = um;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    cantitateUmb = cantitate * oReader.GetDouble(0) / oReader.GetDouble(1);
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

            return cantitateUmb;

        }

        public static bool isArticolPromo(string codArticol)
        {
            bool isArtPromo = false;

            OracleConnection connection = new OracleConnection();
            OracleDataReader oReader = null;

            string connectionString = DatabaseConnections.ConnectToProdEnvironment();
            connection.ConnectionString = connectionString;
            connection.Open();
            OracleCommand cmd = connection.CreateCommand();

            try
            {
                cmd.CommandText = " select 1 from sapprd.zart_promo where mandt = '900' and matnr = :codArt and to_char(sysdate,'yyyymmdd') between datab and datbi ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArt", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArticol;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    isArtPromo = true;
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

            return isArtPromo;
        }


        public static string getTipZonaMathaus(string tipZona)
        {
            string zonaMathaus = "";

            if (tipZona == null)
                zonaMathaus = "";
            else if (tipZona.ToUpper().Equals("ZM"))
                zonaMathaus = "METRO";
            else if (tipZona.ToUpper().Equals("ZMA") || tipZona.ToUpper().Equals("ZEMA"))
                zonaMathaus = "EXTRA_A";
            else if (tipZona.ToUpper().Equals("ZMB") || tipZona.ToUpper().Equals("ZEMB"))
                zonaMathaus = "EXTRA_B";

            return zonaMathaus;
        }

        public static double getGreutateArticol(string codArticol, double cantitatePoz, ComandaMathaus comandaMathaus)
        {

            double greutateTotalArt = 0;
            double cantitateTotalArt = 0;
            double greutateArticol = 0;

            foreach (DateArticolMathaus articolMathaus in comandaMathaus.deliveryEntryDataList)
            {
                if (articolMathaus.productCode.Equals(codArticol))
                {
                    cantitateTotalArt += articolMathaus.quantity;
                    greutateTotalArt = Double.Parse(articolMathaus.greutate);
                }
            }

            if (cantitateTotalArt > 0)
                greutateArticol = (greutateTotalArt / cantitateTotalArt) * cantitatePoz;
            else
                greutateArticol = 0;

            return Math.Round(greutateArticol, 2); 
            
        }

        public static double getValoareTaxeComanda(List<TaxaComanda> taxeComanda)
        {
            double valoareTaxe = 0;

            if (taxeComanda == null)
                return 0;

            foreach (TaxaComanda taxa in taxeComanda)
            {
                valoareTaxe += taxa.valoare;
            }

            return valoareTaxe;
        }



        public static string getSinteticeFasonate()
        {
            return " ('437','438','439','440') ";
        }

        public static bool isComandaSimulata(string statusComanda)
        {
            if (statusComanda == null)
                return false;

            return statusComanda.Equals("20") || statusComanda.Equals("21");
        }


    }
}