using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class PersAprob
    {
        public string nume;
        public string telefon;

        public override string ToString()
        {
            return "Decident [ nume=" + nume + ", telefon=" + telefon + "]";
        }


    }



    public class AprobariNecesare
    {
        public string aprobSD;
        public string aprobDV;

        public override string ToString()
        {
            return "AprobariNecesare [ aprobSD=" + aprobSD + ", aprobDV=" + aprobDV + "]";
        }

    }

}