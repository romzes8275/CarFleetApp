using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CarFleetApp.Models;
using CarFleetApp.Logic;
namespace CarFleetApp
{
    public partial class MainWindow : Window
    {
        private FleetManager _manager;
        private Random _rnd = new Random();
    public MainWindow()
        {
            InitializeComponent();
            _manager = new FleetManager();
            SeedInitialData(); // Наповнюємо систему даними при старті
            RefreshUI();
        }

        // Початкові дані для демонстрації
        private void SeedInitialData()
        {
            // Mercedes-Benz Sprinter (VAN)
            _manager.Vehicles.Add(new Van
            {
                Model = "Mercedes-Benz Sprinter",
                PlateNumber = "AX9988HI",
                FuelConsumption = 9.8,
                AmortizationPerKm = 3.5m,
                MaxLoadCapacity = 1500,
                PassengersCount = 3,
                IsAvailable = true
            });

            // Volkswagen Passat B8 (CAR)
            _manager.Vehicles.Add(new Car
            {
                Model = "Volkswagen Passat B8",
                PlateNumber = "AA1234KA",
                FuelConsumption = 6.2,
                AmortizationPerKm = 2.0m,
                NumberOfSeats = 5,
                IsAvailable = true
            });

            // Volvo FH16 Heavy (TRUCK)
            _manager.Vehicles.Add(new Truck
            {
                Model = "Volvo FH16 Heavy",
                PlateNumber = "AE4433OO",
                FuelConsumption = 28.5,
                AmortizationPerKm = 8.0m,
                MaxWeight = 20000,
                IsAvailable = true
            });
        }

        private void RefreshUI()
        {
            // Оновлюємо список карток
            icVehicles.ItemsSource = null;
            icVehicles.ItemsSource = _manager.Vehicles;

            UpdateFinancials();
        }

        private void UpdateFinancials()
        {
            var report = _manager.GenerateFinancialReport();
            // Використовуємо N0 для цілих чисел або N2 для копійок
            txtTotalIncome.Text = report.TotalIncome.ToString("N0");
            txtTotalExpenses.Text = report.TotalExpenses.ToString("N0");
            txtTotalProfit.Text = report.TotalNetProfit.ToString("N0");
        }

        // --- ОБРОБНИКИ ПОДІЙ ---

        private void SortVehicles_Click(object sender, RoutedEventArgs e)
        {
            // Тепер сортуємо картки
            icVehicles.ItemsSource = _manager.GetVehiclesSortedByPrice();
        }

        private void ShowAvailable_Click(object sender, RoutedEventArgs e)
        {
            // Показуємо лише вільні авто в картках
            icVehicles.ItemsSource = _manager.GetAvailableVehicles();
        }

        private void ShowAllVehicles_Click(object sender, RoutedEventArgs e)
        {
            icVehicles.ItemsSource = _manager.Vehicles;
        }

        private void AddTrip_Click(object sender, RoutedEventArgs e)
        {
            var car = _manager.Vehicles.FirstOrDefault(v => v.IsAvailable);
            var driver = _manager.Drivers.FirstOrDefault();

            if (car != null && driver != null)
            {
                double dist = _rnd.Next(50, 300);
                decimal income = (decimal)dist * car.RentalPricePerKm;

                var newTrip = new Trip
                {
                    Vehicle = car,
                    Driver = driver,
                    Distance = dist,
                    FuelPriceAtTime = 52.50m,
                    ClientPayment = income,
                    Date = DateTime.Now
                };

                _manager.Trips.Add(newTrip);

                // Машина тепер "зайнята" (опціонально)
                // car.IsAvailable = false; 

                RefreshUI(); // Це оновить і картки, і цифри вгорі
                MessageBox.Show($"Поїздку додано!\nПрибуток: {newTrip.NetProfit:N2} грн", "Успіх");
            }
        }

        private void UpdateReport_Click(object sender, RoutedEventArgs e)
        {
            UpdateFinancials();
            MessageBox.Show("Дані оновлено!");
        }
    }
}