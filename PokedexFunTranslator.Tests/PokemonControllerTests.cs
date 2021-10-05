using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PokedexFunTranslator.Api.Controllers;
using PokedexFunTranslator.Api.Core.Models;
using PokedexFunTranslator.Api.Core.Services;

namespace PokedexFunTranslator.Tests
{
    [TestClass]
    public class PokemonControllerTests
    {
        private readonly Mock<IPokemonSpeciesService> _pokemonServiceMock;

        public PokemonControllerTests()
        {
            _pokemonServiceMock = new Mock<IPokemonSpeciesService>();
        }

        [TestMethod]
        public async Task PokemonController_Get_Empty_Name_Returns_BadRequest()
        {
            _pokemonServiceMock.Setup(s => s.Get("")).ReturnsAsync(new PokemonSpeciesResponseDto
            { Response = new ResponseModel { StatusCode = HttpStatusCode.BadRequest } });
            var controller = new PokemonController(_pokemonServiceMock.Object);

            var result = await controller.Get("");
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task PokemonController_Get_Null_Name_Returns_BadRequest()
        {
            _pokemonServiceMock.Setup(s => s.Get(null)).ReturnsAsync(new PokemonSpeciesResponseDto
            { Response = new ResponseModel { StatusCode = HttpStatusCode.BadRequest } });
            var controller = new PokemonController(_pokemonServiceMock.Object);

            var result = await controller.Get(null);
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task PokemonController_Get_Invalid_Name_Returns_NotFound()
        {
            _pokemonServiceMock.Setup(s => s.Get("test123")).ReturnsAsync(new PokemonSpeciesResponseDto
            { Response = new ResponseModel { StatusCode = HttpStatusCode.NotFound } });
            var controller = new PokemonController(_pokemonServiceMock.Object);

            var result = await controller.Get("test");
            var statusCodeResult = (IStatusCodeActionResult)result;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.NotFound, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task PokemonController_Get_Valid_Name_Returns_Result()
        {
            _pokemonServiceMock.Setup(s => s.Get("mewtwo")).ReturnsAsync(new PokemonSpeciesResponseDto
            {
                Response = new ResponseModel { StatusCode = HttpStatusCode.OK },
                Name = "mewtwo",
                IsLegendary = true,
                Habitat = "rare"
            });
            var controller = new PokemonController(_pokemonServiceMock.Object);

            var result = await controller.Get("mewtwo");
            var statusCodeResult = (ObjectResult)result;
            var pokemonSpecie = (PokemonSpeciesResponseDto)statusCodeResult.Value;
            Assert.IsNotNull(result);
            Assert.IsTrue(pokemonSpecie.IsLegendary ?? false);
            Assert.AreEqual("rare", pokemonSpecie.Habitat);
            Assert.AreEqual((int)HttpStatusCode.OK, statusCodeResult.StatusCode);
        }


    }
}
