using System.Net;
using System.Text.Json.Serialization;

namespace PokedexFunTranslator.Api.Core.Models
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
    }
}
