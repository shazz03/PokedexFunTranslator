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
    public class TranslationServiceTests
    {
        private readonly Mock<IHttpClientFactory> _clientFactoryMock;
        private readonly IOptions<FunTranslationApiConfig> _config;

        public TranslationServiceTests()
        {
            _config = Options.Create(new FunTranslationApiConfig
            {
                BaseUrl = "http://test.com/",
            });

            _clientFactoryMock = new Mock<IHttpClientFactory>();
        }

        [TestMethod]
        public async Task TranslationService_Translate_Empty_Text_Returns_Null()
        {

            var service = new TranslationService(_clientFactoryMock.Object, _config);
            var result = await service.Translate("", TranslationTypeOptions.Yoda);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TranslationService_Translate_Text_Valid_Result()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();

            var data = new FunTranslationResponseModel
            {
                Contents = new Contents
                {
                    Text = "This is simple text",
                    Translated = "Text is simple this",
                    Translation = "Yoda"
                }
            };

            mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(data))
                });

            var httpClient = new HttpClient(mockMessageHandler.Object);
            _clientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new TranslationService(_clientFactoryMock.Object, _config);
            var result = await service.Translate("This is simple text", TranslationTypeOptions.Yoda);

            Assert.IsNotNull(result);
            Assert.AreEqual("Text is simple this", result.Contents?.Translated);
            Assert.AreEqual("Yoda", result.Contents?.Translation);
        }
    }
}
