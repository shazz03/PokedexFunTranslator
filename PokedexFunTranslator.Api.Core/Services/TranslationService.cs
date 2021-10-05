using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PokedexFunTranslator.Api.Core.Configuration;
using PokedexFunTranslator.Api.Core.Models;

namespace PokedexFunTranslator.Api.Core.Services
{
    public class TranslationService : ITranslationService
    {

        private readonly FunTranslationApiConfig _config;
        private readonly IHttpClientFactory _clientFactory;

        public TranslationService(IHttpClientFactory clientFactory, IOptions<FunTranslationApiConfig> config)
        {
            _clientFactory = clientFactory;
            _config = config?.Value;
        }

        public async Task<FunTranslationResponseModel> Translate(string text, TranslationTypeOptions type)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                var httpClient = _clientFactory.CreateClient("FunTranslationApiClient");

                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_config.BaseUrl}/translate/{type.ToString().ToLower()}.json"),
                    Method = HttpMethod.Post,
                    Content = new StringContent($"text={text}", Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                var response = await httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<FunTranslationResponseModel>(content);

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
