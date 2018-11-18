using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using CommonObjects.Models;
using CommonObjects.Helpers;

using Newtonsoft.Json;

namespace Server.Objects
{
    public class UserManager : IDisposable
    {
        private const String _data_base_file_name = "userdatabase.bin";
        private ObservableCollection<User> _userDataBase;
        private Dictionary<String, Socket> _clients;
        private Dictionary<Type, String> _requestTypes;

        public ObservableCollection<User> UserDataBase
        {
            get
            {
                return _userDataBase;
            }
        }
        public ObservableCollection<User> ConnectedUsers
        {
            get
            {
                return (from user in _userDataBase where (user.IsConnected == true) select user) as ObservableCollection<User>;
            }
        }
        public ObservableCollection<User> BannedUsers
        {
            get
            {
                return (from user in _userDataBase where (user.IsBanned == true) select user) as ObservableCollection<User>;
            }
        }

        public UserManager()
        {
            InitFields();
            InitRequestTypes();
            GetUserDataBase();
        }

        private void InitFields()
        {
            _clients = new Dictionary<String, Socket>();
            _requestTypes = new Dictionary<Type, string>();
        }
        private void InitRequestTypes()
        {
            _requestTypes.Add(typeof(DisconnectRequest), "disconnect");
            _requestTypes.Add(typeof(MessageRequest), "message");
        }
        private void GetUserDataBase()
        {
            _userDataBase = new ObservableCollection<User>();

            String data_base_file_path = Environment.CurrentDirectory + "/" + _data_base_file_name;

            if (File.Exists(data_base_file_path))
            {
                String data = null;

                using (FileStream fileStream = new FileStream(data_base_file_path, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        data = reader.ReadToEnd();
                    }
                }

                //User template: {id:1;name:Jack;password:1234567;is_banned:0}
                Regex user_regex = new Regex(@"\{[^\}]+\}");
                MatchCollection matches = user_regex.Matches(data);

                foreach (Match match in matches)
                {
                    String[] user_fields = match.Value.Substring(1, match.Value.Length - 1).Split(';');
                    User temp = new User
                    (
                        user_fields[0].Split(':')[1],
                        user_fields[1].Split(':')[1],
                        user_fields[2].Split(':')[1]
                    )
                    {
                        IsBanned = user_fields[3].Split(':')[1] == "1" ? true : false
                    };
                    
                    _userDataBase.Add(temp);
                }
            }
        }
        private void AddUserToDataBase(User user)
        {
            String data_base_file_path = Environment.CurrentDirectory + "/" + _data_base_file_name;

            using (FileStream fileStream = new FileStream(data_base_file_path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write("{"+String.Format("id:{0};name:{1};password:{2};is_banned:{3}",user.ID,user.Name,user.Password,user.IsBanned == true ? "1" : "0")+"}");
                }
            }
        }

        public void AddUser(User user)
        {
            InvokeHelper.ApplicationInvoke(() => _userDataBase.Add(user));
            AddUserToDataBase(user);
        }
        public Boolean IsConnectedUser(String user_id)
        {
            for (int i = 0; i < _userDataBase.Count; i++)
            {
                if (user_id == _userDataBase[i].ID)
                {
                    return _userDataBase[i].IsConnected;
                }
            }
            return false;
        }
        public Boolean IsBannedUser(String user_id)
        {
            for (int i = 0; i < _userDataBase.Count; i++)
            {
                if (user_id == _userDataBase[i].ID)
                {
                    return _userDataBase[i].IsBanned;
                }
            }
            return false;
        }
        public Boolean IsUserNameExists(String user_name)
        {
            for (int i = 0; i < _userDataBase.Count; i++)
            {
                if (user_name == _userDataBase[i].Name)
                {
                    return true;
                }
            }
            return false;
        }
        public String GetNewUserID()
        {
            return (_userDataBase.Count+1).ToString();
        }
        public User GetUserByID(String user_id)
        {
            for (int i = 0; i < _userDataBase.Count; i++)
            {
                if (user_id == _userDataBase[i].ID)
                {
                    return _userDataBase[i];
                }
            }
            return null;
        }

        public void BanUserByID(String user_id)
        {
            GetUserByID(user_id).IsBanned = true;

            //Response response = new DisconnectResponse(user);
            //response.Ok = false;
            //response.Error = "user_is_banned";
            //mainWindowViewModel.Server.ClientManager.SendResponseToAllClients(response);
        }
        public void UnBanUserByID(String user_id)
        {
            GetUserByID(user_id).IsBanned = false;
        }

        private void SendResponseToAllClients(Response response)
        {
            foreach (Socket client in _clients.Values)
            {
                SendResponseToClient(client, response);
            }
        }
        private void SendResponseToClient(Socket client, Response response)
        {
            String send_data_str = JsonConvert.SerializeObject(response);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            client.Send(send_data_bytes);
        }
        private void SendResponseToUserByUserID(String user_id, Response response)
        {
            SendResponseToClient(GetClientByUserID(user_id),response);
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
                if (temp.request_type == _requestTypes.Values.ElementAt(i))
                {
                    value = JsonConvert.DeserializeObject(recv_data_str, _requestTypes.Keys.ElementAt(i)) as Request;
                    break;
                }
            }

            return value;
        }

        public void AddClient(Socket client)
        {
            String temp_id = Guid.NewGuid().ToString();
            _clients.Add(temp_id, client);
            Thread clientLoop_Thread = new Thread(ClientLoop);
            clientLoop_Thread.IsBackground = true;
            clientLoop_Thread.Start(from pair in _clients where (pair.Key == temp_id) select pair);
        }
        private Socket GetClientByUserID(String user_id)
        {
            foreach (KeyValuePair<String,Socket> item in _clients)
            {
                if (item.Key == user_id) return item.Value;
            }

            return null;
        }

        public void Dispose()
        {
            foreach (Socket client in _clients.Values)
            {
                client?.Close();
            }

            _clients.Clear();
            _requestTypes.Clear();
        }

        private void ClientLoop(object pair)
        {
            Socket temp_client = ((KeyValuePair<String, Socket>)pair).Value;
            User client_user = new User(((KeyValuePair<String, Socket>)pair).Key,null,null);

            try
            {
                while (true)
                {
                    Request client_request = GetRequestFromClient(temp_client);

                    switch (client_request.RequestType)
                    {
                        case "registration":
                            {
                                RegistrationRequest registrationRequest = client_request as RegistrationRequest;

                                if (!IsUserNameExists(registrationRequest.UserName))
                                {
                                    AddUser(new User(GetNewUserID(), registrationRequest.UserName, registrationRequest.Password));
                                    RegistrationResponse registrationResponse = new RegistrationResponse();
                                    SendResponseToClient(temp_client, registrationResponse);
                                }
                                else
                                {
                                    RegistrationResponse registrationResponse = new RegistrationResponse();
                                    registrationResponse.Ok = false;
                                    registrationResponse.Error = "user_name_exists";
                                    SendResponseToClient(temp_client, registrationResponse);
                                }

                                break;
                            }

                        case "disconnect":
                            {
                                GetUserByID(client_user.ID).IsConnected = false;
                                throw new Exception();
                            }

                        case "message":
                            {
                                MessageResponse messageResponse = new MessageResponse((client_request as MessageRequest).Message);
                                SendResponseToAllClients(messageResponse);
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                temp_client?.Close();
                _clients.Remove(client_user.ID);
                if (client_user != null) GetUserByID(client_user.ID).IsConnected = false;
            }
        }
    }
}