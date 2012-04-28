using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml;

namespace cui
{
    public partial class Form1 : Form
    {
        private static String[] ArrayNumInChinese = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        private static Color[] Colors = new Color[11];
        private static String configPath = Application.StartupPath + "\\devices.xml";
        static string[] errors = { null, "串口打开失败", "接收校验错误", "多路打开错误" };
        private static String logFile = Application.StartupPath + "\\log.txt";
        private static int BROADCAST_SEND_PORT = 13456;
        private static int BROADCAST_RECEIVE_PORT = 65431;

        private static String iniFilePath = Application.StartupPath + "\\config.ini";
        private static String Section = "Discover";
        private static string discover;
        private static Byte[] findBytes = Encoding.ASCII.GetBytes("*find*");

        int[] uf = new int[11];
        int[] df = new int[11];

        int uflag;
        int dflag;

        private Button[] UpButtons = new Button[11];
        private Button[] DownButtons = new Button[11];

        XmlDocument doc = new XmlDocument();
        XmlElement root;

        TcpClient client;
        NetworkStream clientStream;

        TcpClient client_log;
        NetworkStream clientStream_log;
        ASCIIEncoding encoder = new ASCIIEncoding();

        StreamWriter sw;
        byte[] tmp;

        private static BindingSource bindingSource;
        ArrayList deviceList = new ArrayList();
        UdpClient udpClient;

        public class DeviceInf
        {
            private string _name;
            private string _ip;
            private string _port;

            public DeviceInf(string name, string ip, string port)
            {
                this._name = name.Trim();
                this._ip = ip.Trim();
                this._port = port.Trim();
            }

            public string displayName
            {
                get
                {
                    return _name + "[" + _ip + "]";
                }
            }

            public string name
            {
                get
                {
                    return _name;
                }

                set
                {
                    this._name = name;
                }
            }

            public string ip
            {
                set
                {
                    this._ip = ip;
                }
                get
                {
                    return _ip;
                }
            }

            public string port
            {
                set
                {
                    this._port = port;
                }
                get
                {
                    return _port;
                }
            }

            public bool Difference(string name, string ip, string port)
            {
                return !(_name.Equals(name) && _ip.Equals(ip) && _port.Equals(port));
            }

            public override string ToString()
            {
                return this._name + " [" + this._ip + ":" + this._port + "]";
            }
        }

        public Form1()
        {
            discover = ReadIniData("enable", "0");
            InitializeComponent();

            UpButtons[1] = U1;
            UpButtons[2] = U2;
            UpButtons[3] = U3;
            UpButtons[4] = U4;
            UpButtons[5] = U5;
            UpButtons[6] = U6;
            UpButtons[7] = U7;
            UpButtons[8] = U8;
            UpButtons[9] = U9;
            UpButtons[10] = U10;

            DownButtons[1] = D1;
            DownButtons[2] = D2;
            DownButtons[3] = D3;
            DownButtons[4] = D4;
            DownButtons[5] = D5;
            DownButtons[6] = D6;
            DownButtons[7] = D7;
            DownButtons[8] = D8;
            DownButtons[9] = D9;
            DownButtons[10] = D10;

            Colors[0] = Color.Black;
            for (int i = 1; i < 11; i++)
            {
                Colors[i] = UpButtons[i].BackColor;
                UpButtons[i].BackColor = Colors[0];
            }

            Init();
        }

        private void Init()
        {
            if (!System.IO.File.Exists(configPath))
            {
                InitXMLFile();
            }

            doc.Load(configPath);
            root = doc.DocumentElement;

            InitList();
            InitPanel();
        }

        private void InitPanel()
        {
            setConnMessage(null);
            for (int i = 1; i < 11; i++)
            {
                uf[i] = 0;
                df[i] = 0;
                UpButtons[i].BackColor = Colors[0];
                DownButtons[i].BackColor = Colors[0];
            }
            refresh();
        }

        private void InitXMLFile()
        {
            FileStream fs = new FileStream(configPath, FileMode.Create);
            XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
            w.WriteStartDocument();
            w.WriteStartElement("Devices");
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Flush();
            fs.Close();
        }

