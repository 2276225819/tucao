using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using tucao;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace test
{
    class Program
    {
        /*
        static async void dd()
        {
            var dc = HtmlDocument.FormHtml(System.IO.File.ReadAllText("HTMLPage1.html"));
            //var dc = HtmlDocument.FormUrl("http://backup.biliplus.com/play/h4065708/");

            // Windows.Web.Http.HttpClient  
            var url = "http://api.tucao.tv/api/down/415221905169990";
            //var url = "http://127.0.0.1:777/test2.flv";
            HttpClient c = new System.Net.Http.HttpClient();
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            var res = await c.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            var ss = await res.Content.ReadAsStreamAsync();
            while (true) {
                var buff = new byte[1];
                await ss.ReadAsync(buff, 0, 1);
                var s = Encoding.UTF8.GetString(buff, 0, 1);
                Console.Write(s);
                await System.Threading.Tasks.Task.Delay(1);
            }
            Console.Read();
            var buf = new byte[200];
            ss.Read(buf, 0, 200);
            var str = Encoding.UTF8.GetString(buf, 0, 200);
             
            Console.WriteLine(str);
            Console.WriteLine(BitConverter.ToString(buf));
            Console.Read();

        }*/

        class ITEMS
        {
            public string Image { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }

        } 
        static void cc()
        {
            var key = "jojo";

            var url = "http://www.tucao.tv/index.php?m=search&c=index&a=init2&catid=&order=&username=&tag=&q=" + key;
            var doc = HtmlDocument.FormUrl(url);
            var list = doc.querySelectorAll("div.search_list div.list");
             

            //DATA 
            var data = new List<ITEMS>();
            foreach (HtmlElement item in list) { 
                data.Add(new ITEMS() {
                    Image = item.querySelector(".pic img").getAttribute("src"),
                    Name = item.querySelector(".info a.blue").innerText,
                    Value = item.querySelector(".info a.blue").getAttribute("href"),
                });
            }
            Console.WriteLine(data);
        }
        static void Main(string[] args)
        {
            var doc = HtmlDocument.FormUrl("http://www.lagou.com/center/job_2366110.html");
             
            var d = doc.querySelectorAll("meta");
            Console.WriteLine(d);

            //dd();
            /* 

            var img = dc.querySelector("img")?.getAttribute("src");
            var tit = dc.querySelector(".videotitle")?.innerHTML;
            //var json = Windows.Data.Json.JsonArray.Parse(dc.innerHTML);
            Console.WriteLine(tit);
            var str = Regex.Match(dc.outerHTML, "items=([^;]+)").Groups[1].Value; 


            foreach (var item in dc.querySelectorAll(".pagelist>div")) {
                var title = item.querySelector("div.listbox");
                Console.WriteLine("---------------------------");
                Console.WriteLine(title.innerText);


                foreach (var a in item.querySelectorAll("a")) {
                    var text = a.innerText; 
                    Console.WriteLine(text+": "+a.getAttribute("href"));
                    Console.WriteLine();


                } 
             
            }   */

            Console.Read();
            ;
        }
    }
}
