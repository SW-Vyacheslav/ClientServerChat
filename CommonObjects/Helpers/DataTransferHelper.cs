using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using CommonObjects.Models;

using Newtonsoft.Json;

namespace CommonObjects.Helpers
{
    public static class DataTransferHelper
    {
        private static Dictionary<Type, String> _responseTypes = new Dictionary<Type, string>()
        {
            { typeof(MessageResponse), "message" },
            { typeof(RegistrationResponse),"registration" },
            { typeof(AuthResponse), "auth" },
            { typeof(BanResponse), "ban" }
        };
        private static Dictionary<Type, String> _requestTypes = new Dictionary<Type, string>()
        {
            { typeof(SignOutRequest), "signout" },
            { typeof(MessageRequest), "message" },
            { typeof(RegistrationRequest),"registration" },
            { typeof(AuthRequest), "auth" },
        };

        public static void SendRequestToServer(Socket server, Request request)
        {
            String send_data_str = JsonConvert.SerializeObject(request);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            server.Send(send_data_bytes);
        }
        public static Response GetResponseFromServer(Socket server)
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
        public static void SendResponseToClient(Socket client, Response response)
        {
            String send_data_str = JsonConvert.SerializeObject(response);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            client.Send(send_data_bytes);
        }
        public static Request GetRequestFromClient(Socket client)
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
    }
}