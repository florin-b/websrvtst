using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TiparireDocumenteTest
{
    public class UserInfo
    {
        public string logonStatus;
        public string departament;
        public string filiala;
        public string tipAcces;
        public string codUser;
        public string numeUser;

    }


    public class Document
    {
        public string id;
        public string client;
        public string emitere;
        public string codArticol;
        public string numeArticol;
        public string pozitieArticol;
        public string cantitate;
        public string um;
        public string isPregatit;
    }



    public class DocumentTiparit
    {
        public String id;
        public String dataEmitere;
        public String client;
        public String departament;
        public String filiala;
        public String seTipareste;
    }


    


}