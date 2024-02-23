using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        //Action<Socket> _onAcceptHandler;
        Func<Session> _sessionFactory;

        //Action<int, string> _testActionHandler;

        public void Init (IPEndPoint endPoint, Func <Session> sessionFactory)
        {
            // Socket
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // Bind
            _listenSocket.Bind(endPoint);

            // Listen
            // 10: Max waiting count
            _listenSocket.Listen(10);

            // Add event
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);

            //_testActionHandler.Invoke(10, "YTTT");
        }

        void RegisterAccept (SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted (object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke ();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                //_onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString ());
            }

            RegisterAccept(args); 
        }
    }
}
