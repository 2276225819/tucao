using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace tucao
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InfoPage : Page
    {
        public InfoPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        { 
            var vid = e.Parameter.ToString();
            var url = "http://backup.biliplus.com/play/" + vid+"/"; 
            try {
                var doc = await HtmlDocument.FormUrlAsync(url);

                var title = doc?.querySelector(".videotitle")?.innerHTML;
                if(title!="") title = Regex.Match(title, "h\\d+...(.+)").Groups[1].Value;
                var str = doc?.innerHTML ?? "items=[]";
                if(str!="")str = Regex.Match(str, "items=([^;]+)").Groups[1].Value; 
                var json = JsonArray.Parse(str.Replace("	", " "));

                //IMAGE
                var img = doc?.querySelector("img")?.getAttribute("src");
                IMAGE.Height = 0;
                IMAGE.Source = new BitmapImage(new Uri(img));
                IMAGE.Loaded += (ss, ee) => { 
                    IMAGE.Height = 200;
                };

                //TITLE
                TITLE.Text = title;
             
                //DATA 
                var data = new List<ITEMS>();
                foreach (IJsonValue item in json) {
                    JsonObject v = item.GetObject();
                    var type = v["type"].GetString();
                    switch (type) {
                        case "youku":
                            data.Add(new ITEMS() { Source = "来源：" + type, Name = v["title"].GetString() });
                            break;
                        default:
                            data.Add(new ITEMS() { Source = "来源：" + type, Name = v["title"].GetString(), Value = v["vid"].GetString() });
                            break;
                    }
                }
                LIST.ItemsSource = data; 
            }
            catch (Exception ee) {
                Debug.WriteLine("========INFOPAGE========");
                Debug.WriteLine("URL: " + url);
                Debug.WriteLine(ee); 

            }
        }
        class ITEMS
        {
            public string Name { get; set; }
            public string Source { get; set; }
            public string Value { get; set; }

        }
         
        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var s = sender as FrameworkElement;
            if (s.Tag == null) {
                var dia = new MessageDialog("不支持的格式").ShowAsync().GetResults();
                return;
            }
            Frame.Navigate(typeof(PlayerPage), "http://api.tucao.tv/api/down/" + s.Tag);
            Debug.WriteLine("TAPPED TAG："+s.Tag);
        }
    }
}
