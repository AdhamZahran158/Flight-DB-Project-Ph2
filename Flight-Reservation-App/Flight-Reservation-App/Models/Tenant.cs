using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class Tenant
{
    public string PassportNum { get; set; } = null!;

    public string? Fname { get; set; }

    public string? Mname { get; set; }

    public string? Lname { get; set; }

    public string? Email { get; set; }

    public string? NationalId { get; set; }

    public string? Nationality { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TenantPhone> TenantPhones { get; set; } = new List<TenantPhone>();
}
