using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonObjects.Helpers;
using CommonObjects.Models;

namespace Server.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private Objects.Server _server;
        private const int _port = 80;

        public List<User> Users { get; set; }

        public void AddUser(User user)
        {
            Users.Add(new User(user.Name,user.ID));
        }

        public void RemoveUserByID(String user_id)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if(user_id == Users[i].ID)
                {
                    Users.RemoveAt(i);
                    break;
                }
            }
        }

        public String ServerStatus
        {
            get
            {
                return _server.IsStarted == true ? "Running" : "Stoped"; 
            }
        }

        public DelegateCommand StartServerCommand { get; set; }
        public DelegateCommand StopServerCommand { get; set; }

        public MainWindowViewModel()
        {
            _server = new Objects.Server(_port,this);
            Users = new List<User>();

            StartServerCommand = new DelegateCommand(o => StartServer(), o => !_server.IsStarted);
            StopServerCommand = new DelegateCommand(o => StopServer(), o => _server.IsStarted);
        }

        ~MainWindowViewModel()
        {
            _server.Dispose();
        }

        private void StartServer()
        {
            _server.Start();
            RaisePropertyChangedEvent("ServerStatus");
        }

        private void StopServer()
        {
            _server.Stop();
            RaisePropertyChangedEvent("ServerStatus");
        }
    }
}
