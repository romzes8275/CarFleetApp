using System;
using System.Text.Json.Serialization;

namespace CarFleetApp.Models
{
    public class Trip
    {
        public string TripId { get; set; }
        public DateTime Date { get; set; }
        public string Route { get; set; }
        public double Distance { get; set; }
        public Vehicle Vehicle { get; set; } // Об'єкт машини
        public Driver Driver { get; set; }
        public decimal ClientPayment { get; set; }
        public decimal FuelPriceAtTime { get; set; }

        // ВИПРАВЛЕНИЙ РОЗРАХУНОК ПАЛЬНОГО
        [JsonIgnore] // Не зберігаємо в JSON, бо воно розраховується на льоту
        public decimal FuelCost
        {
            get
            {
                if (Vehicle == null || Vehicle.FuelConsumption == 0) return 0;
                // Дистанція * (Витрата / 100) * Середня ціна пального (52.5)
                return (decimal)(Distance * (Vehicle.FuelConsumption / 100)) * 52.50m;
            }
        }

        // Загальні витрати (Пальне + Зарплата водія)
        [JsonIgnore]
        public decimal TotalExpenses => FuelCost + (Driver?.SalaryPerTrip ?? 0);

        // Прибуток
        [JsonIgnore]
        public decimal NetProfit => ClientPayment - TotalExpenses;

        [JsonIgnore]
        public bool IsNegative => NetProfit < 0;
    }
}