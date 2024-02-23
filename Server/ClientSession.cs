using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetID;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerID;
        public string playerName;

        public PlayerInfoReq()
        {
            this.packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            //this.size = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            //this.packetID = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.playerName = Encoding.Unicode.GetString(s.Slice(count, nameLength));
            count += nameLength;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetID);
            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerID);
            count += sizeof(long);


            // string
            ushort nameLength = (ushort)Encoding.Unicode.GetByteCount(this.playerName);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLength);
            count += sizeof(ushort);

            Array.Copy(Encoding.Unicode.GetBytes(this.playerName), 0, segment.Array, count, nameLength);
            count += nameLength;


            success &= BitConverter.TryWriteBytes(span, count);


            if (success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(count);
        }
    }

    //class PlayerInfoOk : Packet
    //{
    //    public int hp;
    //    public int attack;
    //}

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

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
