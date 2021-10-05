﻿using System.Text.Json.Serialization;

namespace PokedexFunTranslator.Api.Core.Models
{
    public class Success
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class Contents
    {
        [JsonPropertyName("translated")]
        public string Translated { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("translation")]
        public string Translation { get; set; }
    }

    public class FunTranslationResponseModel
    {
        [JsonPropertyName("success")]
        public Success Success { get; set; }

        [JsonPropertyName("contents")]
        public Contents Contents { get; set; }
    }

}
