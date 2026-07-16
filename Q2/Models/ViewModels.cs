using System.Collections.Generic;

namespace Q2.Models
{
    public class ServiceVM
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal ServicePrice { get; set; }
    }

    public class RoomVM
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class RoomTypeSearchVM
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public List<ServiceVM> Services { get; set; } = new List<ServiceVM>();
    }

    public class RoomTypeDetailVM
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public List<ServiceVM> Services { get; set; } = new List<ServiceVM>();
        public List<RoomVM> Rooms { get; set; } = new List<RoomVM>();
    }

    public class RoomTypeIndexVM
    {
        public int SelectedServiceId { get; set; }
        public string SelectedPriceRange { get; set; } = "All prices";
        public List<ServiceVM> Services { get; set; } = new List<ServiceVM>();
        public List<RoomTypeSearchVM> RoomTypes { get; set; } = new List<RoomTypeSearchVM>();
    }
}
