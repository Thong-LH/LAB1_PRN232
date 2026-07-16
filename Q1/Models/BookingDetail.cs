using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class BookingDetail
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int NightCount { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual Booking? Booking { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual Room? Room { get; set; }
    }
}
