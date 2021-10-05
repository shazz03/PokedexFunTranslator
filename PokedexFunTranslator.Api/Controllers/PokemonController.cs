using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PokedexFunTranslator.Api.Core.Services;

namespace PokedexFunTranslator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokemonController : Controller
    {
        private readonly IPokemonSpeciesService _pokemonService;

        public PokemonController(IPokemonSpeciesService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        [HttpGet("translated/{name}")]
        public async Task<IActionResult> GetTranslated(string name)
        {
            
            var result = await _pokemonService.GetTranslated(name);

            if (result?.Response?.StatusCode != null)
                return StatusCode((int)result.Response?.StatusCode,
                    result.Response?.StatusCode == HttpStatusCode.OK ? result : null);

            return NotFound();
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            var result = await _pokemonService.Get(name);

            if (result?.Response?.StatusCode != null)
                return StatusCode((int)result.Response?.StatusCode,
                    result.Response?.StatusCode == HttpStatusCode.OK ? result : null);

            return NotFound();
        }
    }
}
