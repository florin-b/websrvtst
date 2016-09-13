using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class Utils
    {

        public static string getFilialaGed(string filiala)
        {
            return filiala.Substring(0, 2) + "2" + filiala.Substring(3, 1);
        }


        public static string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
        }


        public static string getCurrentTime()
        {
            string mTime = "";
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            mTime = hour + minute + sec;
            return mTime;
        }

        public static String getCurrentMonth()
        {
            DateTime cDate = DateTime.Now;
            string month = cDate.Month.ToString("00");
            return month;
        }


        public static String getCurrentYear()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            return year;
        }


    }
}