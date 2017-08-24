﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieTESTWebServices
{
    public class Utils
    {

        public static string getDepartName(String departCode)
        {
            string retVal = "";

            if (departCode.Equals("01"))
                retVal = "lemnoase";

            if (departCode.Equals("02"))
                retVal = "feronerie";

            if (departCode.Equals("03"))
                retVal = "parchet";

            if (departCode.Equals("04"))
                retVal = "materiale grele";

            if (departCode.Equals("05"))
                retVal = "electrice";

            if (departCode.Equals("06"))
                retVal = "gips";

            if (departCode.Equals("07"))
                retVal = "chimice";

            if (departCode.Equals("08"))
                retVal = "instalatii";

            if (departCode.Equals("09"))
                retVal = "hidroizolatii";

            if (departCode.Equals("11"))
                retVal = "magazin";

            return retVal;
        }


        public static bool isTimeToSendSmsAlert()
        {

            DateTime t1 = DateTime.Now;

            DateTime t2 = Convert.ToDateTime("07:30:00 AM");

            int i = DateTime.Compare(t1, t2);

            if (i < 0)
                return false;

            return true;
        }


    }
}