namespace Q1.DTOs
{
    public class RoomTypeDetailRoomDTO
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class RoomTypeDetailDTO
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public List<SearchServiceDTO> Services { get; set; } = new List<SearchServiceDTO>();
        public List<RoomTypeDetailRoomDTO> Rooms { get; set; } = new List<RoomTypeDetailRoomDTO>();
    }
}
