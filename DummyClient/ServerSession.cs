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

        public PlayerInfoReq ()
        {
            this.packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte> (segment.Array, segment.Offset, segment.Count);

            //this.size = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            //this.packetID = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.playerID = BitConverter.ToInt64(s.Slice (count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.playerName = Encoding.Unicode.GetString (s.Slice (count, nameLength));
            count += nameLength;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(span.Slice (count, span.Length - count), this.packetID);
            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerID);
            count += sizeof(long);


            // string
            //ushort nameLength = (ushort)Encoding.Unicode.GetByteCount(this.playerName);
            //success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLength);
            //count += sizeof(ushort);

            //Array.Copy (Encoding.Unicode.GetBytes (this.playerName), 0, segment.Array, count, nameLength);
            //count += nameLength;

            ushort nameLength = (ushort)Encoding.Unicode.GetBytes(this.playerName, 0, this.playerName.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLength);
            count += sizeof(ushort);
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { /*size = 4,*/ /*packetID = (ushort)PacketID.PlayerInfoReq,*/ playerID = 1001, playerName = "cool name" };
            // Send
            //for (int i = 0; i < 5; i++)
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
