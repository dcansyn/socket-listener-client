using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static List<Socket> clients = new List<Socket>();
    static object lockObj = new object();

    static void Main(string[] args)
    {
        StartServer();
    }

    static void StartServer()
    {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        listener.Bind(localEndPoint);
        listener.Listen(10);
        Console.WriteLine("Server is running...");

        Thread notificationThread = new Thread(SendNotifications);
        notificationThread.Start();

        while (true)
        {
            Console.WriteLine("Waiting for a connection...");
            Socket clientSocket = listener.Accept();
            lock (lockObj)
            {
                clients.Add(clientSocket);
            }
            Console.WriteLine("New client connected: " + clientSocket.RemoteEndPoint);

            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }

    static void HandleClient(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRec = clientSocket.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRec);
                Console.WriteLine("Received from {0}: {1}", clientSocket.RemoteEndPoint, message);

                string response = $"Hello, {clientSocket.RemoteEndPoint}. You sent: {message}";
                byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                clientSocket.Send(responseBytes);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Client disconnected: " + clientSocket.RemoteEndPoint);
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(clientSocket);
            }
            clientSocket.Close();
        }
    }

    static void SendNotifications()
    {
        while (true)
        {
            Thread.Sleep(5000);
            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        string notification = "Server notification: " + DateTime.Now;
                        byte[] notificationBytes = Encoding.ASCII.GetBytes(notification);
                        client.Send(notificationBytes);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Failed to send notification to: " + client.RemoteEndPoint);
                    }
                }
            }
        }
    }
}
