using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Flight
{
    public int FlightId { get; set; }

    public string? FlightNumber { get; set; }

    public decimal? DistanceKm { get; set; }

    public string? Status { get; set; }

    public int? AircraftId { get; set; }

    public int? DepartureAirportId { get; set; }

    public DateTime? DepartureTime { get; set; }

    public int? ArrivalAirportId { get; set; }

    public DateTime? ArrivalTime { get; set; }

    public virtual Aircraft? Aircraft { get; set; }

    public virtual Airport? ArrivalAirport { get; set; }

    public virtual Airport? DepartureAirport { get; set; }

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
