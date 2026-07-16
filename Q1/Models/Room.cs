namespace Q1.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public int RoomTypeId { get; set; }
        public string Status { get; set; } = null!;

        public virtual RoomType? RoomType { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}
