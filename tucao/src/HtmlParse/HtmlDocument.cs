using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Data;
using System.Runtime.InteropServices.WindowsRuntime;

namespace tucao
{
    class HtmlDocument : HtmlElement
    {
        public static HtmlDocument FormHtml(string html)
        {
            return new HtmlDocument(html);
        }


        public static Task<HtmlDocument> FormUrlAsync(string url)
        {
            return FormUrlAsync(url, Encoding.UTF8);
        }

        public static async Task<HtmlDocument> FormUrlAsync(string url, Encoding encode )
        {
            var web = new HttpClient();
            var buff = await web.GetBufferAsync(new Uri(url));//.AsTask().GetAwaiter().GetResult(); 
            var html = encode.GetString(buff.ToArray(),0,(int)buff.Length);
            return new HtmlDocument(html);
        }
        protected HtmlDocument(string html) : base(html) {  }
    }

}
