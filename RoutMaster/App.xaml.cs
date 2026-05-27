using System.Windows;
using RouteMaster.Services;

namespace RouteMaster
{
    public partial class App : Application
    {
        public static DatabaseService DatabaseService = new DatabaseService();
    }
}