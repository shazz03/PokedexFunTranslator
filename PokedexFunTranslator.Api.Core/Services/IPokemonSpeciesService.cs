using System.Threading.Tasks;
using PokedexFunTranslator.Api.Core.Models;

namespace PokedexFunTranslator.Api.Core.Services
{
    public interface IPokemonSpeciesService
    {
        Task<PokemonSpeciesResponseDto> Get(string name);
        Task<PokemonSpeciesResponseDto> GetTranslated(string name);
    }
}
