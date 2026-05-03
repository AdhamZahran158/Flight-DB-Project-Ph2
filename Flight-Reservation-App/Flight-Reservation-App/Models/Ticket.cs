using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public decimal? TicketPrice { get; set; }

    public int? BookingId { get; set; }

    public int? TripId { get; set; }

    public int? AircraftId { get; set; }

    public string? SeatNumber { get; set; }

    public virtual ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();

    public virtual Booking? Booking { get; set; }

    public virtual Seat? Seat { get; set; }

    public virtual Trip? Trip { get; set; }
}
