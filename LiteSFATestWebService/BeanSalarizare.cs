using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class VenitTPR
    {
        public string codN2;
        public string numeN2;
        public string venitGrInc;
        public string pondere;
        public string targetPropCant;
        public string targetRealCant;
        public string um;
        public string targetPropVal;
        public string targetRealVal;
        public string realizareTarget;
        public string targetPonderat;
    }


    public class VenitTCF
    {
        public string venitGrInc;
        public string targetPropus;
        public string targetRealizat;
        public string coefAfectare;
        public string venitTcf;
    }


    public class SalarizareAv
    {
        public string venitTpr;
        public string venitTcf;
    }


}