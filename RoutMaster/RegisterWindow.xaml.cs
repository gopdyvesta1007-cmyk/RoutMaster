using System;
using System.Windows;
using RouteMaster.Models;

namespace RouteMaster
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            RegisterBtn.Click += RegisterBtn_Click;
            CancelBtn.Click += (s, e) => Close();
        }

        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Заполните ФИО и Email");
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            var user = new User
            {
                FullName = FullNameBox.Text,
                Email = EmailBox.Text,
                PasswordHash = PasswordBox.Password,
                Phone = PhoneBox.Text,
                RoleId = 1
            };

            bool result = await App.DatabaseService.RegisterUser(user);

            if (result)
            {
                MessageBox.Show("Регистрация успешна!");
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка регистрации");
            }
        }
    }
}