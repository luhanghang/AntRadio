using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using System.Collections;

namespace cui
{
    public partial class Form1 : Form
    {
        private static String[] ArrayNumInChinese = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        private static Color[] Colors = new Color[11];
        private static String iniFilePath = Application.StartupPath + "\\config.ini";
        private static String logFile = Application.StartupPath + "\\log.txt";
        private static String Section = "Connections";
        private static string[] MODEL = { "天线", "电台" };
        private static int ANT = 0;
        private static int RADIO = 1;
        private static int ERROR_NO = 0; //无错误
        private static int ERROR_COM = 1; //无法打开串口

        private static int BROADCAST_SEND_PORT = 65431;
        private static int BROADCAST_RECEIVE_PORT = 13456;

        int[] uf = new int[11];
        int[] df = new int[11];

        int uflag;
        int dflag;

        private Button[] UpButtons = new Button[11];
        private Button[] DownButtons = new Button[11];

        private TcpListener tcpListener;
        private Thread listenThread;

        private TcpListener tcpListener_log;
        private Thread listenThread_log;

        StreamWriter sw;

        TcpClient tcpClient;
        NetworkStream clientStream;

        TcpClient tcpClient_log;
        NetworkStream clientStream_log;

        ASCIIEncoding encoder = new ASCIIEncoding();
        byte[] message = new byte[16];

        int port;

        SerialPort[] sp = { new SerialPort(), new SerialPort() };

        static string[] errors = { null, null, "接收校验错误", "多路打开错误" };

        //int errorCode = 0;

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

        #region 写Ini文件

        public static bool WriteIniData(string Key, string Value)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);

