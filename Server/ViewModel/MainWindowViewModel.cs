using System;

using CommonObjects.Helpers;

namespace Server.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private Objects.Server _server;
        private const int _port = 80;

        public Objects.Server Server { get { return _server; } }

        public String ServerStatus
        {
            get
            {
                return (_server?.IsStarted == true ? "Running" : "Stoped") ?? "Stoped"; 
            }
        }
        public String WindowTitle { get; set; }

        public DelegateCommand StartServerCommand { get; set; }
        public DelegateCommand StopServerCommand { get; set; }

        public MainWindowViewModel()
        {
            _server = new Objects.Server(_port);

            WindowTitle = "Server";

            StartServerCommand = new DelegateCommand(o => StartServer(), o => !_server.IsStarted);
            StopServerCommand = new DelegateCommand(o => StopServer(), o => _server.IsStarted);
        }
        ~MainWindowViewModel()
        {
            _server?.Dispose();
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
