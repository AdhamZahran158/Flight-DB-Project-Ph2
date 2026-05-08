using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App
{
    public class GlobalUsing
    {
        public static string connectionString { get; } = @"Server=den1.mssql8.gear.host;
                                                            Database=airlinedb2026;
                                                            User Id = airlinedb2026;
                                                            Password=DB@2026;
                                                            Encrypt=True;
                                                            TrustServerCertificate=True;";



    }
}
