using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Seat
{
    public int AircraftID { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string? ClassType { get; set; }

    public virtual Aircraft Aircraft { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
