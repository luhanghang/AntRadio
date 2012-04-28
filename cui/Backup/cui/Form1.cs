using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cui
{
    public partial class Form1 : Form
    {
        int[] uf = new int[11];
        int[] df = new int[11];
        String dtemp;
        String ptemp;
        String upTemp;
        String downTemp;
        int uflag;
        int dflag;
        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 11; i++)
            {
                uf[i] = 0;
            }
            for (int i = 0; i < 11; i++)
            {
                df[i] = 0;
            }
        }
        private void refesh() 
        {
            stateMessage.Text = null;
            //MessageBox.Show(stateMessage.Text);
            for (int i = 1; i < 11; i++)
            {
                switch (uf[i])
                {
                    case 1:
                        dtemp = "一";
                        break;
                    case 2:
                        dtemp = "二";
                        break;
                    case 3:
                        dtemp = "三";
                        break;
                    case 4:
                        dtemp = "四";
                        break;
                    case 5:
                        dtemp = "五";
                        break;
                    case 6:
                        dtemp = "六";
                        break;
                    case 7:
                        dtemp = "七";
                        break;
                    case 8:
                        dtemp = "八";
                        break;
                    case 9:
                        dtemp = "九";
                        break;
                    case 10:
                        dtemp = "十";
                        break;
                }
                if (uf[i] != 0) 
                {
                    if (i != 10 && uf[i] != 10)
                    {
                        String oltext = stateMessage.Text;
                        stateMessage.Text = oltext + "天线0"+ i + "已与电台" + dtemp + "连接";
                    }
                    else
                    {
                        if (i == 10 && uf[i] != 10)
                        {
                            String oltext = stateMessage.Text;
                            stateMessage.Text = oltext + "天线" + i + "已与电台" + dtemp + "连接";
                        }
                        else
                        {
                            if (uf[i] == 10 && i != 10)
                            {
                                String oltext = stateMessage.Text;
                                stateMessage.Text = oltext + "天线0" + i + "已与电台" + dtemp + "连接";

                            }
                            else
                            {
                                String oltext = stateMessage.Text;
                                stateMessage.Text = oltext + "天线" + i + "已与电台" + dtemp + "连接";
                            }
                    }
                    }
                }
            }
        }
        private void upbtn_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            upTemp = button.Name;
            String str = upTemp.Substring(3,1);
            uflag = int.Parse(str);
            if (uflag == 0)
                uflag = 10;
            //MessageBox.Show(""+uflag);
        }
        private void downbtn_Click(object sender, EventArgs e)
        {
            String temp = upTemp;
            Button button = (Button)sender;
            downTemp = button.Name;
            String str1 = downTemp.Substring(3, 1);
            dflag = int.Parse(str1);
            if (dflag == 0)
                dflag = 10;
            switch (dflag)
            { 
                case 1:
                    dtemp = "一";
                    break;
                case 2:
                    dtemp = "二";
                    break;
                case 3:
                    dtemp = "三";
                    break;
                case 4:
                    dtemp = "四";
                    break;
                case 5:
                    dtemp = "五";
                    break;
                case 6:
                    dtemp = "六";
                    break;
                case 7:
                    dtemp = "七";
                    break;
                case 8:
                    dtemp = "八";
                    break;
                case 9:
                    dtemp = "九";
                    break;
                case 10:
                    dtemp = "十";
                    break;
            }
            //MessageBox.Show("" + dflag);
            if (uf[uflag] == 0 && df[dflag] == 0)
            {
                DialogResult result = MessageBox.Show("是否确定天线"+uflag+"与电台"+dtemp+"连接?", "确定连接", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                switch (result)
                {
                    case DialogResult.OK:
                        String oldtext = connMessage.Text;
                        if (uflag == 10 && dflag == 10)
                        {
                            connMessage.Text = oldtext + "天线" + uflag + "已与电台" + dtemp + "连接";
                            uf[uflag] = dflag;
                            //MessageBox.Show("" + uf[uflag]);
                            df[dflag] = uflag;
                            //MessageBox.Show("" + df[dflag]);
                            refesh();
                        }
                        else
                        {
                            if (uflag != 10 && dflag == 10)
                            {
                                connMessage.Text = oldtext + "天线0" + uflag + "已与电台" + dtemp + "连接";
                                uf[uflag] = dflag;
                                //MessageBox.Show("" + uf[uflag]);
                                df[dflag] = uflag;
                                //MessageBox.Show("" + df[dflag]);
                                refesh();
                            }
                            else
                            {
                                if (uflag == 10 && dflag != 10)
                                {
                                    connMessage.Text = oldtext + "天线" + uflag + "已与电台" + dtemp + "连接";
                                    uf[uflag] = dflag;
                                    //MessageBox.Show("" + uf[uflag]);
                                    df[dflag] = uflag;
                                    //MessageBox.Show("" + df[dflag]);
                                    refesh();
                                }
                                else
                                {
                                    connMessage.Text = oldtext + "天线0" + uflag + "已与电台" + dtemp + "连接";
                                    uf[uflag] = dflag;
                                    //MessageBox.Show("" + uf[uflag]);
                                    df[dflag] = uflag;
                                    //MessageBox.Show("" + df[dflag]);
                                    refesh();
                                }
                            }
                        }
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            else
            {
                switch (uf[uflag])
                {
                    case 1:
                        ptemp = "一";
                        break;
                    case 2:
                        ptemp = "二";
                        break;
                    case 3:
                        ptemp = "三";
                        break;
                    case 4:
                        ptemp = "四";
                        break;
                    case 5:
                        ptemp = "五";
                        break;
                    case 6:
                        ptemp = "六";
                        break;
                    case 7:
                        ptemp = "七";
                        break;
                    case 8:
                        ptemp = "八";
                        break;
                    case 9:
                        ptemp = "九";
                        break;
                    case 10:
                        ptemp = "十";
                        break;
                }
                DialogResult result = MessageBox.Show("是否确定断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                switch (result)
                {
                    case DialogResult.OK:
                        String oldtext = connMessage.Text;
                        if (uf[uflag] == 0 && df[dflag] != 0)
                        {
                            DialogResult result1 = MessageBox.Show("是否确定天线" + df[dflag] + "与电台" + dtemp + "断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                           switch (result1){
                               case DialogResult.OK:
                            if (df[dflag] < 10 && dflag < 10)
                            {
                                connMessage.Text = oldtext + "天线0" + df[dflag] + "已与电台" + dtemp + "断开";
                            }
                            else
                            {
                                if (df[dflag] == 10 && dflag < 10)
                                {
                                    connMessage.Text = oldtext + "天线" + df[dflag] + "已与电台" + dtemp + "断开";
                                }
                                else
                                {
                                    if (df[dflag] < 10 && dflag == 10)
                                    {
                                        connMessage.Text = oldtext + "天线0" + df[dflag] + "已与电台" + dtemp + "断开";
                                    }
                                    else
                                    {
                                        connMessage.Text = oldtext + "天线" + df[dflag] + "已与电台" + dtemp + "断开";
                                    }
                                }
                            }
                            uf[df[dflag]] = 0;
                            df[dflag] = 0;
                            break;
                            case DialogResult.Cancel:
                            break;
                           }
                        }
                        if (uf[uflag] != 0 && df[dflag] == 0)
                        {
                            DialogResult result2 = MessageBox.Show("是否确定天线" + uflag + "与电台" + ptemp + "断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            switch (result2)
                            {
                                case DialogResult.OK:
                                    if (uf[uflag] < 10 && uflag < 10)
                                        connMessage.Text = oldtext + "天线0" + uflag + "已与电台" + ptemp + "断开";
                                    else
                                    {
                                        if (uf[uflag] == 10 && uflag < 10)
                                            connMessage.Text = oldtext + "天线0" + uflag + "已与电台" + ptemp + "断开";
                                        else
                                        {
                                            if (uf[uflag] < 10 && uflag == 10)
                                            {
                                                connMessage.Text = oldtext + "天线" + uflag + "已与电台" + ptemp + "断开";
                                            }
                                            else
                                            {
                                                connMessage.Text = oldtext + "天线" + uflag + "已与电台" + ptemp + "断开";
                                            }
                                        }
                                    }
                                    df[uf[uflag]] = 0;
                                    uf[uflag] = 0;
                                    break;
                                case DialogResult.Cancel:
                                    break;
                            }
                        }
                        if (uf[uflag] != 0 && df[dflag] != 0)
                        {
                            DialogResult result3 = MessageBox.Show("是否确定天线" + df[dflag] + "与电台" + ptemp + "断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            switch (result3)
                            {
                                case DialogResult.OK:
                                    if (df[dflag] < 10 && uf[uflag] < 10)
                                        connMessage.Text = oldtext + "天线0" + df[dflag] + "已与电台" + ptemp + "断开";
                                    else
                                    {
                                        if (df[dflag] == 10 && uf[uflag] < 10)
                                            connMessage.Text = oldtext + "天线" + df[dflag] + "已与电台" + ptemp + "断开";
                                        else
                                        {
                                            if (df[dflag] < 10 && uf[uflag] == 10)
                                            {
                                                connMessage.Text = oldtext + "天线0" + df[dflag] + "已与电台" + ptemp + "断开";
                                            }
                                            else
                                            {
                                                connMessage.Text = oldtext + "天线" + df[dflag] + "已与电台" + ptemp + "断开";
                                            }
                                        }
                                    }
                                    uf[uflag] = 0;
                                    df[dflag] = 0;
                                    break;
                                case DialogResult.Cancel:
                                    break;
                            }
                        }
                        refesh();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
        }
        private void btn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否确定断开所有连接?", "确定断开所有连接", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            switch (result)
            {
                case DialogResult.OK:
                    for (int i = 0; i < 11; i++)
                    {
                        uf[i] = 0;
                    }
                    for (int i = 0; i < 11; i++)
                    {
                        df[i] = 0;
                    }
                    refesh();
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }
    }
}
