using LiteSFATestWebService.SapWsSalarizare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class VenitTPR
    {
        public string codN2;
        public string numeN2;
        public string venitGrInc;
        public string pondere;
        public string targetPropCant;
        public string targetRealCant;
        public string um;
        public string targetPropVal;
        public string targetRealVal;
        public string realizareTarget;
        public string targetPonderat;
    }


    public class VenitTCF
    {
        public string venitGrInc;
        public string targetPropus;
        public string targetRealizat;
        public string coefAfectare;
        public string venitTcf;
    }


    public class SalarizareAv
    {
        public string venitTpr;
        public string venitTcf;
    }

    public class NTCF
    {
        public Dictionary<string, int> clientFactAnAnterior;
        public Dictionary<string, int> targetAnCurent;
        public Dictionary<string, int> clientFactAnCurent;
        public Dictionary<string, int> coefAfectare;
    }

    public class ClientiFactAnAnterior
    {
        public int luna;
        public double valoare;
    }


    public class TargetAnCurent
    {
        public int luna;
        public double valoare;
    }


    public class ClientiFactAnCurent
    {
        public int luna;
        public double valoare;
    }

    public class CoeficientAfectare
    {

    }

    public class BazaSalariala
    {
        public double marjaT1;
        public double procentT1;

    }

    public class SalarizareAgentAfis
    {
        public string codAgent;
        public string numeAgent;
        public DatePrincipale datePrincipale;
    }


    public class SalarizareAgent
    {
        public DatePrincipale datePrincipale;
        public List<DetaliiBaza> detaliiBaza;
        public DetaliiTCF detaliiTCF;
        public DetaliiCorectie detaliiCorectie;
        public List<DetaliiIncasari08> detaliiIncasari08;
        public List<DetaliiMalus1> detaliiMalus;
        public List<ZSUM_VANZ_VS> detaliiVanzariVS;
        public List<ZclIncrAlocat> detaliiIncrAlocat;
    }


    public class SalarizareSD : SalarizareAgent
    {
        public List<DetaliiCVS> detaliiCVS;
    }

    public class DatePrincipale
    {
        public double venitMJ_T1;
        public double venitTCF;
        public double corectieIncasare;
        public double venitFinal;
        public double venitCVS;
        public double venitStocNociv;
        public double venitIncrucisate;
    }


    public class DetaliiBaza
    {
        public string numeClient;
        public string codSintetic;
        public string numeSintetic;
        public double valoareNeta;
        public double T0;
        public double T1A;
        public double T1D;
        public double T1;
        public double venitBaza;
    }

    public class DetaliiTCF
    {
        public double venitBaza;
        public string clientiAnterior = "0";
        public string clientiCurent = "0";
        public string target = "0";
        public double coeficient;
        public double venitTcf;
    }

    public class DetaliiCorectie
    {
        public double venitBaza;
        public double incasari08;
        public double malus;
        public double venitCorectat;
    }


    public class DetaliiIncasari08
    {
        public string numeClient;
        public double valoareIncasare;
        public double venitCorectat;
    }


    public class DetaliiCVS
    {
        public string agent;
        public double venitBaza;
        public double venitCvs;
        public double valTotal;
        public double valNociv;
        public double prag;
        public double procent;

        

    }


    public class DetaliiMalus1
    {
        public string numeClient;
        public string codClient;
        public double valoareFactura;
        public double penalizare;

        public string nrFactura;
        public string dataFactura;
        public int tpFact;
        public int tpAgreat;
        public int tpIstoric;
        public double valIncasare;
        public string dataIncasare;
        public int zileIntarziere;
        public double coefPenalizare;

    }

    public class DetaliiVenitVS
    {
        public double coefSal;
        public string ename;
        public string matnr;
        public double netwrCalc;
        public double netwrVf;
        public double netwrVfRed;
        public string pernr;
        public double posnrVf;
        public double procCant;
        public string shortStr;
        public string spart;
        public double T0prim;
        public double T1a;
        public double T1aProc;
        public double T1d;
        public double T1dProc;
        public double T1net;
        public string VbelnVf;
        public string VbelnVfRed;
        public double venitBaza;
        public string werks;
        public double ZmarjaCoef;
    }

}