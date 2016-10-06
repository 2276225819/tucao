using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace tucao
{ 

    public class FlvReader
    {  
        public FlvReader(Func<Stream> s)
        { 
            this.CreateStream = s;

            Stream stream = this.CreateStream();// new HttpStream(url);
            var Head = new FlvHead(stream);           //9
            if (Head.Signature != "FLV") {
                throw new Exception("格式错误:" + Head.Signature);
            }
            this.readPreviousTag(stream);
            this.readPreviousTag(stream);
            this.readPreviousTag(stream);
            //加载keyframes
            Task.Run(() => {
                double nxf = 0;
                double off = MediaData.Duration / 250;
                 

                var obj =  MediaData["keyframes"]?.GetObject();
                if  (obj != null) {
                    times = obj["times"].GetArray().Select((d) => d.GetNumber()).ToList();
                    kfs = obj["filepositions"].GetArray().Select((d) => (long)d.GetNumber()).ToList();
                    Debug.WriteLine("@ keyframes：ALL");
                    Debug.WriteLine("@ keyframes：ALL");
                }
                else {
                    Debug.WriteLine("@ keyframes：LOADSTART");
                    while (true) {
                        var PreviousTagSize = stream.ReadUInt32();
                        FlvTag flvTag = FlvTag.createTag(stream);
                        if (flvTag == null) break;
                        var data = flvTag.LoadTagData(stream);
                        //关键帧
                        VideoTag tag = flvTag as VideoTag;
                        if (tag != null) { 
                            if (flvTag.TimeSpan.TotalSeconds >= nxf) {
                                nxf += off;
                                times.Add(flvTag.TimeSpan.TotalSeconds);
                                kfs.Add(flvTag.Offset); //PreviousTagSize:4 
                            }
                        }
                    }
                    Debug.WriteLine("@ keyframes：LOAdEND");
                }
            });
            
        }

         
        bool running = false;

        List<double> times = new List<double>();
        List<long> kfs = new List<long>();
        bool Reload(double time)
        {
            Stream m  = null;
            try {
                for (int i = 0; i < times.Count; i++) {
                    if (times[i] >= time) {
                        m = this.CreateStream();
                        //Debug.WriteLine("====Reload:"+i+"=====:" );
                        Debug.WriteLine(time+"==>  TIMER：" +times[i] + "    KFS:{0}({0:X})",kfs[i]);
                        m.Position = kfs[i] - 4;
                        VideoCache.Clear();
                        AudioCache.Clear();
                        break;
                    }
                }
            }
            catch (Exception e) {
                m = null;
                Debug.WriteLine(e); 
            }  
            if (m == null)
                return false;
            else
                MainStream = m;

            if (!running) {
                running = true;
                Debug.WriteLine("==running:true==");
                Task.Run(async () => {
                    try { 
                        while (true) {
                            if (VideoCache.Count > 1024 || AudioCache.Count > 1024) {
                                await Task.Delay(500);
                                //Debug.WriteLine("@@@ WAIT:RUNNING:" + VideoCache.Count + "|" + AudioCache.Count + " @@@");
                                continue;
                            } 
                            if (!this.readPreviousTag(MainStream))
                                break;
                        }
                        Debug.WriteLine("==running:false==");
                        running = false;
                    }
                    catch (Exception ERR) {
                        Debug.WriteLine("==running:false==");
                        running = false; 
                        Debug.WriteLine(ERR);
                        Reload(time+5);
                        //throw;
                    }
                });
            }

            return true;

           
        }

        Stream MainStream = null; 
        Func<Stream> CreateStream;
        //string url = "";

        public bool readPreviousTag(Stream stream)
        {
            var p = stream.Position;
            //Debug.WriteLine("readPreviousTag：{0}({0:X})+4+11=>{1}({1:X})", p,p+11+4);
            var PreviousTagSize = stream.ReadUInt32();   //4    倒置读取位置   
            FlvTag flvTag = FlvTag.createTag(stream);    //11   头部
            if (flvTag == null)
                return false;
            var data = flvTag.LoadTagData(stream);

            //媒体数据
            if (flvTag is ScriptDataTag) {
                MediaData = flvTag as ScriptDataTag;
                return true;
            }

            //音频头帧
            if (flvTag is AudioTag) {
                var tag = flvTag as AudioTag;
                if (FirstAudio == null) {
                    FirstAudio = tag as AudioTag;
                    return true;
                }
                AudioCache.Enqueue(tag);
            }

            //视频头帧
            if (flvTag is VideoTag) {
                var tag = flvTag as VideoTag;
                if (FirstVideo == null) {
                    FirstVideo = flvTag as VideoTag;
                    return true;
                }

                VideoCache.Enqueue(tag);
            }
             
            return true;
        }

     

        public VideoTag FirstVideo = null;
        public AudioTag FirstAudio = null;
        public ScriptDataTag MediaData = null;

        Queue<VideoTag> VideoCache = new Queue<VideoTag>();
        Queue<AudioTag> AudioCache = new Queue<AudioTag>(); 
        public MediaStreamSource CreateSource()
        {
            var audiodesc = FirstAudio.CreateAudioDesc( MediaData);
            var videodesc = FirstVideo.CreateVideoDesc(); 
            var c = new MediaStreamSource(videodesc, audiodesc);// audiodesc);//, videodesc);
            c.Starting += this.Starting;
            c.SampleRequested += this.SampleRequested;
            c.Paused += this.Paused;
            c.Duration = TimeSpan.FromSeconds(this.MediaData.Duration);
            c.CanSeek = true;
            return c;
        }

        public async Task<VideoTag> ReadVideoTag()
        {
            while (VideoCache.Count <=1) {
                //Debug.WriteLine("@@@ WAIT:ReadVideoTag:" + running + " @@@");
                await Task.Delay(500);
            }

            var tag = VideoCache.Dequeue();
            if (tag.Offset == FirstVideo.Offset)//跳过第一帧
                tag = VideoCache.Dequeue();
            return tag;
        }
        public async Task<AudioTag> ReadAudioTag()
        {
            while (AudioCache.Count <= 1) {
                //Debug.WriteLine("@@@ WAIT:ReadAudioTag:" + running + " @@@");
                await Task.Delay(500);
            }
            var tag = AudioCache.Dequeue();
            if (tag.Offset == FirstAudio.Offset)//跳过第一帧
                tag = AudioCache.Dequeue();
            return tag;
        } 
        public async void SampleRequested(MediaStreamSource s, MediaStreamSourceSampleRequestedEventArgs e)
        {
            var req = e.Request;
            var deferal = req.GetDeferral();

            while (req.Sample == null) {
                if (req.StreamDescriptor is AudioStreamDescriptor) {
                    var flvTag = await this.ReadAudioTag();
                    if (flvTag != null) {
                        req.Sample = await flvTag.CreateAudioSample();
                    }
                }
                if (req.StreamDescriptor is VideoStreamDescriptor) {
                    var flvTag = await this.ReadVideoTag();
                    if (flvTag != null) {
                        req.Sample = await flvTag.CreateVideoSample(FirstVideo, true);// vi <= 2);
                    }
                }
                if (req.Sample == null)
                    Debug.WriteLine("SampleRequested:NULL");
            }
            deferal.Complete();
        }


        bool lk = true;
        public void Starting(MediaStreamSource s, MediaStreamSourceStartingEventArgs e)
        {
            var req = e.Request;
            var spos = req.StartPosition;
            Debug.WriteLine("Starting:" + lk);
            if ( lk && (spos != null) && spos.Value <= s.Duration) {  
                if (!Reload(spos.GetValueOrDefault().TotalSeconds)) {
                    lk = false;
                    Task.Run(async () => {
                        await Task.Delay(3000);
                        lk = true;
                        Debug.WriteLine("Starting:END\n\n");
                        req.SetActualStartPosition(spos.GetValueOrDefault());
                    });
                }
            }
        }
        private void Paused(MediaStreamSource sender, object args)
        {
            //Debug.WriteLine(args == null); 
        }



    }
}
