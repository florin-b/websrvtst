using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class BeansAMOB
    {
    }



    public class ComandaAMOBAfis
    {
        public string idComanda;
        public string idAmob;
        public string dataCreare;
        public string valoare;
    }


    public class ComandaAMOB
    {
        public string idComanda;
        public string codFiliala;
        public string codAgent;
        public double valoare;
        public double procReducere;
        public List<ArticolAMOB> listArticole;

    }


    public class ArticolAMOB
    {
        public string codArticol;
        public double cantitate;
        public string um;
        public string depozit;
        public double pretUnitar;
        public double procentReducere;
        public string numeArticol;
        public string umVanz;
        public string depart;
        public string tipAB;
    }



}