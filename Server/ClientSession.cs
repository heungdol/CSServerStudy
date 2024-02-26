using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");


            //Packet packet = new Packet() { size = 100, packetID = 10};
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetID);

            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);

            //ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            //Send(sendBuffer);

            Thread.Sleep(1000);

            Disconnect();
        }

        //public override int OnReceive(ArraySegment<byte> buffer)
        //{
        //    string recieveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"From Client: {recieveData}");

        //    return buffer.Count;
        //}

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;


            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:

                    PlayerInfoReq p = new PlayerInfoReq();
                    p.Read(buffer);

                    Console.WriteLine($"Player InfoReq: {p.playerID} {p.playerName}");

                    foreach (PlayerInfoReq.Skill skill in p.skills)
                    {
                        Console.WriteLine($"Skill ({skill.id}) ({skill.level}) ({skill.duration})");
                    }

                    break;

                case PacketID.PlayerInfoOk:

                break;
            }


            Console.WriteLine($"Receive Packet ID: {id}, Size: {size}");
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
