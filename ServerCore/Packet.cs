//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ServerCore
//{
//    //public abstract class Packet
//    //{
//    //    //public ushort size;
//    //    //public ushort packetID;

//    //    public abstract ArraySegment<byte> Write();
//    //    public abstract void Read(ArraySegment<byte> s);
//    //}


//    public class PlayerInfoReq
//    {
//        public long playerID;
//        public string playerName;
//        public struct Skill
//        {
//            public int id;
//            public short level;
//            public float duration;

//            public void Read(ReadOnlySpan<byte> s, ref ushort count)
//            {
//                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
//                count += sizeof(int);
//                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
//                count += sizeof(short);
//                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
//                count += sizeof(float);
//            }

//            public bool Write(Span<byte> s, ref ushort count)
//            {
//                bool success = true;

//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
//                count += sizeof(int);
//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
//                count += sizeof(short);
//                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
//                count += sizeof(float);

//                return success;
//            }
//        }

//        public List<Skill> skills = new List<Skill>();

//        public void Read(ArraySegment<byte> segment)
//        {
//            ushort count = 0;

//            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
//            count += sizeof(ushort);
//            count += sizeof(ushort);

//            this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
//            count += sizeof(long);
//            ushort playerNameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//            count += sizeof(ushort);

//            this.playerName = Encoding.Unicode.GetString(s.Slice(count, playerNameLength));
//            count += playerNameLength;
//            ushort skillLenth = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
//            count += sizeof(ushort);

//            skills.Clear();

//            for (ushort i = 0; i < skillLenth; i++)
//            {
//                Skill skill = new Skill();
//                skill.Read(s, ref count);

//                skills.Add(skill);
//            }
//        }

//        public ArraySegment<byte> Write()
//        {
//            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

//            bool success = true;
//            ushort count = 0;

//            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

//            count += sizeof(ushort);

//            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInfoReq);
//            count += sizeof(ushort);


//            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerID);
//            count += sizeof(long);
//            ushort playerNameLength = (ushort)Encoding.Unicode.GetBytes(this.playerName, 0, this.playerName.Length, segment.Array, segment.Offset + count + sizeof(ushort));
//            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), playerNameLength);
//            count += sizeof(ushort);
//            count += playerNameLength;
//            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
//            count += sizeof(ushort);

//            foreach (Skill skill in this.skills)
//            {
//                success &= skill.Write(span, ref count);
//            }


//            success &= BitConverter.TryWriteBytes(span, count);

//            if (success == false)
//            {
//                return null;
//            }

//            return SendBufferHelper.Close(count);
//        }
//    }





//    //class PlayerInfoOk : Packet
//    //{
//    //    public int hp;
//    //    public int attack;
//    //}

//    public enum PacketID
//    {

//        PlayerInfoReq = 1,
//        PlayerInfoOk = 2,
//    }
//}
