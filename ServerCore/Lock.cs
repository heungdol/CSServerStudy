using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            int desired = 1;
            int expected = 0;

            while (true)
            {
                if (expected == Interlocked.CompareExchange(ref _locked, desired, expected))
                {
                    break;
                }
            }
        }

        public void Release()
        {
            int desired = 0;

            Interlocked.Exchange(ref _locked, desired);
        }
    }


    // 재귀적 락 허용
    // 5000 스핀락 수행 후 Yield
    class Lock
    {
        const int EMPTY_FLAG        = 0x00000000;
        const int WRITE_MASK        = 0x7FFF0000;
        const int READ_MASK         = 0x0000FFFF;
        const int MAX_SPIN_COUNT    = 5000;

        // 0: Unused, 1~15: WriteThread, 16~31: ReadCount
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock () 
        {
            // 동일 쓰레드가 이미 라이트락을 수행하고 있을 때

            int lockThreadID = (int)((_flag & WRITE_MASK) >> 16);
            if (lockThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                _writeCount++;
                return;
            }

            // 어떤 쓰레드도 WriteLock or ReadLock을 가지고 있지 않을 때
            // 경합을 통해 해당 쓰레드에서 소유권을 얻는다

            int desired = (int)(Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;

            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange (ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock ()
        {
            _writeCount--;

            if (_writeCount == 0)
            {
                Interlocked.Exchange (ref _flag, EMPTY_FLAG);
            }
        }

        public void ReadLock ()
        {
            // 동일 쓰레드가 이미 라이트락을 수행하고 있을 때

            int lockThreadID = (int)((_flag & WRITE_MASK) >> 16);
            if (lockThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                Interlocked.Increment (ref _flag);
                return;
            }

            // Read와 관계 없이, 어떠한 쓰레드도 Write의 소유권을 가지고 있지 않을 때
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (int)(_flag & READ_MASK);
                    
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }

                Thread.Yield ();    
            }
        }

        public void ReadUnlock ()
        {
            Interlocked.Decrement (ref _flag);
        }
    }
}
