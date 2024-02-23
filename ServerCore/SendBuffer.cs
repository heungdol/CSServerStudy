using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> currentBuffer 
            = new ThreadLocal<SendBuffer> (() => { return null;  });

        public static int chunkSize = 4096;

        public static ArraySegment<byte> Open (int reserveSize)
        {
            if (currentBuffer.Value == null)
            {
                currentBuffer.Value = new SendBuffer (chunkSize); 
            }

            if (currentBuffer.Value.FreeSize < reserveSize)
            {
                currentBuffer.Value = new SendBuffer(chunkSize);
            }

            return currentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close (int usedSize)
        {
            return currentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunckSize)
        {
            _buffer = new byte[chunckSize];
        }

        public int FreeSize
        {
            get { return _buffer.Length - _usedSize; }
        }

        public ArraySegment <byte> Open (int reserveSize)
        {
            if (reserveSize > FreeSize)
            {
                return null;
            }

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment <byte> Close (int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte> (_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }
    }
}
