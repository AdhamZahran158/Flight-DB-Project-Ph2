using Flight_Reservation_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App.Services
{
    internal class PassengerDomain
    {
        private readonly Repository<Tenant> _tenantRepo;

        public PassengerDomain()
        {
            _tenantRepo = new Repository<Tenant>(GlobalUsing.connectionString);
        }

        public PassengerDomain(string connectionString)
        {
            _tenantRepo = new Repository<Tenant>(connectionString);
        }

        // SELECT * FROM Tenant
        public async Task<List<Tenant>> GetPassengersAsync()
        {
            string[] cols =
            {
                "PassportNum",
                "Fname",
                "Mname",
                "Lname",
                "Email",
                "NationalID AS NationalId",
                "Nationality"
            };

            return await _tenantRepo.GetAsync(cols);
        }

        public List<Tenant> GetPassengers()
        {
            return GetPassengersAsync().GetAwaiter().GetResult();
        }

        // SELECT * FROM Tenant WHERE PassportNum = @p0
        public async Task<List<Tenant>> GetPassengerByPassportAsync(string passportNum)
        {
            string[] cols =
            {
                "PassportNum",
                "Fname",
                "Mname",
                "Lname",
                "Email",
                "NationalID AS NationalId",
                "Nationality"
            };

            Where[] conditions =
            {
                new Where { Column = "PassportNum", Operator = "=", Value = passportNum }
            };

            return await _tenantRepo.GetAsync(cols, conditions);
        }

        public List<Tenant> GetPassengerByPassport(string passportNum)
        {
            return GetPassengerByPassportAsync(passportNum).GetAwaiter().GetResult();
        }

        // INSERT INTO Tenant (...) VALUES (...)
        public async Task AddPassengerAsync(Tenant tenant)
        {
            string[] columns =
            {
                "PassportNum",
                "Fname",
                "Mname",
                "Lname",
                "Email",
                "NationalID",
                "Nationality"
            };

            object[] values =
            {
                (object?)tenant.PassportNum  ?? DBNull.Value,
                (object?)tenant.Fname        ?? DBNull.Value,
                (object?)tenant.Mname        ?? DBNull.Value,
                (object?)tenant.Lname        ?? DBNull.Value,
                (object?)tenant.Email        ?? DBNull.Value,
                (object?)tenant.NationalId   ?? DBNull.Value,
                (object?)tenant.Nationality  ?? DBNull.Value
            };

            await _tenantRepo.AddAsync(columns, values);
        }

        public void AddPassenger(Tenant tenant)
        {
            AddPassengerAsync(tenant).GetAwaiter().GetResult();
        }

        // UPDATE Tenant SET ... WHERE PassportNum = @w0
        public async Task UpdatePassengerAsync(Tenant tenant)
        {
            string[] columns =
            {
                "Fname",
                "Mname",
                "Lname",
                "Email",
                "NationalID",
                "Nationality"
            };

            object[] values =
            {
                (object?)tenant.Fname       ?? DBNull.Value,
                (object?)tenant.Mname       ?? DBNull.Value,
                (object?)tenant.Lname       ?? DBNull.Value,
                (object?)tenant.Email       ?? DBNull.Value,
                (object?)tenant.NationalId  ?? DBNull.Value,
                (object?)tenant.Nationality ?? DBNull.Value
            };

            Where[] conditions =
            {
                new Where { Column = "PassportNum", Operator = "=", Value = tenant.PassportNum }
            };

            await _tenantRepo.UpdateAsync(columns, values, conditions);
        }

        public void UpdatePassenger(Tenant tenant)
        {
            UpdatePassengerAsync(tenant).GetAwaiter().GetResult();
        }

        // DELETE FROM Tenant WHERE PassportNum = @p0
        public async Task DeletePassengerAsync(string passportNum)
        {
            Where[] conditions =
            {
                new Where { Column = "PassportNum", Operator = "=", Value = passportNum }
            };

            await _tenantRepo.DeleteAsync(conditions);
        }

        public void DeletePassenger(string passportNum)
        {
            DeletePassengerAsync(passportNum).GetAwaiter().GetResult();
        }
    }
}
