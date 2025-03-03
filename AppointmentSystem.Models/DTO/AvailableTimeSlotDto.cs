namespace AppointmentSystem.Models.DTO
{
    using System;
    using Newtonsoft.Json;

    public class AvailableTimeSlotDto
    {
        [JsonProperty("available_count")]
        public int AvailableCount { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }
    }
}
