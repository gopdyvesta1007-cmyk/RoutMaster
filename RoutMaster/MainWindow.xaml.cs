using System;
using System.Linq;
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
            UserInfoText.Text = _currentUser.FullName;
            LogoutButton.Click += (s, e) => Logout();
            SearchButton.Click += SearchOrders;
            ClearButton.Click += (s, e) => { SearchBox.Text = ""; LoadData(); };
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            var orders = await App.DatabaseService.GetAllOrders(_currentUser.Id);
            foreach (var o in orders) o.Progress = await App.DatabaseService.GetOrderProgress(o.Id);
            AllOrdersGrid.ItemsSource = orders;

            var myOrders = await App.DatabaseService.GetMyOrders(_currentUser.Id);
            foreach (var o in myOrders) o.Progress = await App.DatabaseService.GetOrderProgress(o.Id);
            MyOrdersGrid.ItemsSource = myOrders;

            StatusText.Text = $"Добро пожаловать, {_currentUser.FullName}";
        }

        private async void SearchOrders(object sender, RoutedEventArgs e)
        {
            var text = SearchBox.Text.ToLower();
            if (string.IsNullOrEmpty(text)) await LoadData();
            else
            {
                var all = await App.DatabaseService.GetAllOrders(_currentUser.Id);
                var filtered = all.Where(o => o.PickupCity.ToLower().Contains(text) || o.DeliveryCity.ToLower().Contains(text)).ToList();
                AllOrdersGrid.ItemsSource = filtered;
                StatusText.Text = $"Найдено: {filtered.Count}";
            }
        }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button)?.Tag as Order;
            if (order == null) return;

            var details = await App.DatabaseService.GetOrderDetails(order.Id);
            var tracking = await App.DatabaseService.GetOrderTracking(order.Id);
            var progress = await App.DatabaseService.GetOrderProgress(order.Id);

            string track = string.Join("\n", tracking.Select(t => $"• {t.CreatedAt:dd.MM.yyyy HH:mm} - {t.Status}: {t.Comment ?? ""}"));
            MessageBox.Show($"Заказ #{details.OrderNumber}\nМаршрут: {details.PickupCity} → {details.DeliveryCity}\nПрогресс: {progress}%\nСтатус: {details.CurrentStatus}\n\nИстория:\n{track}", "Детали");
        }

        private void Logout()
        {
            if (MessageBox.Show("Выйти?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                new LoginWindow().Show();
                Close();
            }
        }
    }
}