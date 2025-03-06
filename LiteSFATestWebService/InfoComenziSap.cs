﻿using LiteSFATestWebService.SAPWebServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class InfoComenziSap
    {

        public static void getDateHeader(List<Comanda> listComenzi)
        {

            try {
                SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webService.Credentials = nc;
                webService.Timeout = 300000;

                ZheadInfocv[] itComenzi = new ZheadInfocv[listComenzi.Count];

                int i = 0;
                foreach (Comanda com in listComenzi)
                {
                    ZheadInfocv itCmd = new ZheadInfocv();
                    itCmd.Nrcmdsap = com.cmdSap;
                    itCmd.Status = " ";
                    itCmd.Waers = " ";
                    itCmd.t0 = 0;
                    itCmd.T0Proc = 0;
                    itCmd.t1 = 0;
                    itCmd.T1Proc = 0;
                    itCmd.ValBrut = 0;
                    itCmd.ValNet = 0;

                    itComenzi[i] = itCmd;

                    i++;
                }

                SAPWebServices.ZgetInfoheadcv infoHead = new ZgetInfoheadcv();
                infoHead.ItComenzi = itComenzi;

                ZgetInfoheadcvResponse response = webService.ZgetInfoheadcv(infoHead);

                foreach (Comanda com in listComenzi)
                {
                    foreach (ZheadInfocv resp in response.ItComenzi)
                    {
                        if (com.cmdSap.Equals(resp.Nrcmdsap))
                        {
                            com.bazaSalariala = Double.Parse(resp.t1.ToString());
                            break;
                        }
                    }
                }

                webService.Dispose();
            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail("getDateHeader: " + ex.ToString());
            }

        }




        public static void getDateArticole(string nrCmd, DateLivrareCmd dateLivrare, List<ArticolComandaRap> listArticole, string tipUser)
        {

            string nrCmdSap = OperatiiComenzi.getNrComandaSap(nrCmd);


            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            SAPWebServices.ZgetInfocv infoCV = new ZgetInfocv();
            infoCV.IpSapdoc = nrCmdSap;
            infoCV.ItDet = new ZdetInfocv[1];

            SAPWebServices.ZgetInfocvResponse response = webService.ZgetInfocv(infoCV);

            ZheadInfocv antet = response.WaHead;

            dateLivrare.marjaT1 = Double.Parse(antet.t1.ToString());
            dateLivrare.procentT1 = Double.Parse(antet.T1Proc.ToString()) / 100;


            dateLivrare.marjaT1Tot = Double.Parse(antet.T1Tot.ToString());
            dateLivrare.procentT1Tot = Double.Parse(antet.T1TotProc.ToString()) / 100;

            if (tipUser.Equals("DV"))
            {
                dateLivrare.marjaBruta = Double.Parse(antet.t0.ToString());
                dateLivrare.procMarjaBruta = Double.Parse(antet.T0Proc.ToString());

                dateLivrare.marjaBrutaTot = Double.Parse(antet.T0Tot.ToString());
                dateLivrare.procMarjaBrutaTot = Double.Parse(antet.T0TotProc.ToString());
            } else
            {
                dateLivrare.marjaBruta = 0;
                dateLivrare.procMarjaBruta = 0;

                dateLivrare.marjaBrutaTot = 0;
                dateLivrare.procMarjaBrutaTot = 0;
            }

            for (int i = 0; i < response.ItDet.Length; i++)
            {
                string codArt = response.ItDet[i].Matnr.TrimStart('0');

                foreach (ArticolComandaRap art in listArticole)
                {
                    if (codArt.Equals(art.codArticol))
                    {
                        art.valT1 = Double.Parse(response.ItDet[i].t1.ToString());
                        art.procT1 = Double.Parse(response.ItDet[i].T1Proc.ToString());

                        break;

                    }
                }

            }


            webService.Dispose();

        }


    }
}