using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

namespace SuUtil
{
    public abstract class Util
    {
        /// <summary>
        /// XOR校验和
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte CheckSum(byte[] source)
        {
            if(source.Length==0)
            {
                throw new Exception("Byte array is empty");
            }

            byte temp = source[0];

            for(int i =1; i<source.Length; i++)
            {
                temp ^= source[i];
            }

            return temp;
        }

        /// <summary>
        /// test wheather com is ready
        /// </summary>
        /// <param name="com"></param>
        /// <param name="retry">default 30 second</param>
        /// <returns></returns>
        public static bool IsComReady(string com, int retry = 30)
        {
            //int times = retry;
            SerialPort port = new SerialPort(com);
            Stopwatch watch = new Stopwatch();

            watch.Start();
            do
            {
                try
                {
                    Thread.Sleep(1000);
                    port.Open();
                    port.Close();
                    return true;
                }
                catch
                {
                    //Logger.Bard(string.Format("The retry for {0} of {1} time(s) failed", com, retry));
                }
            } while (watch.Elapsed.TotalSeconds < retry);

            Logger.Bard(string.Format("The retry for {0} of {1}S failed", com, watch.Elapsed.TotalSeconds));
            return false;
        }

        public static bool IsSatisfiedFrameFromCom(SerialPort sp, byte[] input, byte[] output,int delay = 1000)
        {
            if(IsComReady(sp.PortName))
            {
                try
                {
                    sp.Open();
                    sp.DiscardInBuffer();
                    sp.Write(input, 0, input.Length);
                    //Thread.Sleep(100);
                    //sp.Write(input, 0, input.Length);
                    //Thread.Sleep(100);
                    //sp.Write(input, 0, input.Length);
                    Thread.Sleep(delay);
                    int len = sp.BytesToRead;
                    byte[] buff = new byte[len];
                    sp.Read(buff, 0, buff.Length);

                    return IsSubByteArray(buff, output);
                }
                catch(Exception ex)
                {
                    SuUtil.Logger.Bard(ex);
                    return false;
                }
                finally
                {
                    if (sp!= null && sp.IsOpen)
                    {
                        sp.Close();
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// whether a father byte array contains a baby array continually
        /// </summary>
        /// <param name="father"></param>
        /// <param name="baby"></param>
        /// <returns></returns>
        public static bool IsSubByteArray(byte[] father, byte[] baby)
        {
            for (int i = 0; i <= father.Length - baby.Length; i++)
            {
                for (int j = 0; j < baby.Length; j++) 
                {
                    if (baby[j] == father[i + j])
                    {
                        if (j + 1 == baby.Length)
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// test whether adb device is ready
        /// </summary>
        /// <returns></returns>
        public static bool IsAdbReady(int retry = 30)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();
            do
            {
                try
                {
                    if (HardwareClass.IsDeviceReady("ADB") || HardwareClass.IsDeviceReady("Fastboot") || HardwareClass.IsDeviceReady("Bootloader"))
                    {
                        return true;
                    }
                    Thread.Sleep(1000);
                }
                catch
                {
                    //Logger.Bard(string.Format("The retry for {0} of {1} time(s) failed", com, retry));
                }
            } while (watch.Elapsed.TotalSeconds < retry);

            //Logger.Bard(string.Format("The retry for ADB Device of {0}S failed", watch.Elapsed.TotalSeconds));
            return false;
        }

        #region detect a defined tcp port
        static AutoResetEvent connectedDone = new AutoResetEvent(false);//just for tcp detected
        /// <summary>
        /// test whether tcp server is ready
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public static bool IsTcpReady(IPEndPoint remote,int retry = 10)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();
            do
            {
                try
                {
                    TcpClient tcp = new TcpClient();
                    tcp.BeginConnect(remote.Address.ToString(),remote.Port, connectedCallback, tcp);
                    if(connectedDone.WaitOne(1000))
                    {
                        return true;
                    }
                }
                catch
                {
                }
            } while (watch.Elapsed.TotalSeconds < retry);

            //Logger.Bard(string.Format("The retry for Tcp Server of {0}S failed", watch.Elapsed.TotalSeconds));
            return false;
        }
        private static void connectedCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar);
                client.Close();
                connectedDone.Set();
            }
            catch //(Exception ex)
            {
                //SuUtil.Logger.Bard(ex);
            }
        }
        #endregion

    }
}