        private void InitList()
        {
            bindingSource = new BindingSource(deviceList, null);
            listBox1.DataSource = bindingSource;
            listBox1.DisplayMember = "displayName";

            if (discover.Equals("0"))
            {
                panel3.Visible = true;
                XmlNodeList nl = root.SelectNodes("//Device");
                for (int i = 0; i < nl.Count; i++)
                {
                    XmlNode node = nl.Item(i);
                    bindingSource.Add(new DeviceInf(node.Attributes.GetNamedItem("name").InnerText, node.Attributes.GetNamedItem("ip").InnerText, node.Attributes.GetNamedItem("port").InnerText));
                }
            }
            else
            {
                panel5.Visible = true;
                Thread udpListenTread = new Thread(new ThreadStart(StartUDPServer));
                udpListenTread.Start();
                /*
                timer1.Enabled = true;
                timer1.Interval = int.Parse(ReadIniData("interval","5")) * 1000;
                timer1.Start();
                 */
            }
        }

        #region API函数声明

        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);


        #endregion

        #region 读Ini文件

        public static string ReadIniData(string Key, string NoText)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion

        private void refresh()
        {
            setStateMessage(null);

            for (int i = 1; i < 11; i++)
            {
                if (uf[i] != 0)
                {
                    setStateMessage("天线" + (i > 9 ? "" : "0") + i + "已与电台" + ArrayNumInChinese[uf[i]] + "连接" + Environment.NewLine);
                }
            }
        }

        private void upbtn_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (uflag != 0 && uf[uflag] == 0)
            {
                UpButtons[uflag].BackColor = Colors[0];
            }
            uflag = int.Parse(button.Tag.ToString());
            if (uf[uflag] != 0)
            {
                if (MessageBox.Show("是否确定断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    if (MessageBox.Show("是否确定天线" + (uflag > 9 ? "" : "0") + uflag + "与电台" + ArrayNumInChinese[uf[uflag]] + "断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                    {
                        disconnect(uflag);
                        refresh();
                    }
                }
            }
            else
            {
                button.BackColor = Colors[uflag];
            }
        }

        private void downbtn_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            dflag = int.Parse(button.Tag.ToString());

            if (uflag == 0 && df[dflag] == 0) return;

            if (uf[uflag] == 0 && df[dflag] == 0)
            {
                button.BackColor = Colors[uflag];
                if (MessageBox.Show("是否确定天线" + (uflag > 9 ? "" : "0") + uflag + "与电台" + ArrayNumInChinese[dflag] + "连接?", "确定连接", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    connect(uflag, dflag);
                    refresh();
                }
                else
                {
                    UpButtons[uflag].BackColor = Colors[0];
                    DownButtons[dflag].BackColor = Colors[0];
                }
                uflag = 0;
            }
            else
            {
                if (MessageBox.Show("是否确定断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    if (MessageBox.Show("是否确定天线" + (df[dflag] > 9 ? "" : "0") + df[dflag] + "与电台" + ArrayNumInChinese[dflag] + "断开?", "确定断开", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                    {
                        disconnect(df[dflag]);
                        refresh();
                    }
                }
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确定断开所有连接?", "确定断开所有连接", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                for (int i = 1; i < 11; i++)
                {
                    disconnect(i);
                }
                refresh();
            }
        }

        /*
 一共七个字节
Char0----char6

首字节char0代表该板子的地址 均为0x01
末尾字节char6为校验位
Char6=（char0+char1+char2+char3+char4+char5）& 0x7F

Char1—Char5为命令位
  Char1=0x01 关闭供电  01 01 00 00 00 00 02
  Char1=0x02 打开供电  01 02 00 00 00 00 03
  Char1=0x03 关闭所有开关
  Char1=0x04 打开某路开关，此命令后char2：模块号 0x01-0x0A
                                   Char3：该模块中低八位的状态 0关闭 1打开
                                   Char4：该模块中高八位状态
                                   （目前每个模块中为10路）即，char4只用到了2位
Char1=0x05 测试某模块，此命令后char2：模块号 0x01-0x0A 01 05 01 00 00 00 07
                                   Char3：00
                                   Char4：00
                                  
Char1=0x06 停止测试模块，     此命令后char2：00  01 06 00 00 00 00 07
                                   Char3：00
                                   Char4：00

Char1=0x07 Reset，          char2：00 
 Char3：00
                           Char4：00


回复命令：
一共七个字节
Char0----char6
当接受命令完全正确执行时，直接将命令7字节返回。其余状态时：

首字节char0代表该板子的地址 均为0x01
末尾字节char6为校验位
Char6=（char0+char1+char2+char3+char4+char5）& 0x7F

Char1—Char5为错位代码
  Char1=0xFF 错误
，此命令后char2：错误代码号

                                   Char3：0x02 接收校验错误
Char3：0x03 多路打开错误

 01 04 01 80 00 00 06

*/

