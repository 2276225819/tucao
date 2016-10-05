using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace tucao
{
    class HtmlDocument : HtmlElement
    {
        public static HtmlDocument FormHtml(string html)
        {
            return new HtmlDocument(html);
        }
        public static HtmlDocument FormUrlAsync(string url)
        {
            var web = new HttpClient();
            var html = web.GetStringAsync(new Uri(url)).AsTask().GetAwaiter().GetResult();
            return new HtmlDocument(html);
        }
        protected HtmlDocument(string html) : base(html) {  }
    }

}
