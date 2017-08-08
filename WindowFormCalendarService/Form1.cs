using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Data.SQLite;

namespace LIEZHONG.CalendarService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string url = string.Format("https://fangjia.51240.com/{0}__fangjia/", DateTime.Now.Year);
            timerGetYearHoliday.Enabled = true;
            HtmlNodeCollection aa = GetHtmlNode(url, "//table");
            if (aa != null)
            {
                List<holidayModel> mylist = new List<holidayModel>();
                holidayModel mymodel = null;
                Regex regex = new Regex(@"<td>((\w|[、（）()~])*)</td>", RegexOptions.IgnoreCase);
                MatchCollection matchlist = regex.Matches(aa[0].InnerHtml);
                for (int i = 0; i < matchlist.Count; i+=3) {
                    mymodel = new holidayModel();
                    mymodel.休息 = matchlist[i].Groups[1].Value;
                    mymodel.调休 = matchlist[i + 1].Groups[1].Value;
                    mymodel.放假天数 = matchlist[i + 2].Groups[1].Value;
                    mylist.Add(mymodel);
                }
                dataGridView1.DataSource = mylist;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //政务官网数据 string url = string.Format("http://sousuo.gov.cn/list.htm?q={0}%E5%B9%B4%E8%8A%82%E5%81%87%E6%97%A5%E5%AE%89%E6%8E%92%E9%80%9A%E7%9F%A5&n=15&t=paper&childtype=&subchildtype=gc189&pcodeJiguan=&pcodeYear=&pcodeNum=&location=E7BBBCE59088E694BFE58AA1E585B6E4BB96&sort=pubtime&searchfield=title%3Acontent%3Apcode%3Apuborg%3Akeyword&title=&content=&pcode=&puborg=&timetype=timezd&mintime={1}-01-01&maxtime={2}-12-31&Submit=", DateTime.Now.Year, (DateTime.Now.Year - 1), (DateTime.Now.Year - 1));
            //string xpathstring = "//td[@class='info']";
            string url = string.Format("https://fangjia.51240.com/{0}__fangjia/", DateTime.Now.Year);
            HtmlNodeCollection aa = GetHtmlNode(url, "//table");
            if (aa != null)
            {
                int holidaycount = 0, adjustcount = 0;
                Regex regex = new Regex(@"<td>((\w|[、（）()~])*)</td>", RegexOptions.IgnoreCase);
                MatchCollection matchlist = regex.Matches(aa[0].InnerHtml);

                string sqlconn = @"Data Source =db\test.db; Pooling = true; FailIfMissing = false";
                string sqlstr = @"CREATE TABLE IF NOT EXISTS specialdays (  
                         id INTEGER PRIMARY KEY AUTOINCREMENT,   
                         name TEXT,  
                         age REAL,  
                         label TEXT,  
                         create_time TEXT);";
                SQLiteConnection SQLiteConn = new SQLiteConnection(sqlconn);
                SQLiteConn.Open();
                SQLiteCommand SQLiteCmd = SQLiteConn.CreateCommand();
                SQLiteCmd.CommandText = sqlstr;
                SQLiteCmd.CommandType = CommandType.Text;
                int row = SQLiteCmd.ExecuteNonQuery();
                //SQLite.ConnectionString
                //休假
                for (int i = 0; i < matchlist.Count; i += 3)
                {
                    string days = matchlist[i].Groups[1].Value;
                    //休假不止一天
                    if (days.IndexOf('~') > 0){
                        MatchCollection matchdaylist = new Regex(@"(\d{1,2})月(\d{1,2})日", RegexOptions.IgnoreCase).Matches(days);
                        DateTime startsdate = new DateTime((matchdaylist[0].Groups[1].Value == "12" ? (DateTime.Now.Year - 1) : DateTime.Now.Year), int.Parse(matchdaylist[0].Groups[1].Value), int.Parse(matchdaylist[0].Groups[2].Value));
                        DateTime enddate = new DateTime(DateTime.Now.Year, int.Parse(matchdaylist[1].Groups[1].Value), int.Parse(matchdaylist[1].Groups[2].Value));
                        for (; startsdate <= enddate; startsdate = startsdate.AddDays(1))
                        {
                            SQLiteCmd = SQLiteConn.CreateCommand();
                            //可能有重复的情况，排重
                            sqlstr = "select * from specialdays where label = 'Holiday' and name = '" + startsdate.ToString("yyyy-MM-dd 00:00:00") + "'";
                            SQLiteCmd.CommandText = sqlstr;
                            SQLiteCmd.CommandType = CommandType.Text;
                            if (!SQLiteCmd.ExecuteReader().HasRows) {
                                SQLiteConn.Close();
                                SQLiteCmd.Dispose();
                                SQLiteConn.Open();
                                sqlstr = String.Format("insert into specialdays(name,age,label,create_time) values('{0}',{1},'{2}','{3}');", startsdate.ToString("yyyy-MM-dd 00:00:00"),0, "Holiday", DateTime.Now);
                                SQLiteCmd.CommandText = sqlstr;
                                SQLiteCmd.CommandType = CommandType.Text;
                                if (SQLiteCmd.ExecuteNonQuery() > 0)
                                {
                                    holidaycount += 1;
                                }
                                else {
                                    
                                }
                                SQLiteConn.Close();
                                SQLiteCmd.Dispose();
                            }
                            SQLiteConn.Close();
                            SQLiteCmd.Dispose();
                            SQLiteConn.Open();
                        }
                    }
                    else {
                        SQLiteCmd = SQLiteConn.CreateCommand();
                        //休假一天
                        MatchCollection matchdaylist = new Regex(@"(\d{1,2})月(\d{1,2})日", RegexOptions.IgnoreCase).Matches(days);
                        DateTime startsdate = new DateTime(DateTime.Now.Year, int.Parse(matchdaylist[0].Groups[1].Value), int.Parse(matchdaylist[0].Groups[2].Value));
                        sqlstr = "select * from specialdays where label = 'Holiday' and name = '" + startsdate.ToString("yyyy-MM-dd 00:00:00") + "'";
                        SQLiteCmd.CommandText = sqlstr;
                        SQLiteCmd.CommandType = CommandType.Text;
                        if (!SQLiteCmd.ExecuteReader().HasRows)
                        {
                            SQLiteConn.Close();
                            SQLiteCmd.Dispose();
                            SQLiteConn.Open();
                            sqlstr = String.Format("insert into specialdays(name,age,label,create_time) values('{0}',{1},'{2}','{3}');", startsdate.ToString("yyyy-MM-dd 00:00:00"), 0, "Holiday", DateTime.Now);
                            SQLiteCmd.CommandText = sqlstr;
                            SQLiteCmd.CommandType = CommandType.Text;
                            if (SQLiteCmd.ExecuteNonQuery() > 0)
                            {
                                holidaycount += 1;
                            }
                            else
                            {

                            }
                            SQLiteConn.Close();
                            SQLiteCmd.Dispose();
                        }
                        SQLiteConn.Close();
                        SQLiteCmd.Dispose();
                        SQLiteConn.Open();
                    }
                }
                
                //调整
                for (int i = 1; i < matchlist.Count; i += 3)
                {
                    string days = matchlist[i].Groups[1].Value;
                    if (days.IndexOf('班') > 0)
                    {
                        MatchCollection matchdaylist = new Regex(@"(\d{1,2})月(\d{1,2})日", RegexOptions.IgnoreCase).Matches(days);
                        foreach (Match model in matchdaylist) {
                            SQLiteCmd = SQLiteConn.CreateCommand();
                            DateTime startsdate = new DateTime(DateTime.Now.Year, int.Parse(model.Groups[1].Value), int.Parse(model.Groups[2].Value));
                            sqlstr = "select * from specialdays where label = 'Holiday' and name = '" + startsdate.ToString("yyyy-MM-dd 00:00:00") + "'";
                            SQLiteCmd.CommandText = sqlstr;
                            SQLiteCmd.CommandType = CommandType.Text;
                            if (!SQLiteCmd.ExecuteReader().HasRows)
                            {
                                SQLiteConn.Close();
                                SQLiteCmd.Dispose();
                                SQLiteConn.Open();
                                sqlstr = String.Format("insert into specialdays(name,age,label,create_time) values('{0}',{1},'{2}','{3}');", startsdate.ToString("yyyy-MM-dd 00:00:00"), 0, "adjust", DateTime.Now);
                                SQLiteCmd.CommandText = sqlstr;
                                SQLiteCmd.CommandType = CommandType.Text;
                                if (SQLiteCmd.ExecuteNonQuery() > 0)
                                {
                                    adjustcount += 1;
                                }
                                else
                                {

                                }
                                SQLiteConn.Close();
                                SQLiteCmd.Dispose();
                            }
                            SQLiteConn.Close();
                            SQLiteCmd.Dispose();
                            SQLiteConn.Open();
                        }
                    }
                }
                label1.Text = "提取完成，并成功更新数据库！本次新增放假日"+holidaycount+"天，调休日"+adjustcount+"天";
            }
        }

        /// <summary>
        /// 向url发起请求，得到返回的数据
        /// </summary>
        /// http://sousuo.gov.cn/list.htm?q=2016年节假日安排通知&n=15&t=paper&childtype=&subchildtype=gc189&pcodeJiguan=&pcodeYear=&pcodeNum=&location=E7BBBCE59088E694BFE58AA1E585B6E4BB96&sort=pubtime&searchfield=title:content:pcode:puborg:keyword&title=&content=&pcode=&puborg=&timetype=timezd&mintime=2015-01-01&maxtime=2015-12-31&Submit=
        /// http://sousuo.gov.cn/list.htm?q=2017年节假日安排通知&n=15&t=paper&childtype=&subchildtype=gc189&pcodeJiguan=&pcodeYear=&pcodeNum=&location=E7BBBCE59088E694BFE58AA1E585B6E4BB96&sort=pubtime&searchfield=title:content:pcode:puborg:keyword&title=&content=&pcode=&puborg=&timetype=timezd&mintime=2016-01-01&maxtime=2016-12-31&Submit=
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetHtmlStr(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 抓取网页dom
        /// </summary>
        /// <param name="url"></param>
        /// <param name="xpath">根据网页的内容设置XPath路径表达式</param>
        /// <returns>HtmlNodeCollection</returns>
        public HtmlNodeCollection GetHtmlNode(string url, string xpath)
        {
            string htmlstr = GetHtmlStr(url);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;
            return rootnode.SelectNodes(xpath);
        }

        private class holidayModel {
            public string 休息 { set;get; }
            public string 调休{ set;get; }
            public string 放假天数 { set; get; }
        }

        private void btnDataHolidays_Click(object sender, EventArgs e)
        {

            new DataHolidaysForm().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //label1.Text = GetDateNowTimeDay(textBox1.Text).ToString();
        }
        /// <summary>
        /// 得到N个工作日之后的时间（排除节假日）算当天时间  
        /// </summary>
        /// 2017-07-06
        /// <param name="time"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        //public static string GetDateTimeDay(string paramtime, int paramday)
        //{
        //    string result = string.Empty;
        //    try
        //    {
        //        DateTime time1 = Convert.ToDateTime(paramtime);
        //        LZCommonDataBLL myLZCommonDataBLL = new LZCommonDataBLL();
        //        List<LZCommonData> myLZCommonDataList = myLZCommonDataBLL.GetModelList(" Code='010' and FullName >='" + paramtime + "'");
        //        for (int i = 0; i < paramday;)
        //        {
        //            LZCommonData myLZCommonData = myLZCommonDataList.Where(o => o.FullName == time1.ToString("yyyy-MM-dd 00:00:00")).FirstOrDefault();
        //            //当前时间为特殊时间
        //            if (myLZCommonData != null)
        //            {
        //                //调休上班  +1工作日
        //                if (myLZCommonData.Label == "adjust")
        //                {
        //                    i += 1;
        //                }
        //                else
        //                {

        //                }
        //                time1 = time1.AddDays(1);
        //                continue;
        //            }
        //            else
        //            {
        //                //普通时间  判断是否是周末
        //                if (time1.DayOfWeek.GetHashCode() == 0 || time1.DayOfWeek.GetHashCode() == 6)
        //                {

        //                }
        //                else
        //                {
        //                    i += 1;
        //                }
        //                time1 = time1.AddDays(1);
        //                continue;
        //            }
        //        }
        //        //判断最后得到的时间是否为节假日
        //        do
        //        {
        //            LZCommonData myLZCommonData = myLZCommonDataList.Where(o => o.FullName == time1.ToString("yyyy-MM-dd 00:00:00")).FirstOrDefault();
        //            //当前时间为特殊时间
        //            if (myLZCommonData != null)
        //            {
        //                //调休上班  +1工作日
        //                if (myLZCommonData.Label == "adjust")
        //                {
        //                    break;
        //                }
        //                else
        //                {
        //                    time1 = time1.AddDays(1);
        //                    continue;
        //                }
        //            }
        //            else
        //            {
        //                //普通时间  判断是否是周末
        //                if (time1.DayOfWeek.GetHashCode() == 0 || time1.DayOfWeek.GetHashCode() == 6)
        //                {
        //                    time1 = time1.AddDays(1);
        //                    continue;
        //                }
        //                else
        //                {
        //                    break;
        //                }

        //            }
        //        } while (true);
        //        result = time1.ToString("yyyy-MM-dd hh:mm:ss");
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return result;
        //}

        /// <summary>
        /// 得到特定时间与当天相差几个工作日  特定时间和当天也算进入
        /// </summary>
        /// 2017-07-06
        /// <param name="paramtime"></param>
        /// <param name="paramday"></param>
        /// <returns></returns>
        //public static int GetDateNowTimeDay(string paramtime)
        //{
        //    int result = 0;
        //    try
        //    {
        //        DateTime time1 = Convert.ToDateTime(paramtime);
        //        LZCommonDataBLL myLZCommonDataBLL = new LZCommonDataBLL();
        //        List<LZCommonData> myLZCommonDataList = myLZCommonDataBLL.GetModelList(" Code='010' and FullName >='" + paramtime + "'");
        //        for (; time1 < DateTime.Now;)
        //        {
        //            LZCommonData myLZCommonData = myLZCommonDataList.Where(o => o.FullName == time1.ToString("yyyy-MM-dd 00:00:00")).FirstOrDefault();
        //            当前时间为特殊时间
        //            if (myLZCommonData != null)
        //            {
        //                调休上班 + 1工作日
        //                if (myLZCommonData.Label == "adjust")
        //                {
        //                    result += 1;
        //                }
        //                else
        //                {

        //                }
        //                time1 = time1.AddDays(1);
        //                continue;
        //            }
        //            else
        //            {
        //                普通时间 判断是否是周末
        //                if (time1.DayOfWeek.GetHashCode() == 0 || time1.DayOfWeek.GetHashCode() == 6)
        //                {

        //                }
        //                else
        //                {
        //                    result += 1;
        //                }
        //                time1 = time1.AddDays(1);
        //                continue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return result;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// 2017-07-06
        /// <param name="paramtimelist"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        //public static int GetDateNowTimeListDay(string paramtimelist, int day)
        //{
        //    int count = 0;
        //    try
        //    {
        //        string[] timelist = paramtimelist.Split(',');
        //        foreach (string paramtime in timelist)
        //        {
        //            int result = 0;
        //            DateTime time1 = Convert.ToDateTime(paramtime);
        //            LZCommonDataBLL myLZCommonDataBLL = new LZCommonDataBLL();
        //            List<LZCommonData> myLZCommonDataList = myLZCommonDataBLL.GetModelList(" Code='010' and FullName >='" + paramtime + "'");
        //            for (; time1 < DateTime.Now;)
        //            {
        //                LZCommonData myLZCommonData = myLZCommonDataList.Where(o => o.FullName == time1.ToString("yyyy-MM-dd 00:00:00")).FirstOrDefault();
        //                //当前时间为特殊时间
        //                if (myLZCommonData != null)
        //                {
        //                    //调休上班  +1工作日
        //                    if (myLZCommonData.Label == "adjust")
        //                    {
        //                        result += 1;
        //                    }
        //                    else
        //                    {

        //                    }
        //                    time1 = time1.AddDays(1);
        //                    continue;
        //                }
        //                else
        //                {
        //                    //普通时间  判断是否是周末
        //                    if (time1.DayOfWeek.GetHashCode() == 0 || time1.DayOfWeek.GetHashCode() == 6)
        //                    {

        //                    }
        //                    else
        //                    {
        //                        result += 1;
        //                    }
        //                    time1 = time1.AddDays(1);
        //                    continue;
        //                }
        //            }
        //            if (result >= day)
        //            {
        //                count += 1;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return count;
        //}
    }
}
