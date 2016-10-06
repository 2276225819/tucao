using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace tucao
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ListPage : Page
    {
        public ListPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                return;

            List<string> v = e.Parameter as List<string>;
            this.key = int.Parse(v[0]);
            this.TITLE.Text = v[1];
            this.page = 1;
            this.data.Clear();
            this.search();
        }


        public int key;
        public int page;
        List<ITEMS> data = new List<ITEMS>(); 
        class ITEMS
        {
            public string Image { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }

        }
        async void search()
        {
            PROGRESS.Visibility = Visibility.Visible; 
            var url = "http://www.tucao.tv/list/" + key + "/index_" + page+".html"; 
            var doc = await HtmlDocument.FormUrlAsync(url);
            var list = doc.querySelectorAll("div.list li");

            if (list.Count == 0)
                return;

            foreach (HtmlElement item in list) {
                data.Add(new ITEMS() {
                    Image = item.querySelector("img")?.getAttribute("src"),
                    Name = item.querySelector("a.title")?.innerText,
                    Value = item.querySelector("a.title")?.getAttribute("href"),
                });
            }
            LIST.ItemsSource = null;
            LIST.ItemsSource = data;
            page++;
            await Task.Delay(6);
            LIST.Focus(FocusState.Programmatic); ;

            PROGRESS.Visibility = Visibility.Collapsed;
        }



        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var obj = sender as FrameworkElement;
            var d = Regex.Match(obj.Tag.ToString(), "h\\d+").Value;
            Debug.WriteLine(d);
            if (d == "") {
                new MessageDialog(obj.Tag?.ToString(), "Error").ShowAsync().GetResults();
                return;
            }
            Frame.Navigate(typeof(InfoPage), d);

        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var s = sender as ScrollViewer;
            if (e.IsIntermediate)
                return;
            if ((int)s.VerticalOffset != (int)s.ScrollableHeight)
                return;

            search();

        }
    }
}
