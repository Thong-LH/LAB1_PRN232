using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class RoomTypeService
    {
        public int RoomTypeId { get; set; }
        public int ServiceId { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual RoomType? RoomType { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual Service? Service { get; set; }
    }
}
