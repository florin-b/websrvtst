using LiteSFATestWebService.SAPWebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService
{
    public class OperatiiMacara
    {


        public String getCostMacara(String unitLog, String listArt)
        {



            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<ArticolCalculDesc> listArtCmd = serializer.Deserialize<List<ArticolCalculDesc>>(listArt);


            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZNrPaleti inParam = new SAPWebServices.ZNrPaleti();
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;


            SAPWebServices.ZstNrPaleti[] nrpal = new ZstNrPaleti[listArtCmd.Count];

            for (int i = 0; i < listArtCmd.Count; i++)
            {
                nrpal[i] = new ZstNrPaleti();

                string codArt = listArtCmd[i].cod;
                if (listArtCmd[i].cod.Length == 8)
                    codArt = "0000000000" + listArtCmd[i].cod;


                nrpal[i].Matnr = codArt;
                nrpal[i].Meins = listArtCmd[i].um;
                nrpal[i].Menge = Convert.ToDecimal(listArtCmd[i].cant);

            }


            inParam.ItTable = nrpal;
            inParam.IpWerks = unitLog;


            SAPWebServices.ZNrPaletiResponse resp = webService.ZNrPaleti(inParam);

            SAPWebServices.ZstEtPaleti[] valPaleti = resp.EtValpal;


            CostDescarcare costDescarcare = new CostDescarcare();
            costDescarcare.sePermite = !resp.EpMacara.Equals("X");

            

            List<ArticolDescarcare> listArticole = new List<ArticolDescarcare>();

            for (int i = 0; i < valPaleti.Length; i++)
            {
                ArticolDescarcare articol = new ArticolDescarcare();
                articol.cod = valPaleti[i].Matnr;
                articol.depart = valPaleti[i].Spart;
                articol.valoare = valPaleti[i].Valpal.Trim();
                listArticole.Add(articol);
            }


            costDescarcare.articoleDescarcare = listArticole;

            return new JavaScriptSerializer().Serialize(costDescarcare);



            

        }


    }
}