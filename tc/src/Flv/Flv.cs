using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace tucao
{
    public class FlvParse
    {
        public FlvHead Head;
        public List<FlvTag> tags = new List<FlvTag>();
        public List<VideoTag> Videos = new List<VideoTag>();
        public List<AudioTag> Audios = new List<AudioTag>();
        public ScriptDataTag MediaData;
        public FlvParse(Stream stream)
        {
            Head = new FlvHead(stream);
            if (Head.Signature != "FLV") {
                throw new Exception("格式错误:"+Head.Signature);
            }
            while (true) {
                var PreviousTagSize = stream.ReadUInt32();  
                FlvTag flvTag = FlvTag.createTag(stream);  
                if (flvTag == null) break; 
                var data =  flvTag.LoadTagData(stream); 
                VideoTag v = flvTag as VideoTag;
                if (v != null) Videos.Add(v);
                AudioTag a = flvTag as AudioTag;
                if (a != null) Audios.Add(a);
                ScriptDataTag d = flvTag as ScriptDataTag;
                if (d != null) MediaData = d; 
                tags.Add(flvTag ); 
            } 
        } 


    }



    public class FlvHead
    { 
        public string Signature;
        public uint Version;
        public int Size;
        public TypeFlags Type;
        public FlvHead(Stream stream)
        { 
            var d = stream.ReadChars(3); 
            this.Signature =  new String(d);                 //#3 
            this.Version = stream.ReadUInt8();               //#1 
            this.Type = (TypeFlags)stream.ReadUInt8();       //#1 
            this.Size = (int)stream.ReadUInt32();            //#4  
        } 
    }
    public class FlvTag
    { 
        public TimeSpan TimeSpan;
        public string Name;
        public uint DataSize; 
        public long Offset; 

        protected Stream DataStream; 
        public static FlvTag createTag(Stream stream)
        {  
            var TagType = (int)stream.ReadUInt8();                //1 TagType
            switch (TagType) {
                case -1: return null;//TagType.Null
                case 8: return new AudioTag(stream) { Name= "AudioTag" }; 
                case 9: return new VideoTag(stream) { Name = "VideoTag" }; 
                case 18: return new ScriptDataTag(stream) { Name = "ScriptDataTag" }; 
                default:
                    throw new Exception("TagType:"+ TagType);
            }
        } 
        public FlvTag(Stream stream)
        {
            //=== Head ===
            this.Offset = stream.Position - 1;
            this.DataSize = stream.ReadUInt24();                   //3 DataSize          
            var time = stream.ReadUInt24( );                       //3 Timestamp         
            time |= stream.ReadUInt8( ) << 24;                     //1 TimestampExtended 
            this.TimeSpan = TimeSpan.FromMilliseconds(time ); 
            var streamid = (int)stream.ReadUInt24( );              //3 StreamID    
        }
 
        public Stream GetDataStream()
        { 
            //DataStream.Position = 0;
            // Debug.WriteLine("{0} ({1:X}) {2}|{3}", Name ,Offset , DataStream.Length , BitConverter.ToString(DataStream.ReadBytes(10) ) );
            DataStream.Position = 0;
            return DataStream;
        }
        public virtual Stream LoadTagData(Stream stream)
        {
            return null;
        }
    }
    public class AudioTag : FlvTag //type 8
    {
        public SoundFormat Codec;
        public SoundRate Rate;
        public SoundSize Size;
        public SoundType Type;

        public AudioTag(Stream stream) : base(stream)
        {
        }

        public override Stream LoadTagData(Stream stream) 
        {  
            var MediaInfo = stream.ReadUInt8();                                //#1
            this.Codec = (SoundFormat)(MediaInfo >> 4);//ACC 10                
            this.Rate = (SoundRate)((MediaInfo & 0x0f) >> 2);//AAC: always 3
            this.Size = (SoundSize)((MediaInfo & 0x02) >> 1); //AAC: always 1
            this.Type = (SoundType)(MediaInfo & 0x01); //AAC: always 1 
            //=== Data ===                        
            var pos = stream.Position;
            var wtf = stream.ReadBytes(1);                                      //#1
            var buf = stream.ReadBytes((int)this.DataSize - 1 - 1);             //#n
             
            DataStream = new MemoryStream();
            DataStream.Write(buf, 0, buf.Length);
            return DataStream;
 
        }
    }
    public class VideoTag : FlvTag //type 9
    {
        public CodecID Codec;
        public FrameType Type;
        public Stream Data;
        public bool IsHeader;

        public VideoTag(Stream stream) : base(stream)
        {
        }

        public override Stream LoadTagData(Stream stream)
        {
            var MediaInfo = stream.ReadUInt8();                     //#1
            this.Type = (FrameType)(MediaInfo >> 4);//Keyframe:1
            this.Codec = (CodecID)(MediaInfo & 0x0F);//AVC:7
            //=== Data === 
            var pos = stream.Position;
            var wtf = stream.ReadBytes(4);                          //#1
            var buf = stream.ReadBytes((int)this.DataSize - 1 - 4); //#n

            //===Packet===
            

            DataStream = new MemoryStream();
            DataStream.Write(buf, 0, buf.Length);
            return DataStream;
 
        }
    }
    public class ScriptDataTag : FlvTag  //type 18
    {
        public JsonArray AMF = new JsonArray();

        public ScriptDataTag(Stream stream) : base(stream)
        {
        }

        public override Stream LoadTagData(Stream stream)
        {   
            //=== Data  ===  
            var end = stream.Position + DataSize;//??  
            var name = ParseAMF(stream);
            var data  = ParseAMF(stream); 
            AMF.Add(name);
            AMF.Add(data);
            var a9 = stream.ReadUInt24(); 
            //stream.Position = end;//BUG 
            return null; 
        }

        IJsonValue ParseAMF(Stream stream )
        {
            int type = (int)stream.ReadUInt8();                   //1
            switch (type) {
                case 0: return JsonValue.CreateNumberValue(stream.ReadDouble(true) );   //8 
                case 1: return JsonValue.CreateBooleanValue(stream.ReadBoolean()) ; ; //1
                case 3:
                    var data = new JsonObject();
                    while (true) {
                        var l3 = (int)stream.ReadUInt16();
                        if (l3 == 0)  
                            if (stream.ReadUInt8() == 9)
                                break; 
                        var s3 = stream.ReadString(l3);
                        var d3 = ParseAMF(stream);
                        data.Add(s3, d3);                            //n    
                    }  
                    return data;
                case 2:
                    var len = stream.ReadUInt16(); 
                    return JsonValue.CreateStringValue(stream.ReadString((int)len)) ;  //n
                case 8:
                    var l8 = stream.ReadUInt32();
                    var obj = new JsonObject();
                    for (int i = 0; i < l8; i++) {
                        var key = stream.ReadString((int)stream.ReadUInt16());        //2+n
                        var val = ParseAMF(stream);                                 //n
                        obj.Add(key, val);                                          
                    }
                    return obj;
                case 9: 
                    return null;//END
                case 10:
                    var l10 = stream.ReadUInt32();
                    var arr = new JsonArray();
                    for (int i = 0; i < l10; i++) { 
                        var val = ParseAMF(stream);                                 //n
                        arr.Add(val);
                    }
                    return arr;
                default:
                    throw new Exception("JsonValue:"+type);
            }  
        }

        public IJsonValue this[string i]
        {
            get  { if (AMF[1].GetObject().ContainsKey(i)) return AMF[1].GetObject()[i]; else return null; }
        } 
        public double Duration
        {
            get { return this["duration"].GetNumber(); }
        }
    }













    /////////////////////// AVCPacket  ///////////////////////  


    public class AVCPacket {
        AVCPacketType Type;
        int Time;
        byte[] AVCDecoderConfigurationRecord;
        byte[] NALUs;
        public AVCPacket(Stream stream,int Count)
        {
            //Header:  00 00 00 00
            //NALU:    01 00 00 00
            this.Type = (AVCPacketType)stream.ReadUInt8();              //1
            this.Time = (int)stream.ReadUInt24(); //CompositionTime     //3 
            if (this.Type == AVCPacketType.AVCSequenceHeader)
                this.AVCDecoderConfigurationRecord = stream.ReadBytes(Count-4);
            else
                this.NALUs = stream.ReadBytes(Count-4); ;
        }  

    }


    public enum AVCPacketType
    {
        AVCSequenceHeader = 0,
        AVCNALU = 1,
        AVCEndOfSequence = 2,
    }

    /////////////////////// FlvTag  ///////////////////////  

    public enum TypeFlags
    {
        Reserved = 0,
        AudioVideo = 5,
        Audio = 4,
        Video = 1,
    }
    public enum TagType
    {
        Audio = 8,
        Video = 9,
        ScriptData = 18,
        Null = -1,
    }

    /////////////////////// AudioTag  /////////////////////// 

    /// <summary>
    /// For AAC: always 3
    /// </summary>
    public enum SoundRate
    {
        _5kHz = 0,
        _11kHz = 1,
        _22kHz = 2,
        _44kHz = 3,
    }
    /// <summary>
    /// Mono or stereo sound
    /// For Nellymoser: always 0
    /// For AAC: always 1
    /// </summary>
    public enum SoundType
    {
        sndMono = 0,
        sndStereo = 1,
    } 
    public enum SoundFormat
    {
        ADPCM = 1,
        MP3 = 2,
        Linear_PCM_little_endian = 3,
        Nellymoser_16_kHz_mono = 4,
        Nellymoser_8_kHz_mono = 5,
        Nellymoser = 6,
        G_711_A_law_logarithmic_PCM = 7,
        G_711_mu_law_logarithmic_PCM = 8,
        reserved = 9,
        AAC = 10,
        Speex = 11,
        MP3_8_Khz = 14,
        Device_specific_sound = 15,
    }
    public enum SoundSize
    {
        snd8Bit = 0,
        snd16Bit = 1,
    }

    /////////////////////// VideoTag  /////////////////////// 

    public enum CodecID
    {
        JPEG = 1,
        H263 = 2,
        ScreenVideo = 3,
        On2VP6 = 4,
        On2VP6WithAlphaChannel = 5,
        ScreenVideoV2 = 6,
        AVC = 7,
    } 
    public enum FrameType
    {
        Keyframe = 1,
        InterFrame = 2,
        /// <summary>
        /// H.263 only
        /// </summary>
        DisposableInterFrame = 3,
        /// <summary>
        /// reserved for server use only
        /// </summary>
        GeneratedKeyframe = 4,
        VideoInfoOrCommandFrame = 5,
    }
}
