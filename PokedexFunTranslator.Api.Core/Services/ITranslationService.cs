using System.Threading.Tasks;
using PokedexFunTranslator.Api.Core.Models;

namespace PokedexFunTranslator.Api.Core.Services
{
    public interface ITranslationService
    {
        Task<FunTranslationResponseModel> Translate(string text, TranslationTypeOptions type);
    }
}
