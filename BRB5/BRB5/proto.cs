using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BRB5
{
    public static class proto
    {
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { DateFormatString = "dd.MM.yyyy", FloatParseHandling = FloatParseHandling.Decimal, NullValueHandling = NullValueHandling.Ignore }; //, Culture = MyCulture 

        public static string ToJSON(this object s, string pDateFormatString = null)
        {
            JsonSerializerSettings Setting = pDateFormatString == null ? JsonSettings :
                new JsonSerializerSettings()
                {
                    DateFormatString = pDateFormatString,
                    FloatParseHandling = FloatParseHandling.Decimal,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            var res = JsonConvert.SerializeObject(s, Setting);
            return res;
        }
    }
}
