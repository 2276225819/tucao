using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace tucao 
{
    class WebRandomAccessStream : IRandomAccessStream
    {
        public bool CanRead { get { return true; } }
        public bool CanWrite { get { return false; } }
        public ulong Position { get { return pos; } }
        public ulong Size { get; set; }

    
        public WebRandomAccessStream(string url) : this(new HttpRequestMessage(HttpMethod.Get, new Uri(url))) { }
        public WebRandomAccessStream(HttpRequestMessage req)
        {
            using (HttpClient client = new HttpClient()) { 
                var res = client.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                Headers = req.Headers;
                Method = req.Method;
                RequestUri = req.RequestUri;
                Size = res.Content.Headers.ContentLength.Value;
                Stream = res.Content.ReadAsInputStreamAsync().GetAwaiter().GetResult(); 
            }
        }

        ulong pos = 0; 
        IInputStream Stream;  
        HttpRequestHeaderCollection Headers;
        HttpMethod Method;
        Uri RequestUri;

        public IInputStream GetInputStreamAt(ulong position)
        { 
            using (HttpClient client = new HttpClient()) {
                var req = new HttpRequestMessage(Method, RequestUri);
                foreach (var item in Headers)
                    req.Headers[item.Key] = item.Value;
                req.Headers["Range"] = ("bytes=" + position + "-" + Size);

                Debug.WriteLine("--------Headers--------");
                Debug.WriteLine(req.Headers);
                var res = client.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                Debug.WriteLine("SIZE：" + res.Content.Headers.ContentLength.Value);
                return res.Content.ReadAsInputStreamAsync().GetAwaiter().GetResult(); 
            } 
        }
        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            Debug.WriteLine("WebRandomAccessStream.ReadAsync:" + pos + "|" + (pos+count) +"|"+ Size);
            pos += count;
            return Stream.ReadAsync(buffer, count, options);
        } 
        public void Seek(ulong position)
        {
            Stream.Dispose();
            Stream = GetInputStreamAt(pos = position);
        }

        public IRandomAccessStream CloneStream()
        {
            throw new NotImplementedException();
        } 
        public void Dispose()
        {
            throw new NotImplementedException();
        } 
        public IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        } 
        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        } 
        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
