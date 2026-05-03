using System;
using System.Collections.Generic;

namespace Flight_Reservation_App.Models;

public partial class TenantPhone
{
    public string PassportNum { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual Tenant PassportNumNavigation { get; set; } = null!;
}
