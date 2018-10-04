﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class CostDescarcare
    {

        public List<ArticolDescarcare> articoleDescarcare;
        public bool sePermite;

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


    public class CalculDescarcare
    {
        public string filiala;
        public List<ArticolCalculDesc> listArticole;

    }

   

}