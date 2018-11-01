using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows;

using CommonObjects.Models;
using CommonObjects.Helpers;
using Newtonsoft.Json;

namespace Server.Objects
{
    public class ClientManager : IDisposable
    {
        private ViewModel.MainWindowViewModel _mainWindowViewModel;
        private List<Socket> _clients;
        private Dictionary<Type, String> _requestTypes;

        public UserManager UserManager { get; private set; }

        public ClientManager()
        {
            InitFields();
            InitRequestTypes();
        }

        private void InitFields()
        {
            UserManager = new UserManager();
            _clients = new List<Socket>();
            _requestTypes = new Dictionary<Type, string>();
        }
        private void InitRequestTypes()
        {
            _requestTypes.Add(typeof(ConnectRequest),"connect");
            _requestTypes.Add(typeof(DisconnectRequest),"disconnect");
            _requestTypes.Add(typeof(MessageRequest),"message");
        }

        public void AddClient(Socket client)
        {
            if(_mainWindowViewModel == null) _mainWindowViewModel = InvokeHelper.ApplicationInvoke( () => Application.Current.Windows.OfType<View.MainWindow>().FirstOrDefault().DataContext as ViewModel.MainWindowViewModel);

            _clients.Add(client);
            Thread clientLoop_Thread = new Thread(ClientLoop);
            clientLoop_Thread.IsBackground = true;
            clientLoop_Thread.Start(client);
        }
        public void Dispose()
        {
            foreach (Socket client in _clients)
            {
                client?.Close();
            }

            _clients.Clear();
        }

        public void SendResponseToAllClients(Response response)
        {
            foreach (Socket client in _clients)
            {
                SendResponseToClient(client,response);
            }
        }
        private void SendResponseToClient(Socket client, Response response)
        {
            String send_data_str = JsonConvert.SerializeObject(response);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            client.Send(send_data_bytes);
        }
        private Request GetRequestFromClient(Socket client)
        {
            Request value = null;

            byte[] recv_data_bytes = new byte[1024];
            client.Receive(recv_data_bytes);
            String recv_data_str = Encoding.UTF8.GetString(recv_data_bytes);

            dynamic temp = JsonConvert.DeserializeObject(recv_data_str);

            for (int i = 0; i < _requestTypes.Count; i++)
            {
                if(temp.request_type == _requestTypes.Values.ElementAt(i))
                {
                    value = JsonConvert.DeserializeObject(recv_data_str,_requestTypes.Keys.ElementAt(i)) as Request;
                    break;
                }
            }

            return value;
        }

        private void ClientLoop(object client)
        {
            Socket temp_client = client as Socket;
            User client_user = null;

            try
            {
                while (true)
                {
                    Request client_request = GetRequestFromClient(temp_client);

                    switch (client_request.RequestType)
                    {
                        case "connect":
                            {
                                User temp_user = (client_request as ConnectRequest).User;

                                if (!UserManager.IsBannedUser(temp_user.ID))
                                {
                                    if(!UserManager.IsConnectedUser(temp_user.ID))
                                    {
                                        Response connectResponse = new ConnectResponse(temp_user);
                                        SendResponseToClient(temp_client, connectResponse);
                                        UserManager.AddUser(temp_user);
                                        Response userListResponse = new UserListResponse(UserManager.Users.ToList());
                                        SendResponseToAllClients(userListResponse);
                                        client_user = temp_user;
                                    }
                                    else
                                    {
                                        Response connectResponse = new ConnectResponse(temp_user);
                                        connectResponse.Ok = false;
                                        connectResponse.Error = "user_is_connected";
                                        SendResponseToClient(temp_client, connectResponse);
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    Response connectResponse = new ConnectResponse(temp_user);
                                    connectResponse.Ok = false;
                                    connectResponse.Error = "user_is_banned";
                                    SendResponseToClient(temp_client, connectResponse);
                                    throw new Exception();
                                }
                                
                                break;
                            }

                        case "disconnect":
                            {
                                Response userListResponse = new UserListResponse(UserManager.Users.ToList());
                                SendResponseToAllClients(userListResponse);
                                UserManager.RemoveUserByID((client_request as DisconnectRequest).User.ID);
                                throw new Exception();
                            }

                        case "message":
                            {
                                Response messageResponse = new MessageResponse((client_request as MessageRequest).Message);
                                SendResponseToAllClients(messageResponse);
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
                temp_client?.Close();
                _clients.Remove(temp_client);
                if(client_user != null) UserManager.RemoveUserByID(client_user.ID);
            }
        }
    }
}
