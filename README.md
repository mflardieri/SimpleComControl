# SimpleComControl

## .NET Socket Class (TCP)
In this implementation, the _ComTCPSocket_ base class is used to send and receive messages via the _Socket_ class and TCP.
You can do more than just send simple text to the server; you can bridge communication between one or more machines and etc.


Check out the [Examples](#Examples)

## Examples
### Demo Server and Client
Uses a Socket wrapper in separate projects to assign a server and the client endpoints.
These projects are console applications. Below are steps to build and run this demo.

1. Build the projects (DemoServer and DemoClient).

1. Run the DemoServer.exe in the debug output bin (project runs on port 65353 by default)
If you choose to change the port do so in _Program.cs_ for 
both the server and client projects and rebuild them.

1. Run the DemoClient.exe in the debug output bin.
  
At this point you should see the server started and listening for messages and the client ready to send them.

4. Try sending some messages to the server (Be nice :)!). While you send messages to the server, you will see the messages appear in the server console app.

1. After you have sent some messages if you want to start another DemoClient.exe and send some messages from both clients.

`Example below is one server and two clients. One client Types 'HI' and the other client types "There"`
```
Com TCP Server listening on port: 65353...
Enter Text or type '/E' to exit:
TCP RECV: 127.0.0.1:59452: HI
TCP RECV: 127.0.0.1:59459: There
```

6. Once you are done exit all client consoles by typing _"/E"_ and hit _ENTER_. Do the same on the server console when done.


### .NET Maui (When will then be now?)
Most examples of using the _Socket_ class are simply sending a message to a server via client.
But what about client to server to client(s)? The Server should be able to receive, log, and send messages and responses to clients.
Example: Client wants to send a message to another Client or to Chat room. Individual message from client to client would be "Client to Server to Client" (1:1).
Where as a Client to room would be  "Client to Server filter message to Clients in a room" (1:many).
For this we will need to make a simple message a little more complex. 
TODO!!!