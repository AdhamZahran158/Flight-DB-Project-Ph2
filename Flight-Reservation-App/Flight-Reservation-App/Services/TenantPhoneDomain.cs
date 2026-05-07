using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flight_Reservation_App.Models;

namespace Flight_Reservation_App.Services
{
    public class TenantPhoneDomain
    {
        private readonly Repository<TenantPhone> _phoneRepo;

        public TenantPhoneDomain()
        {
            _phoneRepo = new Repository<TenantPhone>(GlobalUsing.connectionString);
        }

        public TenantPhoneDomain(string connectionString)
        {
            _phoneRepo = new Repository<TenantPhone>(connectionString);
        }

        // SELECT * FROM TenantPhone WHERE PassportNum = @p0
        public async Task<List<TenantPhone>> GetPhonesByPassportAsync(string passportNum)
        {
            string[] cols =
            {
                "PassportNum",
                "PhoneNumber"
            };

            Where[] conditions =
            {
                new Where { Column = "PassportNum", Operator = "=", Value = passportNum }
            };

            return await _phoneRepo.GetAsync(cols, conditions);
        }

        public List<TenantPhone> GetPhonesByPassport(string passportNum)
        {
            return GetPhonesByPassportAsync(passportNum).GetAwaiter().GetResult();
        }

        // SELECT * FROM TenantPhone
        public async Task<List<TenantPhone>> GetAllPhonesAsync()
        {
            string[] cols =
            {
                "PassportNum",
                "PhoneNumber"
            };

            return await _phoneRepo.GetAsync(cols);
        }

        public List<TenantPhone> GetAllPhones()
        {
            return GetAllPhonesAsync().GetAwaiter().GetResult();
        }

        // INSERT INTO TenantPhone (PassportNum, PhoneNumber) VALUES (@p0, @p1)
        public async Task AddPhoneAsync(TenantPhone phone)
        {
            string[] columns =
            {
                "PassportNum",
                "PhoneNumber"
            };

            object[] values =
            {
                (object?)phone.PassportNum  ?? DBNull.Value,
                (object?)phone.PhoneNumber  ?? DBNull.Value
            };

            await _phoneRepo.AddAsync(columns, values);
        }

        public void AddPhone(TenantPhone phone)
        {
            AddPhoneAsync(phone).GetAwaiter().GetResult();
        }

        // DELETE FROM TenantPhone WHERE PassportNum = @p0 AND PhoneNumber = @p1
        public async Task DeletePhoneAsync(string passportNum, string phoneNumber)
        {
            Where[] conditions =
            {
                new Where { Column = "PassportNum",  Operator = "=", Value = passportNum  },
                new Where { Column = "PhoneNumber",  Operator = "=", Value = phoneNumber  }
            };

            await _phoneRepo.DeleteAsync(conditions);
        }

        public void DeletePhone(string passportNum, string phoneNumber)
        {
            DeletePhoneAsync(passportNum, phoneNumber).GetAwaiter().GetResult();
        }

        // DELETE FROM TenantPhone WHERE PassportNum = @p0  (removes all phones for a tenant)
        public async Task DeleteAllPhonesForPassengerAsync(string passportNum)
        {
            Where[] conditions =
            {
                new Where { Column = "PassportNum", Operator = "=", Value = passportNum }
            };

            await _phoneRepo.DeleteAsync(conditions);
        }

        public void DeleteAllPhonesForPassenger(string passportNum)
        {
            DeleteAllPhonesForPassengerAsync(passportNum).GetAwaiter().GetResult();
        }
    }
}