﻿using System;
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
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
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
            await Task.Delay(1000);

            if (LIST.Items.Count == 0)
                TEXT.Focus(FocusState.Keyboard);  
        }


        string key = "";
        int page = 0;

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key== Windows.System.VirtualKey.Enter) {
                var obj = sender as TextBox;
                this.key =obj.Text;
                this.page = 1;
                data.Clear();
                search();
            } 
        }


        List<ITEMS> data = new List<ITEMS>(); 

        async void search()
        {
            var url = "http://www.tucao.tv/index.php?m=search&c=index&a=init2&catid=&order=&username=&tag=&q=" + key + "&page=" + page;
            var doc = HtmlDocument.FormUrlAsync(url);
            var list = doc.querySelectorAll("div.search_list div.list");

            if (list.Count == 0)
                return;
             
            foreach (HtmlElement item in list) { 
                data.Add(new ITEMS() {
                    Image = item.querySelector(".pic img").getAttribute("src"),
                    Name = item.querySelector(".info a.blue").innerText,
                    Value = item.querySelector(".info a.blue").getAttribute("href"),
                });
            }
            LIST.ItemsSource = null;
            LIST.ItemsSource = data;
            page++;
            await Task.Delay(6);
            LIST.Focus(FocusState.Programmatic); ;
        }
        class ITEMS
        {
            public string Image { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }

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
         
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var obj = sender as FrameworkElement;
            var d = Regex.Match(obj.Tag.ToString(), "h\\d+").Value;
            if (d == "") {
                new MessageDialog(obj.Tag?.ToString(), "Error").ShowAsync().GetResults(); 
                return;
            }
            Frame.Navigate(typeof(InfoPage), d); 

        }
    }
}
