﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Drawing;
using System.IO;



namespace Flota
{

    [WebService(Namespace = "http://flotaTest.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class FlotaWS : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string getListaSoferi(string filiala)
        {
            OperatiiSofer opSofer = new OperatiiSofer();
            return opSofer.getListaSoferi(filiala);
        }

        [WebMethod]
        public string getActivitateSofer(string codSofer, string dataStart, string dataStop)
        {
            OperatiiSofer opSofer = new OperatiiSofer();
            return opSofer.getActivitateSofer(codSofer, dataStart, dataStop);
        }

        [WebMethod]
        public string getActivitateDocument(string nrDocument)
        {
            OperatiiSofer opSofer = new OperatiiSofer();
            return opSofer.getActivitateDocument(nrDocument);
        }


        [WebMethod]
        public string getProduct(string nrDocument)
        {
            FlotaTESTWS.HelloWorld hello = new FlotaTESTWS.HelloWorld();
            return hello.getProductData(nrDocument).ToString();
        }

        [WebMethod]
        public string getRutaDocument(string nrDocument)
        {
            OperatiiSofer opSofer = new OperatiiSofer();
            return opSofer.getRutaDocument(nrDocument);
        }

        [WebMethod]
        public string getPozitieSoferi(string listSoferi)
        {
            Localizare localizare = new Localizare();
            return localizare.getPozitieCurenta(listSoferi);
        }

        [WebMethod]
        public string addTabletaSofer(string codSofer, string codTableta, string creatDe)
        {
            OperatiiTablete opTablete = new OperatiiTablete();
            return opTablete.addTabletaSofer(codSofer, codTableta, creatDe);
        }

        [WebMethod]
        public string removeTabletaSofer(string codSofer, string creatDe)
        {
            OperatiiTablete opTablete = new OperatiiTablete();
            return opTablete.removeTabletaSofer(codSofer, creatDe);
        }


        [WebMethod]
        public string getTableteSofer(string codSofer)
        {
            OperatiiTablete opTablete = new OperatiiTablete();
            return opTablete.getTableteSofer(codSofer);
        }

        [WebMethod]
        public string getBorderouri(string codSofer, string dataStart, string dataStop)
        {
            OperatiiDocumente opDocumente = new OperatiiDocumente();
            return opDocumente.getBorderouri( codSofer,  dataStart,  dataStop);
        }


        [WebMethod]
        public Bitmap getGps()
        {

            Bitmap buddyIcon = null;
            try
            {

                Uri uri = new Uri("https://maps.googleapis.com/maps/api/staticmap?center=40.749825,-73.987963&markers=40.749825,-73.987963&size=600x300&zoom=12");

                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                Stream imageStream = httpResponse.GetResponseStream();
                buddyIcon = new Bitmap(imageStream);

                //buddyIcon.Save("d:\\123.jpg", System.Drawing.Imaging.ImageFormat.Jpeg); 

                httpResponse.Close();
                imageStream.Close();


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }

            return buddyIcon;
        }


    }
}
