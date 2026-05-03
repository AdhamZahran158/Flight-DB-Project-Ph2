using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public DateOnly? BookingDate { get; set; }

    public string? BookingStatus { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? PassportNum { get; set; }

    public virtual Tenant? PassportNumNavigation { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
