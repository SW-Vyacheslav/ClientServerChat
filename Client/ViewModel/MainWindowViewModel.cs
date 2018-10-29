using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using CommonObjects.Helpers;
using CommonObjects.Models;
using Newtonsoft.Json;
using Client.View;
using System.Windows;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Client.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private MainWindow _mainWindow;
        private Dictionary<Type, String> _responseTypes;

        private Socket _client;
        private IPAddress _serverIPAddress;
        private IPEndPoint _serverEndPoint;
        private User _client_user;
        private SettingsView _settingsView;
       
        public Boolean IsConnected { get; set; }
        public String Connection
        {
            get
            {
                return IsConnected == true ? "Connected" : "Disconnected";
            }
        }
        public User @User
        {
            get { return _client_user; }
        }
        public String WindowTitle { get; set; }
        public String MessageText { get; set; }
        public ObservableCollection<Message> Messages { get; set; }

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand OpenSettingsDialog { get; set; }
        public DelegateCommand SendMessageCommand { get; set; }

        public MainWindowViewModel()
        {
            _mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            _settingsView = new SettingsView();

            IsConnected = false;
            WindowTitle = "Client";
            Messages = new ObservableCollection<Message>(); 

            ConnectCommand = new DelegateCommand(o => Connect(),o => !IsConnected);
            OpenSettingsDialog = new DelegateCommand(o => ShowSettingsDialog());
            SendMessageCommand = new DelegateCommand(o => SendMessage(), o => IsConnected);

            InitTypes();

            Connect();
        }

        ~MainWindowViewModel()
        {
            Disconnect();
        }

        private void InitTypes()
        {
            _responseTypes = new Dictionary<Type, string>();
            _responseTypes.Add(typeof(ConnectResponse), "connect");
            _responseTypes.Add(typeof(MessageResponse), "message");
        }

        private void SendRequestToServer(Socket server, Request request)
        {
            String send_data_str = JsonConvert.SerializeObject(request);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            server.Send(send_data_bytes);
        }

        private Response GetResponseFromServer(Socket server)
        {
            Response value = null;

            byte[] recv_data_bytes = new byte[1024];
            server.Receive(recv_data_bytes);
            String recv_data_str = Encoding.UTF8.GetString(recv_data_bytes);

            dynamic temp = JsonConvert.DeserializeObject(recv_data_str);

            for (int i = 0; i < _responseTypes.Count; i++)
            {
                if (temp.response_type == _responseTypes.Values.ElementAt(i))
                {
                    value = JsonConvert.DeserializeObject(recv_data_str, _responseTypes.Keys.ElementAt(i)) as Response;
                    break;
                }
            }

            return value;
        }

        private void Connect()
        {
            try
            {
                SettingsViewModel settingsViewModel = _settingsView.DataContext as SettingsViewModel;
                _serverIPAddress = IPAddress.Parse(settingsViewModel.ServerIPAddress);
                _serverEndPoint = new IPEndPoint(_serverIPAddress, 80);
                _client_user = new User(settingsViewModel.UserName);

                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _client.Connect(_serverEndPoint);

                Thread serverListenerLoop_Thread = new Thread(serverListenerLoop);
                serverListenerLoop_Thread.IsBackground = true;
                serverListenerLoop_Thread.Start();

                Request connectRequest = new ConnectRequest(_client_user);
                SendRequestToServer(_client,connectRequest);
            }
            catch(Exception e)
            {
                _client?.Close();
                ShowSnackBarMessage("Error: Unable to connect to server.");
            }
        }

        private void Disconnect()
        {
            if (!IsConnected) return;

            Request disconnectRequest = new DisconnectRequest(_client_user);
            SendRequestToServer(_client,disconnectRequest);
            _client?.Close();

            IsConnected = false;
            RaisePropertyChangedEvent("Connection");
        }

        private void SendMessage()
        {
            Request messageRequest = new MessageRequest(new Message(_client_user,MessageText));
            SendRequestToServer(_client,messageRequest);
        }

        private void ShowSettingsDialog()
        {            
            _mainWindow.SettingsDialog.ShowDialog(_settingsView,SettingsClosingEventHandler);
        }

        private void SettingsClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if((bool)eventArgs.Parameter == false) return;
            SettingsViewModel settingsViewModel = _settingsView.DataContext as SettingsViewModel;
            settingsViewModel.ServerIPAddress = _settingsView.serverIPAddress.Text;
            settingsViewModel.UserName = _settingsView.userName.Text;
            settingsViewModel.ApplySettings();
            _serverIPAddress = IPAddress.Parse(settingsViewModel.ServerIPAddress);
            _serverEndPoint = new IPEndPoint(_serverIPAddress, 80);
            _client_user = new User(settingsViewModel.UserName);
        }

        private void serverListenerLoop()
        {
            try
            {
                while (true)
                {
                    Response server_response = GetResponseFromServer(_client);

                    switch (server_response.ResponseType)
                    {
                        case "connect":
                            {
                                if(_client_user.ID == (server_response as ConnectResponse).User.ID)
                                {
                                    IsConnected = true;
                                    RaisePropertyChangedEvent("Connection");
                                }

                                break;
                            }
                        case "message":
                            {
                                Application.Current.Dispatcher.Invoke(() => { Messages.Add((server_response as MessageResponse).Message); });
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                IsConnected = false;
                RaisePropertyChangedEvent("Connection");
                _client?.Close();
                ShowSnackBarMessage("Error: Connection with server was destroyed.");
            }
        }

        private void ShowSnackBarMessage(String message)
        {
            if(_mainWindow.SnackBar != null) _mainWindow.Dispatcher.Invoke(() => _mainWindow.SnackBar.MessageQueue.Enqueue(message));
        }
    }
}
