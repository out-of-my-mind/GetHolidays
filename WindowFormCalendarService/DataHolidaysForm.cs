using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace LIEZHONG.CalendarService
{
    public partial class DataHolidaysForm : Form
    {
        public DataHolidaysForm()
        {
            InitializeComponent();
        }

        private void DataHolidaysForm_Load(object sender, EventArgs e)
        {
            string sqlconn = @"Data Source =db\test.db; Pooling = true; FailIfMissing = false";
            string sqlstr = @"CREATE TABLE IF NOT EXISTS specialdays (  
                         id INTEGER PRIMARY KEY AUTOINCREMENT,   
                         name TEXT,  
                         age REAL,  
                         label TEXT,  
                         create_time TEXT); select * from specialdays;";
            SQLiteConnection SQLiteConn = new SQLiteConnection(sqlconn);
            SQLiteConn.Open();
            SQLiteCommand SQLiteCmd = SQLiteConn.CreateCommand();
            SQLiteCmd.CommandText = sqlstr;
            SQLiteCmd.CommandType = CommandType.Text;
            SQLiteDataReader dr = SQLiteCmd.ExecuteReader();
            holidaymode myholidaymode = null;
            List<holidaymode> mylist = new List<holidaymode>();
            while (dr.Read()) {
                myholidaymode = new holidaymode();
                myholidaymode.放假时间 = dr["name"].ToString();
                myholidaymode.类型 = dr["label"].ToString() == "adjust" ? "上班" : "放假";
                myholidaymode.添加时间 = dr["create_time"].ToString();
                mylist.Add(myholidaymode);
            }
            dataGridView1.DataSource = mylist.OrderBy(o=>o.放假时间).ToList();
        }
        private class holidaymode
        {
            public string 放假时间 { set; get; }
            public string 类型 { set; get; }
            public string 添加时间 { set; get; }
        }
    }
}
