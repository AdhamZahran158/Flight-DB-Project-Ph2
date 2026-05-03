using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Baggage
{
    public int TicketId { get; set; }

    public int BaggageId { get; set; }

    public decimal? Weight { get; set; }

    public decimal? ExtraFee { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
