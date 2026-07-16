namespace Q1.DTOs
{
    public class RoomTypeDTO
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public int ServiceCount { get; set; }
    }
}
