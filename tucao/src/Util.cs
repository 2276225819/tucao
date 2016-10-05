using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace tucao
{
    class Util
    {
        /*
        public async static void Load(string src, MediaElement media)
        {
            //url = "http://api.tucao.tv/api/down/7154311002879580";//FLV
            //url = "http://api.tucao.tv/api/down/915431902014450";//MP4
            var c = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get,new Uri(src)); 
            var res = await c.SendRequestAsync(req,HttpCompletionOption.ResponseHeadersRead);             
            var url = req.RequestUri.ToString();

            var sr = await res.Content.ReadAsInputStreamAsync();
            var buf = await sr.ReadBuffer(11);  
            FlvHead head = new FlvHead( buf.AsStream() );
            if (head.Signature == "FLV") {
                Debug.WriteLine("FLV ## " + url); 
                media.SetMediaStreamSource(new FlvReader(() => new HttpStream(url)).CreateSource());
                return;

            }

            if (url.IndexOf("mp4") != -1) {
                Debug.WriteLine("MP4 ## "+ url); 
                media.Source = new Uri(url);
                return;
            }


            Debug.WriteLine(url); 
      
            var client = new Windows.Web.Http.HttpClient(); 
            IInputStream stream = await client.GetInputStreamAsync(new Uri(url));
        
            var ss = stream.AsStreamForRead();
            

            
            var buf = new byte[99];
            ss.Read(buf, 0, 99);
            var str = Encoding.UTF8.GetString(buf, 0, 99);
            Debug.WriteLine(ss.Length);
            Debug.WriteLine(str);
            Debug.WriteLine(BitConverter.ToString(buf)); 

 
        }*/




    }
}
