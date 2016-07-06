using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieTESTWebServices
{
    public class BeanAdresa
    {
        public string country;
        public string region;
        public string city;
        public string streetName;
        public string streetNo;
    }


    public class LatLng
    {
        public double latitude;
        public double longitude;
    }


    public class RouteBounds
    {
        public BeanAdresa adresaDest;
        public LatLng pozMasina;
    }


}