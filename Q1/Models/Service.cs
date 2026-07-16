using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal ServicePrice { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<RoomTypeService> RoomTypeServices { get; set; } = new List<RoomTypeService>();
    }
}
