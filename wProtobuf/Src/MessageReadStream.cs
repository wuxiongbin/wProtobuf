using System;

namespace wProtobuf
{
    public partial class MessageStream : IReadStream
    {
        // Ìø¹ý´Ë×Ö¶Î
        public void SkipLastField(uint tag)
        {
            if (tag == 0)
            {
                throw new InvalidOperationException("SkipLastField cannot be called at the end of a stream");
            }
            switch (WireFormat.GetTagWireType(tag))
            {
            case WireFormat.WireType.StartGroup:
            case WireFormat.WireType.EndGroup:
                throw new InvalidOperationException("SkipLastField cannot be called by StartGroup or EndGroup!");

            case WireFormat.WireType.Fixed32:
                ReadFixed32();
                break;
            case WireFormat.WireType.Fixed64:
                ReadFixed64();
                break;
            case WireFormat.WireType.LengthDelimited:
                var length = ReadLength();
                CheckReadSize(length);
                mReadPos += length;
                break;
            case WireFormat.WireType.Varint:
                ReadRawVarint32();
                break;
            }
        }

        public uint ReadTag()
        {
            uint tag = 0;

            // Optimize for the incredibly common case of having at least two bytes left in the buffer,
            // and those two bytes being enough to get the tag. This will be true for fields up to 4095.
            if (mReadPos + 2 <= mWritePos)
            {
                int tmp = mBuffer[mReadPos++];
                if (tmp < 128)
                {
                    tag = (uint)tmp;
                }
                else
                {
                    int result = tmp & 0x7f;
                    if ((tmp = mBuffer[mReadPos++]) < 128)
                    {
                        result |= tmp << 7;
                        tag = (uint)result;
                    }
                    else
                    {
                        // Nope, rewind and go the potentially slow route.
                        mReadPos -= 2;
                        tag = ReadRawVarint32();
                    }
                }
            }
            else
            {
                if (ReadSize == 0)
                {
                    tag = 0;
                    return 0; // This is the only case in which we return 0.
                }

                tag = ReadRawVarint32();
            }
            if (tag == 0)
            {
                // If we actually read zero, that's not a valid tag.
                throw InvalidProtocolBufferException.InvalidTag();
            }
            return tag;
        }

        // ¶ÁÈ¡
        public int ReadInt32()
        {
            return (int)ReadRawVarint32();
        }

        public uint ReadUInt32()
        {
            return ReadRawVarint32();
        }

        public long ReadInt64()
        {
            return (long)ReadRawVarint64();
        }

        public ulong ReadUInt64()
        {
            return ReadRawVarint64();
        }

        public string ReadString()
        {
            int length = ReadLength();
            // No need to read any data for an empty string.
            if (length == 0)
            {
                return "";
            }

            CheckReadSize(length);

            // Fast path:  We already have the bytes in a contiguous buffer, so
            //   just copy directly from it.
            String result = Utf8Encoding.GetString(mBuffer, mReadPos, length);
            mReadPos += length;
            return result;
        }

        public bool ReadBool()
        {
            CheckReadSize(1);
            return mBuffer[mReadPos++] != 0;
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble((long)ReadRawLittleEndian64());
        }

        public float ReadFloat()
        {
            if (BitConverter.IsLittleEndian)
            {
                CheckReadSize(4);
                float ret = BitConverter.ToSingle(mBuffer, mReadPos);
                mReadPos += 4;
                return ret;
            }
            else
            {
                byte[] rawBytes = ReadRawBytes(4);
                ByteArray.Reverse(rawBytes);
                return BitConverter.ToSingle(rawBytes, 0);
            }
        }

        public int ReadSInt32()
        {
            return DecodeZigZag32(ReadRawVarint32());
        }

        public long ReadSInt64()
        {
            return DecodeZigZag64(ReadRawVarint64());
        }

        public int ReadEnum()
        {
            // Currently just a pass-through, but it's nice to separate it logically from WriteInt32.
            return (int)ReadRawVarint32();
        }

        public uint ReadFixed32()
        {
            return ReadRawLittleEndian32();
        }

        public ulong ReadFixed64()
        {
            return ReadRawLittleEndian64();
        }

        public int ReadSFixed32()
        {
            return (int)ReadRawLittleEndian32();
        }

        public long ReadSFixed64()
        {
            return (long)ReadRawLittleEndian64();
        }

        public ByteString ReadBytes()
        {
            int length = ReadLength();
            return ByteString.AttachBytes(ReadRawBytes(length));
        }

        public void ReadMessage(IMessage builder)
        {
            int wpos = WritePos;
            int count = ReadLength();
            int readpos = ReadPos;
            WritePos = readpos + count;

            builder.MergeFrom(this);

            ReadPos = readpos + count;
            WritePos = wpos;
        }

        public void ReadMessage(Action fun)
        {
            int wpos = WritePos;
            int count = ReadLength();
            int readpos = ReadPos;
            WritePos = readpos + count;

            while (ReadSize != 0)
                fun();

            ReadPos = readpos + count;
            WritePos = wpos;
        }
    }
}