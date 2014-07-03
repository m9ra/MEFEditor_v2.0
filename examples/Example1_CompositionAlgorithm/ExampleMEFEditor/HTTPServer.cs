using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace Main
{
    class HTTPServer
    {
        public HTTPServer(string outputHTML)
        {
            //start listening
            var listenPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 80);
            var listener = new Socket(listenPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(listenPoint);
                listener.Listen(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot listen on localhost:80, because of exception\n{0}", ex);
            }

            //buffer for recieving headers
            var buffer = new byte[10000];
            //data which will be sended to client
            var toSend = ASCIIEncoding.ASCII.GetBytes(outputHTML);

            for (; ; )
            {
                try
                {
                    var client = listener.Accept();
                    client.Receive(buffer);
                    client.Send(toSend);
                    client.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Handling client failed with exception\n{0}", ex);
                }
            }


        }


    }
}
