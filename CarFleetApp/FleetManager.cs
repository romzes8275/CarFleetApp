using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CarFleetApp.Models;

namespace CarFleetApp.Logic
{
    public class FleetManager
    {
        public ObservableCollection<Vehicle> Vehicles { get; set; } = new ObservableCollection<Vehicle>();
        public ObservableCollection<Driver> Drivers { get; set; } = new ObservableCollection<Driver>();
        public ObservableCollection<Trip> Trips { get; set; } = new ObservableCollection<Trip>();

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
                TotalExpenses = Trips.Sum(t => t.TotalExpenses),
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