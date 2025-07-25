﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class BeanClientSemiactiv
    {

        public string numeClient;
        public string codClient;
        public string judet;
        public string localitate;
        public string strada;
        public string numePersContact;
        public string telPersContact;
        public string vanzMedie;
        public string vanz03;
        public string vanz06;
        public string vanz07;
        public string vanz09;
        public string vanz040;
        public string vanz041;


    }


    public class BeanDatePersonale
    {
        public string cnp;
        public string nume;
        public string codjudet;
        public string localitate;
        public string strada;
        public List<string> termenPlata;
        public bool clientBlocat;
        public string tipPlata;
        public string codClient;
        public string divizii;
    }

    public class AdresaClientGed
    {
        public string codAdresa;
        public string codJudet;
        public string localitate;
        public string strada;
    }

    public class CreditClient
    {
        public double limitaCredit;
        public double restCredit;
        public bool isBlocat;
        public string motivBlocat;
    }


    public class DateClientSap
    {
        public string cui;
        public string numeCompanie;
        public string emailCompanie;
        public string strada;
        public string numarStrada;
        public string localitate;
        public string judet;
        public string numePersContact;
        public string prenumePersContact;
        public string telPersContact;
        public string codJ;
        public string platitorTVA;
        public string filialaAsociata;
        public string coordonateAdresa;
        public string tipClient;
        public string codAgent;
        public string tipAngajat;
        public string codDepart;
    }

    public class RaspunsClientSap
    {
        public string codClient;
        public string diviziiClient;
        public string msg;
    }
   

}