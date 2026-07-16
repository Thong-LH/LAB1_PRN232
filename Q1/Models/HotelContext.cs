using Microsoft.EntityFrameworkCore;

namespace Q1.Models
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<RoomTypeService> RoomTypeServices { get; set; } = null!;
        public DbSet<Guest> Guests { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingDetail> BookingDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.ToTable("RoomTypes");
                entity.HasKey(e => e.RoomTypeId);
                entity.Property(e => e.RoomTypeId).HasColumnName("RoomTypeID").ValueGeneratedOnAdd();
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Rooms");
                entity.HasKey(e => e.RoomId);
                entity.Property(e => e.RoomId).HasColumnName("RoomID").ValueGeneratedOnAdd();
                entity.Property(e => e.RoomTypeId).HasColumnName("RoomTypeID");

                entity.HasOne(d => d.RoomType)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.RoomTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");
                entity.HasKey(e => e.ServiceId);
                entity.Property(e => e.ServiceId).HasColumnName("ServiceID").ValueGeneratedOnAdd();
                entity.Property(e => e.ServicePrice).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<RoomTypeService>(entity =>
            {
                entity.ToTable("RoomTypeServices");
                entity.HasKey(e => new { e.RoomTypeId, e.ServiceId });
                entity.Property(e => e.RoomTypeId).HasColumnName("RoomTypeID");
                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.HasOne(d => d.RoomType)
                    .WithMany(p => p.RoomTypeServices)
                    .HasForeignKey(d => d.RoomTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.RoomTypeServices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Guest>(entity =>
            {
                entity.ToTable("Guests");
                entity.HasKey(e => e.GuestId);
                entity.Property(e => e.GuestId).HasColumnName("GuestID").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingId).HasColumnName("BookingID").ValueGeneratedOnAdd();
                entity.Property(e => e.GuestId).HasColumnName("GuestID");

                entity.HasOne(d => d.Guest)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.GuestId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<BookingDetail>(entity =>
            {
                entity.ToTable("BookingDetails");
                entity.HasKey(e => new { e.BookingId, e.RoomId });
                entity.Property(e => e.BookingId).HasColumnName("BookingID");
                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.HasOne(d => d.Booking)
                    .WithMany(p => p.BookingDetails)
                    .HasForeignKey(d => d.BookingId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.BookingDetails)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }
}
