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
            // Машини
            _manager.Vehicles.Add(new Car { Model = "Volkswagen Passat", PlateNumber = "AA1234BB", FuelConsumption = 7.5, RentalPricePerKm = 15, IsAvailable = true });
            _manager.Vehicles.Add(new Car { Model = "Toyota Camry", PlateNumber = "BC7777CB", FuelConsumption = 9.0, RentalPricePerKm = 20, IsAvailable = true });
            _manager.Vehicles.Add(new Van { Model = "Mercedes-Benz Sprinter", PlateNumber = "AE5566HH", FuelConsumption = 11.2, RentalPricePerKm = 35, IsAvailable = true });
            _manager.Vehicles.Add(new Van { Model = "Renault Master", PlateNumber = "AI9080II", FuelConsumption = 10.5, RentalPricePerKm = 30, IsAvailable = false });

            // Водії
            _manager.Drivers.Add(new Driver { FullName = "Іван Петренко", SalaryPerTrip = 500 });
            _manager.Drivers.Add(new Driver { FullName = "Олег Сидоренко", SalaryPerTrip = 650 });
        }

        private void RefreshUI()
        {
            dgVehicles.ItemsSource = null;
            dgVehicles.ItemsSource = _manager.Vehicles;

            dgTrips.ItemsSource = null;
            dgTrips.ItemsSource = _manager.Trips;

            UpdateFinancials();
        }

        private void UpdateFinancials()
        {
            var report = _manager.GenerateFinancialReport();
            txtTotalIncome.Text = $"{report.TotalIncome:N2} грн";
            txtTotalExpenses.Text = $"{report.TotalExpenses:N2} грн";
            txtTotalProfit.Text = $"{report.TotalNetProfit:N2} грн";
        }

        // --- ОБРОБНИКИ ПОДІЙ ---

        private void SortVehicles_Click(object sender, RoutedEventArgs e)
        {
            dgVehicles.ItemsSource = _manager.GetVehiclesSortedByPrice();
        }

        private void ShowAvailable_Click(object sender, RoutedEventArgs e)
        {
            dgVehicles.ItemsSource = _manager.GetAvailableVehicles();
        }

        private void ShowAllVehicles_Click(object sender, RoutedEventArgs e)
        {
            dgVehicles.ItemsSource = _manager.Vehicles;
        }

        private void AddTrip_Click(object sender, RoutedEventArgs e)
        {
            // Беремо випадкову доступну машину та водія
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
                RefreshUI();
                MessageBox.Show($"Поїздку додано!\nПрибуток: {newTrip.NetProfit:N2} грн", "Успіх");
            }
            else
            {
                MessageBox.Show("Немає доступних машин для поїздки!");
            }
        }

        private void UpdateReport_Click(object sender, RoutedEventArgs e)
        {
            UpdateFinancials();
            MessageBox.Show("Дані оновлено!");
        }
    }
}