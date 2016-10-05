using System;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.Data.Xml.Dom;
using Windows.Media.MediaProperties;
using System.Linq;
using MediaParsers.FlvParser;
using Windows.Storage.FileProperties;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text; 

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace tucao
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {

            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。

            HID.Text = "h4065708";/**/
            //DD.Source = new Uri("ms-appx:///Assets/test2.mp4");
            DD.BufferingProgressChanged += (ss, ee) => {
                //Debug.WriteLine("==BufferingProgressChanged" + DD.BufferingProgress);
            };
            DD.DownloadProgressChanged += (ss, ee) => {
                //Debug.WriteLine("==DownloadProgressChanged" + DD.DownloadProgress);
            };
            DD.SeekCompleted += (ss, ee) => {
                Debug.WriteLine("==SeekCompleted" + DD.DownloadProgress);
            };
            DD.MediaFailed += (ss, ee) => {
                Debug.WriteLine("==MediaFailed" + DD.DownloadProgress);
            };
            DD.MediaOpened += (ss, ee) => {
                Debug.WriteLine("==MediaOpened" + DD.DownloadProgress);
            };
            DD.MediaEnded += (ss, ee) => {
                Debug.WriteLine("==MediaEnded" + DD.DownloadProgress);
            };




            //DD.Source = new Uri("http://gz189cloud.oos-gz.ctyunapi.cn/d12876ca-77ad-4f33-b2ec-b646b313958c?response-content-type=video/mp4&Expires=1475257455&response-content-disposition=attachment%3Bfilename%3D\"02%2B%25E8%25B0%2581%25E6%259D%25A5%25E5%2588%25B6%25E8%25A3%2581.mp4\"&AWSAccessKeyId=fad0e782cd5132563e39&Signature=BcYMuqJFkwgZLcLM2D0jc9g/XKU%3D");
            // 
            test();
            //Util.Load("",DD); 

        }

        public void test()
        {





            //问题是网络的FLV没用成功过（MP4自带解码支持

            /*var url = "ms-appx:///Assets/test2.flv"; 
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
            var stream = await file.OpenReadAsync();*/

            //var url = "http://api.tucao.tv/api/down/415221905169990	";
            //var url = "http://www.panda.tv/act/bananaow20160909.html";


            DD.SetMediaStreamSource(new FlvReader(() => {
                //分段加载全部

                /*
                //return Stream 支持
                //return IRandomAccessStream 支持 
                var url = "ms-appx:///Assets/test2.flv";
                var file = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(url)).AsTask().GetAwaiter().GetResult();
                var stream = (  file.OpenReadAsync() ).AsTask().GetAwaiter().GetResult().AsStream();
             */

                /* 
                //stream.ReadXXXX 同步执行超时 */
                var stream = new HttpStream("http://api.tucao.tv/api/down/7154311002879580");
                //var stream = new HttpStream("http://api.tucao.tv/api/down/415221901240825");//若叶
                


                return stream;
            }).CreateSource());
            /*/
            DD.SetMediaStreamSource(createMediaStream(stream));//一次把所有内容加载完毕（卡死了
             /*/
            //DD.SetMediaStreamSource(createHttpStream(stream));//没用



            //This IRandomAccessStream does not support the GetInputStreamAt method because it requires cloning and this stream does not support cloning.
            //因为它需要克隆和该流不支持克隆此IRandomAccessStream不支持getInputStream方法。
            //DD.SetMediaStreamSource(await new FlvReader(stream).CreateSource());


            /* 
            //return InputStream不支持Seek
            //url = "http://api.tucao.tv/api/down/915431902014450";//MP4 
            //var url = "http://api.tucao.tv/api/down/7154311002879580";//FLV
            var url = "http://183.61.26.49/m10.music.126.net/20160926220901/aeb3511301b91331c3f246749c9899c7/ymusic/b811/a01e/8a6a/e181c5d277639941b8cbc1c44e436076.mp3?wshc_tag=1&wsts_tag=57e92622&wsid_tag=3b260fc3&wsiphost=ipdbm";

            var c = new Windows.Web.Http.HttpClient();
            var req = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(url));
            var res = await c.SendRequestAsync(req, Windows.Web.Http.HttpCompletionOption.ResponseHeadersRead); 
            var stream = await res.Content.ReadAsInputStreamAsync();


            var buff = WindowsRuntimeBuffer.Create(10);
            var async = stream.ReadAsync(buff, 1024 * 1024 * 8, InputStreamOptions.None);
            var sss = buff.AsStream();

            sss.Seek(100, SeekOrigin.Begin);
            Debug.WriteLine(sss.Position); */

        }

        /// <summary>
        /// IInputStream:不支持Seek
        /// Stream 可以
        /// IRandomAccessStream 可以
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        MediaStreamSource createMediaStream(Stream stream)
        {

            var flv = new FlvParse(stream);
            var audiodesc =  flv.Audios[0].CreateAudioDesc( flv.MediaData);
            var videodesc =  flv.Videos[0].CreateVideoDesc();
            var c = new MediaStreamSource(audiodesc,videodesc);
            c.Duration = TimeSpan.FromSeconds(flv.MediaData.Duration);
            c.CanSeek = true;

            Debug.WriteLine("-----------------------");
            Debug.WriteLine("flv.tags.Count:" + flv.tags.Count);



            var ai = 1;
            var at = new TimeSpan(0);
            var vi = 1;
            var vt = new TimeSpan(0);
            c.Starting += (s, e) => {
                var req = e.Request;

                Debug.WriteLine("==Starting==");
                if ((req.StartPosition != null) && req.StartPosition.Value <= c.Duration) {
                    vi = ai = 1;
                    var time = req.StartPosition.GetValueOrDefault();
                    foreach (var item in flv.Audios) {
                        if (item.TimeSpan >= time) {
                            Debug.WriteLine(time + "    " + ai);
                            break;
                        }
                        ai++;
                    }
                    foreach (var item in flv.Videos) {
                        if (item.TimeSpan >= time) {
                            Debug.WriteLine(time + "    " + ai);
                            break;
                        }
                        vi++;
                    } 
                    Debug.WriteLine(time + "    " + ai);
                    Debug.WriteLine(time + "    " + vi); 
                }
            };
            c.SampleRequested += async (s, e) => {
                var req = e.Request;
                var deferal = req.GetDeferral();
                if (req.StreamDescriptor is AudioStreamDescriptor) {
                    if (flv.Audios.Count > ai) {
                        var flvTag = flv.Audios[ai];  
                        req.Sample = await flvTag.CreateAudioSample();
                        at = flvTag.TimeSpan;
                        ai++;
                    }
                }
                if (req.StreamDescriptor is VideoStreamDescriptor) {
                    if (flv.Videos.Count > vi) {
                        var flvTag = flv.Videos[vi];   //每一段进度条移动距离  
                        req.Sample = await flvTag.CreateVideoSample(flv.Videos[0], true);// vi == 1);
                        vt = flvTag.TimeSpan;
                        vi++;
                    }
                }
                deferal.Complete();
            };

            return c;

        }


        MediaStreamSource createHttpStream(Stream stream)
        {

            var flv = new FlvParse(stream );
            var audiodesc = flv.Audios[0].CreateAudioDesc(flv.MediaData);
            var videodesc = flv.Videos[0].CreateVideoDesc();
            var c = new MediaStreamSource(audiodesc, videodesc);
            c.Duration = TimeSpan.FromSeconds(flv.MediaData.Duration);
            c.CanSeek = true;

            return c;

        }

        struct A { public string Name { get; set; } public string Value { get; set; } }
        public async void b()
        { 
            await Task.Delay(1);

           //var d = Winista.Text.HtmlParser.Parser.CreateParser("<div></div>","utf8");
            //Debug.WriteLine(d);


            /*
            var c = new HttpClient();
            var html = await c.GetStringAsync("https://www.baidu.com/"); 
            var d = new CSharp_HtmlParser_Library.HtmlParser(html);
            d.ParseHtml();

            Debug.WriteLine(d.RootNode);
            */

            //Debug.WriteLine(a.Length);
        }

        async Task<MediaStreamSource> createMediaStream2(string url = "ms-appx:///Assets/test.mp3" )
        {
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
            var stream = await file.OpenReadAsync();
            MusicProperties mp3FileProperties = await file.Properties.GetMusicPropertiesAsync();


            List<string> encodingPropertiesToRetrieve = new List<string>();
            encodingPropertiesToRetrieve.Add("System.Audio.SampleRate");
            encodingPropertiesToRetrieve.Add("System.Audio.ChannelCount");
            encodingPropertiesToRetrieve.Add("System.Audio.EncodingBitrate");
            var encodingProperties = await file.Properties.RetrievePropertiesAsync(encodingPropertiesToRetrieve);
            uint sampleRate =   (uint)encodingProperties["System.Audio.SampleRate"];
            uint channelCount = (uint)encodingProperties["System.Audio.ChannelCount"];
            uint bitRate =  (uint)encodingProperties["System.Audio.EncodingBitrate"];

            /*
            44100 2 128000
            44100 2 171896   music: 00:03:33.2114285
            44100 2 130000   music: 00:00:30.0930000
            SampleRate/SamplesPerSec
                  Stereo/Channels
                     SR*SO*SS/BA  

            130.3 16  2
            DataRate
                 SampleSize/BitsPerSample
                      BlockAlign
                    */
            // var audiodesc = new AudioStreamDescriptor(AudioEncodingProperties.CreateMp3(44100, 2, 128000));
            var audiodesc = new AudioStreamDescriptor(AudioEncodingProperties.CreateMp3(sampleRate, channelCount, bitRate));
            var c = new MediaStreamSource(audiodesc);
            c.Duration = mp3FileProperties.Duration;
            c.CanSeek = true;

            Debug.WriteLine("music: " + c.Duration);
            Debug.WriteLine(mp3FileProperties.Title + "  " + sampleRate + " " + channelCount + " " + bitRate);





            UInt32 sampleSize = 300;//每一段
            TimeSpan sampleDuration = new TimeSpan(0, 0, 0, 0, 70);//每一段进度条移动距离
            ulong byteOffset = 0;
            TimeSpan timeOffset = new TimeSpan(0);
            c.Starting += (s, e) => {
                Debug.WriteLine("==Starting==");
                MediaStreamSourceStartingRequest request = e.Request;
                if ((request.StartPosition != null) && request.StartPosition.Value <= c.Duration) {
                    UInt64 sampleOffset = (UInt64)request.StartPosition.Value.Ticks / (UInt64)sampleDuration.Ticks;
                    timeOffset = new TimeSpan((long)sampleOffset * sampleDuration.Ticks);
                    byteOffset = sampleOffset * sampleSize;
                }
                request.SetActualStartPosition(timeOffset);

            };
            c.SampleRequested += async (s, e) => {
                //Debug.WriteLine(timeOffset);
                var deferal = e.Request.GetDeferral();
                if (byteOffset + sampleSize <= stream.Size) {
                    Debug.WriteLine(sampleSize + "    " + timeOffset);
                    var sample = await MediaStreamSample.CreateFromStreamAsync(stream.GetInputStreamAt(byteOffset), sampleSize, timeOffset);//每一段的数据大小
                    sample.Duration = sampleDuration; //每一段进度条移动距离
                    sample.KeyFrame = true;
                    e.Request.Sample = sample;
                    byteOffset += sampleSize;
                    timeOffset = timeOffset.Add(sampleDuration);
                }
                deferal.Complete();
            };
            return c;
        }


        /**/
        void a()
        {
            //var str = "http://gz189cloud.oos-gz.ctyunapi.cn/555f331f-7426-4529-b9df-04dfb92ee47a?Expires=1474497109&response-content-disposition=attachment%3Bfilename%3D%2220160914_000101_x264_FLV%25E5%25B0%2581%25E8%25A3%2585.flv%22&AWSAccessKeyId=fad0e782cd5132563e39&Signature=HIFtsqqGUSo9mwbD96RyOkxgqdM%3D";
            //HttpAccessStream hs = new HttpAccessStream( "http://www.mediacollege.com/video-gallery/testclips/barsandtone.flv");  
            //var bytes = new byte[hs.Size];
            //await hs.AsStream().ReadAsync(bytes, 0, bytes.Length);
            //var d = new MemoryStream(bytes);
             





            /*
            var videodesc = new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()); 
            var audiodesc = new AudioStreamDescriptor(AudioEncodingProperties.CreateMp3(16000, 1, 8)); 
            var c = new MediaStreamSource(videodesc, audiodesc);
            c.Duration = flv.tags.Last().TimeSpan;

            DD.SetMediaStreamSource(c);

            VideoTag v=null;
            AudioTag a=null;
            c.Starting += (s, e) => { 
                var req = e.Request;

            };
            TimeSpan vt = new TimeSpan(0);
            TimeSpan at = new TimeSpan(0);
            c.SampleRequested += async (s, e) => {  
                var req = e.Request;
                var next = req.GetDeferral();
                if (req.StreamDescriptor is VideoStreamDescriptor) {
                    if (flv.videos.Count!=0) {
                        v = flv.videos[0];
                        var ss = new MemoryStream(v.Data);
                        req.Sample = await MediaStreamSample.CreateFromStreamAsync(ss.AsInputStream(), v.DataSize, v.TimeSpan);// new TimeSpan()); 
                        req.Sample.Duration = v.TimeSpan - vt;
                        req.Sample.KeyFrame = true;
                        vt = v.TimeSpan;
                        flv.videos.RemoveAt(0);
                        Debug.WriteLine("Timestamp:" + req.Sample.Timestamp);
                        Debug.WriteLine("KeyFrame :" + req.Sample.KeyFrame);
                        Debug.WriteLine("Duration :" + req.Sample.Duration);
                    }
                    else {
                        //var stream = new MemoryStream(v.Data);
                        //req.Sample = await MediaStreamSample.CreateFromStreamAsync(stream.AsInputStream(), v.DataSize, v.TimeSpan);
                        Debug.WriteLine("err");
                    }
                }
                if (req.StreamDescriptor is AudioStreamDescriptor) {
                    if (flv.audios.Count != 0) {
                        a = flv.audios[0];
                        var ss = new MemoryStream(a.Data);
                        req.Sample = await MediaStreamSample.CreateFromStreamAsync(ss.AsInputStream(), a.DataSize, a.TimeSpan);// new TimeSpan());
                        req.Sample.Duration = a.TimeSpan - at;
                        req.Sample.KeyFrame = true;
                        at = a.TimeSpan;
                        flv.audios.RemoveAt(0);
                        Debug.WriteLine("Timestamp:" + req.Sample.Timestamp);
                        Debug.WriteLine("KeyFrame :" + req.Sample.KeyFrame);
                        Debug.WriteLine("Duration :" + req.Sample.Duration);
                    }
                    else {
                        //var stream = new MemoryStream(a.Data);
                        //req.Sample = await MediaStreamSample.CreateFromStreamAsync(stream.AsInputStream(), a.DataSize, a.TimeSpan);
                        Debug.WriteLine("err");
                    }

                }
                next.Complete(); 
            };
            */



            /*
            var flvFile = new MediaParsers.FlvParser.FlvFile(d); 
            var audioSamples = flvFile.FlvFileBody.Tags.Where(tag => tag.TagType == MediaParsers.FlvParser.TagType.Audio).ToList();
            var videoSamples = flvFile.FlvFileBody.Tags.Where(tag => tag.TagType == MediaParsers.FlvParser.TagType.Video).ToList();
            Debug.WriteLine(audioSamples.Count);
            Debug.WriteLine(videoSamples.Count);


            var videodesc = new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()); 
            videodesc.EncodingProperties.Width = 
            videodesc.Name = "video"; 
            var audiodesc = new AudioStreamDescriptor(AudioEncodingProperties.CreateMp3(320, 2, 5));
            audiodesc.Name = "audio";
            var c = new MediaStreamSource(videodesc, audiodesc); 
            DD.SetMediaStreamSource(c);


            c.Starting += (s, e) => {
                var req = e.Request; 
            };

            var vi = 0;
            var ai = 0;
            c.SampleRequested += async (s, e) => {
                var req = e.Request;
                var next = req.GetDeferral();

                var name = req.StreamDescriptor.GetType().Name;
                if (name == "AudioStreamDescriptor") {
                    var ti = new TimeSpan(audioSamples[ai].Timestamp*1000);
                    var off = audioSamples[ai].Offset;
                    var size = audioSamples[ai].DataSize; 
                    var bs = new byte[size];
                    Debug.WriteLine(ti);
                    Debug.WriteLine(d.Length);
                    Debug.WriteLine(off);
                    Debug.WriteLine(size);
                    d.Position = off;
                    await d.ReadAsync(bs, 0, (int)size);
                    var stream = new MemoryStream(bs);
                    req.Sample = await MediaStreamSample.CreateFromStreamAsync(stream.AsInputStream(), size, ti);
                    ai++;
                }
                if (name == "VideoStreamDescriptor") {  
                    var ti = new TimeSpan(videoSamples[vi].Timestamp * 1000);
                    var off = videoSamples[vi].Offset;
                    var size = videoSamples[vi].DataSize;
                    var bs = new byte[size]; 
                    Debug.WriteLine(ti);
                    Debug.WriteLine(d.Length);
                    Debug.WriteLine(off);
                    Debug.WriteLine(size);
                    d.Position = off;
                    var stream = new MemoryStream(bs);
                    await d.ReadAsync(bs, 0, (int)size); 
                    req.Sample = await MediaStreamSample.CreateFromStreamAsync(stream.AsInputStream(), size, ti);
                    vi++;  
                }
                next.Complete();
            };*/

        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
            //Frame.Navigate(typeof(InfoPage),HID.Text); 
        }

         
 
        private void LIST_OPEN(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void ListViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        { 
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("ee");

        }
    }

 

}
