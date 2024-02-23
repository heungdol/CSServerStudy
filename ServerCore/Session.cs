using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                if (buffer.Count < HeaderSize)
                {
                    break;
                }

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                {
                    break;
                }

                OnReceivePacket(new ArraySegment<byte> (buffer.Array, buffer.Offset, dataSize));
                
                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnReceivePacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        ReceiveBuffer _receieveBuffer = new ReceiveBuffer(1024);

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnReceive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start (Socket socket)
        {
            _socket = socket;

            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs> (OnReceiveComplete);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendComplete);
            
            RegisterReceive();
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue (sendBuffer);

                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
            
            //RegisterSend();
        }

        public void Disconnect () 
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) 
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown (SocketShutdown.Both);
            _socket.Close ();
        }

        #region Network Connection

        public void RegisterSend ()
        {
            //_pending = true;
            _pendingList.Clear ();

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue ();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            // TODO check
            //_sendArgs.AcceptSocket = null;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) 
            {
                OnSendComplete(null, _sendArgs);
            }
        }

        public void OnSendComplete (object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0
                    && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear ();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                            //_pending = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                else
                {
                    Disconnect ();  
                }
            }
        }

        public void RegisterReceive ()
        {
            // TODO check
            //_recieveArgs.AcceptSocket = null;

            _receieveBuffer.Clean ();

            ArraySegment<byte> segment = _receieveBuffer.WriteSegment;
            _receiveArgs.SetBuffer (segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_receiveArgs);
            if (pending == false)
            {
                OnReceiveComplete(null, _receiveArgs);
            }
        }

        void OnReceiveComplete (object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0
                && args.SocketError == SocketError.Success)
            {
                try
                {
                    // write 커서 이동
                    if (_receieveBuffer.OnWrite (args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 예외 처리
                    int processLength = OnReceive(_receieveBuffer.ReadSegment);
                    if (processLength < 0 || _receieveBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // read 커서 이동
                    if (_receieveBuffer.OnRead (processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString ());
                }

            }
            else
            {
                Disconnect ();
            }
        }

        #endregion
    }
}
