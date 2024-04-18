using LiteSFATestWebService.General;
using LiteSFATestWebService.SAPWebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService
{
    public class OperatiiStocuri
    {
        public string getStocSap(string codArticol, string um, string filiala, string tipUser)
        {

            string retVal = "";
            SAPWebServices.ZTBL_WEBSERVICE webService = new ZTBL_WEBSERVICE();

            SAPWebServices.ZstocSfa inParam = new ZstocSfa();
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
            webService.Credentials = nc;
            webService.Timeout = 300000;

            inParam.IpMatnr = codArticol;
            inParam.IpPlant = filiala;
            inParam.Meins = um;

            SAPWebServices.ZstocSfaResponse outParams = webService.ZstocSfa(inParam);

            retVal = outParams.EpStoc + "#" + outParams.Meins + "#1";

            return retVal;

        }
    }
}