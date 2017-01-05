using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SuUtil
{
    public class TcpRWHelper : IDisposable
    {
        private NetworkStream stream = null;
        private TcpClient client= null;
        private int headerLength = 8;
        private DBFactroy db = new DBFactroy(DataBaseType.SqlServer);
        private string clientIp = string.Empty;

        public TcpRWHelper(TcpClient client, int timeout = 2000)
        {
            //timeout = 100000;
            this.client = client;
            client.SendBufferSize = 0;
            client.NoDelay = true;
            stream = client.GetStream();
            stream.ReadTimeout = timeout;
            stream.WriteTimeout = timeout; 
            clientIp = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address.ToString();
        }

        public string Read(string token = "susususu-susu-susu-susu-susususususu")//8-4-4-4-12,default guid
        {
            byte[] data = new byte[getSize()];
            int index = 0;

            while(index<data.Length)
            {
                int r = stream.Read(data, index, data.Length - index);
                index += r;
            }

            string message = Encoding.UTF8.GetString(data);
            db.ExecNoQuery(string.Format("INSERT INTO [dbo].[DistributedMessage] ([token] ,[clientIp],[rw],[message] ,[insertTime]) VALUES ('{0}','{1}','{2}','{3}',{4})", token, clientIp, "Read", message, "getdate()"));
            
            return message;
        }

        public void Write(string message,string token)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] dataLength = BitConverter.GetBytes(data.Length);
            byte[] buffer = new byte[data.Length + headerLength];
            Array.Copy(dataLength, 0, buffer, 0, dataLength.Length);
            Array.Copy(data, 0, buffer, headerLength, data.Length);

            int i = client.Available;
            stream.Write(buffer, 0, buffer.Length);
            //stream.Flush();
            db.ExecNoQuery(string.Format("INSERT INTO [dbo].[DistributedMessage] ([token] ,[clientIp],[rw],[message] ,[insertTime]) VALUES ('{0}','{1}','{2}','{3}',{4})", token, clientIp, "Write", message, "getdate()"));
            
            //clear read buffer
            //if(stream.DataAvailable)
            //{
            //    stream.Read(new byte[1024], 0, 1024);
            //}
        }

        private int getSize()
        {
            int count = 0;
            byte[] countBytes = new byte[headerLength];
            int r = stream.Read(countBytes, 0, headerLength);

            if(r==0)
            {
                throw new Exception("client is closed");
            }
            if (r == headerLength)
            {
                byte[] c = new byte[4];
                Array.Copy(countBytes, c, 2);//
                count = BitConverter.ToInt32(c, 0);
            }
            else
            {
                throw new Exception("Invalid message. Read message header failed");
            }

            return count;
        }

        public void Dispose()
        {
            if(client!=null)
            {
                //Calling this method will eventually result in the close of the associated Socket and will also close the associated NetworkStream that is used to send and receive data if one was created.
                client.Close();
            }
        }
    }
}
