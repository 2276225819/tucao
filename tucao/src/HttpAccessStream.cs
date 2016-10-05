using System;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;

namespace tucao
{

    class HttpAccessStream : IRandomAccessStream
    {
        public bool CanRead
        {
            get { return true; }
        }
        public bool CanWrite
        {
            get { return false; }
        }
        public ulong Position
        {
            get { return (ulong)pos; }
            set { throw new NotImplementedException(); }
        }
        public ulong Size
        {
            get { return size; }
            set { throw new NotImplementedException(); }
        }

        ulong size;
        ulong pos;
        HttpRequestMessage _req; 
        public HttpAccessStream(string url) : this(new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
        {  
        }
        public HttpAccessStream(HttpRequestMessage req)
        {
            _req = req;
        }  

        public async Task load( )
        { 
            HttpClient client = new HttpClient();
            var res = await client.SendRequestAsync(_req, HttpCompletionOption.ResponseHeadersRead);
            if (res.StatusCode != HttpStatusCode.Ok) {
                throw new Exception(((int)res.StatusCode) + "  " + res.StatusCode.ToString());
            }

            this.size = (ulong)res.Content.Headers.ContentLength;
            client.Dispose();
            Debug.WriteLine("== Content:" + size + " ==");

           ;
        }
        public void Seek(ulong position)
        {
            Debug.WriteLine("HttpAccessStream.Seek:" + position );
            pos = position;
        }
        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            Debug.WriteLine("HttpAccessStream.Read:" + pos + "/" + count);
            HttpClient client = new HttpClient();
            var req = new HttpRequestMessage(_req.Method, _req.RequestUri);
            foreach (var item in req.Headers) 
                 req.Headers.Add(item.Key, _req.Headers[item.Key]);
            req.Headers["Range"] = (pos) + "-" + (pos + count); 
            pos += count;//
            var res = client.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead).GetResults();
            var stream = res.Content.ReadAsInputStreamAsync().GetResults();

            //同步超时了
            //需要同步改成异步



            var result = stream.ReadAsync(buffer, count, options);  
            result.Completed += (s, e) => {
                Debug.WriteLine("HttpAccessStream.Read:" + e);
                req.Dispose();
                client.Dispose(); 
            }; 
            return result;
        } 
        public void Dispose()
        {
            _req.Dispose();
        }
        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException(); 
        }
        public IRandomAccessStream CloneStream()
        {
            throw new NotImplementedException();
        }
        public IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        }
        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }
        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

    }
     
}
