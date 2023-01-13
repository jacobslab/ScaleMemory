using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class NeuralConnectionThread : ThreadedJob
{
    public bool isConnected = false;
    public IPAddress ipAddress;
    public NeuralConnectionThread()
    {
        isConnected = true;

    }


    void TCPClient(IPAddress servAddr)
    {
        Debug.Log("outside try");
        try
        {
            UnityEngine.Debug.Log("about to create client");

            UnityEngine.Debug.Log("server address " + servAddr.ToString());
            try
            {
                TcpClient client = new TcpClient(servAddr.ToString(), 9997);

                Debug.Log("created client");
                isConnected = true;
                Byte[] data = new byte[256];
                NetworkStream stream;
                while (isConnected)
                {
                    //convert the last message
                    data = System.Text.Encoding.ASCII.GetBytes("TEST");

                    // Get a client stream for reading and writing.
                    //  Stream stream = client.GetStream();

                    stream = client.GetStream();

                    //    }
                    Debug.Log("writing into the stream");
                    // Send the message to the connected TcpServer. 
                    stream.Write(data, 0, data.Length);

                    Debug.Log("Sent message");
                    //stream.Close();
                    Thread.Sleep(100);
                }

                //close the client
                client.Close();
            }
            catch (SocketException e)
            {
                Debug.Log("got socket exception " + e.Message);
                Debug.Log(e.SocketErrorCode.ToString());
                Debug.Log("attempting to run again");
                TCPClient(ipAddress);
            }
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    protected override void ThreadFunction()
    {
        TCPClient(ipAddress);
    }

    protected override void OnFinished()
    {

    }

    public virtual void close()
    {
        isConnected = false;
    }
}

public class DiscoveryThread : ThreadedJob
{
    public bool isConnected = false;
    public IPAddress ipadAddress;
    public DiscoveryThread()
    {

    }

    protected override void ThreadFunction()
    {
        ServerSocket();
    }

    protected override void OnFinished()
    {

    }

    void ServerSocket()
    {
        var Server = new UdpClient(9999);
        var ResponseData = Encoding.ASCII.GetBytes("Connected");

        while (!isConnected)
        {
            Debug.Log("waiting for client to be found");
            var ClientEp = new IPEndPoint(IPAddress.Any, 0);
            var ClientRequestData = Server.Receive(ref ClientEp);
            var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);
            UnityEngine.Debug.Log("Received " + ClientRequest + " from " + ClientEp.Address.ToString() + " sending response");
            Server.Send(ResponseData, ResponseData.Length, ClientEp);
            string[] clientData = ClientRequest.Split('\t');
            for (int i = 0; i < clientData.Length; i++)
            {
                Debug.Log("client data  " + i.ToString() + ": " + clientData[i]);
            }
            isConnected = true;
            Debug.Log("established connection");
        }
        Debug.Log("closing server");
        Server.Close();

    }

    public virtual void close()
    {
        isConnected = false;
    }
}

//this will be the actual purpose of the application
public class ServerThread : ThreadedJob
{
    private bool isConnected = false;
    private DiscoveryThread _discoveryThread;
    //public NeuralConnectionThread neuralConnection;
    public ServerThread()
    {
    }

    protected override void ThreadFunction()
    {
        //StartListening();
        _discoveryThread = new DiscoveryThread();
        _discoveryThread.Start();
        TCPListener();
    }


    void TCPListener()
    {
        TcpListener server = null;
        bool shouldReconnect = true; 
        try
        {
            // Set the TcpListener on specified port.
            Int32 port = Configuration.portNumber;
            IPAddress targetAddr = IPAddress.Parse(Configuration.ipAddress);

            server = new TcpListener(targetAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            // Enter the listening loop.
            while (shouldReconnect)
            {
                Debug.Log("Waiting for a connection... ");
                //EPADApplication.Instance.UpdateIPADConnectionStatus(false);
                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                //server.
                TcpClient client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                //if connected, we can close the discovery thread
                _discoveryThread.close();


                //EPADApplication.Instance.UpdateIPADConnectionStatus(true);
                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Debug.Log("Received: " + data);

                    string[] resultArr = new string[2];
                    resultArr = data.Split('\t');

                    //EPADApplication.Instance.timeSyncLog.LogIPADSyncTime(resultArr);
                    //EPADApplication.Instance.BeginClockSync(resultArr);

                    // Process the data sent by the client.
                    //data = data.ToUpper();

                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    //// Send back a response.
                    //stream.Write(msg, 0, msg.Length);
                    //Debug.Log("Sent: " + data);
                }

                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);

        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
    }

    protected override void OnFinished()
    {
    }

    // Incoming data from the client.  
    public static string data = null;

    public static void StartListening()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Establish the local endpoint for the socket.  
        // Dns.GetHostName returns the name of the   
        // host running the application.  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9999);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and   
        // listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            while (true)
            {
                Debug.Log("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                data = null;

                // An incoming connection needs to be processed.  
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                }

                // Show the data on the console.  
                Debug.Log("Text received : " + data);

                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes(data);

                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }


    }

    void ListeningServer()
    {
        Debug.Log("starting listening server");
        Socket listenSocket = new Socket(AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);

        // bind the listening socket to the port
        int port = 9999;
        int backlog = 10;
        IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, port);
        listenSocket.Bind(ep);
        Debug.Log("binded");

        // start listening
        Debug.Log("listening");
        listenSocket.Listen(backlog);

        Debug.Log("about to accept");
        Socket s = listenSocket.Accept();
        Debug.Log("accepted");
        SocketAsyncEventArgs e = new SocketAsyncEventArgs();

        byte[] bytes = new byte[256];
        Debug.Log("about to receive");
        int byteCount = s.Receive(bytes, 0, s.Available, SocketFlags.None);

        Debug.Log("byte count " + byteCount.ToString());
        if (byteCount > 0)
        {
            Debug.Log(Encoding.UTF8.GetString(bytes));
        }

    }





    public virtual void close()
    {
        if (_discoveryThread != null)
            _discoveryThread.close();
        isConnected = false;
        //if(neuralConnection!=null)
        //    neuralConnection.close();

    }


}


