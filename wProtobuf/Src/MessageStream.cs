using System;

namespace wProtobuf
{
    public partial class MessageStream
    {
        public int WriteRemain { get { return mSize - mWritePos; } }
        public int ReadPos { get { return mReadPos; } set { mReadPos = value; } }
        public int WritePos { get { return mWritePos; } set { mWritePos = value; } }

        // 还可读取的大小
        public int ReadSize { get { return WritePos - ReadPos; } }

        protected int mSize = 0;
        protected byte[] mBuffer; // 缓存
        protected int mReadPos = 0; // 当前读取的位置
        protected int mWritePos = 0; // 当前写入的位置

        public int size { get { return mSize; } }

        // 缓存
        public byte[] Buffer { get { return mBuffer; } }

        public MessageStream(byte[] data)
        {
            mBuffer = data;
            if (mBuffer != null)
                mSize = mBuffer.Length;
        }

        public MessageStream(int capacity)
        {
            mSize = capacity;
            mBuffer = new byte[capacity];
        }

        void CheckReadSize(int size)
        {
            if (mReadPos + size > mSize)
            {
                throw InvalidProtocolBufferException.NegativeSize();
            }
        }

        public void ensureCapacity(int length)
        {
            if (length > WriteRemain)
            {
                int newsize = (length + mSize) > (mSize * 2) ? (length + mSize) : (mSize * 2);

                Array.Resize<byte>(ref mBuffer, newsize);
                mSize = newsize;
            }
        }

        public void Reset()
        {
            mReadPos = 0; // 当前读取的位置
            mWritePos = 0; // 当前写入的位置
        }

        // 移动数据块,从offset位置移到初始位置，offset后面的数据保留，其他的全部丢弃
        public void Move(int offset)
        {
            Array.Copy(mBuffer, offset, mBuffer, 0, WritePos - offset);
            WritePos -= offset; // 写入位置转换下
            ReadPos = 0; // 读取位置为0
        }

        public void Write(byte value)
        {
            ensureCapacity(1);
            mBuffer[mWritePos++] = value;
        }

        public void Write(bool value)
        {
            ensureCapacity(1);
            mBuffer[mWritePos++] = (byte)(value ? 1 : 0);
        }

        public void Write(byte[] buffer)
        {
            ensureCapacity(buffer.Length);
            Array.Copy(buffer, 0, mBuffer, mWritePos, buffer.Length);
            mWritePos += buffer.Length;
        }

        public void Write(short value)
        {
            ensureCapacity(2);
            mBuffer[mWritePos + 1] = (byte)(value >> 8);
            mBuffer[mWritePos + 0] = (byte)(value >> 0);
            mWritePos += 2;
        }

        public void Write(ushort value)
        {
            ensureCapacity(2);
            mBuffer[mWritePos + 1] = (byte)(value >> 8);
            mBuffer[mWritePos + 0] = (byte)(value >> 0);
            mWritePos += 2;
        }

        public byte ReadByte()
        {
            CheckReadSize(1);
            return mBuffer[mReadPos++];
        }

        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            CheckReadSize(count);
            Array.Copy(mBuffer, mReadPos, bytes, offset, count);
            mReadPos += count;
        }


        public short ReadInt16()
        {
            CheckReadSize(2);
            short i = (short)(((mBuffer[mReadPos + 1] & 0xFF) << 8) + ((mBuffer[mReadPos + 0] & 0xFF) << 0));
            mReadPos += 2;
            return i;
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadInt16();
        }

        public void Write(byte[] data, int offset, int count)
        {
            ensureCapacity(count);
            Array.Copy(data, offset, mBuffer, mWritePos, count);
            mWritePos += count;
        }
    }
}