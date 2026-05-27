using System;
using System.Windows;
using System.Windows.Controls;
using RouteMaster.Models;

namespace RouteMaster
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            UserInfoText.Text = _currentUser?.FullName ?? "Пользователь";
            LogoutButton.Click += (s, e) => Logout();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                StatusBarText.Text = "Загрузка...";

                var allOrders = await App.DatabaseService.GetAllOrders();
                AllOrdersDataGrid.ItemsSource = allOrders;

                var myOrders = await App.DatabaseService.GetMyOrders(_currentUser.Id);
                MyOrdersDataGrid.ItemsSource = myOrders;

                var favorites = await App.DatabaseService.GetFavoriteOrders(_currentUser.Id);
                FavoriteOrdersDataGrid.ItemsSource = favorites;

                StatusBarText.Text = $"Загружено {allOrders?.Count ?? 0} заказов";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewOrderDetails(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.Tag as Order;

            if (order == null || order.Id == 0)
            {
                MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var details = await App.DatabaseService.GetOrderDetails(order.Id);

                if (details == null)
                {
                    MessageBox.Show($"Заказ #{order.OrderNumber} не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var tracking = await App.DatabaseService.GetOrderTracking(order.Id);
                var progress = await App.DatabaseService.GetOrderProgress(order.Id);

                string trackingText = "";
                if (tracking != null && tracking.Count > 0)
                {
                    foreach (var t in tracking)
                    {
                        trackingText += $"• {t.CreatedAt:dd.MM.yyyy HH:mm} - {GetStatusText(t.Status)}: {t.Comment ?? ""}\n";
                    }
                }
                else
                {
                    trackingText = "Нет истории статусов";
                }

                var message = $"ЗАКАЗ #{details.OrderNumber}\n\n" +
                             $"Клиент: {details.ClientName ?? "Не указан"}\n" +
                             $"Маршрут: {details.PickupCity ?? "?"} → {details.DeliveryCity ?? "?"}\n" +
                             $"Груз: {details.CargoDescription ?? "Не указан"}\n" +
                             $"Вес: {details.Weight} кг\n" +
                             $"Прогресс: {progress}%\n" +
                             $"Статус: {GetStatusText(details.CurrentStatus)}\n\n" +
                             $"История:\n{trackingText}";

                MessageBox.Show(message, "Детали заказа", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStatusText(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Неизвестно";

            switch (status)
            {
                case "pending": return "Ожидает";
                case "packing": return "В сборке";
                case "with_courier": return "Передан в доставку";
                case "in_transit": return "В пути";
                case "at_pickup_point": return "В пункте выдачи";
                case "delivered": return "Получен";
                default: return status;
            }
        }

        private void Logout()
        {
            var result = MessageBox.Show("Выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}