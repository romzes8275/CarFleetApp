using System.Collections.Generic;
using System.Linq;
using CarFleetApp.Models;
namespace CarFleetApp.Logic
{
    public class FleetManager
    {
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<Driver> Drivers { get; set; } = new List<Driver>();
        public List<Trip> Trips { get; set; } = new List<Trip>();

    // 1. Пошук доступних машин (стор. 39 методички)
    public List<Vehicle> GetAvailableVehicles()
        {
            return Vehicles.Where(v => v.IsAvailable).ToList();
        }

        // 2. Сортування за вартістю оренди (стор. 39 методички)
        public List<Vehicle> GetVehiclesSortedByPrice()
        {
            return Vehicles.OrderBy(v => v.RentalPricePerKm).ToList();
        }

        // 3. Фінансовий звіт (Заробіток, Витрати, Прибуток)
        public FinancialReport GenerateFinancialReport()
        {
            return new FinancialReport
            {
                TotalIncome = Trips.Sum(t => t.ClientPayment),
                TotalExpenses = Trips.Sum(t => t.CurrentExpenses),
                TotalNetProfit = Trips.Sum(t => t.NetProfit)
            };
        }
    }

    public class FinancialReport
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalNetProfit { get; set; }
    }
}