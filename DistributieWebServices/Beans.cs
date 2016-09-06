using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributieTESTWebServices
{
    public class Borderouri
    {
        public string numarBorderou;
        public string dataEmiterii;
        public string evenimentBorderou;
        public string tipBorderou;

    }


    public class FacturiBorderou
    {
        public string codFurnizor;
        public string numeFurnizor;
        public string adresaFurnizor;
        public string codAdresaFurnizor;
        public string sosireFurnizor;
        public string plecareFurnizor;


        public string codClient;
        public string numeClient;
        public string adresaClient;
        public string codAdresaClient;
        public string sosireClient;
        public string plecareClient;

        public string dataStartCursa;

        public string pozitie;
        public string nrFactura;


    }


    public class EvenimentBorderou
    {
        public string numeClient;
        public string codClient;
        public string codAdresa;
        public string adresa;
        public string oraStartCursa;
        public string kmStartCursa;
        public string oraSosireClient;
        public string kmSosireClient;
        public string oraPlecare;
    }


    public class Eveniment
    {
        public string eveniment;
        public string data;
        public string ora;
        public string distantaKM;
    }


    public class EvenimentNou
    {
        public string codSofer;
        public string document;
        public string client;
        public string codAdresa;
        public string eveniment;
        public string truckData;
        public string tipEveniment;
        public string data;
        public string ora;
    }


    public class ArticoleFactura
    {
        public string nume;
        public string cantitate;
        public string umCant;
        public string tipOperatiune;
        public string departament;
        public string greutate;
        public string umGreutate;
    }

    public class User
    {
        public string status;
        public string nume;
        public string id;
        public string filiala;
        public string departament;
        public string tipAcces;
        public InitStatus initStatus;

    }

    public class InitStatus
    {
        public string document;
        public string client;
        public string eveniment;
        public string tipDocument;
    }


    public class EvenimentStop
    {
        public string idEveniment;
        public string codSofer;
        public string codBorderou;
        public string codEveniment;
    }

    public class EvenimentStopIncarcare
    {
        public string document;
        public string codSofer;
        public string data;
        public string ora;
    }

    public class Etapa
    {
        public string borderou;
        public string client;
        public string codAdresa;
        public string pozitie;
        public string document;
    }
    


}