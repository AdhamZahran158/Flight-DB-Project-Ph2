using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Airport
{
    public int AirportId { get; set; }

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? Iatacode { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }

    public virtual ICollection<Flight> FlightArrivalAirports { get; set; } = new List<Flight>();

    public virtual ICollection<Flight> FlightDepartureAirports { get; set; } = new List<Flight>();
}
