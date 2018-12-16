using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

using CommonObjects.Models;
using CommonObjects.Helpers;

namespace Server.Objects
{
    public class UserManager : ObservableObject, IDisposable
    {
        private const String _data_base_file_name = "userdatabase.bin";
        private ObservableCollection<User> _userDataBase;
        private Dictionary<String, Socket> _clients;

        public IEnumerable<User> UserDataBase
        {
            get
            {
                return from user in _userDataBase select user;
            }
        }
        public IEnumerable<User> ConnectedUsers
        {
            get
            {
                return from user in _userDataBase where (user.IsConnected == true) select user;
            }
        }
        public IEnumerable<User> BannedUsers
        {
            get
            {
                return from user in _userDataBase where (user.IsBanned == true) select user;
            }
        }

        public UserManager()
        {
            InitFields();
            GetUserDataBase();
        }
        ~UserManager()
        {
            MergeDataBases();
        }

        private void InitFields()
        {
            _clients = new Dictionary<String, Socket>();
            _userDataBase = new ObservableCollection<User>();
        }
        private void GetUserDataBase()
        {
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
                    String[] user_fields = match.Value.Substring(1, match.Value.Length - 2).Split(';');
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
        private void MergeDataBases()
        {
            String data_base_file_path = Environment.CurrentDirectory + "/" + _data_base_file_name;

            File.Delete(data_base_file_path);

            foreach (User user in _userDataBase)
            {
                AddUserToDataBase(user);
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
        public User GetUserByName(String user_name)
        {
            for (int i = 0; i < _userDataBase.Count; i++)
            {
                if (user_name == _userDataBase[i].Name)
                {
                    return _userDataBase[i];
                }
            }
            return null;
        }

        public void BanUserByID(String user_id)
        {
            GetUserByID(user_id).IsBanned = true;

            Response response = new BanResponse();
            SendResponseToUserByUserID(user_id,response);

            RaisePropertyChangedEvent("BannedUsers");
            RaisePropertyChangedEvent("UserDataBase");
        }
        public void UnBanUserByID(String user_id)
        {
            GetUserByID(user_id).IsBanned = false;

            RaisePropertyChangedEvent("BannedUsers");
            RaisePropertyChangedEvent("UserDataBase");
        }

        private void SendResponseToAllClients(Response response)
        {
            foreach (Socket client in _clients.Values)
            {
                DataTransferHelper.SendResponseToClient(client, response);
            }
        }
        private void SendResponseToUserByUserID(String user_id, Response response)
        {
            DataTransferHelper.SendResponseToClient(GetClientByUserID(user_id),response);
        }

        public void AddClient(Socket client)
        {
            String temp_id = Guid.NewGuid().ToString();
            _clients.Add(temp_id, client);
            Thread clientLoop_Thread = new Thread(ClientLoop) { IsBackground = true };
            IDSocket idSocket = new IDSocket()
            {
                Handle_ID = temp_id,
                Handle_Socket = client
            };
            clientLoop_Thread.Start(idSocket);
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
        }

        private void ClientLoop(object o)
        {
            Socket temp_client = ((IDSocket)o).Handle_Socket;
            User client_user = new User(((IDSocket)o).Handle_ID,null,null);

            try
            {
                while (true)
                {
                    Request client_request = DataTransferHelper.GetRequestFromClient(temp_client);

                    switch (client_request.RequestType)
                    {
                        case "registration":
                            {
                                RegistrationRequest registrationRequest = client_request as RegistrationRequest;

                                if (!IsUserNameExists(registrationRequest.UserName))
                                {
                                    AddUser(new User(GetNewUserID(), registrationRequest.UserName, registrationRequest.Password));
                                    RegistrationResponse registrationResponse = new RegistrationResponse();
                                    DataTransferHelper.SendResponseToClient(temp_client, registrationResponse);
                                }
                                else
                                {
                                    RegistrationResponse registrationResponse = new RegistrationResponse()
                                    {
                                        Ok = false,
                                        Error = "user_name_exists"
                                    };
                                    DataTransferHelper.SendResponseToClient(temp_client, registrationResponse);
                                }

                                break;
                            }

                        case "auth":
                            {
                                AuthRequest authRequest = client_request as AuthRequest;

                                if(IsUserNameExists(authRequest.UserName))
                                {
                                    User temp_user = GetUserByName(authRequest.UserName);
                                    if (authRequest.Password == temp_user.Password)
                                    {
                                        if (IsBannedUser(temp_user.ID))
                                        {
                                            BanResponse banResponse = new BanResponse();
                                            DataTransferHelper.SendResponseToClient(temp_client, banResponse);
                                        }
                                        else
                                        {
                                            if(IsConnectedUser(temp_user.ID))
                                            {
                                                AuthResponse authResponse = new AuthResponse(temp_user);
                                                authResponse.Error = "user_is_connected";
                                                authResponse.Ok = false;
                                                DataTransferHelper.SendResponseToClient(temp_client, authResponse);
                                            }
                                            else
                                            {
                                                AuthResponse authResponse = new AuthResponse(temp_user);
                                                DataTransferHelper.SendResponseToClient(temp_client, authResponse);
                                                temp_user.IsConnected = true;
                                                RaisePropertyChangedEvent("UserDataBase");
                                                RaisePropertyChangedEvent("ConnectedUsers");
                                                _clients.Add(temp_user.ID, temp_client);
                                                _clients.Remove(client_user.ID);
                                                client_user.ID = temp_user.ID;
                                                client_user.Name = temp_user.Name;
                                                client_user.Password = temp_user.Password;
                                                
                                                for (int i = 0; i < ConnectedUsers.Count(); i++)
                                                {
                                                    if (ConnectedUsers.ElementAt(i).ID == client_user.ID) continue;
                                                    UserSignInResponse userSignInResponse = new UserSignInResponse(ConnectedUsers.ElementAt(i));
                                                    DataTransferHelper.SendResponseToClient(temp_client, userSignInResponse);
                                                }
                                                for (int i = 0; i < _clients.Count(); i++)
                                                {
                                                    if (_clients.ElementAt(i).Key == client_user.ID) continue;
                                                    UserSignInResponse userSignInResponse = new UserSignInResponse(client_user);
                                                    DataTransferHelper.SendResponseToClient(_clients.ElementAt(i).Value, userSignInResponse);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        AuthResponse authResponse = new AuthResponse(null)
                                        {
                                            Ok = false,
                                            Error = "invalid_password"
                                        };
                                        DataTransferHelper.SendResponseToClient(temp_client, authResponse);
                                    }
                                }
                                else
                                {
                                    AuthResponse authResponse = new AuthResponse(null)
                                    {
                                        Ok = false,
                                        Error = "user_name_not_exists"
                                    };
                                    DataTransferHelper.SendResponseToClient(temp_client,authResponse);
                                }
                                
                                break;
                            }

                        case "signout":
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
                if (client_user != null)
                {
                    User temp_user = GetUserByID(client_user.ID);
                    if (temp_user != null) temp_user.IsConnected = false;
                    UserSignOutResponse userSignOutResponse = new UserSignOutResponse(client_user);
                    SendResponseToAllClients(userSignOutResponse);
                }
                RaisePropertyChangedEvent("UserDataBase");
                RaisePropertyChangedEvent("ConnectedUsers");
            }
        }
    }
}