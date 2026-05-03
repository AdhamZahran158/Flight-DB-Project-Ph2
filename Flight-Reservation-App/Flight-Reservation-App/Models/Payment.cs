using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public int? BookingId { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public virtual Booking? Booking { get; set; }
}
