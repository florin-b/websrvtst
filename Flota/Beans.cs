using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flota
{
    public class Sofer
    {
        public string nume;
        public string cod;
    }


    public class ActivitateBorderou
    {
        public string nr;
        public string data;
        public string km;
        public string durata;
    }

    public class DetaliiBorderou
    {
        public string client;
        public string oraSosire;
        public string locatieSosire;
        public string oraPlecare;
        public string locatiePlecare;
        public string durataStationare;
        public string distanta;

    }


    public class TabletaSofer
    {
        public string idTableta;
        public string dataInreg;
        public string stare;
    }



    public class PozitieActualaSofer
    {
        public string codSofer;
        public string latitudine;
        public string longitudine;
        public string data;
    }

    public class Borderou
    {
        public string cod;
        public string dataEmitere;
    }

}