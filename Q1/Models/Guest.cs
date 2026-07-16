using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class Guest
    {
        public int GuestId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
