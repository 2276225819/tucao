﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaParsers.FlvParser
{
    public class VideoData
    {
        public VideoData(uint mediaInfo, byte[] buffer)
        {
            this.FrameType = (FrameType)(mediaInfo >> 4);
            this.CodecID = (CodecID)(mediaInfo & 0x0F);
            this.AVCVideoPacket = new AVCVideoPacket(buffer);
        }

        public FrameType FrameType
        {
            get;
            private set;
        }

        public CodecID CodecID
        {
            get;
            private set;
        }

        public AVCVideoPacket AVCVideoPacket
        {
            get;
            private set;
        }
    }
}
