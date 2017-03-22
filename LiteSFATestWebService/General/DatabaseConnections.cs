﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;

namespace LiteSFATestWebService
{
    public class DatabaseConnections
    {

        static public string ConnectToTestEnvironment()
        {



            //TES
            //return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
            //        " (HOST = 10.1.3.89)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = TES))); " +
            //        " User Id = WEBSAP; Password = 2INTER7; ";


            //QAS
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.88)(PORT = 1527)))(CONNECT_DATA = (SERVICE_NAME = QAS))); " +
                    " User Id = WEBSAP; Password = 2INTER7; ";


            //DR
            //return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
            //        " (HOST = 172.17.18.34)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = dr_site) )); " +
            //        " User Id = WEBSAP; Password = 2INTER7;";

        }


        static public string ConnectToProdEnvironment()
        {
            //PRD
            return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                    " (HOST = 10.1.3.95)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = TABLET) )); " +
                    " User Id = WEBSAP; Password = 2INTER7;";



            //DR
            //return "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
            //        " (HOST = 172.17.18.34)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = dr_site) )); " +
            //        " User Id = WEBSAP; Password = 2INTER7;";
        }


       

        public static void CloseConnections(OracleDataReader reader, OracleCommand command, OracleConnection connection)
        {

            try
            {

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (command != null)
                {
                    command.Dispose();
                }


                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }



        public static void CloseConnections(OracleDataReader reader, OracleCommand command)
        {

            try
            {

                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                if (command != null)
                {
                    command.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }


        public static void CloseConnections(OracleCommand command, OracleConnection connection)
        {

            try
            {

                if (command != null)
                {
                    command.Dispose();
                }


                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }


            }
            catch (Exception)
            {

            }


        }



        public static OracleConnection createTESTConnection()
        {
            string connectionString = DatabaseConnections.ConnectToTestEnvironment();

            OracleConnection connection = new OracleConnection();
            connection.ConnectionString = connectionString;
            connection.Open();

            return connection;
        }

        static public string getUser()
        {
            return "USER_RFC";
        }

        static public string getPass()
        {
            return "2rfc7tes3";
        }

    }
}