using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaParsers.FlvParser
{
    public class FlvFileBody
    {
        private List<FlvTag> tags = new List<FlvTag>();

        public FlvFileBody(Stream stream, ref long offset)
        {
            uint previousTagSize = stream.ReadUInt32(ref offset);

            while (offset < stream.Length)
            { 
                tags.Add(new FlvTag(stream, ref offset));
            }
        }

        public List<FlvTag> Tags
        {
            get
            {
                return tags;
            }
        }
    }
}
