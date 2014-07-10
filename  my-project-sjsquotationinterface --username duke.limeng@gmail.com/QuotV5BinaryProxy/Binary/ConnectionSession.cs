using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace QuotV5.Binary
{
    class ConnectionSession
    {
        /// <summary>
        /// 接收数据缓冲区的长度
        /// </summary>
        internal static int RecvBufferLength = 4096;

        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        internal readonly byte[] RecvBuffer = new byte[RecvBufferLength];

        /// <summary>
        /// 已经接收到的数据长度
        /// </summary>
        internal int RecvLength = 0;

        /// <summary>
        /// 接收到的请求的包头结构
        /// </summary>
        internal MessagePack MessagePack;

        /// <summary>
        /// 接收到的请求的包体数据
        /// </summary>
        internal readonly MemoryStream RecvDataStream = new MemoryStream();


        /// <summary>
        /// 
        /// </summary>
        internal TcpClient Client = null;


        internal NetworkStream Stream = null;

        internal ConnectionSession(TcpClient client)
        {
            this.Stream = client.GetStream();
            this.Client = client;
        }

        /// <summary>
        /// 手动关闭Session
        /// </summary>
        internal void ManualClose()
        {
            try
            {
                if (this.Stream != null)
                    this.Stream.Close();

                if (this.Client != null)
                    this.Client.Close();
            }
            finally
            {
                this.Stream = null;
                this.Client = null;
            }
        }

    }
}
