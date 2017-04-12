using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace wProtobuf
{
    public sealed class ByteString : IEnumerable<byte>, IEquatable<ByteString>
    {
        private static readonly ByteString empty = new ByteString(new byte[0]);

        private readonly byte[] bytes;

        public static ByteString AttachBytes(byte[] bytes)
        {
            return new ByteString(bytes);
        }

        private ByteString(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public byte[] buffer { get { return bytes; } }

        public static ByteString Empty
        {
            get { return empty; }
        }

        public int Length
        {
            get { return bytes == null ? 0 : bytes.Length; }
        }

        public bool IsEmpty
        {
            get { return Length == 0; }
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(bytes);
        }

        public static ByteString FromBase64(string bytes)
        {
            return bytes == "" ? Empty : new ByteString(Convert.FromBase64String(bytes));
        }

        public byte this[int index]
        {
            get { return bytes[index]; }
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>) bytes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool operator ==(ByteString lhs, ByteString rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
            {
                return false;
            }
            if (lhs.bytes.Length != rhs.bytes.Length)
            {
                return false;
            }
            for (int i = 0; i < lhs.Length; i++)
            {
                if (rhs.bytes[i] != lhs.bytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(ByteString lhs, ByteString rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as ByteString);
        }

        public override int GetHashCode()
        {
            int ret = 23;
            foreach (byte b in bytes)
            {
                ret = (ret * 31) + b;
            }
            return ret;
        }

        public bool Equals(ByteString other)
        {
            return this == other;
        }
    }
}