using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService.General
{
    public class GeneralUtils
    {

        public static string serializeObject(Object customObject)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(customObject);
        }




        public static string  addDays(string date, int nrDays)
        {
            return Convert.ToDateTime(date).AddDays(nrDays).ToString("dd MMMM yyyy",CultureInfo.CreateSpecificCulture("ro"));
        }

        public static long getCurrentMillis()
        {
            DateTime Jan1St2016 = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)((DateTime.UtcNow - Jan1St2016).TotalMilliseconds);
        }

        public static String getCurrentMillis2()
        {
            DateTime Jan1St2017 = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ((DateTime.UtcNow - Jan1St2017).TotalMilliseconds).ToString();
        }

        public static string formatIstoricPret(string istoricPret)
        {

            if (istoricPret == null)
                return " ";

            if (istoricPret.Trim().Length == 0)
                return " ";

            if (istoricPret.Contains("#"))
                return istoricPret.Replace("#", "\n");

            return istoricPret;
        }


    }
}