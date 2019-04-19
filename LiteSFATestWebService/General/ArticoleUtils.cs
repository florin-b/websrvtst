using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService.General
{
    public class ArticoleUtils
    {

        public static bool isMaterialServiciiWood(string codArticol)
        {
            List<string> listArticole = new List<string>();

            listArticole.Add("000000000030100021");
            listArticole.Add("000000000030100553");
            listArticole.Add("000000000030101463");
            listArticole.Add("000000000030100060");
            listArticole.Add("000000000030100061");
            listArticole.Add("000000000030101102");
            listArticole.Add("000000000030100073");
            listArticole.Add("000000000030100071");
            listArticole.Add("000000000030100029");
            listArticole.Add("000000000030100028");
            listArticole.Add("000000000030100840");
            listArticole.Add("000000000030100072");

            return listArticole.Contains(codArticol);


        }

    }
}