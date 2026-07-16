using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual Guest? Guest { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}
