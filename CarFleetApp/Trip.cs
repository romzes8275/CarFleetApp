using System;
namespace CarFleetApp.Models
{
    public class Trip
    {
        public Vehicle Vehicle { get; set; }
        public Driver Driver { get; set; }
        public double Distance { get; set; }
        public decimal FuelPriceAtTime { get; set; } // Ціна пального
        public decimal ClientPayment { get; set; } // Скільки заплатив клієнт (Дохід)
                                                   // Поточні витрати: пальне + зарплата водія
        public decimal CurrentExpenses => Vehicle.CalculateFuelCost(Distance, FuelPriceAtTime) + Driver.SalaryPerTrip;

        // Чистий прибуток
        public decimal NetProfit => ClientPayment - CurrentExpenses;

        public DateTime Date { get; set; } = DateTime.Now;
    }
}