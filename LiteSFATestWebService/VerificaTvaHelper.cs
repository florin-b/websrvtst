﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class StarePlatitorTva
    {
        public string Raspuns;
        public string Nume;
        public string CUI;
        public string NrInmatr;
        public string Judet;
        public string Localitate;
        public string Tip;
        public string Adresa;
        public string Nr;
        public string Stare;
        public string Actualizat;
        public string TVA;
        public string TVAIncasare;
        public string DataTVA;
        public string errMessage;
        public string TVA_data;

        public override string ToString()
        {
            return "StarePlatitorTva [Raspuns=" + Raspuns + ", Nume=" + Nume + ", CUI=" + CUI
                    + ", NrInmatr=" + NrInmatr + ", Judet=" + Judet + ", Localitate=" + Localitate + ", Tip=" + Tip + ", Adresa=" + Adresa + ", Nr=" + Nr
                    + ", Stare=" + Stare + ", Actualizat=" + Actualizat + ", TVA=" + TVA + ", TVAIncasare=" + TVAIncasare + ", DataTVA=" + DataTVA + "]";
        }


    }


    public class PlatitorTvaResponse
    {
        public bool isPlatitor;
        public string numeClient;
        public string nrInreg;
        public string errMessage;
        public string codJudet;
        public string localitate;
        public string strada;

        public override string ToString()
        {
            return "PlatitorTvaResponse [ isPlatitor=" + isPlatitor + " numeClient = " + numeClient + ", errMessage=" + errMessage + "]";

        }
    }






}