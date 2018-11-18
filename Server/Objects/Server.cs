using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Server.Objects
{
    public class Server : IDisposable
    {
        private Socket _listenerSocket;
        private IPEndPoint _localEndPoint;
        private IPAddress _localIPAddress;
        private bool _isStarted;

        public UserManager UserManager { get; private set; }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public Server(int port)
        {
            _localIPAddress = IPAddress.Parse("127.0.0.1");
            _localEndPoint = new IPEndPoint(_localIPAddress, port);
            _isStarted = false;
            UserManager = new UserManager();
        }

        public void Start()
        {
            _isStarted = true;

            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(_localEndPoint);

            Thread listenerLoop_Thread = new Thread(ListenerLoop);
            listenerLoop_Thread.IsBackground = true;
            listenerLoop_Thread.Start();
        }

        private void ListenerLoop()
        {
            _listenerSocket.Listen(10);

            try
            {
                while(_isStarted)
                {
                    Socket accepted_socket = _listenerSocket.Accept();
                    UserManager.AddClient(accepted_socket);
                }
            }
            catch (Exception e)
            {
             
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            _isStarted = false;
            _listenerSocket?.Close();
            UserManager.Dispose();
        }

        public void Dispose()
        {
            _listenerSocket?.Close();
            _listenerSocket = null;
            UserManager.Dispose();
            UserManager = null;
        }
    }
}
