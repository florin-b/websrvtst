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
        private static string[] listSinteticePal = { "100", "102" , "103" , "104", "105", "107", "142", "143" };


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

                    if (transaction != null )
                        cmd.Transaction = transaction;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            if (sintCherestea.Contains(oReader.GetString(0))){
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
            catch(Exception ex)
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





    }
}