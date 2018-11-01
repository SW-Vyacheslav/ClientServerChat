using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.ObjectModel;
using System.Windows;

using CommonObjects.Helpers;
using CommonObjects.Models;

using Newtonsoft.Json;

using Client.View;

using MaterialDesignThemes.Wpf;

namespace Client.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private MainWindow _mainWindow;
        private Dictionary<Type, String> _responseTypes;

        private Socket _client;
        private IPAddress _serverIPAddress;
        private IPEndPoint _serverEndPoint;
        private SettingsView _settingsView;
       
        public Boolean IsConnected { get; set; }
        public String Connection
        {
            get
            {
                return IsConnected == true ? "Connected" : "Disconnected";
            }
        }
        public User ClientUser { get; private set; }
        public String WindowTitle { get; set; }
        public String MessageText { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<User> Users { get; set; }

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand OpenSettingsDialog { get; set; }
        public DelegateCommand SendMessageCommand { get; set; }

        public MainWindowViewModel()
        {
            InitFields();
            InitTypes();
            Connect();
        }
        ~MainWindowViewModel()
        {
            Disconnect();
        }

        private void InitFields()
        {
            _mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            _settingsView = new SettingsView();
            ClientUser = (_settingsView.DataContext as SettingsViewModel).Settings.User;

            IsConnected = false;
            WindowTitle = "Client";
            Messages = new ObservableCollection<Message>();
            Users = new ObservableCollection<User>();

            ConnectCommand = new DelegateCommand(o => Connect(), o => !IsConnected);
            OpenSettingsDialog = new DelegateCommand(o => ShowSettingsDialog());
            SendMessageCommand = new DelegateCommand(o => SendMessage(), o => IsConnected);
        }
        private void InitTypes()
        {
            _responseTypes = new Dictionary<Type, string>();
            _responseTypes.Add(typeof(ConnectResponse), "connect");
            _responseTypes.Add(typeof(MessageResponse), "message");
            _responseTypes.Add(typeof(UserListResponse), "user_list");
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
                _serverIPAddress = IPAddress.Parse(settingsViewModel.Settings.ServerIPAddress);
                _serverEndPoint = new IPEndPoint(_serverIPAddress, 80);

                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _client.Connect(_serverEndPoint);

                Thread serverListenerLoop_Thread = new Thread(serverListenerLoop);
                serverListenerLoop_Thread.IsBackground = true;
                serverListenerLoop_Thread.Start();

                Request connectRequest = new ConnectRequest(ClientUser);
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

            Request disconnectRequest = new DisconnectRequest(ClientUser);
            SendRequestToServer(_client,disconnectRequest);
            _client?.Close();

            IsConnected = false;
            RaisePropertyChangedEvent("Connection");
        }
        private void SendMessage()
        {
            Request messageRequest = new MessageRequest(new Message(ClientUser, MessageText));
            SendRequestToServer(_client,messageRequest);
        }

        private void ShowSnackBarMessage(String message)
        {
            if (_mainWindow.SnackBar != null) InvokeHelper.ApplicationInvoke(() => _mainWindow.SnackBar.MessageQueue.Enqueue(message));
        }
        private void ShowSettingsDialog()
        {            
            _mainWindow.SettingsDialog.ShowDialog(_settingsView,SettingsClosingEventHandler);
        }
        private void SettingsClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if((bool)eventArgs.Parameter == false) return;
            SettingsViewModel settingsViewModel = _settingsView.DataContext as SettingsViewModel;
            settingsViewModel.Settings.ServerIPAddress = _settingsView.serverIPAddress.Text;
            settingsViewModel.Settings.User.Name = _settingsView.userName.Text;
            settingsViewModel.ApplySettings();
            _serverIPAddress = IPAddress.Parse(settingsViewModel.Settings.ServerIPAddress);
            _serverEndPoint = new IPEndPoint(_serverIPAddress, 80);
            ClientUser.Name = settingsViewModel.Settings.User.Name;
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
                                ConnectResponse connectResponse = server_response as ConnectResponse;
                                if (connectResponse.Ok)
                                {
                                    IsConnected = true;
                                    RaisePropertyChangedEvent("Connection");
                                }
                                else
                                {
                                    String error_message = "Error: ";
                                    switch (connectResponse.Error)
                                    {
                                        case "user_is_banned":
                                            {
                                                error_message += "User is banned.";
                                                break;
                                            }
                                        case "user_is_connected":
                                            {
                                                error_message += "User is already connected.";
                                                break;
                                            }

                                        default:
                                            break;
                                    }
                                    ShowSnackBarMessage(error_message);
                                }
                                break;
                            }
                        case "message":
                            {
                                InvokeHelper.ApplicationInvoke(() => Messages.Add((server_response as MessageResponse).Message));
                                break;
                            }
                        case "user_list":
                            {
                                UserListResponse userListResponse = server_response as UserListResponse;
                                InvokeHelper.ApplicationInvoke(() => Users.Clear());
                                foreach (User user in userListResponse.Users)
                                {
                                    InvokeHelper.ApplicationInvoke(() => Users.Add(user));
                                }
                                break;
                            }
                        case "disconnect":
                            {
                                DisconnectResponse disconnectResponse = server_response as DisconnectResponse;
                                String error_message = "Error: ";
                                switch (disconnectResponse.Error)
                                {
                                    case "user_is_banned":
                                        {
                                            error_message += "User is banned.";
                                            break;
                                        }

                                    default:
                                        break;
                                }
                                ShowSnackBarMessage(error_message);
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
                InvokeHelper.ApplicationInvoke(() => Users.Clear());
                ShowSnackBarMessage("Error: Connection with server was destroyed.");
            }
        }
    }
}
