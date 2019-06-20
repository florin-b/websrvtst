using System;
using System.Collections.Generic;
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

    }
}