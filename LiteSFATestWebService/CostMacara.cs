﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{

    public class ComandaCalculDescarcare
    {
        public string filiala;
        public string listArticole;
    }

    public class CostDescarcare
    {
        public string filiala;
        public List<ArticolDescarcare> articoleDescarcare;
        public bool sePermite;
        public List<ArticolPalet> articolePaleti;
    }


    public class ArticolDescarcare
    {
        public string cod;
        public string depart;
        public string valoare;
        public string cantitate;
        public string valoareMin;
    }


    public class ArticolCalculDesc
    {
        public string cod;
        public double cant;
        public string um;
        public string depoz;
    }

    public class ArticolPalet
    {
        public string codPalet;
        public string numePalet;
        public string depart;
        public string cantitate;
        public string pretUnit;
        public string furnizor;
        public string codArticol;
        public string numeArticol;
        public string cantArticol;
        public string umArticol;
        public string filiala;
        public string depozit;
        public List<CantitateFiliala> paletiFiliala;
    }

    public class CalculDescarcare
    {
        public string filiala;
        public List<ArticolCalculDesc> listArticole;

    }

    public class CantitateFiliala
    {
        public string filiala;
        public string cantitate;
        public string pretUnitPalet;
        public string cantTotal;
        public string depozit;
    }

   

}