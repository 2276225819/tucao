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
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace tucao 
{
    class HttpStream : Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { return Size; } }
        public override long Position
        {
            get { return Pos; }
            set { if (value != Pos) Seek(value, SeekOrigin.Begin); }
        }
         

        long Size = 0;
        long Pos = 0;
        /* long Pos {
            get { Debug.WriteLine("POSGET " + p);
                return p; }
            set { Debug.WriteLine("POSSET " + p + " " + value);
                p = value;  } }*/
        Stream Stream;
        HttpRequestHeaderCollection Headers;
        HttpMethod Method;
        Uri RequestUri;
        public HttpStream(string url, int buffsize = 1024 * 1024 * 8) 
            : this(new HttpRequestMessage(HttpMethod.Get, new Uri(url)), buffsize) { }
        public HttpStream(HttpRequestMessage req,int buffsize )
        {
            //初始化Size（获取全部内容长度）
            //var buff = WindowsRuntimeBuffer.Create(buffsize);
            using (HttpClient client = new HttpClient()) {
                var res = client.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead).WaitResult(3000); 
                var istream = res.Content.ReadAsInputStreamAsync().WaitResult(3000);  
                Headers = req.Headers;
                Method = req.Method;
                RequestUri = req.RequestUri;
                Size = (long)res.Content.Headers.ContentLength.Value;
                Stream = istream.AsStreamForRead();
                //规则 不能 边读边写  Stream.Position 
                client.Dispose();
            }
        }



        public override int Read(byte[] buffer, int offset, int count)
        {
            var c = Stream.Read(buffer, offset, count);
            Pos += c;
            return c;
        } 
        public override long Seek(long offset, SeekOrigin origin)
        {
            var ollpos = Pos;
            switch (origin) {
                case SeekOrigin.Begin:
                    Pos = offset; 
                    break;
                case SeekOrigin.Current:
                    Pos += offset;
                    break;
                case SeekOrigin.End:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            using (HttpClient client = new HttpClient()) {
                var req = new HttpRequestMessage(Method, RequestUri);
                foreach (var item in Headers)
                    req.Headers[item.Key] = item.Value;
                req.Headers["Range"] = ("bytes=" + Pos + "-" + Size);
            
                try {
                    Debug.WriteLine("┌─-------HttpStream.Seek--------");
                    Debug.WriteLine(req.Headers); 
                    var res = client.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead).WaitResult(3000);  
                    Debug.WriteLine("└─-----               --------");
                    Stream = res.Content.ReadAsInputStreamAsync().GetAwaiter().GetResult().AsStreamForRead(); 
                }
                catch (Exception err) {
                    Debug.WriteLine("Seek ERRORERRORERRORERROR ");
                    Debug.WriteLine(err);
                    Pos = ollpos;
                    throw;
                }
                return Pos;
            }
        }
 
        public IRandomAccessStream AsRandomAccessStream()
        {
            return null;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }  
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
         
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

         
    }
}
