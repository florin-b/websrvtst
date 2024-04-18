﻿using System;
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
        public string tip1;
        public string tip2;
        public bool isLocal;
        public bool isArticolSite;
        public string planificator;
    }

    public class RezultatArtMathaus
    {
        public string nrTotalArticole;
        public List<ArticolMathaus> listArticole;
    }

    public class AntetCmdMathaus
    {
        public string localitate;
        public string codJudet;
        public string codClient;
        public string tipPers;
        public string depart;
        public string codPers;
        public string tipTransp;
        public string camionDescoperit;
        public string macara;
        public string tipCamion;
        public double greutateComanda;
        public string tipComandaCamion;
        public string isComandaDL;


    }

    public class DateTransportMathaus
    {
        public List<CostTransportMathaus> listCostTransport;
        public List<DepozitArticolTransport> listDepozite;
        public List<DataLivrare> zileLivrare;
    }

    public class DataLivrare
    {
        public string filiala;
        public string dataLivrare;
    }

    public class CostTransportMathaus
    {
        public string filiala;
        public string tipTransp;
        public string valTransp;
        public string codArtTransp;
        public string depart;
        public string numeCost;
    }

    public class DepozitArticolTransport
    {
        public string codArticol;
        public string filiala;
        public string depozit;
    }

    public class ComandaMathaus
    {
        public string sellingPlant;
        public string countyCode;
        public string deliveryZoneType;
        public List<DateArticolMathaus> deliveryEntryDataList;
    }

    public class LivrareMathaus
    {
        public ComandaMathaus comandaMathaus;
        public List<CostTransportMathaus> costTransport;
        public List<DataLivrare> zileLivrare;
    }

    public class DateArticolMathaus
    {
        public string deliveryWarehouse;
        public string productCode;
        public double quantity;
        public string unit;
        public double valPoz;
        public string tip2;
        public string depozit;
        public string ulStoc;
        public string greutate;
        public double quantity50;
        public string unit50;
        public double cantUmb;
        public string tipStoc;
    }

    public class StockMathaus
    {
        public string plant;
        public string deliveryZoneType;
        public List<StockEntryDataList> stockEntryDataList;
    }

    public class StockEntryDataList
    {
        public string productCode;
        public double availableQuantity;
        public string warehouse;
    }

    public class OptiuneCamion
    {
        public string nume;
        public bool exista;
        public bool selectat;
    }

}