namespace Q1.DTOs
{
    public class SearchServiceDTO
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
    }

    public class SearchRoomTypeDTO
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public List<SearchServiceDTO> Services { get; set; } = new List<SearchServiceDTO>();
    }
}