        private void setConnMessage(string text)
        {
            if (connMessage.InvokeRequired)
            {
                if (text == null)
                {
                    connMessage.Invoke(new MethodInvoker(delegate { connMessage.Text = text; }));
                }
                else
                {
                    connMessage.Invoke(new MethodInvoker(delegate { connMessage.Text += text; }));
                }
                connMessage.Invoke(new MethodInvoker(delegate
                {
                    connMessage.SelectionStart = connMessage.Text.Length;
                    connMessage.ScrollToCaret();
                }));
            }
            else
            {
                if (text == null)
                {
                    connMessage.Text = text;
                }
                else
                {
                    connMessage.Text += text;
                }
                connMessage.SelectionStart = connMessage.Text.Length;
                connMessage.ScrollToCaret();
            }
        }

        private void setStateMessage(string text)
        {
            if (stateMessage.InvokeRequired)
            {
                if (text == null)
                {
                    stateMessage.Invoke(new MethodInvoker(delegate { stateMessage.Text = text; }));
                }
                else
                {
                    stateMessage.Invoke(new MethodInvoker(delegate { stateMessage.Text += text; }));
                }
                stateMessage.Invoke(new MethodInvoker(delegate
                {
                    stateMessage.SelectionStart = stateMessage.Text.Length;
                    stateMessage.ScrollToCaret();
                }));
            }
            else
            {
                if (text == null)
                {
                    stateMessage.Text = text;
                }
                else
                {
                    stateMessage.Text += text;
                }
                stateMessage.SelectionStart = stateMessage.Text.Length;
                stateMessage.ScrollToCaret();
            }
        }

        private void connect(int ant, int radio, bool sendToRemote = true)
        {
            if (sendToRemote && client != null && client.Connected)
            {
                SendData("c" + (ant == 10 ? "a" : ant + "") + (radio == 10 ? "a" : radio + ""));
            }
            else
            {
                setConnMessage("天线" + (ant > 9 ? "" : "0") + ant + "已与电台" + ArrayNumInChinese[radio] + "连接" + Environment.NewLine);
                uf[ant] = radio;
                df[radio] = ant;
                UpButtons[ant].BackColor = Colors[ant];
                DownButtons[radio].BackColor = Colors[ant];
                refresh();
            }

        }

        private void disconnect(int ant, bool sendToRemote = true)
        {
            if (uf[ant] != 0)
            {
                if (sendToRemote && client != null && client.Connected)
                {
                    SendData("d" + (ant == 10 ? "a" : ant + ""));
                }
                else
                {
                    setConnMessage("天线" + (ant > 9 ? "" : "0") + ant + "已与电台" + ArrayNumInChinese[uf[ant]] + "断开" + Environment.NewLine);

                    UpButtons[ant].BackColor = Colors[0];
                    DownButtons[uf[ant]].BackColor = Colors[0];
                    df[uf[ant]] = 0;
                    uf[ant] = 0;
                    uflag = 0;
                    refresh();
                }
            }
        }

