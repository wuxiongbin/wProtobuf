namespace wProtobuf
{
    public partial class MessageStream
    {
        internal void WriteRawVarint32(uint value)
        {
            // Optimize for the common case of a single byte value
            if (value < 128)
            {
                ensureCapacity(1);
                mBuffer[mWritePos++] = (byte)value;
                return;
            }

            while (value > 127)
            {
                ensureCapacity(1);
                mBuffer[mWritePos++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }

            ensureCapacity(1);
            mBuffer[mWritePos++] = (byte)value;
        }

        internal void WriteRawVarint64(ulong value)
        {
            while (value > 127)
            {
                ensureCapacity(1);
                mBuffer[mWritePos++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }

            ensureCapacity(1);
            mBuffer[mWritePos++] = (byte)value;
        }

        internal void WriteRawBytes(byte[] value)
        {
            WriteRawBytes(value, 0, value.Length);
        }

        /// <summary>
        /// Writes out part of an array of bytes.
        /// </summary>
        internal void WriteRawBytes(byte[] value, int offset, int length)
        {
            ensureCapacity(length);
            ByteArray.Copy(value, offset, mBuffer, mWritePos, length);
            mWritePos += length;
        }

        internal void WriteRawLittleEndian64(ulong value)
        {
            ensureCapacity(8);

            mBuffer[mWritePos++] = ((byte)value);
            mBuffer[mWritePos++] = ((byte)(value >> 8));
            mBuffer[mWritePos++] = ((byte)(value >> 16));
            mBuffer[mWritePos++] = ((byte)(value >> 24));
            mBuffer[mWritePos++] = ((byte)(value >> 32));
            mBuffer[mWritePos++] = ((byte)(value >> 40));
            mBuffer[mWritePos++] = ((byte)(value >> 48));
            mBuffer[mWritePos++] = ((byte)(value >> 56));
        }

        internal void WriteRawLittleEndian32(uint value)
        {
            ensureCapacity(4);

            mBuffer[mWritePos++] = ((byte)value);
            mBuffer[mWritePos++] = ((byte)(value >> 8));
            mBuffer[mWritePos++] = ((byte)(value >> 16));
            mBuffer[mWritePos++] = ((byte)(value >> 24));
        }

        internal uint ReadRawVarint32()
        {
            if (ReadSize < 5)
            {
                return SlowReadRawVarint32();
            }

            int tmp = mBuffer[mReadPos++];
            if (tmp < 128)
            {
                return (uint)tmp;
            }
            int result = tmp & 0x7f;
            if ((tmp = mBuffer[mReadPos++]) < 128)
            {
                result |= tmp << 7;
            }
            else
            {
                result |= (tmp & 0x7f) << 7;
                if ((tmp = mBuffer[mReadPos++]) < 128)
                {
                    result |= tmp << 14;
                }
                else
                {
                    result |= (tmp & 0x7f) << 14;
                    if ((tmp = mBuffer[mReadPos++]) < 128)
                    {
                        result |= tmp << 21;
                    }
                    else
                    {
                        result |= (tmp & 0x7f) << 21;
                        result |= (tmp = mBuffer[mReadPos++]) << 28;
                        if (tmp >= 128)
                        {
                            // Discard upper 32 bits.
                            // Note that this has to use ReadRawByte() as we only ensure we've
                            // got at least 5 bytes at the start of the method. This lets us
                            // use the fast path in more cases, and we rarely hit this section of code.
                            for (int i = 0; i < 5; i++)
                            {
                                if (ReadRawByte() < 128)
                                {
                                    return (uint)result;
                                }
                            }
                            throw InvalidProtocolBufferException.MalformedVarint();
                        }
                    }
                }
            }
            return (uint)result;
        }

        internal byte ReadRawByte()
        {
            CheckReadSize(1);
            return mBuffer[mReadPos++];
        }

        private uint SlowReadRawVarint32()
        {
            int tmp = ReadRawByte();
            if (tmp < 128)
            {
                return (uint)tmp;
            }
            int result = tmp & 0x7f;
            if ((tmp = ReadRawByte()) < 128)
            {
                result |= tmp << 7;
            }
            else
            {
                result |= (tmp & 0x7f) << 7;
                if ((tmp = ReadRawByte()) < 128)
                {
                    result |= tmp << 14;
                }
                else
                {
                    result |= (tmp & 0x7f) << 14;
                    if ((tmp = ReadRawByte()) < 128)
                    {
                        result |= tmp << 21;
                    }
                    else
                    {
                        result |= (tmp & 0x7f) << 21;
                        result |= (tmp = ReadRawByte()) << 28;
                        if (tmp >= 128)
                        {
                            // Discard upper 32 bits.
                            for (int i = 0; i < 5; i++)
                            {
                                if (ReadRawByte() < 128)
                                {
                                    return (uint)result;
                                }
                            }
                            throw InvalidProtocolBufferException.MalformedVarint();
                        }
                    }
                }
            }
            return (uint)result;
        }

        internal ulong ReadRawVarint64()
        {
            int shift = 0;
            ulong result = 0;
            while (shift < 64)
            {
                byte b = ReadRawByte();
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
                shift += 7;
            }
            throw InvalidProtocolBufferException.MalformedVarint();
        }

        public int ReadLength()
        {
            return (int)ReadRawVarint32();
        }

        internal ulong ReadRawLittleEndian64()
        {
            CheckReadSize(8);

            ulong b1 = mBuffer[mReadPos++];
            ulong b2 = mBuffer[mReadPos++];
            ulong b3 = mBuffer[mReadPos++];
            ulong b4 = mBuffer[mReadPos++];
            ulong b5 = mBuffer[mReadPos++];
            ulong b6 = mBuffer[mReadPos++];
            ulong b7 = mBuffer[mReadPos++];
            ulong b8 = mBuffer[mReadPos++];
            return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24)
                   | (b5 << 32) | (b6 << 40) | (b7 << 48) | (b8 << 56);
        }

        internal byte[] ReadRawBytes(int size)
        {
            if (size < 0)
            {
                throw InvalidProtocolBufferException.NegativeSize();
            }

            CheckReadSize(size);

            byte[] bytes = new byte[size];
            ByteArray.Copy(mBuffer, mReadPos, bytes, 0, size);
            mReadPos += size;
            return bytes;
        }

        internal static int DecodeZigZag32(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1);
        }

        internal static long DecodeZigZag64(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1);
        }

        internal uint ReadRawLittleEndian32()
        {
            CheckReadSize(4);
            uint b1 = mBuffer[mReadPos++];
            uint b2 = mBuffer[mReadPos++];
            uint b3 = mBuffer[mReadPos++];
            uint b4 = mBuffer[mReadPos++];
            return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
        }
    }
}