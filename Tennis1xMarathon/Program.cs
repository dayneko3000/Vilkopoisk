using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;

//using System.Windows.Forms.WebBrowser;


namespace Tennis1xMarathon
{

    public class Event : IEquatable<Event>
    {
        private readonly string _t1;
        private readonly string _t2;
        private readonly string _time;
        public double P1 { get; set; }
        public double P2 { get; set; }

        private string _href;

        public Event(string t1, string t2, string time, string p1, string p2, string href)
        {
            _t1 = t1;
            _t2 = t2;
            _time = time;
            P1 = Convert.ToDouble(p1.Replace('.', ','));
            P2 = Convert.ToDouble(p2.Replace('.', ','));
            _href = href;
        }

        public static bool operator== (Event e1, Event e2)
        {
            //if (e1._time != e2._time)
             //   return false;
            var t11 = new HashSet<string>(e1._t1.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var t21 = new HashSet<string>(e1._t2.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var t12 = new HashSet<string>(e2._t1.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var t22 = new HashSet<string>(e2._t2.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var o = t11.Any(name => name.Length >= 2 && (t12.Contains(name) || (t22.Contains(name))));
            var o2 = t21.Any(name => name.Length >= 2 && (t12.Contains(name) || (t22.Contains(name))));
            return o && o2;
        }
        public static bool operator !=(Event e1, Event e2)
        {
            return !(e1 == e2);
        }

        public bool isReverseTo(Event e)
        {
            var t11 = new HashSet<string>(_t1.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var t12 = new HashSet<string>(e._t1.Replace('.', ' ').Replace('-', ' ').Replace('/', ' ').Replace(',', ' ').Split(' '));
            var o = t11.Any(name => name.Length >= 2 && t12.Contains(name));
            return !o;
        }

        public override int GetHashCode()
        {
            return _t1.GetHashCode() + _t2.GetHashCode();
        }

        public bool Equals(Event e)
        {
            return e == this;
        }

        public override string ToString()
        {
            return _t1 + " vs " + _t2 + " " + _time + " П1/П2 " + P1 + "/" + P2 + "\n";// + _href + "\n\n";
        }
    }

    internal class Program
    {

        public static FirefoxDriver Browser { get; set; }

        [STAThread]
        private static void Wait()
        {
            try
            {
                IWait<IWebDriver> wait = new WebDriverWait(Browser,
                    TimeSpan.FromSeconds(30.00));
                wait.Until(
                    driver1 =>
                        ((IJavaScriptExecutor)Browser).ExecuteScript("return document.readyState").Equals("complete"));
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
            }
        }

        private static HashSet<Event> Marathon2()
        {
            var result = new HashSet<Event>();
            WebRequest reqGET = WebRequest.Create(@"https://www.marathonbet0.com/su/popular/Tennis/");
            var resp = reqGET.GetResponse();
            var stream = resp.GetResponseStream();
            var sr = new System.IO.StreamReader(stream);
            var s = sr.ReadToEnd();
            var html = new HtmlDocument();
            html.LoadHtml(s);
            var table =
                html.DocumentNode.SelectNodes("//tbody[@class and @id and @data-event-treeid and @data-event-name and @data-live]");
            foreach (var body in table)
            {
                try
                {
                    var names = body.SelectNodes(".//div[(@class=\"member-name nowrap \" or @class=\"today-member-name nowrap \") and @data-ellipsis=\"{}\"]");
                    var t1 = names[0].InnerText;
                    var t2 = names[1].InnerText;
                    var time = body.SelectSingleNode(".//td[@class=\"date\"]").InnerText;
                    var coefs = body.SelectNodes(".//span[@class and @data-selection-price and @data-selection-key]");
                    var p1 = coefs[0].InnerText;
                    var p2 = coefs[1].InnerText;
                    result.Add(new Event(t1, t2, time, p1, p2, ""));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return result;
        }

        private static HashSet<Event> Marathon()
        {
            var result = new HashSet<Event>();
            Browser.Navigate().GoToUrl("https://www.marathonbet0.com/su/popular/Tennis/");
            var table =
                Browser.FindElements(
                    By.XPath("//tbody[@class and @id and @data-event-treeid and @data-event-name and @data-live]"));
            foreach (var body in table)
            {
                try
                {
                    var names = body.FindElements(By.XPath(".//div[(@class=\"member-name nowrap \" or @class=\"today-member-name nowrap \") and @data-ellipsis=\"{}\"]"));
                    var t1 = names[0].Text;
                    var t2 = names[1].Text;
                    var time = body.FindElement(By.XPath(".//td[@class=\"date\"]")).Text;
                    var coefs = body.FindElements(By.XPath(".//span[@class and @data-selection-price and @data-selection-key]"));
                    var p1 = coefs[0].Text;
                    var p2 = coefs[1].Text;
                    result.Add(new Event(t1, t2, time, p1, p2, ""));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return result;
        }

        private static HashSet<Event> XBet2()
        {
            var result = new HashSet<Event>();
            WebRequest reqGET = WebRequest.Create(@"https://sport4321.com/line/Tennis/");
            var resp = reqGET.GetResponse();
            var stream = resp.GetResponseStream();
            var sr = new System.IO.StreamReader(stream);
            var s = sr.ReadToEnd();
            var html = new HtmlDocument();
            html.LoadHtml(s);
            var table =
                html.DocumentNode.SelectSingleNode("//div[@id=\"games_content\" and @class=\"game_content_line on_main\"]");
            var coll = table.SelectNodes(".//div[@class=\"tb2\"]");
            foreach (var div in coll)
            {
                var name = div.SelectSingleNode(".//span[@class=\"gname hotGameTitle\" and @title]");
                var t1 = name.InnerText.Split('—')[0].Remove(0, 26);
                var t2 = name.InnerText.Split('—')[1].Remove(0, 1);
                t2 = t2.Remove(t2.IndexOf('\r') - 1, t2.Length - t2.IndexOf('\r') + 1);
                var time = div.SelectSingleNode(".//td[@class=\"score\"]").InnerText.Split('|')[1].Remove(0, 1);
                time = time.Remove(5, time.Length - 5);
                var p1 = div.SelectSingleNode(".//div[@data-betname=\"П1\"]").InnerText;
                var p2 = div.SelectSingleNode(".//div[@data-betname=\"П2\"]").InnerText;
                result.Add(new Event(t1, t2, time, p1, p2, ""));
            }
            return result;
        }

        private static HashSet<Event> XBet()
        {
            var result = new HashSet<Event>();
            Browser.Navigate().GoToUrl("https://sport4321.com/line/Tennis/");
            var table =
                Browser.FindElement(By.XPath("//div[@id=\"games_content\" and @class=\"game_content_line on_main\"]"));
            var coll = table.FindElements(By.XPath(".//div[@class=\"tb2\"]"));
            foreach (var div in coll)
            {
                var name = div.FindElement(By.XPath(".//span[@class=\"gname hotGameTitle\" and @title]"));
                var t1 = name.Text.Split('—')[0];
                var t2 = name.Text.Split('—')[1];
                var time = div.FindElement(By.XPath(".//td[@class=\"score\"]")).Text.Split('|')[1].Remove(0, 1);
                time = time.Remove(5, time.Length - 5);
                var p1 = div.FindElement(By.XPath(".//div[@data-betname=\"П1\"]")).Text;
                var p2 = div.FindElement(By.XPath(".//div[@data-betname=\"П2\"]")).Text;
                result.Add(new Event(t1, t2, time, p1, p2, ""));
            }
            return result;
        }

        [STAThread]
        private static void Main()
        {
            var result = new HashSet<Event>();
            while (true)
            {
                //Thread.Sleep(1000);
                //Browser = new FirefoxDriver();
                var t = Marathon2();
                //var t2 = XBet();
                var t3 = XBet2();
                foreach (var el in t)
                {
                    foreach (var el2 in t3)
                    {
                        if (el != el2) continue;
                        var res = 1/el.P2 + 1/el2.P1;
                        var res2 = 1/el.P1 + 1/el2.P2;
                        if (el.isReverseTo(el2))
                        {
                            res = 1/el.P1 + 1/el2.P1;
                            res2 = 1/el.P2 + 1/el2.P2;
                        }
                        if (result.Contains(el) || (!(res < 1) && !(res2 < 1))) continue;
                        result.Add(el);
                        Console.WriteLine("M: " + el + "1x: " + el2 + res + "\n" + res2 + "\n");
                    }
                }
            }
        }
    }
}