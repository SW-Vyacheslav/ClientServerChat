using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using CommonObjects.Helpers;
using CommonObjects.Models;
using CommonObjects.Exceptions;

using Client.View;
using Client.Objects;

namespace Client.ViewModel
{
    public class MainWindowViewModel
    {
        public static Socket Client { get; set; }
        public static SettingsManager SettingsManager { get; private set; } = new SettingsManager();

        private static ChatViewModel _chatViewModel;

        public static Boolean IsConnected { get; set; } = false;
        public static User ClientUser { get; private set; }
        public static String WindowTitle { get; set; } = "Client";

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Loaded += (o, e) =>
            {
                (Application.Current.MainWindow as MainWindow).controlsHolder.Children.Add(new StartView());
            };
        }
        ~MainWindowViewModel()
        {
            Disconnect();
        }

        public static void Connect()
        {
            try
            {
                IPAddress serverIPAddress = IPAddress.Parse(SettingsManager.Settings.ServerIPAddress);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, 80);

                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client.Connect(serverEndPoint);

                IsConnected = true;

                Thread serverListenerLoop_Thread = new Thread(ServerListenerLoop){ IsBackground = true };
                serverListenerLoop_Thread.Start();
            }
            catch(SocketException)
            {
                Client?.Close();
                ShowSnackBarMessage("Error: Unable to connect to server.");
            }
        }
        public static void Disconnect()
        {
            if (!IsConnected) return;

            Request disconnectRequest = new SignOutRequest();
            DataTransferHelper.SendRequestToServer(Client,disconnectRequest);
            Client?.Close();

            IsConnected = false;
        }
        
        private static void SetView(UserControl userControl)
        {
            (Application.Current.MainWindow as MainWindow).controlsHolder.Children.Clear();
            (Application.Current.MainWindow as MainWindow).controlsHolder.Children.Add(userControl);
        }

        private static void ShowSnackBarMessage(String message)
        {
            if ((Application.Current.MainWindow as MainWindow).SnackBar != null) (Application.Current.MainWindow as MainWindow).SnackBar.MessageQueue.Enqueue(message);
        }

        private static void ServerListenerLoop()
        {
            try
            {
                while (true)
                {
                    Response server_response = DataTransferHelper.GetResponseFromServer(Client);

                    switch (server_response.ResponseType)
                    {
                        case "auth":
                            {
                                if(server_response.Ok)
                                {
                                    InvokeHelper.ApplicationInvoke
                                    (   
                                        () => 
                                        {
                                            ChatView chatView = new ChatView();
                                            _chatViewModel = chatView.DataContext as ChatViewModel;
                                            SetView(chatView);
                                        }
                                    );
                                }
                                else
                                {
                                    if (server_response.Error == "user_name_not_exists") throw new InvalidUserNameException();
                                    else if (server_response.Error == "invalid_password") throw new InvalidPasswordException();
                                }

                                break;
                            }
                        case "registration":
                            {
                                if (server_response.Ok)
                                {
                                    InvokeHelper.ApplicationInvoke(() => ShowSnackBarMessage("User successfully registered"));
                                    Disconnect();
                                }
                                else
                                {
                                    if (server_response.Error == "user_name_exists") throw new InvalidUserNameException("This user name already exists");
                                }

                                break;
                            }
                        case "message":
                            {
                                InvokeHelper.ApplicationInvoke(() => _chatViewModel.Messages.Add((server_response as MessageResponse).Message));
                                break;
                            }
                        case "ban":
                            {
                                throw new UserBannedException();
                            }

                        default:
                            break;
                    }
                }
            }
            catch(UserBannedException e)
            {
                InvokeHelper.ApplicationInvoke(() => ShowSnackBarMessage(e.Message));
            }
            catch(InvalidUserNameException e)
            {
                InvokeHelper.ApplicationInvoke(() => ShowSnackBarMessage(e.Message));
            }
            catch(InvalidPasswordException e)
            {
                InvokeHelper.ApplicationInvoke(() => ShowSnackBarMessage(e.Message));
            }
            catch(SocketException)
            {
                InvokeHelper.ApplicationInvoke(() => ShowSnackBarMessage("Connection with server was destroyed."));
            }
            catch(Exception){}
            finally
            {
                IsConnected = false;
                Client?.Close();
                _chatViewModel = null;
                InvokeHelper.ApplicationInvoke(() => SetView(new StartView()));
            }
        }
    }
}