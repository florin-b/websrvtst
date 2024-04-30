﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LiteSFATestWebService
{
    public class HelperAprobari
    {
        public static bool isAprobareDV(List<ArticolComanda> listArticole, string tipUser, List<TaxaComanda> taxeComanda, DateLivrare dateLivrare, ComandaVanzare comandaVanzare)
        {

            if (tipUser.Equals("AV") || tipUser.Equals("SD") || tipUser.Equals("KA") || tipUser.Equals("CVA") || tipUser.Equals("SDCVA"))
                return isAprobareDV_1(listArticole, tipUser,  comandaVanzare);
            else
                return isAprobareDV_2(listArticole, tipUser, comandaVanzare, taxeComanda, dateLivrare);
        }


        private static bool isAprobareDV_1(List<ArticolComanda> listArticole, string tipUser, ComandaVanzare comandaVanzare)
        {

            bool isAprobare = false;
            HashSet<string> setAprobari = new HashSet<string>();

            foreach (ArticolComanda articol in listArticole)
            {
                if (articol.pretMinim == 0)
                    continue;

                if (articol.pretUnit < articol.pretMinim)
                {
                    isAprobare = true;
                    setAprobari.Add(articol.depart.Substring(0, 2));
                }
            }

            if (comandaVanzare.canalDistrib.Equals("20"))
                comandaVanzare.necesarAprobariCV = " ";

            if ((tipUser.Equals("CVA") || tipUser.Equals("SDCVA")) && setAprobari.Count > 0)
                comandaVanzare.necesarAprobariCV = string.Join(",", setAprobari);
                
            
            return isAprobare;

        }


        private static bool isAprobareDV_2(List<ArticolComanda> listArticole, string tipUser, ComandaVanzare comandaVanzare, List<TaxaComanda> taxeComanda, DateLivrare dateLivrare)
        {
            bool marjaCmdPozitiva = true;
            bool articolSubCmp = false;

            double totalAdaos = 0;
            double adaosArticol = 0;
            double cmpCorectatUnit = 0;
            HashSet<string> setAprobari = new HashSet<string>();

            comandaVanzare.necesarAprobariCV = " ";

            foreach (ArticolComanda articol in listArticole)
            {

                if (articol.pretMinim == 0 || articol.cmpCorectat == 0)
                    continue;

                adaosArticol = (articol.pretUnit / articol.multiplu - articol.pretMinim / articol.multiplu) * articol.cantitate;

                if (isUserAprobariCV(tipUser) && adaosArticol < 0)
                    setAprobari.Add(articol.depart.Substring(0, 2));

                totalAdaos += adaosArticol;

                cmpCorectatUnit = (articol.cmpCorectat / articol.cantitate) * articol.multiplu;

                if (articol.pretUnit < cmpCorectatUnit)
                {
                    articolSubCmp = true;
                    setAprobari.Add(articol.depart.Substring(0, 2));
                }
            }


            if (isUserAprobariCV(tipUser) && setAprobari.Count > 0)
                comandaVanzare.necesarAprobariCV = string.Join(",", setAprobari);

            double pragAprobare = 0;

            if (tipUser.Contains("IP") && taxeComanda != null)
                pragAprobare = getPragAprobare(taxeComanda, dateLivrare);

            marjaCmdPozitiva = totalAdaos >= pragAprobare;

            return !marjaCmdPozitiva || articolSubCmp;
        }

        private static bool isUserAprobariCV(string tipUser)
        {
            return tipUser.Contains("VR") || tipUser.Contains("VO") || tipUser.Contains("W") || tipUser.Contains("VS");
        }


        private static double getPragAprobare(List<TaxaComanda> taxeComanda, DateLivrare dateLivrare)
        {
            double pragAprobare = 0;

            string unitLog = dateLivrare.unitLog;

            if (dateLivrare.filialaCLP != null && dateLivrare.filialaCLP.Trim().Length == 4)
                unitLog = dateLivrare.filialaCLP;
                
            foreach (TaxaComanda taxa in taxeComanda)
            {
                if (taxa.filiala.Equals(unitLog))
                    pragAprobare += taxa.valoare;
                    
            }

            return pragAprobare;
        }


    }
}