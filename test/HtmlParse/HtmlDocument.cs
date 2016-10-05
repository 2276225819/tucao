using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace tucao
{
    class HtmlDocument : HtmlElement
    {
        public static HtmlDocument FormHtml(string html)
        {
            return new HtmlDocument(html);
        }

        public static HtmlDocument FormUrl(string url)
        {
            var c = new WebClient();
            var buff = c.DownloadData(url); ;
            var html = Encoding.UTF8.GetString(buff);
            return HtmlDocument.FormHtml(html);  
        }
        protected HtmlDocument(string html) : base(html)
        {
        }
    } 
     
}
