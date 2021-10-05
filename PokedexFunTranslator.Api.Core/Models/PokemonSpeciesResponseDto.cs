using System.Text.Json.Serialization;

namespace PokedexFunTranslator.Api.Core.Models
{
    public class PokemonSpeciesResponseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Habitat { get; set; }
        public bool? IsLegendary { get; set; }

        [JsonIgnore]
        public ResponseModel Response { get; set; }
    }
}
