using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        StartClient();
    }

    static void StartClient()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            Socket clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(remoteEP);

            Console.WriteLine("Connected to server.");
            Thread listenThread = new Thread(() => ListenForMessages(clientSocket));
            listenThread.Start();

            while (true)
            {
                Console.Write("Enter a message to send (or type 'exit' to close): ");
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }

                byte[] msg = Encoding.ASCII.GetBytes(message);
                clientSocket.Send(msg);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ListenForMessages(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRec = clientSocket.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRec);
                Console.WriteLine("\nServer: {0}", message);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Disconnected from server.");
        }
    }
}
