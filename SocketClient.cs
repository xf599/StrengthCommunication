using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lemony.SystemInfo;
using Monitoring.Service.CodeResolve.TaskAssigns.HelperClass;

namespace Monitoring.Service.CodeResolve.TaskAssigns
{
    /// <summary>
    /// socket监听客户端
    /// </summary>
    public class SocketClient
    {
        private byte[] result = new byte[1024 * 1024];
        private int myProt = 8885;   //端口 
        private string ipstr = "127.0.0.1";

        private Socket serverSocket;

        public SocketClient(int myProt = 8885, string ipstr = "127.0.0.1")
        {
            this.myProt = myProt;
            this.ipstr = ipstr;
        }
        /// <summary>
        /// 开启客户端，如果处于未连接状态，会持续间隔2秒与主服务端连接一次
        /// </summary>
        public void Client_Strate()
        {
            Thread myThread = new Thread(Client_Connect);
            myThread.Start();
        }

        public void Client_Connect()
        {

            while (true)
            {
                Thread.Sleep(2000);    //等待2秒钟  
                                       //设定服务器IP地址  
                IPAddress ip = IPAddress.Parse(ipstr);
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    clientSocket.Connect(new IPEndPoint(ip, myProt)); //配置服务器IP与端口  
                                                                      // Console.WriteLine("连接服务器成功");
                }
                catch
                {
                    // Console.WriteLine("连接服务器失败，请按回车键退出！");
                    //return;
                    continue;
                }
                //连接成功 监听服务端发送数据
                //通过clientSocket接收数据  
                int receiveNumber = clientSocket.Receive(result);
                string resultMsg = Encoding.ASCII.GetString(result, 0, receiveNumber);
                resultMsg = System.Web.HttpUtility.UrlDecode(resultMsg);
                Console.WriteLine(resultMsg);
                serverSocket = clientSocket;
                Thread myThread2 = new Thread(Client_Connect2);
                myThread2.Start(clientSocket);
                break;
            }
        }
        public void Client_Connect2(object o)
        {
            Socket clientSocket = (Socket)o;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = clientSocket.Receive(result);
                    string resultMsg = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    resultMsg = System.Web.HttpUtility.UrlDecode(resultMsg);
                    //接收到以后做操作
                    if (CountDown != null)
                    {
                        CountDown(clientSocket, resultMsg);
                    }
                    else
                    {
                        Console.WriteLine("客户端没有命令处理事件");
                    }
                }
                catch (Exception ex)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    serverSocket = null;
                    Console.WriteLine("客户端接收消息时异常，中断了一次通讯:" + ex.Message);
                    break;
                }
            }
            Thread myThread = new Thread(Client_Connect);
            myThread.Start();
        }


        /// <summary>
        /// 向服务端发送消息
        /// </summary>
        /// <param name="sendMsg">命令字符</param>
        public void SendServerMsg(ServerBetweenClient<object> sendMsg)
        {
            try
            {
                if(serverSocket==null)
                {
                    throw new Exception("没有连接服务端");
                }
                string msg = JsonConvert.SerializeObject(sendMsg);
                msg = System.Web.HttpUtility.UrlEncode(msg);
                serverSocket.Send(Encoding.ASCII.GetBytes(msg));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 客户端处理命令的委托
        /// </summary>
        /// <returns></returns>
        public delegate void ClientHandleDelegate(Socket myServerSocket, string commandMsgServer);
        /// <summary>
        /// 处理事件
        /// </summary>
        public event ClientHandleDelegate CountDown;



    }
}
