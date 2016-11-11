using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

namespace DrugsQR
{
    public partial class Form1 : Form
    {
        private BootParameter bootParameter;
        
        public Form1()
        {
            InitializeComponent();

        }

        public Form1(BootParameter bootParameter)
        {
            this.bootParameter = bootParameter;
            if (string.Empty == this.bootParameter.Host || null == this.bootParameter.Host)
            {
                this.bootParameter.Host = "118.26.131.19";
            }
            if (0 == this.bootParameter.Port) { this.bootParameter.Port = 8885; }
            InitializeComponent();

        }

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        /*
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
         * */
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private const int WM_CHAR = 0X102;
        private byte[] result = new byte[1024];
        private int myProt = 8885;   //端口  

        //private string serverHost = "www.jhxmsy.com";
        //private string serverHost = "118.26.131.19";
        //private string serverHost = "127.0.0.1";

        //private int serverProt = 8886;   //端口 
        //private byte[] recBuffer = new byte[2048];

        private Socket serverSocket;
        private string fLoginUri = @"http://www.jhxmsy.com:8080/drugs/winform/login";

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible==false){
                this.notifyIcon1.Visible = true;
                this.notifyIcon1.ShowBalloonTip(30, "消息", "登录成功", ToolTipIcon.Info);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.button1.Visible = false;
            this.button2.Visible = false;
            this.button3.Visible = false;
            this.notifyIcon1.Visible = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void tsmQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtUID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                txtPWD.Focus();
            }
        }
        private void txtPWD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //doLogin();
                doLogin1();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //doLogin();
            doLogin1();
        }

        private void doLogin1()
        {
            if (txtPWD.Text == string.Empty || txtUID.Text == string.Empty)
            {
                MessageBox.Show("请输入用户名及密码");
                return;
            }

            IDictionary<string, string> paramters = new Dictionary<string, string>();
            paramters.Add("uname", txtUID.Text);
            paramters.Add("pwd", txtPWD.Text);

            //IPEndPoint ipe = new IPEndPoint(ip, port);
            
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("118.26.131.19"), 8886);
            tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 30000, 10000), null);//设置Keep-Alive参数
            //NetworkStream ns = tcpClient.GetStream();
            LoginCmd rc = new LoginCmd();
            rc.action = "slogin";
            rc.uname = txtUID.Text;
            rc.pwd = txtPWD.Text;
            String loginString=ToJson(rc);
            //String loginString = (new JavaScriptSerializer()).Serialize(rc);

            Byte[] sendBytes = Encoding.UTF8.GetBytes(loginString);
            tcpClient.Client.Send(sendBytes);
            byte[] recBuffer = new byte[2048];
            int receiveNumber = tcpClient.Client.Receive(recBuffer);
            String resultString = Encoding.UTF8.GetString(recBuffer, 0, receiveNumber);
            Result result = Parse<Result>(resultString);
            if (result.failed)
            {
                MessageBox.Show(result.statusText);
                return;
            }
            Thread receiveThread = new Thread(ReceiveMessage1);
            receiveThread.IsBackground = true;
            receiveThread.Start(tcpClient.Client);
            this.Visible = false;
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="clientSocket"></param>  
        private void ReceiveMessage1(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    byte[] recBuffer = new byte[2048];
                    int receiveNumber = myClientSocket.Receive(recBuffer);
                    //Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));
                    //MessageBox.Show(Encoding.UTF8.GetString(result, 0, receiveNumber));
                    HandleMessage(Encoding.UTF8.GetString(recBuffer, 0, receiveNumber));
                }
                catch (Exception ex)
                {
                    /*
                    Console.WriteLine(ex.Message);  
                    myClientSocket.Shutdown(SocketShutdown.Both);  
                    myClientSocket.Close();  
                    break;  
                     * */
                    MessageBox.Show("系统异常！程序将退出" + ex.Message);
                    Application.Exit();
                }
            }
        }

        private void doLogin()
        {
            if (txtPWD.Text == string.Empty || txtUID.Text == string.Empty)
            {
                MessageBox.Show("请输入用户名及密码");
                return;
            }

            IDictionary<string, string> paramters = new Dictionary<string, string>();
            paramters.Add("account", txtUID.Text);
            paramters.Add("password", txtPWD.Text);
            paramters.Add("port", myProt.ToString());

            String resultString = SendPost(fLoginUri, paramters);
            //MessageBox.Show(resultString);
            Result result = Parse<Result>(resultString);
            if (result.failed)
            {
                MessageBox.Show(result.statusText);
                return;
            }
            StartListen();
            this.Visible = false;
        }
        public string SendGet(string url, IDictionary<String, String> paramters)
        {
            string queryString = "&";
            foreach (string key in paramters.Keys)
            {
                queryString += key + "=" + paramters[key] + "&";
            }
            queryString = queryString.Substring(0, queryString.Length - 1);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + queryString);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;//20s 超时
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            httpWebResponse.Close();
            streamReader.Close();
            return responseContent;
        }

        public string SendPost(string url, IDictionary<String, String> paramters)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in paramters)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            //httpWebRequest.ContentLength = data.Length;  
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;//20s 超时
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();
            httpWebResponse.Close();
            streamReader.Close();
            return responseContent;
        }




        public void StartListen()
        {
            //服务器IP地址  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求  
            //Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.IsBackground = true;
            myThread.Start();
            //Console.ReadLine();  
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));  
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.IsBackground = true;
                receiveThread.Start(clientSocket);
            }
        }

       

        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="clientSocket"></param>  
        private void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    //Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(result, 0, receiveNumber));
                    //MessageBox.Show(Encoding.UTF8.GetString(result, 0, receiveNumber));
                    HandleMessage(Encoding.UTF8.GetString(result, 0, receiveNumber));
                }
                catch (Exception ex)
                {
                    /*
                    Console.WriteLine(ex.Message);  
                    myClientSocket.Shutdown(SocketShutdown.Both);  
                    myClientSocket.Close();  
                    break;  
                     * */
                    MessageBox.Show("系统异常！程序将退出"+ex.Message);
                    Application.Exit();
                }
            }
        }

      


        private void HandleMessage(string input)
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;
            Point p = new Point(x, y);
            IntPtr formHandle = WindowFromPoint(p);
            InputStr(formHandle, input);
        }

        

        public void InputStr(IntPtr myIntPtr, string message)
        {
            byte[] ch = (ASCIIEncoding.Default.GetBytes(message));
            for (int i = 0; i < ch.Length; i++)
            {
                SendMessage(myIntPtr, WM_CHAR, ch[i], 0);
            }
            SendMessage(myIntPtr, 256, 0xD, 0);// 发送回车键盘
            //SendMessage(myIntPtr, 257, 0xD, 0);
            SendMessage(myIntPtr, WM_CHAR, 13, 0);
            //SendMessage(handle, WM_KEYDOWN, VK_RETURN, 0);
        }

        public string SendPost(string uri, string paramStr, Encoding encoding)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            string result = string.Empty;

            WebClient wc = new WebClient();

            // 采取POST方式必须加的Header
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] postData = encoding.GetBytes(paramStr);



            byte[] responseData = wc.UploadData(uri, "POST", postData); // 得到返回字符流
            return encoding.GetString(responseData);// 解码                  
        }


        public T Parse<T>(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }

        public string ToJson(object jsonObject)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IDictionary<string, string> paramters = new Dictionary<string, string>();
            paramters.Add("uid", "uid");
            paramters.Add("qr", "药品编号");
            paramters.Add("port", myProt.ToString());

            String str = SendPost(@"http://127.0.0.1:8080/main?action=mqrupload", "uid=uid&qr=药品编码", null);
            Result result = Parse<Result>(str);
            MessageBox.Show(str);

            //Thread.Sleep(5000);
            // handle(str);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = "中国人名";
            Thread.Sleep(5000);
            HandleMessage(str);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 开启线程不停的 发送 扫码信息
            Thread t = new Thread(upload);
            t.IsBackground = true;
            t.Start();
        }

        private void upload()
        {
            while (true)
            {
                SendPost(@"http://127.0.0.1:8080/main?action=mqrupload", "uid=uid&qr=药品编码", null);
                Thread.Sleep(3000);
            }
        }

        

       

       

       
        

    

        

       
        

        


    }


}
