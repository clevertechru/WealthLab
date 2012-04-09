using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketListener
{

    // Incoming data from the client.
    byte[] bytes;
    Socket handler;
    public SynchronousSocketListener(int port)
    {
        // Data buffer for incoming data.
        bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // Dns.GetHostName returns the name of the 
        // host running the application.
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            //while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                handler = listener.Accept();
                
                /*
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                */
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        /*
        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
        */
    }

    public void raysend(String data)
    {
        // Echo the data back to the client.
        byte[] msg = Encoding.ASCII.GetBytes(data);
        handler.Send(msg);

    }

    public void rayreceive()
    {
        bytes = new byte[1024];
        int bytesRec = handler.Receive(bytes);
        // Show the data on the console.
        Console.WriteLine("Text received : {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

    }
}