        private bool sendCommand(char[] command)
        {
            //command[6] = (char)((command[0] + command[1] + command[2] + command[3] + command[4] + command[5]) & 0x7F);
            return true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sw != null)
            {
                sw.Close();
            }
            if (udpClient != null)
            {
                udpClient.Close();
            }
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 editDialog = new Form2(this);
            editDialog.setIsNew(true);
            editDialog.Show();
        }

        private DeviceInf getCurrentDeviceInf()
        {
            return (DeviceInf)listBox1.SelectedItem;
        }

        public int addDevice(string name, string ip, string port)
        {
            foreach (DeviceInf dev in bindingSource)
            {
                if (dev.name.Equals(name.Trim()))
                {
                    return 1;
                }

                if (dev.ip.Equals(ip))
                {
                    return 2;
                } 
            }
            AddDevice(name,ip,port);
            return 0;
        }

        public void AddDevice(string name, string ip, string port)
        {
            DeviceInf dev = new DeviceInf(name,ip,port);
            bindingSource.Add(dev);
            
            if (discover.Equals("0"))
            {
                XmlElement device = doc.CreateElement("Device");
                device.SetAttribute("name", name);
                device.SetAttribute("ip", ip);
                device.SetAttribute("port", port);
                root.AppendChild(device);
                doc.Save(configPath);
            }
        }

        public int updateDevice(string oldname, string name, string ip, string port)
        {
            foreach (DeviceInf dev in bindingSource)
            {
                if (dev.name.Equals(name) && !dev.name.Equals(oldname))
                {
                    return 1;
                }

                if (dev.ip.Equals(ip) && !dev.name.Equals(oldname))
                {
                    return 2;
                }
            }

            DeviceInf d = getCurrentDeviceInf();
            if (d.Difference(name, ip, port))
            {
                bindingSource.Remove(d);
                d = new DeviceInf(name, ip,port);
                bindingSource.Add(d);
                listBox1.SelectedItem = d;

                if (discover.Equals("0"))
                {
                    XmlNode device = root.SelectSingleNode("/Devices/Device[@name=\"" + oldname + "\"]");
                    device.Attributes.GetNamedItem("name").InnerText = name;
                    device.Attributes.GetNamedItem("ip").InnerText = ip;
                    device.Attributes.GetNamedItem("port").InnerText = port;
                    doc.Save(configPath);
                }
            }
            return 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeviceInf device = (DeviceInf)listBox1.SelectedItem;
            if (device != null)
            {
                Form2 editDialog = new Form2(this);
                editDialog.setInf(device.name, device.ip, device.port);
                editDialog.setIsNew(false);
                editDialog.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeviceInf device = getCurrentDeviceInf();
            if (device != null && MessageBox.Show("确定要从列表中删除设备" + device.displayName + "?", "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (discover.Equals("0"))
                {
                    XmlNode node = root.SelectSingleNode("/Devices/Device[@name=\"" + device.name + "\"]");
                    root.RemoveChild(node);
                    doc.Save(configPath);
                }
                bindingSource.Remove(device);
             }
        }

        private void setPanel(string message)
        {
            for (int i = 1; i < 11; i++)
            {
                string s = message.Substring(i, 1);
                if (s.Equals("a")) s = "10";
                if (!s.Equals("0")) connect(i, int.Parse(s), false);
            }
            refresh();
            if (panel1.InvokeRequired)
            {
                panel1.Invoke(new MethodInvoker(delegate { panel1.Enabled = true; }));
            }
            else
            {
                panel1.Enabled = true;
            }
        }

        private void SendData(string message)
        {
            SendData(encoder.GetBytes(message + "|"));
        }

        private void SendData(byte[] data)
        {
            if (clientStream != null)
            {
                clientStream.Write(data, 0, data.Length);
            }
        }

        delegate void GetData(byte[] data);
        delegate void GetDataLog(byte[] data);

        private void OnGetData(byte[] data)
        {
            string[] resp = encoder.GetString(data).Split('|');
            for (int i = 0; i < resp.Length; i++)
            {
                //状态来了
                if (resp[i].StartsWith("q"))
                {
                    InitPanel();
                    setPanel(resp[i]);
                    continue;
                }
                //连接
                if (resp[i].StartsWith("c"))
                {
                    string ant = resp[i].Substring(1, 1);
                    string radio = resp[i].Substring(2, 1);
                    if (ant.Equals("a")) ant = "10";
                    if (radio.Equals("a")) radio = "10";
                    connect(int.Parse(ant), int.Parse(radio), false);
                    continue;
                }
                //断开
                if (resp[i].StartsWith("d"))
                {
                    string ant = resp[i].Substring(1, 1);
                    if (ant.Equals("a")) ant = "10";
                    disconnect(int.Parse(ant), false);
                    continue;
                }
                //错误信息
                if (resp[i].StartsWith("e"))
                {
                    if (resp[i].Length == 4)
                    {
                        int ant = int.Parse(resp[i].Substring(2, 1));
                        int radio = int.Parse(resp[i].Substring(3, 1));
                        UpButtons[ant].BackColor = Colors[0];
                        DownButtons[radio].BackColor = Colors[0];
                    }
                    MessageBox.Show(errors[int.Parse(resp[i].Substring(1, 1))]);
                }
            }
        }

        private void OnGetDataLog(byte[] data)
        {
            if (tmp == null)
            {
                tmp = new byte[0];
            }
            
            byte[] _data = new byte[tmp.Length + data.Length];
            
            tmp.CopyTo(_data, 0);
            data.CopyTo(_data, tmp.Length);
            
            string resp = Encoding.UTF8.GetString(_data);
            string s = "";
            string[] lines = resp.Split('$');
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i].Length > 0)
                {
                    s += lines[i] + "$";
                    log(lines[i]);
                }
            }
            if (lines[lines.Length - 1].Equals("|"))
            {
                tmp = null;
                log("");
                sw.Flush();
                sw.Close();
                sw = null;
                System.Diagnostics.Process.Start("notepad.EXE ", logFile);
                if (button4.InvokeRequired)
                {
                    button4.Invoke(new MethodInvoker(delegate { button4.Enabled = true; }));
                }
                else
                {
                    button4.Enabled = true;
                }
            }
            else
            {
                byte[] prefix = Encoding.UTF8.GetBytes(s);
                tmp = new byte[_data.Length - prefix.Length];
                Array.Copy(_data, prefix.Length, tmp, 0, tmp.Length); 
            }
        }

        public ManualResetEvent connectDone = new ManualResetEvent(false);
        public ManualResetEvent connectDoneLog = new ManualResetEvent(false);

        private void ConnectCallback(IAsyncResult ar)
        {
            connectDone.Set();
            TcpClient t = (TcpClient)ar.AsyncState;
            try
            {
                if (!t.Connected)
                {
                    DeviceInf dev = getCurrentDeviceInf();
                    MessageBox.Show("无法连接至" + dev.name + "(" + dev.ip + ":" + dev.port + ")");
                }
                t.EndConnect(ar);
            }
            catch (SocketException se)
            {
                //MessageBox.Show("无法连接至" + dev.name + "(" + dev.ip + ":" + dev.port + ")");
            }
        }

        private void ConnectCallbackLog(IAsyncResult ar)
        {
            connectDoneLog.Set();
            TcpClient t = (TcpClient)ar.AsyncState;
            
            try
            {
                t.EndConnect(ar);
            }
            catch (SocketException se)
            {
                //MessageBox.Show("无法连接至" + dev.name + "(" + dev.ip + ":" + (dev.port + 1) + ")");
            }
        }

        private void connect()
        {
            DeviceInf dev = getCurrentDeviceInf();
            if ((client == null) || (!client.Connected))
            {
                try
                {
                    client = new TcpClient();
                    client.ReceiveTimeout = 10;

                    connectDone.Reset();

                    client.BeginConnect(dev.ip, int.Parse(dev.port),
                        new AsyncCallback(ConnectCallback), client);

                    connectDone.WaitOne();

                    if ((client != null) && (client.Connected))
                    {
                        clientStream = client.GetStream();
                        SendData("q");
                        asyncread(client);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("无法连接至" + dev.name + "(" + dev.ip + ":" + dev.port + ")");
                }
            }
        }

        private void connectLog()
        {
            DeviceInf dev = getCurrentDeviceInf();
            int port = int.Parse(dev.port) + 1;

            if ((client_log == null) || (!client_log.Connected))
            {
                try
                {
                    client_log = new TcpClient();
                    client_log.ReceiveTimeout = 10;

                    connectDoneLog.Reset();
                    
                    client_log.BeginConnect(dev.ip, port,
                        new AsyncCallback(ConnectCallbackLog), client_log);

                    connectDoneLog.WaitOne();

                    if ((client_log != null) && (client_log.Connected))
                    {
                        clientStream_log = client_log.GetStream();
                        byte[] data = encoder.GetBytes("v");
                        clientStream_log.Write(data,0,data.Length);
                        asyncreadLog(client_log);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("无法连接至" + dev.name + "(" + dev.ip + ":" + port + ")");
                }
            }
        }

        private void DisConnect()
        {
            if ((client != null) && (client.Connected))
            {
                clientStream.Close();
                client.Close();
            }
        }

        private void DisConnectLog()
        {
            if ((client_log != null) && (client_log.Connected))
            {
                clientStream_log.Close();
                client_log.Close();
            }
        }

        private void asyncread(TcpClient sock)
        {
            StateObject state = new StateObject();
            state.client = sock;
            NetworkStream stream = sock.GetStream();

            if (stream.CanRead)
            {
                try
                {
                    IAsyncResult ar = stream.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBack), state);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Network IO problem " + e.ToString());
                }
            }
        }

        private void asyncreadLog(TcpClient sock)
        {
            StateObject state = new StateObject();
            state.client = sock;
            NetworkStream stream = sock.GetStream();

            if (stream.CanRead)
            {
                try
                {
                    IAsyncResult ar = stream.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBackLog), state);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Network IO problem " + e.ToString());
                }
            }
        }

        private void TCPReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            if ((state.client == null) || (!state.client.Connected))
                return;
            int numberOfBytesRead;
            NetworkStream mas = state.client.GetStream();
            //string type = null;

            try
            {
                numberOfBytesRead = mas.EndRead(ar);
                state.totalBytesRead += numberOfBytesRead;

                if (numberOfBytesRead > 0)
                {
                    byte[] dd = new byte[numberOfBytesRead];
                    Array.Copy(state.buffer, 0, dd, 0, numberOfBytesRead);
                    OnGetData(dd);
                    mas.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBack), state);
                }
                else
                {
                    mas.Close();
                    state.client.Close();
                    mas = null;
                    state = null;
                }
            }
            catch (Exception exception)
            {
                mas.Close();
                state.client.Close();
                mas = null;
                state = null;
                DisConnect();
                MessageBox.Show("连接断开 (" + exception.Message + ")");
            }
        }

        private void TCPReadCallBackLog(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            if ((state.client == null) || (!state.client.Connected))
                return;
            int numberOfBytesRead;
            NetworkStream mas = state.client.GetStream();
            //string type = null;

            try
            {
                numberOfBytesRead = mas.EndRead(ar);
                state.totalBytesRead += numberOfBytesRead;

                if (numberOfBytesRead > 0)
                {
                    byte[] dd = new byte[numberOfBytesRead];
                    Array.Copy(state.buffer, 0, dd, 0, numberOfBytesRead);
                    OnGetDataLog(dd);
                    mas.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBackLog), state);
                }
                else
                {
                    mas.Close();
                    state.client.Close();
                    mas = null;
                    state = null;
                }
            }
            catch (Exception exception)
            {
                mas.Close();
                state.client.Close();
                mas = null;
                state = null;
                DisConnectLog();
                MessageBox.Show("连接断开 (" + exception.Message + ")");
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                DisConnect();
                connect();
            }
            else
            {
                InitPanel();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清除目标终端日志?", "确认清除日志", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                SendData("cl");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            DisConnectLog();
            connectLog();
        }

        private void log(string txt)
        {
            if (sw == null)
            {
                sw = File.CreateText(logFile);
            }
            sw.WriteLine(txt);
        }

        private void StartUDPServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, BROADCAST_RECEIVE_PORT);
            socket.Bind(iep);

            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(ReadIniData("broadcast", "255.255.255.255")), BROADCAST_SEND_PORT);

            DiscoverDevices();

            EndPoint ep = (EndPoint)iep;

            while (true)
            {
                byte[] bytes = new byte[1024];
                socket.ReceiveFrom(bytes, ref ep);
                string receiveData = Encoding.UTF8.GetString(bytes);
                receiveData = receiveData.TrimEnd('\u0000');
                //MessageBox.Show(receiveData);
                string ip = ((IPEndPoint)ep).Address + "";
                string[] inf = receiveData.Split('|');
                AddDevice(inf[0], ip, inf[1]);
                if (listBox1.InvokeRequired)
                {
                    listBox1.Invoke(new MethodInvoker(delegate { listBox1.DataSource = null; listBox1.DataSource = bindingSource; }));
                }
            }
            //socket.Close();　
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeviceInf dev = getCurrentDeviceInf();
            if (dev != null)
            {
                toolTip1.SetToolTip(listBox1, dev.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DiscoverDevices();    
        }

        private void DiscoverDevices()
        {
            try
            {
                bindingSource.Clear();
                udpClient.Send(findBytes, findBytes.Length);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DiscoverDevices();
        }
    }


    internal class StateObject
    {
        public TcpClient client = null;
        public int totalBytesRead = 0;
        public const int BufferSize = 1024;
        public string readType = null;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder messageBuffer = new StringBuilder();
    }
}
