using CommonObjects.Helpers;
using CommonObjects.Models;
using System.Windows;
using System.Linq;
using Server.View;

namespace Server.ViewModel
{
    public static class UsersListViewModel
    {
        private static MainWindowViewModel mainWindowViewModel;
        public static DelegateCommand BanCommand { get; set; }
        public static DelegateCommand UnBanCommand { get; set; }

        static UsersListViewModel()
        {
            mainWindowViewModel = Application.Current.Windows.OfType<MainWindow>().LastOrDefault().DataContext as MainWindowViewModel;
            BanCommand = new DelegateCommand(o => Ban(o));
            UnBanCommand = new DelegateCommand(o => UnBan(o));
        }

        private static void Ban(object param)
        {
            User user = param as User;
            mainWindowViewModel.Server.UserManager.BanUserByID(user.ID);
        }
        private static void UnBan(object param)
        {
            User user = param as User;
            mainWindowViewModel.Server.UserManager.UnBanUserByID(user.ID);
        }
    }
}
