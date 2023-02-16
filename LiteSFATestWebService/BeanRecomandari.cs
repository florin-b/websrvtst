using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    class ProductInfo
    {

        public ProductInfo()
        {

        }

        public ProductInfo(string ProductID, string CombinedProductID)
        {
            this.ProductID = ProductID;
            this.CombinedProductID = CombinedProductID;
        }

        public string ProductID;
        public string CombinedProductID;
    }

     class Copurchase_prediction
    {
        public float Score { get; set; }
    }
}