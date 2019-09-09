# CodeArtEng.Tcp
CodeArtEng.Tcp is a .NET Tcp Server and Client implementation with multiple client handling written in C#.

TCP Server:
- Multi-threaded TCP server with multi client support.
- Detect client connect / disconnect.
- Delimited message mode, ideal for instrument control.

TCP Client: 
- Read / Write bytes array / string to Server.
- Check connection status with Server.

#### USAGE
<b>TCP Server</b>
```C#
private TCPServer Server;
private void Init()
{
    Server = new TcpServer("MyTcpServer"); //Create TCP Server
    Server.ServerStarted += Server_StateChanged; //Subscribe to Server Events
    Server.ServerStopped += Server_StateChanged;
    Server.ClientConnected += Server_ClientConnected;
}

private void Server_ClientConnected(object sender, TcpServerEventArgs e)
{
    //Subscribe to TCPServerConnection bytes received event to capture incoming byte
    e.Client.BytesReceived += Client_BytesReceived;
    
    //OR
    
    //Subscribe to TCPServerConnection message received event to capture delimited string
    e.Client.MessageReceived += Client_MessageReceived;   
}

private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
{
    //Read from Client
    byte [] data = e.Client.ReceivedBytes;
    
    //Handle incoming messages from TCP Client.

    //Write to Client
    e.Client.WriteToClient("Message to Client.");
    ...

```

<b>TCP Client</b>
```C#
private TCPClient Client;
private void Init()
{
    Client.HostName = "127.0.0.1";
    Client.Port = 10000;
    Client.DataReceived += Client_DataReceived;
}

private void Client_DataReceived(object sender, EventArgs e)
{
    //Read message from TCP Server.
    String inputMsg = Client.ReadString();
    //...
    //Process incoming message ...
}

private void WriteDataToServer()
{
    Client.Write("Message to Server.");
    ...
}

```
Documentation and explanation regarding TcpAppServer and TcpAppClient is available at
[TCP Application Protocol – TCP/IP based Inter-process Communication](https://www.codeproject.com/Articles/5205700/TCP-Application-Protocol-TCP-IP-based-Inter-proces)

Code Artist 2017  
www.codearteng.com

