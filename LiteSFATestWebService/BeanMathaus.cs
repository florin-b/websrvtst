using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class CategorieMathaus
    {
        public string cod;
        public string nume;
        public string codHybris;
        public string codParinte;

    }


    public class ArticolMathaus : ArticolCautare
    {
        public string adresaImg;
        public string adresaImgMare;
        public string descriere;
        public string catMathaus;
        public string pretUnitar;
    }

}