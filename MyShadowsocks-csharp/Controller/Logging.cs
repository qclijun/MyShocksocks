using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using Shadowsocks.Util;

namespace Shadowsocks.Controller
{
    class Logging
    {
        private static string logFilePath;
        private static FileStream _fs;
        private static StreamWriterWithTimestamp _sw;
        
        public static bool OpenLogFile()
        {
            try
            {
                logFilePath = Utils.GetTempPath("shadowsocks.log");

                _fs = new FileStream(logFilePath, FileMode.Append);
                _sw = new StreamWriterWithTimestamp(_fs);
                _sw.AutoFlush = true;
                Console.SetOut(_sw);
                Console.SetError(_sw);
                return true;
            }catch(IOException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        private static void WriteToLogFile(object o)
        {
            try
            {
                Console.WriteLine(o);
            }
            catch (ObjectDisposedException) { }
        }

        public static void Error(object o)
        {
            WriteToLogFile("[E] " + o);
        }

        public static void Info(object o)
        {
            WriteToLogFile(o);
        }

        public static void Clear()
        {
            _sw.Close();
            _sw.Dispose();
            _fs.Close();
            _fs.Dispose();
            File.Delete(logFilePath);
            OpenLogFile();
        }

        [Conditional("DEBUG")]
        public static void Debug(object o)
        {
            WriteToLogFile("[D] " + o);
        }

        [Conditional("DEBUG")]
        public static void Debug(EndPoint local, EndPoint remote,int len, string header =null,
            string tailer = null)
        {
            StringBuilder sb = new StringBuilder();
            if (header != null)
                sb.Append(header).Append(": ");
            sb.Append(local).Append(" => ").Append(remote).Append("(size=").Append(len).Append(")");
            if (tailer != null)
                sb.Append(", ").Append(tailer);


            Debug(sb.ToString());
        }

        [Conditional("DEBUG")]
        public static void Debug(System.Net.Sockets.Socket sock,int len, string header=null,string tailer = null)
        {
            Debug(sock.LocalEndPoint, sock.RemoteEndPoint, len, header, tailer);
        }

        public static void LogUsefulException(Exception e)
        {
            if(e is SocketException)
            {
                SocketException se = (SocketException)e;

                switch (se.SocketErrorCode)
                {
                    case SocketError.ConnectionAborted:
                        break;
                    case SocketError.ConnectionReset:
                        break;
                    case SocketError.NotConnected:
                        break;
                    case SocketError.HostUnreachable:
                        break;
                    case SocketError.TimedOut:           
                        break;
                    default:
                        Info(e);
                        break;
                }
            }else if(e is ObjectDisposedException)
            {

            }
            else
            {
                Info(e);
            }
        }

    }

    class StreamWriterWithTimestamp : StreamWriter
    {
        public StreamWriterWithTimestamp(Stream stream) : base(stream)
        {

        }

        private string GetTimestamp()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(GetTimestamp() + value);
        }

        public override void Write(string value)
        {
            base.WriteLine(GetTimestamp() + value);
        }
    }
}
