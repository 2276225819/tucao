using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaParsers.FlvParser
{
    public class FlvTag
    {
        public FlvTag(Stream stream, ref long offset)
        {  
            this.TagType = (TagType)stream.ReadUInt8(ref offset);

            this.DataSize = stream.ReadUInt24(ref offset);

            var value = stream.ReadUInt24(ref offset);
            value |= stream.ReadUInt8(ref offset) << 24;
            this.Timestamp = TimeSpan.FromMilliseconds(value).Ticks;

            this.StreamID = stream.ReadUInt24(ref offset);
            var mediaInfo = stream.ReadUInt8(ref offset);
            this.Count = this.DataSize - 1;

            if ((int)this.TagType == 0x8)
            {
                this.Offset = offset + 1;
            }
            else if (((int)this.TagType == 0x9))
            {
                this.Offset = offset + 4;
            }

            byte[] bytes = stream.ReadBytes(ref offset, (int)this.Count);

            if (this.TagType == TagType.Video)
                this.VideoData = new VideoData(mediaInfo, bytes);

            this.TagSize = stream.ReadUInt32(ref offset);
        }

        public TagType TagType
        {
            get;
            private set;
        }

        public uint DataSize
        {
            get;
            private set;
        }

        public long Timestamp
        {
            get;
            private set;
        }

        public uint StreamID
        {
            get;
            private set;
        }

        public AudioData AudioData
        {
            get;
            private set;
        }

        public VideoData VideoData
        {
            get;
            private set;
        }

        public uint TagSize
        {
            get;
            private set;
        }

        public long Offset
        {
            get;
            private set;
        }

        public uint Count
        {
            get;
            private set;
        }
    }
}
