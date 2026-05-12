using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EtaskMinstry.Api.Configuration
{
    /// <summary>
    /// Mutates the Web API JsonFormatter to use:
    ///   - camelCase property names (CamelCasePropertyNamesContractResolver)
    ///   - ISO 8601 dates (yyyy-MM-ddTHH:mm:ss.fffZ)
    ///   - omit nulls
    ///
    /// Applied to GlobalConfiguration.Configuration.Formatters only, which is
    /// the Web API pipeline. MVC's Json() action result is unaffected because
    /// it uses JavaScriptSerializer, not Newtonsoft.
    /// </summary>
    public static class ApiJsonFormatter
    {
        public static void Apply(MediaTypeFormatterCollection formatters)
        {
            var json = formatters.JsonFormatter;
            var s = json.SerializerSettings;

            s.ContractResolver = new CamelCasePropertyNamesContractResolver();
            s.NullValueHandling = NullValueHandling.Ignore;
            s.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            s.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            // Replace the default ISO converter with one that always emits "Z"
            // (kept default — Newtonsoft's IsoDateTimeConverter does this when
            // DateTimeZoneHandling.Utc is set).
            s.Converters.Add(new IsoDateTimeConverter());

            // Drop the XML formatter so debugging via browser doesn't leak XML.
            formatters.Remove(formatters.XmlFormatter);
        }
    }
}
