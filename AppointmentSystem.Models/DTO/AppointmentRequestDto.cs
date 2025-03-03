using Newtonsoft.Json;

namespace AppointmentSystem.Models.DTO
{
    public class AppointmentRequestDto
    {
        [JsonProperty("Date")]
        public string Date { get; set; }

        [JsonProperty("Products")]
        public List<string> Products { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Rating")]
        public string Rating { get; set; }
    }
}
