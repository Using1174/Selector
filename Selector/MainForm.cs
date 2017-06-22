using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace Selector
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        MySqlConnection conn = null;
        MySqlCommand mycmd = null;
        Config cfg = null;
        List<DBConfig> dbConfigs = null;

        const int LOG_ERROR = -1;
        const int LOG_INFO = 1;
        const int LOG_WARN = 2;
        const int LOG_LOG = 0;
        const string CONFIG_FILE = "dbconfig.json";


        private void MainForm_Load(object sender, EventArgs e)
        {
            string conStr = ReadConfig();
            if (conStr == null || conStr.Trim() == "")
            {
                writeLog("con't open config file：" + CONFIG_FILE, LOG_ERROR);
                return;
            }
            this.comboBox1.Items.Clear();
            cfg = JsonConvert.DeserializeObject<Config>(conStr);
            dbConfigs = cfg.dbConfigs;
            this.InitDBConfig(dbConfigs);
            if (this.comboBox1.Items.Count > 0)
            {
                this.comboBox1.SelectedIndex = 0;
            }
            this.InitButtons(cfg.btnConfigs);
            this.InitSearchConfig(cfg.searchConfig);
            //DBConfig dbConfig = JsonConvert.DeserializeObject<DBConfig>(config);

        }
        private void InitDBConfig(List<DBConfig> dbConfigs)
        {
            if (dbConfigs == null)
            {
                writeLog("DBConfig cannot be resolved! Please check the " + CONFIG_FILE + " file!", LOG_ERROR);
                return;
            }
            for (var i = 0; i < dbConfigs.Count; i++)
            {
                var config = dbConfigs[i];
                if (config == null)
                {
                    writeLog("invalid config " + i, LOG_ERROR);
                    continue;
                }
                this.comboBox1.Items.Add(config);
            }
        }
        private void InitButtons(List<ButtonConfig> btnConfigs)
        {
            if (btnConfigs == null)
            {
                return;
            }
            var inx = 10;
            for (var i = 0; i < btnConfigs.Count; i++)
            {
                var config = btnConfigs[i];
                if (config == null)
                {
                    writeLog("BtnConfig cannot be resolved! Please check the " + CONFIG_FILE + " file!", LOG_ERROR);
                    return;
                }
                var newBtn = new ToolStripButton();
                newBtn.Image = global::Selector.Properties.Resources.grid;
                newBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
                newBtn.Name = "toolStripButton" + inx++;
                newBtn.Size = new System.Drawing.Size(76, 22);
                newBtn.Text = config.btnName;
                newBtn.Tag = config.sql;
                newBtn.Click += new System.EventHandler(this.toolStripButton_Click);
                this.toolStrip1.Items.Add(newBtn);
            }
        }

        private void InitSearchConfig(List<SearchConfig> searchConfigs)
        {
            if (searchConfigs == null)
            {
                writeLog("SearchConfig cannot be resolved! Please check the " + CONFIG_FILE + " file!", LOG_ERROR);
                return;
            }
        }
        private string ReadConfig()
        {
            if (File.Exists(CONFIG_FILE))
            {
                string str = null;
                try
                {
                    FileStream fs = new FileStream(CONFIG_FILE, FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamReader sr = new StreamReader(fs);
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    writeLog(ex.Message, LOG_ERROR);
                }
                return str;
            }
            else
            {
                return null;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedItem is DBConfig)
            {
                DBConfig config = (DBConfig)this.comboBox1.SelectedItem;
                this.textBoxPort.Text = config.port + "";
                this.textBoxPwd.Text = config.password;
                this.textBoxUN.Text = config.user;
            }
        }
        private void writeLog(string msg, int state = 0)
        {
            int start = richTextBox1.TextLength;
            int end = start + msg.Length;
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.ScrollToCaret();
            richTextBox1.Select(start, msg.Length);
            switch (state)
            {
                case LOG_ERROR:
                    richTextBox1.SelectionColor = Color.Red;
                    break;
                case LOG_INFO:
                    richTextBox1.SelectionColor = Color.LawnGreen;
                    break;
                case LOG_WARN:
                    richTextBox1.SelectionColor = Color.Yellow;
                    break;
            }
        }

        private void buttonConn_Click(object sender, EventArgs e)
        {

            MySqlConnectionStringBuilder str = new MySqlConnectionStringBuilder();
            try
            {

                DBConfig config = null;

                string connStr = "";

                if (comboBox1.SelectedItem is DBConfig)
                {
                    config = (DBConfig)comboBox1.SelectedItem;
                }
                if (config == null)
                {
                    string text = comboBox1.Text;
                    Match match = Regex.Match(text, "(\\d{1,3}.\\d{1,3}.\\d{1,3}.\\d{1,3})");
                    if (match.Success)
                    {
                        connStr = match.Groups[0].ToString();
                    }
                }
                else
                {
                    connStr = config.server;
                }

                str.Password = textBoxPwd.Text;
                str.UserID = textBoxUN.Text;
                str.Server = connStr;
                str.Port = uint.Parse(textBoxPort.Text);
                str.ConnectionTimeout = 2;

                str.Pooling = true;
                if (conn != null)
                {
                    conn.Close();
                }
                conn = new MySqlConnection(str.ToString());
                conn.Open();
                writeLog(str.Server + ":" + str.Port + " 连接已建立！");
                comboBox2.Items.Clear();
                mycmd = new MySqlCommand("show databases;", conn);
                IDataReader dr = mycmd.ExecuteReader();
                while (dr.Read())
                {
                    comboBox2.Items.Add(dr[0]);
                }
                dr.Close();
                if (config != null)
                {
                    comboBox2.SelectedItem = config.defaultDB;
                }
                else
                {
                    //str.Database = comboBox2.Text;
                }
                toolStrip1.Enabled = true;
            }
            catch (MySqlException mexc)
            {
                writeLog("连接数据库失败！", LOG_ERROR);
                writeLog(mexc.Message, LOG_ERROR);
                //writeLog("连接字符串：" + str.ToString(), LOG_ERROR);
            }
            catch (Exception exc)
            {
                writeLog(exc.Message, LOG_ERROR);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sql = "use " + comboBox2.SelectedItem + ";";
            //new MySqlCommand(sql, conn).ExecuteNonQuery();
            //writeLog(sql, LOG_INFO);
            NonQuery(sql);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Query("show tables;");
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.QueryWindow("select * from tableName");
        }

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (ToolStripButton)sender;
            if (btn != null)
            {
                Query(btn.Tag.ToString());
            }

        }

        private bool NonQuery(string sql, params MySqlParameter[] lists)
        {

            bool result = false;
            if (conn != null)
            {
                try
                {
                    mycmd = new MySqlCommand(sql, conn);
                    foreach (MySqlParameter obj in lists)
                    {
                        mycmd.Parameters.Add(obj);
                    }
                    mycmd.ExecuteNonQuery();
                    writeLog(mycmd.CommandText, LOG_INFO);
                }
                catch (Exception e)
                {
                    writeLog(e.Message, LOG_ERROR);
                }
            }
            else
            {
                writeLog("数据库未连接！", LOG_ERROR);
            }
            return result;
        }

        private bool Query(string sql, params MySqlParameter[] lists)
        {
            bool result = false;
            if (conn != null)
            {
                try
                {
                    mycmd = new MySqlCommand(sql, conn);
                    foreach (MySqlParameter obj in lists)
                    {
                        mycmd.Parameters.Add(obj);
                    }
                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    adapter.SelectCommand.Parameters.AddRange(lists);

                    //adapter.SelectCommand = mycmd;
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dataGridView1.DataSource = ds.Tables[0];
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = true;
                    }
                    writeLog("执行[" + mycmd.CommandText + "]成功", LOG_INFO);
                }
                catch (Exception e)
                {
                    writeLog(e.Message, LOG_ERROR);
                }
            }
            else
            {
                writeLog("数据库未连接！", LOG_ERROR);
            }
            return result;
        }
        private void QueryWindow(string sql)
        {
            SqlEditForm f2 = new SqlEditForm();
            f2.setText(sql);
            if (f2.ShowDialog() == DialogResult.OK)
            {
                sql = f2.getText().Trim();
                if (conn != null)
                {
                    try
                    {
                        mycmd = new MySqlCommand(sql, conn);
                        MySqlDataAdapter adapter = new MySqlDataAdapter(mycmd);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        dataGridView1.DataSource = ds.Tables[0];
                        writeLog("执行[" + sql + "]成功", 1);
                    }
                    catch (Exception e)
                    {
                        writeLog(e.Message, LOG_ERROR);
                    }
                }
                else
                {
                    writeLog("数据库未连接！", LOG_ERROR);
                }
            }
        }
        private bool Search(string searchTxt)
        {
            if (searchTxt == null)
            {
                writeLog("search text cannot be null!", LOG_ERROR);
                return false;
            }
            if (cfg.searchConfig == null)
            {
                writeLog("SearchConfig cannot be resolved! Please check the " + CONFIG_FILE + " file!", LOG_ERROR);
                return false;
            }
            string str = "%" + searchTxt + "%";
            bool result = false;
            for (int i = 0; i < cfg.searchConfig.Count; i++)
            {
                string sql = cfg.searchConfig[i].sql;

                result = Query(sql, new MySqlParameter("?name", MySqlDbType.VarChar, 50) { Value = str });
                if (result)
                {
                    break;
                }
            }
            if (result)
            {
                writeLog("查询结束！", LOG_INFO);
            }
            else
            {
                writeLog("查询结束，未找到相关结果！", LOG_WARN);
            }
            return result;
        }

        private void toolStripTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (conn == null)
                {
                    writeLog("数据库未连接！", LOG_ERROR);
                    return;
                }
                this.Search(toolStripTextBox1.Text);
            }
        }
    }

    public class Config
    {
        public List<DBConfig> dbConfigs;
        public List<ButtonConfig> btnConfigs;
        public List<SearchConfig> searchConfig;

    }

    public class SearchConfig
    {
        public string sql;
    }
    public class ButtonConfig
    {
        public string btnName;
        public string sql;
    }

    public class DBConfig
    {
        public string serverName;
        public string server;
        public uint port;
        public string user;
        public string password;
        public string defaultDB;

        public override string ToString()
        {
            return this.serverName + "(" + this.server + ":" + this.port + ")";
        }
    }
}
