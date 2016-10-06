using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaParsers.FlvParser
{
    public class FlvFile
    {    
        public FlvFile(Stream stream)
        {
            long offset = 0;
            this.FlvHeader = new FlvHeader(stream, ref offset);
            this.FlvFileBody = new FlvFileBody(stream, ref offset);
        }

        public FlvHeader FlvHeader
        {
            get;
            private set;
        }

        public FlvFileBody FlvFileBody
        {
            get;
            private set;
        }
    }
}
