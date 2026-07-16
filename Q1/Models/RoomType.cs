using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Q1.Models
{
    public class RoomType
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<RoomTypeService> RoomTypeServices { get; set; } = new List<RoomTypeService>();
    }
}
