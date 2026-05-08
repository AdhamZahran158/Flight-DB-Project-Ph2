using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Aircraft
{
    public int AircraftID { get; set; }

    public string? Model { get; set; }

    public int? PassengerCapacity { get; set; }

    public decimal? CargoCapacity { get; set; }

    public decimal? MaxTakeOffWeight { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
