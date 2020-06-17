using System;
using System.Collections.Generic;
using System.Text;

namespace OrderPreservingEncryptionDotNet
{
    public class ValueRange
    {
        private long _start;
        private long _end;

        public long Start { get => _start; set => _start = value; }
        public long End { get => _end; set => _end = value; }

        public ValueRange Copy()
        {
            return new ValueRange(Start, End);
        }

        public ValueRange(long start, long end)
        {
            if (end < start)
            {
                throw new Exception("ValueRange: End must be greater than Start");
            }
            Start = start;
            End = end;
        }
        public long Size()
        {
            return End - Start + 1;
        }
        public long BitSize()
        {
            return Convert.ToInt64(Math.Ceiling(Math.Log(Size(), 2)));
        }
        public bool Contains(long value)
        {
            return End >= value && value >= Start;
        }
    }
}