                return OpStation != 0;
            }
            return false;
        }

        #endregion

        public Form1()
        {
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

            if (initSerial()) Init();

            port = int.Parse(ReadIniData("port", "12345"));

            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            this.tcpListener_log = new TcpListener(IPAddress.Any, port + 1);
            this.listenThread_log = new Thread(new ThreadStart(ListenForClients_Log));
            this.listenThread_log.Start();

            Thread udpListenTread = new Thread(new ThreadStart(StartUDPServer));
            udpListenTread.Start();
        }

        private bool initSerial()
        {
            sp[ANT].PortName = ReadIniData("model1", "COM3");
            sp[ANT].BaudRate = 9600;
            sp[ANT].DataBits = 8;
            sp[ANT].StopBits = System.IO.Ports.StopBits.One;

            sp[RADIO].PortName = ReadIniData("model2", "COM5");
            sp[RADIO].BaudRate = 9600;
            sp[RADIO].DataBits = 8;
            sp[RADIO].StopBits = System.IO.Ports.StopBits.One;

            //打开供电
            byte[] command = { (byte)0x01, (byte)0x02, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0 };

            int errorCodeAnt = sendCommand(command, ANT);
            int errorCodeRadio = sendCommand(command, RADIO);

            if (errorCodeAnt != ERROR_NO || errorCodeRadio != ERROR_NO)
            {
                string action = "当打开供电时";
                handleSendCommandErrors(errorCodeAnt, errorCodeRadio, false, false, action);
                return false;
            }

            //先关闭所有开关
            command[1] = (byte)0x03;

            errorCodeAnt = sendCommand(command, ANT);
            errorCodeRadio = sendCommand(command, RADIO);

            if (errorCodeAnt != ERROR_NO || errorCodeRadio != ERROR_NO)
            {
                string action = "当关闭所有开关时";
                handleSendCommandErrors(errorCodeAnt, errorCodeRadio, false, false, action);
                return false;
            }
            return true;
            //sp.DataReceived += new SerialDataReceivedEventHandler(SerialReadData);
        }

        private string getName()
        {
            return ReadIniData("name", "noname");
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void ListenForClients_Log()
        {
            this.tcpListener_log.Start();

            while (true)
            {
                TcpClient client = this.tcpListener_log.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm_Log));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            tcpClient = (TcpClient)client;
            clientStream = tcpClient.GetStream();

            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, message.Length);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                //命令间通过‘|’隔开
                string[] cmd = encoder.GetString(message, 0, bytesRead).Split('|');

                for (int i = 0; i < cmd.Length; i++)
                {
                    //发来"cl"代表清除日志
                    if (cmd[i].Equals("cl"))
                    {
                        ClearLog(true);
                        continue;
                    }

                    //发来一个"q"代表查询当前状态
                    if (cmd[i].Equals("q"))
                    {
                        sendStatesToRemote();
                        continue;
                    }
                    //以"c"开头代表连接，格式cmn, c后面2位，代表天线m与n连接，10用a代替
                    //如c13代表天线01与电台三连接, ca9代表天线10与电台九连接
                    if (cmd[i].StartsWith("c"))
                    {
                        string ant = cmd[i].Substring(1, 1);
                        string radio = cmd[i].Substring(2, 1);
                        if (ant.Equals("a")) ant = "10";
                        if (radio.Equals("a")) radio = "10";
                        connect(int.Parse(ant), int.Parse(radio), true);
                        continue;
                    }
                    //以"d"开头代表断开,格式dm, d后面一位，代表断开与天线m连接的电台,10用a代替
                    //如d8代表断开与天线08连接的电台,da代表断开与天线10连接的电台
                    if (cmd[i].StartsWith("d"))
                    {
                        string ant = cmd[i].Substring(1, 1);
                        if (ant.Equals("a")) ant = "10";
                        disconnect(int.Parse(ant), true);
                    }
                }
            }

            tcpClient.Close();
        }

        private void HandleClientComm_Log(object client)
        {
            tcpClient_log = (TcpClient)client;
            clientStream_log = tcpClient_log.GetStream();

            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream_log.Read(message, 0, message.Length);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                //string resp = encoder.GetString(message, 0, bytesRead);
                SendLog();
            }

            tcpClient_log.Close();
        }

        /*
         * 发送当前连接状态
         * qxxxxxxxxxx,q后面10位，第n位的x代表天线n与电台x已连接，代表未连接,10用a代替
         * 如q23a0001000,代表天线01与电台二连接，天线02与电台三连接，天线03与电台十连接，天线07与电台一连接，天线04,05,06,08,09,10未连接电台
         */
        private void sendStatesToRemote()
        {
            SendData("q" + getStates());
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

        /*
         * 返回当前连接状态
         */
        private string getStates()
        {
            string states = "";
            for (int i = 1; i < 11; i++)
            {
                states += uf[i] == 10 ? "a" : uf[i] + "";
            }
            return states;
        }

        private void Init()
        {
            for (int i = 1; i < 11; i++)
            {
                //初始化连接关系
                uf[i] = 0;
                df[i] = 0;
                //恢复上次连接状态
                int dest = int.Parse(ReadIniData(i + "", "0"));
                if (dest > 0)
                {
                    connect(i, dest);
                }
            }
            refresh();
        }

        //显示连接状态
        private void refresh()
        {
            setStateMessage(null);

            for (int i = 1; i < 11; i++)
            {
                if (uf[i] != 0)
                {
                    setStateMessage("天线" + (i > 9 ? "" : "0") + i + "已与电台" + ArrayNumInChinese[uf[i]] + "连接");
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
                //如果已连接就准备断开连接
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
                //如果未连接就准备连接
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
                    connMessage.Invoke(new MethodInvoker(delegate { connMessage.Text += text + Environment.NewLine; }));
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
                    connMessage.Text += text + Environment.NewLine;
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
                    stateMessage.Invoke(new MethodInvoker(delegate { stateMessage.Text += text + Environment.NewLine; }));
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
                    stateMessage.Text += text + Environment.NewLine;
                }
                stateMessage.SelectionStart = stateMessage.Text.Length;
                stateMessage.ScrollToCaret();
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

        private void connect(int ant, int radio, bool fromRemote = false)
        {
            //天线模块命令
            byte[] ant_command = new byte[7];
            ant_command[0] = (byte)0x01;
            ant_command[1] = (byte)0x04;
            ant_command[2] = (byte)ant;
            if (radio < 9)
            {
                ant_command[3] = (byte)(1 << (radio - 1));
                ant_command[4] = (byte)0;
            }
            else
            {
                ant_command[3] = (byte)0;
                ant_command[4] = (byte)(1 << (radio - 9));
            }

            //电台模块命令
            byte[] radio_command = new byte[7];
            radio_command[0] = (byte)0x01;
            radio_command[1] = (byte)0x04;
            radio_command[2] = (byte)radio;
            if (ant < 9)
            {
                radio_command[3] = (byte)(1 << (ant - 1));
                radio_command[4] = (byte)0;
            }
            else
            {
                radio_command[3] = (byte)0;
                radio_command[4] = (byte)(1 << (ant - 9));
            }

            int errorCodeAnt = sendCommand(ant_command, ANT);
            int errorCodeRadio = sendCommand(radio_command, RADIO);

            bool sendToRemote = tcpClient != null && tcpClient.Connected;

            if (errorCodeAnt == ERROR_NO && errorCodeRadio == ERROR_NO)
            {
                string txt = "天线" + (ant > 9 ? "" : "0") + ant + "已与电台" + ArrayNumInChinese[radio] + "连接";
                setConnMessage(txt);
                uf[ant] = radio;
                df[radio] = ant;
                UpButtons[ant].BackColor = Colors[ant];
                DownButtons[radio].BackColor = Colors[ant];
                WriteIniData(ant + "", radio + "");
                refresh();
                if (sendToRemote)
                {
                    SendData("c" + (ant == 10 ? "a" : ant + "") + (radio == 10 ? "a" : radio + ""));
                }
                log(txt, fromRemote);
            }
            else
            {
                UpButtons[ant].BackColor = Colors[0];
                DownButtons[radio].BackColor = Colors[0];
                WriteIniData(ant + "", "0");
                string action = "当连接天线" + (ant > 9 ? "" : "0") + ant + "与电台" + ArrayNumInChinese[radio] + "时";
                handleSendCommandErrors(errorCodeAnt, errorCodeRadio, sendToRemote, fromRemote, action, (ant == 10 ? "a" : ant + "") + (radio == 10 ? "a" : radio + ""));
            }
        }

        private void log(string txt, bool isRemote)
        {
            string content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + (isRemote ? " (远程)" : " ") + txt;
            if (sw == null)
            {
                if (File.Exists(logFile))
                {
                    sw = File.AppendText(logFile);
                }
                else
                {
                    sw = File.CreateText(logFile);
                }
            }
            sw.WriteLine(content);
        }

        private void disconnect(int ant, bool fromRemote = false)
        {
            if (uf[ant] != 0)
            {
                byte[] ant_command = { (byte)0x01, (byte)0x04, (byte)ant, (byte)0, (byte)0, (byte)0, (byte)0 };
                byte[] radio_command = { (byte)0x01, (byte)0x04, (byte)uf[ant], (byte)0, (byte)0, (byte)0, (byte)0 };

                int errorCodeAnt = sendCommand(ant_command, ANT);
                int errorCodeRadio = sendCommand(radio_command, RADIO);

                bool sendToRemote = tcpClient != null && tcpClient.Connected;

                if (errorCodeAnt == ERROR_NO && errorCodeRadio == ERROR_NO)
                {
                    string txt = "天线" + (ant > 9 ? "" : "0") + ant + "已与电台" + ArrayNumInChinese[uf[ant]] + "断开";
                    setConnMessage(txt);

                    UpButtons[ant].BackColor = Colors[0];
                    DownButtons[uf[ant]].BackColor = Colors[0];
                    df[uf[ant]] = 0;
                    uf[ant] = 0;
                    uflag = 0;
                    WriteIniData(ant + "", "0");
                    refresh();
                    log(txt, fromRemote);
                    if (sendToRemote)
                    {
                        SendData("d" + (ant == 10 ? "a" : ant + ""));
                    }
                }
                else
                {
                    string action = "当断开天线" + (ant > 9 ? "" : "0") + ant + "与电台" + ArrayNumInChinese[uf[ant]] + "时";
                    handleSendCommandErrors(errorCodeAnt, errorCodeRadio, sendToRemote, fromRemote, action);
                }
            }
        }

        private void handleSendCommandErrors(int errorCodeAnt, int errorCodeRadio, bool sendToRemote, bool fromRemote, string action, string ext = "")
        {
            string prefix = "[错误][" + action + "]";
            string errorTxt;
            if (errorCodeAnt != ERROR_NO)
            {
                if (errorCodeAnt == ERROR_COM)
                {
                    errorTxt = "串口" + sp[ANT].PortName + "打开失败";
                }
                else
                {
                    errorTxt = MODEL[ANT] + "模块错误(" + errorCodeAnt + "):" + errors[errorCodeAnt];
                }
                log(prefix + errorTxt, fromRemote);
                if (sendToRemote)
                {
                    //发送错误信息，格式ex,x为错误代码，一位，如果是连接错误，格式为exmn,x为错误代码,m为天线,n为电台
                    //如e13a,代表当天线03与电台十连接时发生错误，错误代码为1
                    SendData("e" + errorCodeAnt + ext);
                }
                else
                {
                    MessageBox.Show(errorTxt);
                }
            }
            if (errorCodeRadio != ERROR_NO)
            {
                if (errorCodeRadio == ERROR_COM)
                {
                    errorTxt = "串口" + sp[RADIO].PortName + "打开失败";
                }
                else
                {
                    errorTxt = MODEL[RADIO] + "模块错误(" + errorCodeRadio + "):" + errors[errorCodeRadio];
                }
                log(prefix + errorTxt, fromRemote);
                if (sendToRemote)
                {
                    SendData("e" + errorCodeRadio + ext);
                }
                else
                {
                    MessageBox.Show(errorTxt);
                }
            }
        }

        private int sendCommand(byte[] command, int model)
        {
            /*
            command[6] = (byte)((command[0] + command[1] + command[2] + command[3] + command[4] + command[5]) & 0x7F);
            string c = "";
            for (int i = 0; i < 7; i++)
            {
                c += "." + (int)command[i];
            }
            MessageBox.Show(c);
            */

            //return ERROR_NO;
            //if (model == RADIO) return ERROR_NO;

            if (!sp[model].IsOpen)
            {
                try
                {
                    sp[model].Open();
                }
                catch (Exception ex)
                {
                    CloseSerials();
                    return ERROR_COM;
                }
            }

            //第七位为校验位
            command[6] = (byte)((command[0] + command[1] + command[2] + command[3] + command[4] + command[5]) & 0x7F);
            sp[model].Write(command, 0, command.Length);

            Thread.Sleep(500);

            int bytes = sp[model].BytesToRead;
            byte[] buffer = new byte[bytes];
            sp[model].Read(buffer, 0, bytes);
            if (buffer.Length == 0 || buffer[1] != 0xFF)
            {
                buffer = null;
                return ERROR_NO;
            }

            int errorCode = buffer[2];
            buffer = null;
            return errorCode;
        }

        private void CloseSerials()
        {
            if (sp[ANT].IsOpen) sp[ANT].Close();
            if (sp[RADIO].IsOpen) sp[RADIO].Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定退出应用程序?", "确认退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                if (sw != null)
                {
                    try
                    {
                        sw.Flush();
                    }
                    catch (Exception ex)
                    { }
                    sw.Close();
                    sw = null;
                }
                CloseSerials();
                Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清除日志?", "确认清除日志", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                ClearLog();
            }
        }

        private void ClearLog(bool remote = false)
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
            sw = File.CreateText(logFile);
            log("清除日志", remote);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sw != null)
            {
                try
                {
                    sw.Flush();
                }
                catch (Exception ex)
                {
                }
                sw.Close();
                sw = null;
            }
            System.Diagnostics.Process.Start("notepad.EXE ", logFile);
        }

        private void SendLog()
        {
            if (sw != null)
            {
                try
                {
                    sw.Flush();
                }
                catch (Exception ex)
                {
                }
                sw.Close();
                sw = null;
            }
            StreamReader objReader = new StreamReader(logFile);
            byte[] data = null;
            data = Encoding.UTF8.GetBytes("$");
            clientStream_log.Write(data, 0, data.Length);
            string sLine;
            while ((sLine = objReader.ReadLine()) != null)
            {
                data = Encoding.UTF8.GetBytes(sLine + "$");
                clientStream_log.Write(data, 0, data.Length);
            }
            objReader.Close();
            data = Encoding.UTF8.GetBytes("|");
            clientStream_log.Write(data,0,data.Length);
        }

        private void StartUDPServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, BROADCAST_RECEIVE_PORT);
            socket.Bind(iep);
            EndPoint ep = (EndPoint)iep;

            while(true)
            {
                byte[] bytes = new byte[1024];
                socket.ReceiveFrom(bytes, ref ep);
                string receiveData = encoder.GetString(bytes);
                receiveData = receiveData.TrimEnd('\u0000');
                if (receiveData.Equals("*find*"))
                {
                    UdpClient udpClient = new UdpClient();
                    try
                    {
                        udpClient.Connect(((IPEndPoint)ep).Address, BROADCAST_SEND_PORT);
                        Byte[] sendBytes = Encoding.UTF8.GetBytes(getName() + "|" + port);
                        udpClient.Send(sendBytes, sendBytes.Length);
                        udpClient.Close();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            //socket.Close();　
        }
    }
}
