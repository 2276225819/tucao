using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace tucao
{

    class HtmlElement
    {
        HtmlParse hp;
        public HtmlElement(string html)
        {
            hp = new HtmlParse(html);
        }
        public override string ToString()
        {
            return hp.ToString();
        }

        public string innerHTML
        {
            get {
                return new Regex("^<.+?>([\\S\\s]+)</.+?>$", RegexOptions.Multiline).Match(outerHTML).Groups[1].Value;
            }
        }
        public string innerText
        {
            get
            {
                return new Regex("<[\\S\\s]+?>|\\s+").Replace(innerHTML, " ");
            }
        }
        public string outerHTML
        {
            get { return hp.ToString(); }
        }
        public string O
        {
            get { return hp.ToString(); }
        }

        public string getAttribute(string key)
        {
            return new Regex(key + "=[\"' ]?([^'\" ]+)").Match(outerHTML).Groups[1].Value;
        }

        public HtmlElement querySelector(string selector)
        {
            var d = querySelectorAll(selector, true);
            if (d.Count == 1) return d[0];
            return null;
        }
        public List<HtmlElement> querySelectorAll(string selector, bool first = false)
        {
            hp.Position = 0;
            var ls = new List<HtmlElement>();
            // #cc .active a[href],body
            foreach (var tag in selector.Split(',')) {
                qq(" " + selector, hp.Length, first, ls);
            }
            return ls;
        } 
        void qq(string selector, int eof, bool first, List<HtmlElement> ls)
        {
            char[] flag = new char[] { ' ', '+', '>' };
            var r = selector.IndexOfAny(flag, 1);
            var sel = r == -1 ? selector.Substring(1) : selector.Substring(1, r-1);


            if (selector[0] == '>') {
                //>aa dd+ee;
                var t = -1;
                while (true) { 
                    var str = hp.ReadNode(sel,ref t);
                    hp.Position++;
                    if (hp.Position > eof)
                        break; ; 
                    t++;
                    if (t != 0) 
                        continue;  
                    if (r != -1)
                        qq(selector.Substring(r), hp.Position + str.Length, first, ls);
                    else if (str != "")
                        ls.Add(new HtmlElement(str));
                    if (first && ls.Count != 0)
                        break;

                }
            }
            else if (selector[0] == ' ') {
                // li+ee;  
                while (true) {
                    var str = hp.ReadNode(sel);
                    hp.Position++;
                    if (hp.Position > eof)
                        break; ;
                    if (r != -1)
                        qq(selector.Substring(r), hp.Position + str.Length, first, ls);
                    else if (str != "")
                        ls.Add(new HtmlElement(str));
                    if (first && ls.Count != 0)
                        break;
                }

            }
            else if (selector[0] == '+') {
                // 
            }
            hp.Position = eof;

        }
    }


    class HtmlParse
    {
        string html;
        int pos; 
        public int Length { get { return html.Length; } }
        public int Position { get { return pos; } set { pos = value; } } 
        public HtmlParse(string html)
        {
            this.html = html;
            this.pos = 0;
        }
        public string T { get { return html.Substring(pos); } }
         public string ReadNode(string sel)
        {
            var  stack = 0;
            return ReadNode(sel,ref stack);
        } 
        public string ReadNode(string sel, ref int stack)
        { 
            var tag = "";
            var start = -1;
            var sl = new List<string>();
            int ss = -1;
            var left = 0; 
            do { 
                if (!fp('<')) return "";
                left = pos + 1;
                if (!fp('>')) return "";
               
                var text = html.Substring(left, pos - left);//TAG... 
                var ma = false;
                var vtag = vstag(text, sel, ref ma);
                if (vtag != "" && start == -1 && ma) {
                    //是标记头 && 无标记 && 完全匹配
                    tag = vtag;
                    start = left - 1;
                    ss = sl.Count;
                }

                //a[href].active#d
                if (vtag != "") {
                    //<TAG ...  
                    sl.Add(vtag);
                    stack++;
                }
                if (text[text.Length - 1] == '/') { 
                    if (sl.Count != 0)
                        sl.RemoveAt(sl.Count - 1);
                    stack--;
                }
                

                if (text[0] == '/') {
                    //</TAG... 
                    var bug = false;
                    while (sl.Count != 0 && text.Substring(1) != sl[sl.Count - 1]) {
                        //BUG
                        sl.RemoveAt(sl.Count - 1);
                        bug = true;
                    }
                    if (start > 0 && bug) {
                        Position = start;
                        fp('>');
                        break; 
                    }

                    if (sl.Count != 0)
                        sl.RemoveAt(sl.Count - 1); 
                    stack--;

                }
                else {
                    //<HTML... 
                }
                if (start > 0 && ss == sl.Count) {
                    break; 
                }

                //fp('>');
                //fp('<');
            } while (true);
             
            var len = pos - start + 1;
            //stack = sl.Count;
            return html.Substring(pos = start, len);
        }
        /// <summary>
        /// valid selector
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        string vstag(string prop, string sel, ref bool matchAll)
        {
            //sel: a[href].active#Id
            if (prop[0] == '/')
                return "";

            //TAG 
            var ptag = new Regex("^\\w+").Match(prop).Value;
            var stag = new Regex("^\\w+").Match(sel).Value;
            if (stag != "" && ptag != stag)
                return ptag;

            //PROP 
            foreach (Match item in new Regex("\\[(.+)\\]").Matches(sel)) {
                if (prop.IndexOf(item.Groups[1].Value) == -1)
                    return ptag;
            }
            //ID
            var id = new Regex("#(\\w+)").Match(sel).Groups[1].Value;
            if (id != "" && !Regex.IsMatch(prop, "id=['\"]?" + id, RegexOptions.IgnoreCase))
                return ptag;
            //CLASS
            foreach (Match item in new Regex("\\.(.+)").Matches(sel)) {
                if (!Regex.IsMatch(prop, "class.+" + item.Groups[1].Value + "['\"]+"))
                    return ptag;
            }

            matchAll = true;//success;
            return ptag;
        }
        /// <summary>
        /// find next pos
        /// </summary>
        /// <param name="c"></param> 
        bool fp(char c)
        {
            var p = html.IndexOfAny(new char[] { c, '"', '\'' }, pos);
            if (p == -1)
                return false; 
            pos = p;

            if (html[pos] != c) {
                pos = html.IndexOf(html[pos], pos + 1) + 1;
                return fp(c);
            }

            if ( html.Length - pos < 7)
                return true;
            if (html.Substring(pos, 7) == "<script") {
                pos = html.IndexOf("</script>", pos + 1) + 1;
                return fp(c);
            }
            if (html.Substring(pos, 3) == "<--") {
                pos = html.IndexOf("-->", pos + 1) + 1;
                return fp(c);
            }
            return true;
        } 

        public override string ToString()
        {
            return html;
        }
    }


}
