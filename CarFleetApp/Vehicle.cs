using System;
using System.Text.Json.Serialization;

namespace CarFleetApp.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Car), "car")]
    [JsonDerivedType(typeof(Van), "van")]
    [JsonDerivedType(typeof(Truck), "truck")]
    public abstract class Vehicle
    {
        public string Brand { get; set; }
        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public double FuelConsumption { get; set; } // л/100км
        public decimal RentalPricePerKm { get; set; } // Ціна для клієнта
        public decimal AmortizationPerKm { get; set; } // Амортизація (з макета)
        public bool IsAvailable { get; set; } = true;

        // Поля для дизайну картки
        public abstract string VehicleType { get; } // CAR, VAN, TRUCK
        public abstract string CapacityInfo { get; } // Поліморфна ємність

        // Розрахунок собівартості 1 км (Пальне + Амортизація)
        // Припустимо, середня ціна пального 73 грн/л
        public decimal CostPerKm => (decimal)((FuelConsumption / 100) * 73) + AmortizationPerKm;

        public decimal CalculateFuelCost(double distance, decimal fuelPrice)
        {
            return (decimal)(distance * (FuelConsumption / 100)) * fuelPrice;
        }
    }

    // ЛЕГКОВИЙ АВТОМОБІЛЬ
    public class Car : Vehicle
    {
        public int NumberOfSeats { get; set; }
        public override string VehicleType => "CAR";
        public override string CapacityInfo => $"Пасажиромісткість: {NumberOfSeats} місць";
    }

    // ВАНТАЖНИЙ ФУРГОН
    public class Van : Vehicle
    {
        public double MaxLoadCapacity { get; set; }
        public int PassengersCount { get; set; }
        public override string VehicleType => "VAN";
        public override string CapacityInfo => $"Вантажність: {MaxLoadCapacity} кг, Пасажири: {PassengersCount}";
    }

    // ВАНТАЖІВКА (НОВИЙ ТИП)
    public class Truck : Vehicle
    {
        public double MaxWeight { get; set; }
        public override string VehicleType => "TRUCK";
        public override string CapacityInfo => $"Важкий тоннаж: {MaxWeight} кг";
    }
}