using Microsoft.ML;
using Microsoft.ML.Trainers;

using Microsoft.ML.Trainers.Recommender;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    class RecomandariML
    {

        public class HouseData
        {
            public float Size { get; set; }
            public float Price { get; set; }
        }


        public void test()
        {

            try {

                HouseData[] houseData = {
           new HouseData() { Size = 1.1F, Price = 1.2F },
           new HouseData() { Size = 1.9F, Price = 2.3F },
           new HouseData() { Size = 2.8F, Price = 3.0F },
           new HouseData() { Size = 3.4F, Price = 3.7F } };

                MLContext mlContext = new MLContext();


                IDataView trainingData = mlContext.Data.LoadFromEnumerable(houseData);
            }catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

        }


        public void test3()
        {
            MLContext mlContext = new MLContext();

            IEnumerable<ProductInfo> items = new ProductInfo[] { new ProductInfo("0","0") };

            List<ProductInfo> listItems = items.ToList<ProductInfo>();

            IEnumerable<ProductInfo> items2 = (IEnumerable<ProductInfo>)getProductData("1");
            var traindata = mlContext.Data.LoadFromEnumerable(items);


            MatrixFactorizationTrainer.Options options = new Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options();
            options.MatrixColumnIndexColumnName = nameof(ProductInfo.ProductID);
            options.MatrixRowIndexColumnName = nameof(ProductInfo.CombinedProductID);

            options.LabelColumnName = "Label";
            options.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass;
            options.Alpha = 0.01;
            options.Lambda = 0.025;

            var est = mlContext.Recommendation().Trainers.MatrixFactorization(options);

            ITransformer model = est.Fit(traindata);

            var predictionengine = mlContext.Model.CreatePredictionEngine<ProductInfo, Copurchase_prediction>(model);

            var prediction = predictionengine.Predict(
                new ProductInfo()
                {
                    ProductID = "000000000010200065",
                    CombinedProductID = "000000000010200066"
                });

            string result = ("\n For ProductID = 3 and  CoPurchaseProductID = 63 the predicted score is " + Math.Round(prediction.Score, 1));
        }

        public List<ProductInfo> getProductData(string codArticol)
        {
            List<ProductInfo> listProduct = new List<ProductInfo>();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToProdEnvironment();

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
                        articol.CombinedProductID = oReader.GetString(0);
                        listProduct.Add(articol);

                    }


                }
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
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