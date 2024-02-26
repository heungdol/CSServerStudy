using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;

namespace DummyClient
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { /*size = 4,*/ /*packetID = (ushort)PacketID.PlayerInfoReq,*/ playerID = 1001, playerName = "cool name" };
            // Send
            //for (int i = 0; i < 5; i++)

            packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, level = 5, duration = 4.5f});
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 10, duration = 3.5f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 15, duration = 2.5f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 20, duration = 1.5f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 501, level = 25, duration = 0.5f });

            {
                ArraySegment <byte> s = packet.Write();

                if (s != null)
                {
                    Send(s);
                }
            }
        }

        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string recieveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"From Server: {recieveData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");


        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }
    }
}
