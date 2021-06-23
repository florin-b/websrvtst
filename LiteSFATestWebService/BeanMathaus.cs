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
        public bool isLocal;
    }

    public class ComandaMathaus
    {
        public string sellingPlant;
        public List<DateArticolMathaus> deliveryEntryDataList;
    }

    public class DateArticolMathaus
    {
        public string deliveryWarehouse;
        public string productCode;
        public double quantity;
        public string unit;
    }

    public class StockMathaus
    {
        public string plant;
        public List<StockEntryDataList> stockEntryDataList;
    }

    public class StockEntryDataList
    {
        public string productCode;
        public double availableQuantity;
        public string warehouse;
    }

}