using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flight_Reservation_App.Models;

namespace Flight_Reservation_App.Services
{
    public class BaggageDomain
    {
        private readonly Repository<Baggage> _baggageRepo;

        public BaggageDomain()
        {
            _baggageRepo = new Repository<Baggage>(GlobalUsing.connectionString);
        }

        public BaggageDomain(string connectionString)
        {
            _baggageRepo = new Repository<Baggage>(connectionString);
        }

        // SELECT * FROM Baggage WHERE TicketID = @p0
        public async Task<List<Baggage>> GetBaggageByTicketAsync(int ticketId)
        {
            string[] cols =
            {
                "TicketID AS TicketId",
                "BaggageID AS BaggageId",
                "Weight",
                "ExtraFee"
            };

            Where[] conditions =
            {
                new Where { Column = "TicketID", Operator = "=", Value = ticketId }
            };

            return await _baggageRepo.GetAsync(cols, conditions);
        }

        public List<Baggage> GetBaggageByTicket(int ticketId)
        {
            return GetBaggageByTicketAsync(ticketId).GetAwaiter().GetResult();
        }

        // SELECT * FROM Baggage
        public async Task<List<Baggage>> GetAllBaggageAsync()
        {
            string[] cols =
            {
                "TicketID AS TicketId",
                "BaggageID AS BaggageId",
                "Weight",
                "ExtraFee"
            };

            return await _baggageRepo.GetAsync(cols);
        }

        public List<Baggage> GetAllBaggage()
        {
            return GetAllBaggageAsync().GetAwaiter().GetResult();
        }

        // INSERT INTO Baggage (TicketID, BaggageID, Weight, ExtraFee) VALUES (...)
        public async Task AddBaggageAsync(Baggage baggage)
        {
            string[] columns =
            {
                "TicketID",
                "BaggageID",
                "Weight",
                "ExtraFee"
            };

            object[] values =
            {
                (object?)baggage.TicketId  ?? DBNull.Value,
                (object?)baggage.BaggageId ?? DBNull.Value,
                (object?)baggage.Weight    ?? DBNull.Value,
                (object?)baggage.ExtraFee  ?? DBNull.Value
            };

            await _baggageRepo.AddAsync(columns, values);
        }

        public void AddBaggage(Baggage baggage)
        {
            AddBaggageAsync(baggage).GetAwaiter().GetResult();
        }

        // UPDATE Baggage SET Weight = @p0, ExtraFee = @p1
        // WHERE TicketID = @w0 AND BaggageID = @w1
        public async Task UpdateBaggageAsync(Baggage baggage)
        {
            string[] columns =
            {
                "Weight",
                "ExtraFee"
            };

            object[] values =
            {
                (object?)baggage.Weight   ?? DBNull.Value,
                (object?)baggage.ExtraFee ?? DBNull.Value
            };

            Where[] conditions =
            {
                new Where { Column = "TicketID",  Operator = "=", Value = baggage.TicketId  },
                new Where { Column = "BaggageID", Operator = "=", Value = baggage.BaggageId }
            };

            await _baggageRepo.UpdateAsync(columns, values, conditions);
        }

        public void UpdateBaggage(Baggage baggage)
        {
            UpdateBaggageAsync(baggage).GetAwaiter().GetResult();
        }

        // DELETE FROM Baggage WHERE TicketID = @p0 AND BaggageID = @p1
        public async Task DeleteBaggageAsync(int ticketId, int baggageId)
        {
            Where[] conditions =
            {
                new Where { Column = "TicketID",  Operator = "=", Value = ticketId  },
                new Where { Column = "BaggageID", Operator = "=", Value = baggageId }
            };

            await _baggageRepo.DeleteAsync(conditions);
        }

        public void DeleteBaggage(int ticketId, int baggageId)
        {
            DeleteBaggageAsync(ticketId, baggageId).GetAwaiter().GetResult();
        }

        // DELETE FROM Baggage WHERE TicketID = @p0  (removes all baggage for a ticket)
        public async Task DeleteAllBaggageForTicketAsync(int ticketId)
        {
            Where[] conditions =
            {
                new Where { Column = "TicketID", Operator = "=", Value = ticketId }
            };

            await _baggageRepo.DeleteAsync(conditions);
        }

        public void DeleteAllBaggageForTicket(int ticketId)
        {
            DeleteAllBaggageForTicketAsync(ticketId).GetAwaiter().GetResult();
        }
    }
}