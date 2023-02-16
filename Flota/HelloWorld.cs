using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Runtime;
using Microsoft.ML.Trainers.Recommender;
using System.Data.OracleClient;
using System.Data;

namespace FlotaTESTWS
{
     class HelloWorld
    {





        public static void Main(String[] args)
        {
            MLContext context = new MLContext();
            MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();

            var traindata = context.Data.LoadFromTextFile("qweqwe");

            options.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass;
            options.Alpha = 0.01;
            options.Lambda = 0.025;

            var est = context.Recommendation().Trainers.MatrixFactorization(options);

        }





        public List<ProductInfo> getProductData(string codArticol)
        {
            List<ProductInfo> listProduct =null;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = "Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP) " +
                   " (HOST = 10.1.3.76)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = PRD))); " +
                   " User Id = WEBSAP; Password = 2INTER7; ";

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select t.cod from sapprd.zcomdet_tableta t where t.id in ( " + 
                                  " select b.id from sapprd.zcomhead_tableta a, sapprd.zcomdet_tableta b where a.mandt = '900' and b.mandt = '900' " + 
                                  " and a.datac >= '20221001' and a.id = b.id " + 
                                  " and b.cod = '000000000010200065' and a.status in (0,2)) and t.cod not like '00000000003%' " + 
                                  " and t.cod != '000000000010200065' ";

                oReader = cmd.ExecuteReader();

                ProductInfo articol;


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {

                        articol = new ProductInfo();
                        articol.ProductID = "000000000010200065";
                        articol.CombinedProductID = oReader.GetString(1);
                        listProduct.Add(articol);

                    }


                }
            }
            catch (Exception ex)
            {
                // ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }

            return listProduct;
        }
    }


    


   

   
}