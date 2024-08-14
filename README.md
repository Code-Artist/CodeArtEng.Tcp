# CodeArtEng.Tcp
## Introduction
<b>CodeArtEng.Tcp</b> is a .NET Tcp Server and Client implementation with multiple client handling written in C#.![NuGet](https://img.shields.io/nuget/v/CodeArtEng.Tcp)<br>
<b>CodeArtEng.Tcp.WinForms</b> contains user controls for WinForms application. ![NuGet](https://img.shields.io/nuget/v/CodeArtEng.Tcp.WinForms)<br>

### Components
- [TcpServer](#TCP-Server): TCP Server with multiple clients handling capability
- [TcpClient](#TCP-Client): TCP Client with connection and incoming data monitoring.
- [TcpAppServer](#TCP-Application-Server): TCP Application Server for application automation.
- [TcpAppServerPlugin](#TCP-Application-Server-Plugin): Multi Instance Plugin for TCP Application Server.
- TcpAppServerWindows: Derived from TcpAppServer. TCP Application Server for WinForms.
- [TCP Application Client](#TCP-Application-Client): TCP Application Client for application automation.

### About TCP Application Protocol
TCP Application Protocol is created as high level communication protocol to provide a common remote interface between applications which can be easily integrated to any application with minimum coding effort from developer on both server and client application.

## TCP Server
### Features
- Multi-threaded TCP server with multi client support.
- Message Receive Mode: Delimiter / Timeout.

### Quick Start
```C#
private TCPServer Server;
private void Init()
{
    Server = new TcpServer("MyTcpServer"); //Create TCP Server
    Server.ServerStarted += Server_StateChanged; //Subscribe to Server Events
    Server.ServerStopped += Server_StateChanged;
    //Optional event: client connecting, before perform max client check
    Server.IncomingConnection += Server_IncomingConnection;     
    //Optional event: Review and decide if client can be accept.
    Server.ClientConnecting = Server_ClientConnecting;     
    //Require: Handle new connected client.
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
    // this event trigger as soon as any byte received ignore message receive end mode.
    e.Client.BytesReceived += Client_BytesReceived;

    //**************** OR ****************

    //Subscribe to either TCPServerConnection message received event or
    //ProcessReceivedMessageCallback callback to process received message string / bytes
    
    //a) Message Received event
    e.Client.MessageReceived += Client_MessageReceived;   
    
    //b) ProcessReceivedMessageCallback callback
    e.Client.ProcessReceivedMessageCallback = ProcessMessage;    
}

private void ProcessMessage(TcpServerConnection client, string message, byte[] messageBytes)
{
    //Process inocming message here.
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
### Disposing and Clean up
Incoming connection monitoring handle by thread.
We recommend to always call the `Dispose()` method to properly terminate thread for unused object. Failing to do may resulting application keep running in background even forms are closed.

## TCP Client
### Features
- Read / Write bytes array / string to Server.
- Check connection status with Server.
- Data receive by event.

### Quick Start
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

### Disposing and Clean up
Both connection status and incoming data monitoring handle by thread.
We recommend to always call the `Dispose()` method to properly terminate thread for unused object. Failing to do may resulting application keep running in background even forms are closed.

## TCP Application Protocol
### Introduction
TCP Application Protocol is a high level communication protocol created on top of TCP (Transmission Control Protocol)  served as common remote interface between applications which can be easily integrated to any application with minimum coding effort from developer on both server and client application. With TCP application, serial port instrument can be accessible by multiple applications.

With TCP Application Protocol, application specific commands can be easily defined by server application. Each registered command keyword can include with  one or more optional or mandatory parameter as needed. Incoming message from client will be verify against registered command set and have defined parameters parse and assigned. Developer can focus on implementation of each of the command.

### About TCP Application Protocol
TCP Application Protocol is a text based protocol, where any TCP client including [RealTerm](https://realterm.sourceforge.io/) can be use to interact TCP Application Server. TCP Application message format is defined as follow:

1. Command sent from client start with command keyword follow by parameters if any and terminated with carriage return (\r, 0x0D). Extra parameters in commands will be ignored.<br>
`<Command Keyword> [Parameter0] ... [ParameterN]`

2. Response from server to client is begin with status (`OK` or `ERR`) following by response message.<br/>
`<Status> <Response Message / Return Value>`

![TCP Application Class Diagram](<Doc/CodeArtEng.Tcp_UML.png>)

- TcpAppServer - TCP Application Server. Host for remote control.
- ITcpAppServerPlugin - TCP Application Server Plugin Interface.
- TcpAppServerPlugin - TCP Application Server Plugin Helper Class for plugin implementation.
- TcpAppCommand - TCP Application Registered Command.
- TcpAppParameter - TCP Application Command Parameter.
- TcpAppInputCommand - TCP Application Server Received Command.
- TcpAppClient - TCP Application Client. Remove control client.

## TCP Application Server
### Quick start
```C#
private TcpAppServer AppServer;

private void SetupServer()
{
    // <------
    AppServer = new TcpAppServer();  //Create instance using Generic TCP Application Server Class
    // OR
    AppServer = new TcpAppServerWindows(this); //Create instance using TCP Application Server for WinForms
    // ------>

    //Setup Server
    AppServer.WelcomeMessage = "Welcome to TCP Application Server";
    AppServer.MaxClients = 5; //Define number of clients allowed to connects
    AppServer.ExecutionTimeout = 1000;
    AppServer.ClientConnected += AppServer_ClientConnected; //
    AppServer.ClientDisconnected += AppServer_ClientDisconnected;
    AppServer.ClientSignedIn += AppServer_ClientSignedIn;
    AppServer.ClientSigningOut += AppServer_ClientSigningOut;

    RegisterAppServerCommands();
    RegisterPlugins();
    AppServer.Start(12000);   //Start Server
}

private void AppServer_ClientConnected(object sender, TcpServerEventArgs e)
{
    //Get information of connected client. We can decide if want to accept this incoming connection.
    string ipAddress = e.Client.ClientIPAddress;
    string port = e.Client.Port;

    //To reject client Connection
    e.Client.Close(); return;

    //(Optional) subscribe to client object event to monitor incoming messages
    e.Client.MessageReceived += Client_MessageReceived;
    e.Client.BytesSent += Client_MessageSent;
}

private void RegisterPlugins()
{
    //Two options available. Either register plugin using type
    AppServer.RegisterPluginType(typeof(TcpAppServerSamplePlugin));

    // OR discover and register all plugin class which implemented ITcpAppServerPlugin
    AppServer.Registerplugins();
}

private void RegisterAppServerCommand()
{
    //Register application specific commands to TCP Application Server.
    //Command can have 0 or more mandatory and/or optional parameters.
    AppServer.RegisterCommand("CustomFunction", "Dummy Custom Function", custonFunctionCallback,
        TcpAppParameter.CreateParameter("P1", "Parameter P1"),          //Add mandatory parameter
        TcpAppParameter.CreateOptionalParameter("P2", "Parameter P2")); //Add optional parameter
}

private void customFunctionCallback(TcpAppInputCommand sender)
{
    //Client information is accessible using sender.AppClient object
    string clientIP = sender.AppClient.Connection.ClientIPAddress;

    //Implement function actions...

    sender.Status = TcpAppcommandStatus.OK;  //Default status is ERR. Update when execution success.
    sender.OutputMessage = "Execution Completed.";
}
```

### Disposing and Clean up
Similar to `TcpServer`, it is recommended to call `Dispose()` method to properly terminate all threads for unused object.
Failing to do may resulting application keep running in background even forms are closed.

## Working with Plugin
TCP Application Server Plugin provide great capability to application where new feature and components can be added at later stage. TCP Application Protocol equipped with capability to handle and extend command set in plugin components as well as instantiate objects in server application, let’s see how.

### TCP Application Server Plugin
```C#
//Example plugin implemetation
public class TcpAppServerSamplePlugin : ITcpAppServerPlugin
{
    //Plugin class must implement ITcpAppServerPlugin interface
    //TcpAppPlugin in helper class contains common implementation for plugin object.
    private readonly TcpAppServerPlugin TcpAppPlugin;

    public string PluginName { get; } = "Sample Plugin";
    public string PluginDescription { get; } = "Long description about this plugin...";
    public string Alias { get; set; } //Plugin instance name, set by TcpAppServer

    public TcpAppServerSamplePlugin()
    {
        TcpAppPlugin = new TcpAppServerPlugin();

        //Register commands.
        TcpAppPlugin.RegisterCommand("PluginCommand1", "Plugin Command 1", delegate (TcpAppInputCommand sender)
            {
                sender.Status = TcpAppCommandStatus.OK;
                sender.OutputMessage = "Command 1 Executed!";
            });

    }

    public bool DisposeRequest() { return true; }

    //Make sure the following methods ShowHelp and ExecutePluginCommand execute from TcpAppServerPlugin
    public void ShowHelp(TcpAppInputCommand sender){ TcpAppPlugin.ShowHelp(sender); }
    public void ExecutePluginCommand(TcpAppInputCommand sender){ TcpAppPlugin.ExecutePluginCommand(sender); }
}
```

## TCP Application Client
TCP Application Client helper class. Communication with TCP Application Server can be done with any TCP client as well.

### Quick Start
```C#
private TcpAppClient AppClient;

public void SetupClient()
{
    AppClient = new TcpAppClient();
    AppClient.ConnectionStatusChanged += Client_ConnectionStatusChanged;
    AppClient.ResponseReceived += Client_ResponseReceived;
    AppClient.CommandSend += Client_CommandSend;
}

private void ConnectToServer()
{
    AppClient.HostName = <Server IP>;
    AppClient.Port = <Server Port>;
    AppClient.Connect(); //Connect to TCP Application Server to retrieve commands and plugin objects list.

    //List of commands and plugin objects are accessible from the following properties.
    List<string> commands = AppClient.Commands;
    List<string> plugins = AppClient.PluginObjects;
}

private void ExecuteCommand()
{
    //Send command to Server
    TcpAppCommandResult result = AppClient.ExecuteCommand(<Command>, 2000); //Execute command, timeout in 2 seconds.

    //Process returned result...
}
```
### TCP Application Client Terminal
<b>TCPAppClientTerminal</b> is an example of TCP client implementation.<br>
This application implemented generic terminal where user can enter the commands manually to interact with server application.

ToDo: TcpAppClientTerminal Screenshot.
![TcpAppClientTerminal](<Doc/TCPAppTerminal.png>)

Code Artist 2017 - 2022  
www.codearteng.com
