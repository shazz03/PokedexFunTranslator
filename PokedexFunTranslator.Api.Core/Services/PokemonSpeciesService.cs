using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PokedexFunTranslator.Api.Core.Configuration;
using PokedexFunTranslator.Api.Core.Models;

namespace PokedexFunTranslator.Api.Core.Services
{
    public class PokemonSpeciesService : IPokemonSpeciesService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly PokemonApiConfig _apiConfig;
        private readonly ITranslationService _translation;

        public PokemonSpeciesService(IHttpClientFactory clientFactory, IOptions<PokemonApiConfig> apiConfig, ITranslationService translation)
        {
            _clientFactory = clientFactory;
            _translation = translation;
            _apiConfig = apiConfig?.Value;
        }

        public async Task<PokemonSpeciesResponseDto> Get(string name)
        {
            if (!Validate(name))
                return new PokemonSpeciesResponseDto
                {
                    Response = new ResponseModel { StatusCode = HttpStatusCode.BadRequest }
                };

            var response = await GetPokemonSpeciesResponse(name);
            if (!response.IsSuccessStatusCode)
            {
                return new PokemonSpeciesResponseDto
                {
                    Response = new ResponseModel { StatusCode = response.StatusCode }
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<PokemonSpeciesModel>(content);

            return new PokemonSpeciesResponseDto
            {
                Name = data.Name,
                Description = data.FlavorTextEntries?.FirstOrDefault(l => l.Language.Name == "en")?.FlavorText,
                Habitat = data.Habitat?.Name,
                IsLegendary = data.IsLegendary,
                Response = new ResponseModel { StatusCode = HttpStatusCode.OK }
            };

        }

        private static bool Validate(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        private async Task<HttpResponseMessage> GetPokemonSpeciesResponse(string name)
        {
            var httpClient = _clientFactory.CreateClient("PokemonApiClient");

            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_apiConfig.BaseUrl}/pokemon-species/{name}")
            };

            return await httpClient.SendAsync(httpRequest);
        }

        public async Task<PokemonSpeciesResponseDto> GetTranslated(string name)
        {
            var pokemon = await Get(name);
            if (pokemon.Response.StatusCode != HttpStatusCode.OK)
                return new PokemonSpeciesResponseDto
                {
                    Response = new ResponseModel { StatusCode = pokemon.Response.StatusCode }
                };

            if (string.IsNullOrWhiteSpace(pokemon.Description))
                return pokemon;

            var translationType = TranslationTypeOptions.Shakespeare;
            if (pokemon.Habitat?.ToLower() == "cave" || (pokemon.IsLegendary ?? false))
            {
                translationType = TranslationTypeOptions.Yoda;
            }

            var translation = await _translation.Translate(pokemon.Description, translationType);
            if (!string.IsNullOrWhiteSpace(translation?.Contents?.Translated))
            {
                pokemon.Description = translation.Contents.Translated;
            }

            return pokemon;
        }
    }
}
