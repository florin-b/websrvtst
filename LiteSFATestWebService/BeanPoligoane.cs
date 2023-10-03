﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace LiteSFATestWebService
{
    public class LatLng
    {
        public double lat;
        public double lon;

        public LatLng()
        {

        }

        public LatLng(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
    }

    public class Poligon
    {
        public string tipPoligon;
        public string numeFisier;
        public string filiala;
        public string tonaj;
        public string nume;
    }

    public class DatePoligon
    {

        public string filialaPrincipala;
        public string filialaSecundara;
        public string tipZona;
        public string limitareTonaj;
        public string nume;

        public DatePoligon()
        {

        }

        public DatePoligon(string filialaPrincipala, string filialaSecundara, string tipZona, string limitareTonaj, string nume)
        {
            this.filialaPrincipala = filialaPrincipala;
            this.filialaSecundara = filialaSecundara;
            this.tipZona = tipZona;
            this.limitareTonaj = limitareTonaj;
            this.nume = nume;
        }


    }

}