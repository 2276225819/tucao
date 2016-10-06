using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MediaParsers.FlvParser;
using MediaParsers.Mp4Parser;
using Windows.Media.Core;
 

namespace MediaParsers
{
    public class FlvMediaStreamSource  
    {
        static readonly byte[] startCode = new byte[] { 0, 0, 1 };

        private MediaStreamDescription videoStreamDescription;
        private MediaStreamDescription audioStreamDescription;

        private int audioSampleIndex = 1;
        private int videoSampleIndex = 1;

        Stream mediaStream;
        List<FlvTag> audioSamples;
        List<FlvTag> videoSamples;

        Dictionary<MediaSampleAttributeKeys, string> emptyDict = new Dictionary<MediaSampleAttributeKeys, string>();

        public FlvMediaStreamSource(Stream stream)
        {
            this.mediaStream = stream;
        }

        protected override void OpenMediaAsync()
        {
            var flvFile = new FlvFile(this.mediaStream);
            this.audioSamples = flvFile.FlvFileBody.Tags.Where(tag => tag.TagType == TagType.Audio).ToList();
            this.videoSamples = flvFile.FlvFileBody.Tags.Where(tag => tag.TagType == TagType.Video).ToList();

            //Audio
            WaveFormatExtensible wfx = new WaveFormatExtensible();
            wfx.FormatTag = 0x00FF;
            wfx.Channels = 2;
            wfx.BlockAlign = 8;
            wfx.BitsPerSample = 16;
            wfx.SamplesPerSec = 44100;
            wfx.AverageBytesPerSecond = wfx.SamplesPerSec * wfx.Channels * wfx.BitsPerSample / wfx.BlockAlign;
            wfx.Size = 0;
            string codecPrivateData = wfx.ToHexString();

            Dictionary<MediaStreamAttributeKeys, string> audioStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            audioStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = codecPrivateData;
            this.audioStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, audioStreamAttributes);

            //Video
            Dictionary<MediaStreamAttributeKeys, string> videoStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            videoStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "H264";
            this.videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, videoStreamAttributes);

            //Media
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = audioSamples.Last().Timestamp.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = true.ToString();

            List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();
            mediaStreamDescriptions.Add(this.audioStreamDescription);
            mediaStreamDescriptions.Add(this.videoStreamDescription);

            this.ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            MediaStreamSample mediaStreamSample = null;

            if (mediaStreamType == MediaStreamType.Audio)
                mediaStreamSample = this.GetAudioSample();
            else if (mediaStreamType == MediaStreamType.Video)
                mediaStreamSample = this.GetVideoSample();

            this.ReportGetSampleCompleted(mediaStreamSample);
        }

        private MediaStreamSample GetAudioSample()
        {
            var sample = audioSamples[audioSampleIndex];
            MediaStreamSample mediaStreamSample = new MediaStreamSample(this.audioStreamDescription, this.mediaStream, sample.Offset, sample.Count, sample.Timestamp, emptyDict);
            audioSampleIndex++;
            return mediaStreamSample;
        }

        private MediaStreamSample GetVideoSample()
        {
            var sample = videoSamples[videoSampleIndex];

            MemoryStream stream = new MemoryStream();

            if (videoSampleIndex == 1)
            {
                mediaStream.Seek(videoSamples[0].Offset, SeekOrigin.Begin);
                BinaryReader binaryReader = new BinaryReader(mediaStream);

                var configurationVersion = binaryReader.ReadByte();
                var avcProfileIndication = binaryReader.ReadByte();
                var avcCompatibleProfiles = binaryReader.ReadByte();
                var avcLevelIndication = binaryReader.ReadByte();
                byte lengthSizeMinusOne = binaryReader.ReadByte();
                var naluLengthSize = (byte)(1 + (lengthSizeMinusOne & 3));

                byte numOfSequenceParameterSets = (byte)(binaryReader.ReadByte() & 0x1f);
                var sequenceParameters = new List<byte[]>(numOfSequenceParameterSets);
                for (uint i = 0; i < numOfSequenceParameterSets; i++)
                {
                    byte[] buffer = new byte[Mp4Util.BytesToUInt16BE(binaryReader.ReadBytes(2))];
                    mediaStream.Read(buffer, 0, buffer.Length);
                    sequenceParameters.Add(buffer);
                }

                byte numOfPictureParameterSets = binaryReader.ReadByte();
                var pictureParameters = new List<byte[]>(numOfPictureParameterSets);
                for (uint i = 0; i < numOfPictureParameterSets; i++)
                {
                    byte[] buffer = new byte[Mp4Util.BytesToUInt16BE(binaryReader.ReadBytes(2))];
                    mediaStream.Read(buffer, 0, buffer.Length);
                    pictureParameters.Add(buffer);
                }

                var sps = sequenceParameters[0];
                var pps = pictureParameters[0];

                stream.Write(startCode, 0, startCode.Length);
                stream.Write(sps, 0, sps.Length);
                stream.Write(startCode, 0, startCode.Length);
                stream.Write(pps, 0, pps.Length);
                 
            }

            mediaStream.Seek(sample.Offset, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(this.mediaStream);
            var sampleSize = sample.Count;
            while (sampleSize > 4L)
            {
                var ui32 = reader.ReadUInt32();
                var count = OldSkool.swaplong(ui32);
                stream.Write(startCode, 0, startCode.Length);
                stream.Write(reader.ReadBytes((int)count), 0, (int)count);
                sampleSize -= 4 + (uint)count;
            }
            MediaStreamSample mediaStreamSample = new MediaStreamSample(this.videoStreamDescription, stream, 0, stream.Length, sample.Timestamp, emptyDict);
            videoSampleIndex++;
            return mediaStreamSample;
        }


    }
}
