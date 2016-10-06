using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Web.Http;
using Windows.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace tucao
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPage : Page
    {
        public PlayerPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null) {
                new MessageDialog("error:Parameter").ShowAsync().GetResults(); 
            }
            play(e.Parameter.ToString(), MEDIA);
        }
         
        static async void play(string src , MediaElement media)
        {  
            var c = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(src));
            var res = await c.SendRequestAsync(req, HttpCompletionOption.ResponseHeadersRead);
            var url = req.RequestUri.ToString();

            var sr = await res.Content.ReadAsInputStreamAsync();
            var buf = await sr.ReadBuffer(11);
            FlvHead head = new FlvHead(buf.AsStream()); 
            if (head.Signature == "FLV") {
                Debug.WriteLine("FLV ## " + url);
                media.SetMediaStreamSource(new FlvReader(() => new HttpStream(url)).CreateSource());
                return;

            }

            if (url.IndexOf("mp4") != -1) {
                Debug.WriteLine("MP4 ## " + url);
                media.Source = new Uri(url);
                return;
            }

            new MessageDialog("不支持数据的格式").ShowAsync().GetResults();
            Debug.WriteLine("ERRRRRRRERRRRRRRERRRRRRRERRRRRRRERRRRRRRERRRRRRR");

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MEDIA.Stop();
        }
    }
}
