using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Flight_Reservation_App.Models;

public partial class AirlineDbContext : DbContext
{
    public AirlineDbContext()
    {
    }

    public AirlineDbContext(DbContextOptions<AirlineDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aircraft> Aircraft { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<Baggage> Baggages { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<TenantPhone> TenantPhones { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=AirlineDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aircraft>(entity =>
        {
            entity.HasKey(e => e.AircraftId).HasName("PK__Aircraft__F75CBC0B1BED214B");

            entity.Property(e => e.AircraftId)
                .ValueGeneratedNever()
                .HasColumnName("AircraftID");
            entity.Property(e => e.CargoCapacity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaxTakeOffWeight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.AirportId).HasName("PK__Airport__E3DBE08A7B90C8BF");

            entity.ToTable("Airport");

            entity.HasIndex(e => e.Iatacode, "UQ__Airport__EFD6F5BECCE7B158").IsUnique();

            entity.Property(e => e.AirportId)
                .ValueGeneratedNever()
                .HasColumnName("AirportID");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Iatacode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("IATACode");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Baggage>(entity =>
        {
            entity.HasKey(e => new { e.TicketId, e.BaggageId }).HasName("PK__Baggage__313639E794760218");

            entity.ToTable("Baggage");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.BaggageId).HasColumnName("BaggageID");
            entity.Property(e => e.ExtraFee).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Baggage__TicketI__59063A47");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951ACDE68B24BE");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId)
                .ValueGeneratedNever()
                .HasColumnName("BookingID");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PassportNum)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.PassportNumNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PassportNum)
                .HasConstraintName("FK__Booking__Passpor__45F365D3");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FlightId).HasName("PK__Flight__8A9E148EF3934368");

            entity.ToTable("Flight");

            entity.Property(e => e.FlightId)
                .ValueGeneratedNever()
                .HasColumnName("FlightID");
            entity.Property(e => e.AircraftId).HasColumnName("AircraftID");
            entity.Property(e => e.ArrivalAirportId).HasColumnName("ArrivalAirportID");
            entity.Property(e => e.ArrivalTime).HasColumnType("datetime");
            entity.Property(e => e.DepartureAirportId).HasColumnName("DepartureAirportID");
            entity.Property(e => e.DepartureTime).HasColumnType("datetime");
            entity.Property(e => e.DistanceKm)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DistanceKM");
            entity.Property(e => e.FlightNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Aircraft).WithMany(p => p.Flights)
                .HasForeignKey(d => d.AircraftId)
                .HasConstraintName("FK__Flight__Aircraft__3C69FB99");

            entity.HasOne(d => d.ArrivalAirport).WithMany(p => p.FlightArrivalAirports)
                .HasForeignKey(d => d.ArrivalAirportId)
                .HasConstraintName("FK__Flight__ArrivalA__3E52440B");

            entity.HasOne(d => d.DepartureAirport).WithMany(p => p.FlightDepartureAirports)
                .HasForeignKey(d => d.DepartureAirportId)
                .HasConstraintName("FK__Flight__Departur__3D5E1FD2");

            entity.HasMany(d => d.Trips).WithMany(p => p.Flights)
                .UsingEntity<Dictionary<string, object>>(
                    "FlightTrip",
                    r => r.HasOne<Trip>().WithMany()
                        .HasForeignKey("TripId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__FlightTri__TripI__4E88ABD4"),
                    l => l.HasOne<Flight>().WithMany()
                        .HasForeignKey("FlightId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__FlightTri__Fligh__4D94879B"),
                    j =>
                    {
                        j.HasKey("FlightId", "TripId").HasName("PK__FlightTr__7F83D39F50C65C1A");
                        j.ToTable("FlightTrip");
                        j.IndexerProperty<int>("FlightId").HasColumnName("FlightID");
                        j.IndexerProperty<int>("TripId").HasColumnName("TripID");
                    });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A58E84780C6");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("PaymentID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Payment__Booking__48CFD27E");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => new { e.AircraftId, e.SeatNumber }).HasName("PK__Seat__E9BE60BB184762FC");

            entity.ToTable("Seat");

            entity.Property(e => e.AircraftId).HasColumnName("AircraftID");
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.ClassType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Aircraft).WithMany(p => p.Seats)
                .HasForeignKey(d => d.AircraftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seat__AircraftID__5165187F");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.PassportNum).HasName("PK__Tenant__A1A4EE5F3D9764C6");

            entity.ToTable("Tenant");

            entity.Property(e => e.PassportNum)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fname)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Lname)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mname)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NationalId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("NationalID");
            entity.Property(e => e.Nationality)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TenantPhone>(entity =>
        {
            entity.HasKey(e => new { e.PassportNum, e.PhoneNumber }).HasName("PK__TenantPh__29FB5ABC78DFB5C1");

            entity.ToTable("TenantPhone");

            entity.Property(e => e.PassportNum)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.PassportNumNavigation).WithMany(p => p.TenantPhones)
                .HasForeignKey(d => d.PassportNum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TenantPho__Passp__4316F928");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC6270350FB2C");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId)
                .ValueGeneratedNever()
                .HasColumnName("TicketID");
            entity.Property(e => e.AircraftId).HasColumnName("AircraftID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TripId).HasColumnName("TripID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Ticket__BookingI__5441852A");

            entity.HasOne(d => d.Trip).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK__Ticket__TripID__5535A963");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => new { d.AircraftId, d.SeatNumber })
                .HasConstraintName("FK__Ticket__5629CD9C");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PK__Trip__51DC711E0E394E7A");

            entity.ToTable("Trip");

            entity.Property(e => e.TripId)
                .ValueGeneratedNever()
                .HasColumnName("TripID");
            entity.Property(e => e.TripType)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
