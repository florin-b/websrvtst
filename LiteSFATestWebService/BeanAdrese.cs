﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class BeanAdreseJudet
    {
        public String listLocalitati;
        public String listStrazi;
    }


    public class Adresa
    {
        public string codJudet;
        public string localitate;
        public string strada;
        public string latitude;
        public string longitude;

        public override string ToString()
        {
            return "Adresa [codJudet=" + codJudet + ", localitate=" + localitate +  ", strada=" + strada + ", latitude=" + latitude + ", longitude=" + longitude + "]";
        }

    }

}