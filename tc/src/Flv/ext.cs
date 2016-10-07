using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace tucao
{
    static class Ext
    {
        public async static Task<IBuffer> ReadBuffer(this IInputStream stream, uint length)
        {
            var buff = WindowsRuntimeBufferExtensions.AsBuffer(new byte[length]);
            await stream.ReadAsync(buff, length, InputStreamOptions.Partial);
            return buff;
        }    /*
        public static Stream ReadStream(this IInputStream stream, int length)
        {
            var m = new MemoryStream(length); 
            stream.AsStreamForRead().CopyTo(m, length);
            return m;
         
            var buff = WindowsRuntimeBufferExtensions.AsBuffer(new byte[length]);
            await stream.ReadAsync(buff, length, InputStreamOptions.Partial);
            return buff
    };*/

        public static string toString(this Stream stream, int length)
        {
            return BitConverter.ToString(stream.ReadBytes(30));
        }
        public static string toString(this IInputStream stream, int length)
        {
            return BitConverter.ToString(stream.AsStreamForRead().ReadBytes(30));
        }


        public static char[] ReadChars(this Stream stream, int length)
        { 
            byte[] value = new byte[length];
            stream.Read(value, 0, length); 
            return Encoding.UTF8.GetString(value, 0, length).ToCharArray(); 
            //return new BinaryReader(stream).ReadChars(length);
        }
        public static uint ReadUInt8(this Stream stream)
        {
            return (uint)stream.ReadByte();
        }
        public static uint ReadUInt16(this Stream stream, bool bigEndian = true)
        {
            byte[] value = new byte[2];
            stream.Read(value, 0, 2);
            if (!bigEndian) //小端
                return (ushort)BitConverter.ToUInt16(value, 0);
            return (ushort)(
               (value[0] << 8) |
               (value[0 + 1])); ;//默认大端（Big-Endian 
        }
        public static uint ReadUInt24(this Stream stream, bool bigEndian = true)
        {
            byte[] value = new byte[4];
            stream.Read(value, 1, 3);
            if (!bigEndian) //小端
                return (ushort)BitConverter.ToUInt16(value, 0);
            return
             ((uint)value[0] << 24) |
             ((uint)value[0 + 1] << 16) |
             ((uint)value[0 + 2] << 8) |
             ((uint)value[0 + 3]); ;//默认大端（Big-Endian
        }
        public static uint ReadUInt32(this Stream stream, bool bigEndian = true)
        {
            byte[] value = new byte[4];
            stream.Read(value, 0, 4);
            if (!bigEndian) //小端
                return (ushort)BitConverter.ToUInt16(value, 0);
            return
                ((uint)value[0] << 24) |
                ((uint)value[0 + 1] << 16) |
                ((uint)value[0 + 2] << 8) |
                ((uint)value[0 + 3]);  //默认大端（Big-Endian
        }
        public static int ReadInt16(this Stream stream, bool bigEndian = false)
        {
            byte[] value = new byte[2];
            stream.Read(value, 0, 2);
            if (bigEndian) value = value.Reverse().ToArray();
            return BitConverter.ToUInt16(value, 0);
        }
        public static int ReadInt32(this Stream stream, bool bigEndian = false)
        {
            byte[] value = new byte[4];
            stream.Read(value, 0, 4);
            if (bigEndian) value = value.Reverse().ToArray();
            return BitConverter.ToUInt16(value, 0);
        }
        public static byte[] ReadBytes(this Stream stream, int length)
        {
            return new BinaryReader(stream).ReadBytes(length);//n*4bit
        }
        public static double ReadDouble(this Stream stream, bool bigEndian = false)
        {
            byte[] value = new byte[8];
            stream.Read(value, 0, 8);
            if (bigEndian) value = value.Reverse().ToArray();
            return BitConverter.ToDouble(value, 0);
        }
        public static float ReadSingle(this Stream stream, bool bigEndian = false)
        {
            byte[] value = new byte[4];
            stream.Read(value, 0, 4);
            if (bigEndian) value = value.Reverse().ToArray();
            return BitConverter.ToSingle(value, 0);
        }
        public static bool ReadBoolean(this Stream stream)
        {
            return new BinaryReader(stream).ReadBoolean(); //1*4bit
        }
        public static string ReadString(this Stream stream, int length)
        {
            return new String(new BinaryReader(stream).ReadChars(length)); //n*4bit
        }

        public static AudioStreamDescriptor CreateAudioDesc(this AudioTag tag, ScriptDataTag MediaData)
        {
            //{"duration":6,"width":640,"height":360,"videodatarate":700,"framerate":30,"videocodecid":4,"audiodatarate":128,"audiodelay":0.038,"audiocodecid":2,"canSeekToEnd":true}
            //{  "duration":30.093,"width":512,"height":288,
            //   "videodatarate":1019.6845703125,"framerate":99999.999999999985,
            //   "videocodecid":7,"audiodatarate":130.0380859375,"audiosamplerate":44100,"audiosamplesize":16,
            //   "stereo":true,"audiocodecid":10,"filesize":4460895  }
            var m = MediaData;
            var sampleRate = (uint)m["audiosamplerate"].GetNumber();
            var channelCount = (uint)2;
            var bitRate = (uint)m["audiodatarate"].GetNumber();
            if (sampleRate < 10 ) {
                Debug.WriteLine("AudioDesc ERRORERRORERRORERRORERRORERROR sampleRate: " + sampleRate);
                if (bitRate > 125U)
                    sampleRate = 44100U;
                else if (bitRate > 120U)
                    sampleRate = 22050U;
                else
                    sampleRate = 44100U; 
            }
            AudioEncodingProperties encode = null;

            if (tag == null) {
                return null;
            }
            if (tag.Codec == SoundFormat.AAC) {
                encode = AudioEncodingProperties.CreateAac(sampleRate, channelCount, bitRate);
            }
            if (tag.Codec == SoundFormat.MP3) {
                encode = AudioEncodingProperties.CreateMp3(sampleRate, channelCount, bitRate);
            } 
            Debug.WriteLine("AudioDesc ## " + tag.Codec + "  " + sampleRate + " " + channelCount + " " + bitRate);

            return new AudioStreamDescriptor(encode);
        }

        public static VideoStreamDescriptor CreateVideoDesc(this VideoTag tag)
        {
            VideoEncodingProperties encode = null;
            if (tag.Codec == CodecID.H263) {
                encode = VideoEncodingProperties.CreateH264();
            }
            if (tag.Codec == CodecID.AVC) {
                encode = VideoEncodingProperties.CreateH264();
            } 
            Debug.WriteLine("VideoDesc ## " + tag.Codec + "  " + tag.Type);
            return new VideoStreamDescriptor(encode);
        }




        static Stream createVideoStream(this VideoTag tag, VideoTag FirstTag, bool hasHead)
        {
            byte[] startCode = new byte[] { 0, 0, 1 };
            MemoryStream stream = new MemoryStream();  
            if (hasHead) {
                var mediaStream = FirstTag.GetDataStream();//.GetDataInputStream().AsStreamForRead(); 
                BinaryReader binaryReader = new BinaryReader(mediaStream);

                var configurationVersion = binaryReader.ReadByte();
                var avcProfileIndication = binaryReader.ReadByte();
                var avcCompatibleProfiles = binaryReader.ReadByte();
                var avcLevelIndication = binaryReader.ReadByte();
                byte lengthSizeMinusOne = binaryReader.ReadByte();
                var naluLengthSize = (byte)(1 + (lengthSizeMinusOne & 3));

                byte numOfSequenceParameterSets = (byte)(binaryReader.ReadByte() & 0x1f);
                var sequenceParameters = new List<byte[]>(numOfSequenceParameterSets);
                for (uint i = 0; i < numOfSequenceParameterSets; i++) {
                    byte[] buffer = new byte[BytesToUInt16BE(binaryReader.ReadBytes(2) )];
                    mediaStream.Read(buffer, 0, buffer.Length);
                    sequenceParameters.Add(buffer);
                }

                byte numOfPictureParameterSets = binaryReader.ReadByte();
                var pictureParameters = new List<byte[]>(numOfPictureParameterSets);
                for (uint i = 0; i < numOfPictureParameterSets; i++) {
                    byte[] buffer = new byte[BytesToUInt16BE(binaryReader.ReadBytes(2) )];
                    mediaStream.Read(buffer, 0, buffer.Length);
                    pictureParameters.Add(buffer);
                }

                var sps = sequenceParameters[0];
                var pps = pictureParameters[0];

                stream.Write(startCode, 0, startCode.Length);
                stream.Write(sps, 0, sps.Length);
                stream.Write(startCode, 0, startCode.Length);
                stream.Write(pps, 0, pps.Length);
                // .WriteLine(mediaStream.Position.ToString("X00")+"|"+String.Join(" ", stream.ToArray().Select((s)=> { return s.ToString("X00"); } ) ) );
                //;
            }

            var tagStream = tag.GetDataStream();//.GetDataInputStream().AsStreamForRead();
            BinaryReader reader = new BinaryReader(tagStream);
            var sampleSize =  tagStream.Length;//sample.Count;//
            while (sampleSize > 4L) {
                var ui32 = reader.ReadUInt32();
                var count = OldSkool.swaplong(ui32);
                stream.Write(startCode, 0, startCode.Length);
                stream.Write(reader.ReadBytes((int)count), 0, (int)count);
                sampleSize -= 4 + (uint)count;
            }
            stream.Position = 0;//非常 重要别删掉（返回的数据流必须从0开始播放 
            return stream;
        }


        public static async Task<MediaStreamSample> CreateAudioSample(this AudioTag tag)
        {

            var stream = tag.GetDataStream(); 
            var sample = await MediaStreamSample.CreateFromStreamAsync(
                stream.AsInputStream(),
                (uint)stream.Length ,
                tag.TimeSpan);//每一段的数据大小 
            //sample.Duration = tag.TimeSpan;//BUG
            return sample;

            #region MyRegion


            //Debug.WriteLine(tag.GetDataStream().toString(30));
            //var ss = tag.GetDataInputStream(); 
            //var sample = await MediaStreamSample.CreateFromStreamAsync(ss, tag.Count, tag.TimeSpan);//每一段的数据大小
            //return sample; 

            /*
            var stream = tag.GetDataStream();//.GetDataStream().AsRandomAccessStream().GetInputStreamAt(); 
            var si = stream.AsInputStream();
            var sample = await MediaStreamSample.CreateFromStreamAsync(si, (uint)stream.Length, tag.TimeSpan);
            */
            //stream.Position = (long)a.Offset;//问题的并发式或交错操作改变了对象的状态，无效此操作
            //var ss = stream.AsInputStream(); //A concurrent or interleaved operation changed the state of the object, invalidating this operation. (Exception from HRESULT: 0x8000000C)

            //var ss = stream.AsRandomAccessStream().GetInputStreamAt(a.Offset);//success
            //var ss = stream.GetInputStreamAt(a.Offset);//success

            //sample.Duration = a.TimeSpan - at; //每一段进度条移动距离 
            //sample.KeyFrame = true; 
            #endregion
        }

        public static async Task<MediaStreamSample> CreateVideoSample(this VideoTag tag, VideoTag FirstTag,bool hasHead)
        {
             
            var stream = tag.createVideoStream(FirstTag, hasHead);// flv.createVideoStream(stream.AsStream(), vi);
            var sample = await MediaStreamSample.CreateFromStreamAsync(
                stream.AsInputStream(),
                (uint)stream.Length , 
                tag.TimeSpan); //每一段的数据大小    
            //sample.Duration = tag.TimeSpan;//BUG
            return sample;

            #region MyRegion

            //Debug.WriteLine("CreateAudioSample：" + tag.Count + "  " + tag.GetDataStream().Length);

            //Debug.WriteLine(BitConverter.ToString( tag.GetDataInputStream().AsStreamForRead().ReadBytes(30) ));




            /*  
            var stream =  tag.createVideoStream(Videos[0], vi <=2);
            var si = stream.AsInputStream();
            var sample = await MediaStreamSample.CreateFromStreamAsync(si, (uint)stream.Length, tag.TimeSpan);//每一段的数据大小       
            */
            #endregion
        }
        public static ushort BytesToUInt16BE(byte[] bytes)
        {
            return (ushort)((bytes[0] << 8) | bytes[1]);
        }

         
        public static R WaitResult<R,P>(this IAsyncOperationWithProgress<R,P> token,int t) where R:class
        { 
            Task.Run(async () => {
                await Task.Delay(t);
                if (token.Status == AsyncStatus.Completed)
                    return;
                token.Cancel();
            });

            return Task.Run( async () => {
                var c =  await token;
                return c;
            }).GetAwaiter().GetResult();
           //  token.AsTask().GetAwaiter().GetResult()  ;//.AsTask().GetAwaiter();
        }
    }
}
