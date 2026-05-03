using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Trip
{
    public int TripId { get; set; }

    public string? TripType { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
