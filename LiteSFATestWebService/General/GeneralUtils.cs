using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace LiteSFATestWebService.General
{
    public class GeneralUtils
    {

        public static string serializeObject(Object customObject)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(customObject);
        }

    }
}