namespace Q1.DTOs
{
    public class FilteredRoomDTO
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public int TotalNights { get; set; }
        public string CurrentGuest { get; set; } = null!;
        public List<string> ServiceList { get; set; } = new List<string>();
    }
}
