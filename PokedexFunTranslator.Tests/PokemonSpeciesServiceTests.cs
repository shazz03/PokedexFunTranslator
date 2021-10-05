using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using PokedexFunTranslator.Api.Core.Configuration;
using PokedexFunTranslator.Api.Core.Models;
using PokedexFunTranslator.Api.Core.Services;

namespace PokedexFunTranslator.Tests
{
    [TestClass]
    public class PokemonSpeciesServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly IOptions<PokemonApiConfig> _config;
        private readonly Mock<ITranslationService> _translationServiceMock;

        public PokemonSpeciesServiceTests()
        {
            _config = Options.Create(new PokemonApiConfig
            {
                BaseUrl = "http://test.com/",
            });

            _clientFactoryMock = new Mock<IHttpClientFactory>();
            _translationServiceMock = new Mock<ITranslationService>();

        }

        public PokemonSpeciesService GetPokemonService(PokemonSpeciesModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();

            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonSerializer.Serialize(model))
                });

            var httpClient = new HttpClient(mockMessageHandler.Object);
            _clientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new PokemonSpeciesService(_clientFactoryMock.Object, _config, _translationServiceMock.Object);
        }

        [TestMethod]
        public async Task PokemonService_GetSpecies_Empty_Name_Returns_BadRequest()
        {
            var service = new PokemonSpeciesService(_clientFactoryMock.Object, _config, _translationServiceMock.Object);
            var result = await service.Get("");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PokemonService_Get_Null_Name_Returns_BadRequest()
        {
            var service = new PokemonSpeciesService(_clientFactoryMock.Object, _config, _translationServiceMock.Object);
            var result = await service.Get(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PokemonService_Get_Valid_Name_Returns_Valid_Result()
        {
            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat
                {
                    Name = "rare"
                },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry>
                {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                    {
                        Name = "en"
                    }}
                }
            };

            var service = GetPokemonService(data);

            var result = await service.Get("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual("Mimikyu", result.Name);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PokemonService_Get_Language_EN_Valid_Name_Returns_Valid_Description()
        {
            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "rare" },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }},
                    new FlavorTextEntry{FlavorText = "this is Japanese language test",
                        Language = new Language
                        {
                            Name = "ja"
                        }}
                },
            };

            var service = GetPokemonService(data);

            var result = await service.Get("Mimikyu");

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Master Obiwan has lost a planet.", result.Description);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PokemonService_Get_InValid_Name_Returns_Result_NotFound()
        {

            var service = GetPokemonService(null, HttpStatusCode.NotFound);

            var result = await service.Get("coco");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NotFound, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_Not_IsLegendary_Habitat_Not_Cave_Returns_Shakespeare_Result()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(new FunTranslationResponseModel
                {
                    Contents = new Contents
                    {
                        Text = "Master Obiwan has lost a planet.",
                        Translated = "Master obiwan hath did lose a planet.",
                        Translation = "shakespeare"
                    }
                });

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "rare" },
                IsLegendary = false,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                },
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Master obiwan hath did lose a planet.", result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_IsLegendary_Habitat_Not_Cave_Returns_Yoda_Result()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(new FunTranslationResponseModel
                {
                    Contents = new Contents
                    {
                        Text = "Master Obiwan has lost a planet.",
                        Translated = "Lost a planet,  master obiwan has.",
                        Translation = "yoda"
                    }
                });

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "rare" },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                },
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Lost a planet,  master obiwan has.", result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_Not_IsLegendary_Habitat_Cave_Returns_Yoda_Result()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(new FunTranslationResponseModel
                {
                    Contents = new Contents
                    {
                        Text = "Master Obiwan has lost a planet.",
                        Translated = "Lost a planet,  master obiwan has.",
                        Translation = "yoda"
                    }
                });

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "cave" },
                IsLegendary = false,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                },
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Lost a planet,  master obiwan has.", result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_IsLegendary_Habitat_Cave_Returns_Yoda_Result()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(new FunTranslationResponseModel
                {
                    Contents = new Contents
                    {
                        Text = "Master Obiwan has lost a planet.",
                        Translated = "Lost a planet,  master obiwan has.",
                        Translation = "yoda"
                    }
                });

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "cave" },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                }
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Lost a planet,  master obiwan has.", result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_Empty_Description_Returns_Empty_Translation_Text()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(() => null);

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "cave" },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                }
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.AreEqual("", result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_Null_Description_Returns_Empty_Translation_Text()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(() => null);

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "cave" },
                IsLegendary = true,
                FlavorTextEntries = null
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNull(result.Description);
        }

        [TestMethod]
        public async Task PokemonService_GetTranslated_Api_Returns_Null_Use_Normal_Description()
        {
            _translationServiceMock.Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<TranslationTypeOptions>()))
                .ReturnsAsync(() => null);

            var data = new PokemonSpeciesModel
            {
                Name = "Mimikyu",
                Habitat = new Habitat { Name = "cave" },
                IsLegendary = true,
                FlavorTextEntries = new List<FlavorTextEntry> {
                    new FlavorTextEntry{FlavorText = "Master Obiwan has lost a planet.",
                        Language = new Language
                        {
                            Name = "en"
                        }}
                },
            };

            var service = GetPokemonService(data);

            var result = await service.GetTranslated("Mimikyu");

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Description);
            Assert.AreEqual("Master Obiwan has lost a planet.", result.Description);
        }
    }
}
