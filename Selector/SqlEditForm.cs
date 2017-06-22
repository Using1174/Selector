using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Selector
{
    public partial class SqlEditForm : Form
    {
        public SqlEditForm()
        {
            InitializeComponent();
        }
        public string getText()
        {
            return richTextBox1.Text;
        }
        public void setText(string text)
        {
            richTextBox1.Text = text;
        }
        public void setTitle(string title)
        {
            this.Text = title;
        }
        public void setGroupboxTitle(string text)
        {
            groupBox1.Text = text;
        }

        private void SqlEditForm_Load(object sender, EventArgs e)
        {
            this.richTextBox1.Focus();
            this.richTextBox1.SelectAll();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "")
            {
                MessageBox.Show("sql语句不能为空", "提示");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
