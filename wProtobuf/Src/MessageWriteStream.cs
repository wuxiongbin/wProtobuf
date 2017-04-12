using System;
using System.Text;

namespace wProtobuf
{
    public partial class MessageStream : IWriteStream
    {
        // Ð´Èë
        public void WriteRawTag(byte b1)
        {
            ensureCapacity(1);
            Buffer[mWritePos++] = b1;
        }

        public void WriteRawTag(byte b1, byte b2)
        {
            ensureCapacity(2);
            Buffer[mWritePos++] = b1;
            Buffer[mWritePos++] = b2;
        }

        public void WriteRawTag(byte b1, byte b2, byte b3)
        {
            ensureCapacity(3);
            Buffer[mWritePos++] = b1;
            Buffer[mWritePos++] = b2;
            Buffer[mWritePos++] = b3;
        }

        public void WriteRawTag(byte b1, byte b2, byte b3, byte b4)
        {
            ensureCapacity(4);
            Buffer[mWritePos++] = b1;
            Buffer[mWritePos++] = b2;
            Buffer[mWritePos++] = b3;
            Buffer[mWritePos++] = b4;
        }

        public void WriteRawTag(byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            ensureCapacity(5);
            Buffer[mWritePos++] = b1;
            Buffer[mWritePos++] = b2;
            Buffer[mWritePos++] = b3;
            Buffer[mWritePos++] = b4;
            Buffer[mWritePos++] = b5;
        }

        public void WriteInt32(int value)
        {
            if (value >= 0)
            {
                WriteRawVarint32((uint)value);
            }
            else
            {
                // Must sign-extend.
                WriteRawVarint64((ulong)value);
            }
        }

        public void WriteInt64(long value)
        {
            WriteRawVarint64((ulong)value);
        }

        public void WriteUInt32(uint value)
        {
            WriteRawVarint32(value);
        }

        public void WriteUInt64(ulong value)
        {
            WriteRawVarint64(value);
        }

        internal static readonly Encoding Utf8Encoding = Encoding.UTF8;

        public void WriteLength(int length)
        {
            WriteRawVarint32((uint)length);
        }

        public void WriteEnum(int value)
        {
            WriteInt32(value);
        }

        public void WriteString(string value)
        {
            int length = Utf8Encoding.GetByteCount(value);
            WriteLength(length);
            ensureCapacity(length);

            if (length == value.Length) // Must be all ASCII...
            {
                for (int i = 0; i < length; i++)
                {
                    mBuffer[mWritePos + i] = (byte)value[i];
                }
            }
            else
            {
                Utf8Encoding.GetBytes(value, 0, value.Length, mBuffer, mWritePos);
            }
            mWritePos += length;
        }

        public void WriteBool(bool value)
        {
            ensureCapacity(1);
            Buffer[mWritePos++] = (value ? (byte)1 : (byte)0);
        }

        public void WriteDouble(double value)
        {
            WriteRawLittleEndian64((ulong)BitConverter.DoubleToInt64Bits(value));
        }

        public void WriteFloat(float value)
        {
            byte[] rawBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                ByteArray.Reverse(rawBytes);
            }

            WriteRawBytes(rawBytes, 0, 4);
        }

        public void WriteSInt32(int value)
        {
            WriteRawVarint32(ComputeSize.EncodeZigZag32(value));
        }

        public void WriteSInt64(long value)
        {
            WriteRawVarint64(ComputeSize.EncodeZigZag64(value));
        }

        public void WriteFixed32(uint value)
        {
            WriteRawLittleEndian32(value);
        }

        public void WriteFixed64(ulong value)
        {
            WriteRawLittleEndian64(value);
        }

        public void WriteSFixed32(int value)
        {
            WriteRawLittleEndian32((uint)value);
        }

        public void WriteSFixed64(long value)
        {
            WriteRawLittleEndian64((ulong)value);
        }

        public void WriteBytes(ByteString value)
        {
            WriteLength(value.Length);

            WriteRawBytes(value.buffer);
        }

        public void WriteMessage(IMessage value)
        {
            WriteLength(value.CalculateSize());
            value.WriteTo(this);
        }
    }
}