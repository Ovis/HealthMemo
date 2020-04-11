﻿using System;
using System.Text.Json.Serialization;

namespace PostDietProgress.Entities.PostEntity
{
    [Serializable]
    sealed class Discord
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}