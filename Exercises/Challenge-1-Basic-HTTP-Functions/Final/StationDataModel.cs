using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServerlessWorkshop.Challenge1
{
    public class StationDataModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("stationModules")]
        public IList<StationModuleModel> StationModules { get; set; }
    }

    public class StationModuleModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("co2")]
        public int Co2 { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }
}