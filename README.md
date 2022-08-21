# CodeArtEng.Tcp
## Introduction
<b>CodeArtEng.Tcp</b> is a .NET Tcp Server and Client implementation with multiple client handling written in C#.![NuGet](https://img.shields.io/nuget/v/CodeArtEng.Tcp)<br>
<b>CodeArtEng.Tcp.WinForms</b> contains user controls for WinForms application. ![NuGet](https://img.shields.io/nuget/v/CodeArtEng.Tcp.WinForms)<br>

### Components
- [TcpServer](#TCP-Server): TCP Server with multiple clients handling capability
- [TcpClient](#TCP-Client): TCP Client with connection and incoming data monitoring.
- [TcpAppServer](#TCP-Application-Server): TCP Application Server for application automation.
- TcpAppServerWindows: Derived from TcpAppServer. TCP Application Server for WinForms.
- [TCP Application Client](#TCP-Application-Client): TCP Application Client for application automation.

#### About TCP Application Protocol
TCP Application Protocol is created as high level communication protocol to provide a common remote interface between applications which can be easily integrated to any application with minimum coding effort from developer on both server and client application.

## TCP Server
#### Features
- Multi-threaded TCP server with multi client support.
- Message Receive Mode: Delimiter / Timeout.

#### Quick Start
```C#
private TCPServer Server;
private void Init()
{
    Server = new TcpServer("MyTcpServer"); //Create TCP Server
    Server.ServerStarted += Server_StateChanged; //Subscribe to Server Events
    Server.ServerStopped += Server_StateChanged;
    Server.ClientConnected += Server_ClientConnected;

    //Configure how MessageReceived event should trigger
    // A) Delimiter character mode (DEFAULT)
    Server.MessageReceivedEndMode = TcpServerMessageEndMode.Delimiter;
    Server.MessageDelimiter = Convert.ToByte('\n');

    // OR
    // B) Trigger by timeout between read cycle.

    Server.MessageReceivedEndMode = TcpServerMessageEndMode.Timeout;
    Server.InterMessageTimeout = 100; //Timeout 100ms
}

private void Server_ClientConnected(object sender, TcpServerEventArgs e)
{
    //Subscribe to TCPServerConnection bytes received event to capture incoming byte
    e.Client.BytesReceived += Client_BytesReceived;

    //OR

    //Subscribe to TCPServerConnection message received event to capture message string
    e.Client.MessageReceived += Client_MessageReceived;   
}

private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
{
    byte [] data = e.Client.ReceivedBytes; //Retrieve read content from Client

    //Handle incoming data from TCP Client.

    e.Client.WriteToClient("Message to Client."); //Write to Client
    ...
  }

private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
{
    string message = e.ReceivedMessage; //Handle incoming message string from TCP Client.
    ...
}
```
#### Disposing and Clean up
Incoming connection monitoring handle by thread.
We recommend to always call the `Dispose()` method to properly terminate thread for unused object.

## TCP Client
#### Features
- Read / Write bytes array / string to Server.
- Check connection status with Server.
- Data receive by event.

#### Quick Start
```C#
private TCPClient Client;
private void Init()
{
    Client.HostName = "127.0.0.1";
    Client.Port = 10000;
    //Notify when connection to server break.
    Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
    Client.DataReceived += Client_DataReceived; //Subscribe to data received event.
    Clinet.Connect();   //Connect to Server.
}

private void Client_DataReceived(object sender, EventArgs e)
{
    String inputMsg = e.GetString(); //Read input from TCP Server as String
    byte [] inputData = e.Data;      //Read input from TCP Server as byte array

    //Process incoming data or message ...
}

private void WriteDataToServer()
{
    //Example
    Client.Write("String message to Server.");  
    Client.WriteLine("Message terminated with LF.");
    Client.Write(new byte[]{'D','a','t','a'}); //Write to Server in byte array
    ...
}
```
To read message from TCP Server without using DataReceived event, skip DataReceived event subscription and use one of the following method to read input from TCP Server.
```C#
    byte[] inputData = ReadBytes();  //Read input as byte array.
    string inputString = ReadString(); //Read input as string.
```

#### Disposing and Clean up
Both connection status and incoming data monitoring handle by thread.
We recommend to always call the `Dispose()` method to properly terminate thread for unused object.

## Reference
Documentation and explanation regarding TcpAppServer and TcpAppClient is available at<br>
[TCP Application Protocol – TCP/IP based Inter-process Communication](https://www.codeproject.com/Articles/5205700/TCP-Application-Protocol-TCP-IP-based-Inter-proces)

Code Artist 2017 - 2022  
www.codearteng.com
