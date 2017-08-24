using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class ComandaCreataCLP
    {
        public string antetComanda;
        public string listArticole;
    }

    public class AntetComandaCLP
    {
        public string codClient;
        public string codJudet;
        public string localitate;
        public string strada;
        public string persCont;
        public string telefon;
        public string codFilialaDest;
        public string dataLivrare;
        public string tipPlata;
        public string tipTransport;
        public string depozDest;
        public string selectedAgent;
        public string cmdFasonate;
        public string numeClientCV;
        public string observatiiCLP;
        public string tipMarfa;
        public string masaMarfa;
        public string tipCamion;
        public string tipIncarcare;
        public string tonaj;

        public override string ToString()
        {
            return "AntetComandaCLP [codClient=" + codClient + ", codJudet=" + codJudet + ", localitate=" + localitate
                    + ", strada=" + strada + ", persCont=" + persCont + ", telefon=" + telefon + ", codFilialaDest=" + codFilialaDest + ", dataLivrare=" + dataLivrare + ", tipPlata=" + tipPlata
                    + ", tipTransport=" + tipTransport + ", depozDest=" + depozDest + ", selectedAgent=" + selectedAgent + ", observatii=" + observatiiCLP + "]";
        }


    }


    public class ArticolComandaCLP
    {
        public string cod;
        public string nume;
        public string cantitate;
        public string umBaza;
        public string depozit;
        public string status;
        public string depart;

        public override string ToString()
        {
            return "ArticolComandaCLP [cod=" + cod + ", nume=" + nume + ", cantitate=" + cantitate
                    + ", umBaza=" + umBaza + ", depozit=" + depozit + ", status=" + status + ", depart=" + depart + "]";
        }

    }


    public class DateComanda
    {
        public string codAgent;
        public string codClient;
        public string filiala;
        public string listArticole;
    }

    public class ClpComanda
    {
        public string nrDocument;
        public string data;
        public string tip;
    }




}