//for testing purposes only
public class ClientThread : ThreadedJob
{

    private bool isConnected = false;
    public ClientThread()
    {

    }

    protected override void ThreadFunction()
    {
        //StartClient();


        //IPAddress serverAddress = ClientSocket();

        //if (serverAddress != IPAddress.Parse("0.0.0.0"))
        //{
        //ConnectToServer(serverAddress);
        TCPClient(IPAddress.Parse("127.0.0.1"));
        //}

    }

    void TCPClient(IPAddress servAddr)
    {
        Debug.Log("outside try");
        try
        {
            Debug.Log("about to create client");
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            Debug.Log("server address " + servAddr.ToString());
            TcpClient client = new TcpClient(servAddr.ToString(), 10001);
            Debug.Log("created FAKE client");
            isConnected = true;
            while (isConnected)
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes("true");

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Debug.Log("Sent message");
                //stream.Close();

                Debug.Log("going to sleep");
                Thread.Sleep(1000);

            }

            //close the client
            client.Close();
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    void ConnectToServer(IPAddress servAddr)
    {
        Debug.Log("establishing connection via socket");
        Socket s = new Socket(AddressFamily.InterNetwork,
         SocketType.Stream,
         ProtocolType.Tcp);
        s.Connect(servAddr, 9999);
        Debug.Log("connected");
        isConnected = true;

        while (isConnected)
        {
            string msg = "ok";
            byte[] bytes = new byte[256];
            bytes = Encoding.UTF8.GetBytes(msg.ToCharArray());
            Debug.Log("about to send");
            s.Send(bytes);
            Debug.Log("sent; about to sleep");
            Thread.Sleep(2000);
            Debug.Log("back");
        }

        s.Close();




    }

    public static void StartClient()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1:1001");

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Debug.Log("Socket connected to " +
                     sender.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);
                Debug.Log("sent byte count " + bytesSent.ToString());
                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);
                Debug.Log("Echoed test = " +
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.  
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Debug.Log("ArgumentNullException : " + ane.ToString());
            }
            catch (SocketException se)
            {
                Debug.Log("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("Unexpected exception : " + e.ToString());
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    protected override void OnFinished()
    {
        isConnected = false;
    }

    IPAddress ClientSocket()
    {
        var Client = new UdpClient();
        var RequestData = Encoding.ASCII.GetBytes("!");
        var ServerEp = new IPEndPoint(IPAddress.Any, 0);

        Client.EnableBroadcast = true;
        Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 10001));
        Debug.Log("waiting to receive as client");
        //var ServerResponseData = Client.Receive(ref ServerEp);
        //var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);

        var ServerResponseData = Client.ReceiveAsync();
        var ServerResponse = ServerResponseData.Result.Buffer.ToString();

        IPAddress targetAddr = ServerResponseData.Result.RemoteEndPoint.Address;

        UnityEngine.Debug.Log("Received " + ServerResponse + " from " + ServerResponseData.Result.RemoteEndPoint.Address.ToString());
        Client.Close();

        return targetAddr;

    }

    public virtual void close()
    {
        isConnected = false;
    }
}

public class SocketControl : MonoBehaviour
{
    ServerThread _server;
    ClientThread _client;
    // Start is called before the first frame update
    void Start()
    {
        //run on start
        StartCoroutine("RunServer");
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //{
        //    StartCoroutine("RunServer");
        //}
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    StartCoroutine("RunClient");
        //}
        //if (Configuration.shouldSearchAgain)
        //{
        //    StartCoroutine("AttemptReconnection");

        //}
    }

    IEnumerator AttemptReconnection()
    {
        if (_server != null)
            _server.close();

        yield return new WaitForSeconds(1f);
        Debug.Log("starting another instance of server thread");
        yield return StartCoroutine(RunServer());
        yield return null;
    }

    IEnumerator RunClient()
    {
        UnityEngine.Debug.Log("starting client thread");
        _client = new ClientThread();
        _client.Start();
        yield return null;
    }

    IEnumerator RunServer()
    {
        UnityEngine.Debug.Log("starting server thread");
        _server = new ServerThread();
        _server.Start();
        yield return null;
    }

    private void OnApplicationQuit()
    {
        if (_client != null)
        {
            _client.close();
            _client.Abort();
        }

        if (_server != null)
        {
            _server.close();
            _server.Abort();
        }
    }
}
