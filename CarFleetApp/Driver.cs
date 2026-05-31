namespace CarFleetApp.Models
{
    public class Driver
    {
        public string FullName { get; set; }
        public string LicenseNumber { get; set; } // Номер посвідчення (напр. BXX 124578)
        public int ExperienceYears { get; set; }   // Стаж (напр. 8 років)
        public decimal SalaryPerTrip { get; set; } // Оплата за рейс
        public string Specialization { get; set; } // "CAR", "VAN" або "TRUCK"
        public string Status { get; set; } = "ПРАЦЮЄ"; // Статус для бейджа
    }
}