# CodeArtEng.Tcp
CodeArtEng.Tcp is a .NET Tcp Server and Client implementation with multiple client handling written in C#.
CodeArtEng.Tcp library is created for instrument control and inter-application communication using TCP/IP.
This library is not meant to be deployed for web server.

<b>NuGet Package:</b>https://www.nuget.org/packages/CodeArtEng.Tcp/

<b>Architecture Overview</b>

![alt text](https://github.com/Code-Artist/CodeArtEng.Tcp/blob/master/Doc/ClassDiagram.PNG)

TCP Server:
- Multi-threaded TCP server with multi client support.
- Detect client connect / disconnect.
- Delimited message mode, ideal for instrument control.

TCP Client: 
- Read / Write bytes array / string to Server.
- Check connection status with Server.

Code Artist 2017  
www.codearteng.com

