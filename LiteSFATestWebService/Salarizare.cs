using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using LiteSFATestWebService.WebServiceSalarizareAV;

namespace LiteSFATestWebService
{
    public class Salarizare
    {
        

        public String getSalarizareAV(string codAgent, string departament, string filiala)
        {
            string serResult = "";
            string localDep = departament.Equals("04") ? Service1.getDepartAgent(codAgent) : departament;

            WebServiceSalarizareAV.ZWBS_SAL_AV webService = new ZWBS_SAL_AV();

            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(DatabaseConnections.getUser(), DatabaseConnections.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            WebServiceSalarizareAV.ZgetSalav inParam = new WebServiceSalarizareAV.ZgetSalav();


            WebServiceSalarizareAV.Ztmarja[] zMarja = new Ztmarja[1];
            WebServiceSalarizareAV.ZtprsSapreport[] zTPR = new ZtprsSapreport[1];
            WebServiceSalarizareAV.Zstcf[] zTCF = new Zstcf[1];
            WebServiceSalarizareAV.Zspenalty[] zPenalty = new Zspenalty[1];
            WebServiceSalarizareAV.Zsalav3[] zFinal = new Zsalav3[1];
            WebServiceSalarizareAV.ZprocincSapreport[] zProcente = new ZprocincSapreport[1];


            WebServiceSalarizareAV.Zpernr[] codAgenti = new Zpernr[3];
            codAgenti[0] = new Zpernr();
            codAgenti[0].Pernr = codAgent;

            inParam.An = Utils.getCurrentYear();
            inParam.Divizie = localDep;
            inParam.Luna = Utils.getCurrentMonth();
            inParam.Ul = filiala;
            inParam.ItPernr = codAgenti;

            inParam.GtMarjaAv = zMarja;
            inParam.GtTprsdsAv = zTPR;
            inParam.GtTcfAv = zTCF;
            inParam.GtPenalty99 = zPenalty;
            inParam.GtOuttabAv = zFinal;
            inParam.GtProcenteAv = zProcente;

            WebServiceSalarizareAV.ZgetSalavResponse response = new ZgetSalavResponse();

            response = webService.ZgetSalav(inParam);

            int lenTpr = response.GtTprsdsAv.Length;

            List<VenitTPR> listTpr = new List<VenitTPR>();

            for (int i = 0; i < lenTpr; i++)
            {
                VenitTPR venitTpr = new VenitTPR();

                venitTpr.codN2 = response.GtTprsdsAv[i].Codnivel2;
                venitTpr.numeN2 = response.GtTprsdsAv[i].Descriere;
                venitTpr.venitGrInc = response.GtTprsdsAv[i].Venitgi.ToString();
                venitTpr.pondere = response.GtTprsdsAv[i].Pondere.ToString();
                venitTpr.targetPropCant = response.GtTprsdsAv[i].Targetcant.ToString();
                venitTpr.targetRealCant = response.GtTprsdsAv[i].Targetcantr.ToString();
                venitTpr.um = response.GtTprsdsAv[i].Vrkme;
                venitTpr.targetPropVal = response.GtTprsdsAv[i].Targetval.ToString();
                venitTpr.targetRealVal = response.GtTprsdsAv[i].Targetvalr.ToString();
                venitTpr.realizareTarget = response.GtTprsdsAv[i].Realizaretarget.ToString();
                venitTpr.targetPonderat = response.GtTprsdsAv[i].Targetponderat.ToString();
                listTpr.Add(venitTpr);

            }

            int lenTcf = response.GtTcfAv.Length;

            List<VenitTCF> listTcf = new List<VenitTCF>();

            for (int i = 0; i < lenTcf; i++)
            {
                VenitTCF venitTcf = new VenitTCF();
                venitTcf.venitGrInc = response.GtTcfAv[i].VenitTpr.ToString();
                venitTcf.targetPropus = response.GtTcfAv[i].TargetP.ToString();
                venitTcf.targetRealizat = response.GtTcfAv[i].TargetR.ToString();
                venitTcf.coefAfectare = response.GtTcfAv[i].CoefAf.ToString();
                venitTcf.venitTcf = response.GtTcfAv[i].VenitTcf.ToString();
                listTcf.Add(venitTcf);

            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            string serTpr = serializer.Serialize(listTpr);
            string serTcf = serializer.Serialize(listTcf);

            SalarizareAv salarizareAv = new SalarizareAv();
            salarizareAv.venitTpr = serTpr;
            salarizareAv.venitTcf = serTcf;

            serResult = serializer.Serialize(salarizareAv);

            webService.Dispose();

            return serResult;
        }
    }
}