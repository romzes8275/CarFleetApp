// Vehicle.cs
namespace CarFleetApp.Models
{
    public abstract class Vehicle
    {
        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public double FuelConsumption { get; set; }
        public decimal RentalPricePerKm { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Змінили з методу на властивість (Property)
        public abstract string VehicleType { get; }

        public decimal CalculateFuelCost(double distance, decimal fuelPrice)
        {
            return (decimal)(distance * (FuelConsumption / 100)) * fuelPrice;
        }
    }

    public class Car : Vehicle
    {
        public int NumberOfSeats { get; set; }
        // Реалізація властивості
        public override string VehicleType => "Легковий автомобіль";
    }

    public class Van : Vehicle
    {
        public double MaxLoadCapacity { get; set; }
        // Реалізація властивості
        public override string VehicleType => "Вантажний фургон";
    }
}