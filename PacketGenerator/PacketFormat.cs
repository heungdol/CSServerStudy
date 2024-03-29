﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    // {0} 패킷 이름 / 넘버
    // {1} 패킷 목록
    class PacketFormat
    {
        public static string fileFormat =
@"
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{{
    {0}
}}

{1}
";
        // {0} 패킷 이름
        // {1} 패킷 넘버
        public static string packetEnumFormat =
@"{0} = {1},";


        /*
         * {0} 패킷 이름
         * {1} 멤버 변수들
         * {2} 멤버 변수 Read
         * {3} 멤버 변수 Write
         * */
        public static string packetFormat =
@"
public class {0}
{{
    {1}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        bool success = true;
        ushort count = 0;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);


        {3}


        success &= BitConverter.TryWriteBytes(span, count);

        if (success == false)
        {{
            return null;
        }}

        return SendBufferHelper.Close(count);
    }}
}}



";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 대문자
        // {1} 리스트 이름 소문자
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"public class {0}
{{
    {2}
    
    public void Read (ReadOnlySpan <byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write (Span <byte> s, ref ushort count)
    {{
        bool success = true;

        {4}

        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();";

        // {0} 변수 이름
        // {1} TO~~~
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof ({1});";

        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Length = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);

this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Length));
count += {0}Length;";

        // {0} 리스트 이름 대문자
        // {1} 리스트 이름 소문자
        public static string readListFormat =
@"ushort {1}Lenth = BitConverter.ToUInt16 (s.Slice (count, s.Length - count));
count += sizeof(ushort);

{1}s.Clear();

for (ushort i = 0; i < {1}Lenth; i++)
{{
    {0} {1} = new {0} ();
    {1}.Read(s, ref count);

    {1}s.Add({1});
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof ({1});";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Length);
count += sizeof(ushort);
count += {0}Length;";

        // {0} 리스트 이름 대문자
        // {1} 리스트 이름 소문자
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);

foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Write(span, ref count);
}}";
    }
}
