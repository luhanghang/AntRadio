using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace cui
{
    public partial class Form2 : Form
    {
        Form1 mainForm;
        bool isNew;
        string oldname;
        public Form2(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }

        public void setInf(string _name,string _ip,string _port)
        {
            oldname = _name;
            name.Text = _name;
            ip.Text = _ip;
            port.Text = _port;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void setIsNew(bool _isNew)
        {
            isNew = _isNew;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            errorIP.Visible = false;
            errorName.Visible = false;
            string _name = name.Text.Trim();
            string _ip = ip.Text.Trim();
            string _port = port.Text.Trim();
            if (_name.Length * _ip.Length * _port.Length != 0)
            {
                int error = 0;
                if (isNew)
                {
                    error = mainForm.addDevice(_name, _ip, _port);
                }
                else
                {
                    error = mainForm.updateDevice(oldname, _name, _ip, _port);
                }
                switch (error)
                {
                    case 1:
                        errorName.Visible = true;
                        break;
                    case 2:
                        errorIP.Visible = true;
                        break;
                    default:
                        Close();
                        break;
                }
            }
        }
    }
}
