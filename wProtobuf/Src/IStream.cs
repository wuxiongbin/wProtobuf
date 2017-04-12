namespace wProtobuf
{
    public interface IWriteStream
    {
        // 写入
        void WriteRawTag(byte b1);
        void WriteRawTag(byte b1, byte b2);
        void WriteRawTag(byte b1, byte b2, byte b3);
        void WriteRawTag(byte b1, byte b2, byte b3, byte b4);
        void WriteRawTag(byte b1, byte b2, byte b3, byte b4, byte b5);

        void WriteInt32(int value);
        void WriteInt64(long value);

        void WriteUInt32(uint value);
        void WriteUInt64(ulong value);

        void WriteString(string value);

        void WriteBool(bool value);

        void WriteDouble(double value);
        void WriteFloat(float value);

        void WriteSInt32(int value);
        void WriteSInt64(long value);

        void WriteFixed32(uint value);
        void WriteFixed64(ulong value);

        void WriteSFixed32(int value);
        void WriteSFixed64(long value);

        void WriteBytes(ByteString value);

        void WriteMessage(IMessage value);

        void WriteEnum(int value);
    }

    public delegate void Action();

    public interface IReadStream
    {
        // 跳过此字段
        void SkipLastField(uint tag);

        uint ReadTag();

        // 读取
        int ReadInt32();
        uint ReadUInt32();

        long ReadInt64();
        ulong ReadUInt64();

        string ReadString();

        bool ReadBool();

        double ReadDouble();
        float ReadFloat();

        int ReadSInt32();
        long ReadSInt64();

        int ReadEnum();

        uint ReadFixed32();
        ulong ReadFixed64();

        int ReadSFixed32();
        long ReadSFixed64();

        ByteString ReadBytes();

        void ReadMessage(IMessage builder);

        void ReadMessage(Action fun);
    }
}