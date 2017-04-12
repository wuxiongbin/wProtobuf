using System;

namespace wProtobuf
{
    public interface IMessage
    {
        void MergeFrom(IReadStream input);

        void WriteTo(IWriteStream output);

        int CalculateSize();
    }
}