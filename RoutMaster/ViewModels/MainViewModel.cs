using RouteMaster.Models;

namespace RouteMaster.ViewModels
{
    public class MainViewModel
    {
        public User CurrentUser { get; set; }

        public MainViewModel(User user)
        {
            CurrentUser = user;
        }
    }
}