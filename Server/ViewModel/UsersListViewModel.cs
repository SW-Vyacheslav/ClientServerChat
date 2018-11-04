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
            mainWindowViewModel.Server.ClientManager.UserManager.AddBannedUser(mainWindowViewModel.Server.ClientManager.UserManager.GetUserByID(user.ID));

            Response response = new DisconnectResponse(user);
            response.Ok = false;
            response.Error = "user_is_banned";
            mainWindowViewModel.Server.ClientManager.SendResponseToAllClients(response);
        }
        private static void UnBan(object param)
        {
            User user = param as User;
            mainWindowViewModel.Server.ClientManager.UserManager.RemoveBannedUserByID(user.ID);
        }
    }
}
