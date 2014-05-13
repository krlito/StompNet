# StompNet
StompNet is an asynchronous [STOMP 1.2][stomp12specification] client library for .NET 4.5.

## Features
- High level interfaces to ease the flow of the application.
- Built using the observer pattern. It can be used for reactive 
  application approaches. This is a great library to be used together 
  with [Reactive Extensions library][reactive-extensions].
- Thread-safe.
- Library user would rarely have to interact with STOMP frames directly, unless 
  it is desired.
- Message receipt confirmations and sequence number generation for frames are 
  automatically handled by the library.

## Installation
 On your Package Manager Console:
    Install-Package StompNet

##Usage

The following code is an example to show the basic usage of StompNet.

```csharp
	// Example: Send three messages before receiving them back.
	static async Task ReadmeExample()
    {
		// Establish a TCP connection with the STOMP service.
        using (TcpClient tcpClient = new TcpClient())
        {
            await tcpClient.ConnectAsync("localhost", 61613);

            //Create a connector.
            using (IStompConnector stompConnector =
                new Stomp12Connector(
                    tcpClient.GetStream(),
                    "localhost", // Virtual host name.
                    "admin",
                    "password"))
            {
                // Create a connection.
                IStompConnection connection = await stompConnector.ConnectAsync();

                // Send a message.
                await connection.SendAsync("/queue/example", "Anybody there!?");

                // Send two messages using a transaction.
                IStompTransaction transaction = await connection.BeginTransactionAsync();
                await transaction.SendAsync("/queue/example", "Hi!");
                await transaction.SendAsync("/queue/example", "My name is StompNet");
                await transaction.CommitAsync();

                // Receive messages back.
                // Message handling is made by the ConsoleWriterObserver instance.
                await transaction.SubscribeAsync(
                    new ConsoleWriterObserver(),
                    "/queue/example");

                // Wait for messages to be received.
                await Task.Delay(250);

                // Disconnect.
                await connection.DisconnectAsync();
            }
        }
    }
```

The observer `ConsoleWriterObserver` class for handling the incoming messages is:


```csharp
    class ConsoleWriterObserver : IObserver<IStompMessage>
    {
	    // Incoming messages are processed here.
        public void OnNext(IStompMessage message)
        {
            Console.WriteLine("MESSAGE: " + message.GetContentAsString());

            if (message.IsAcknowledgeable)
                message.Acknowledge();
        }

		// Any ERROR frame or stream exception comes through here.
        public void OnError(Exception error)
        {
            Console.WriteLine("ERROR: " + error.Message);
        }

		// OnCompleted is invoked when unsubscribing.
        public void OnCompleted()
        {
            Console.WriteLine("UNSUBSCRIBED!");
        }
    }
```

For more examples, take a look at the examples project where
you can see how to:
- Use receipt confirmations automatically.
- Send byte array messages and set its content type.
- Set acknowledgement mode for subscriptions.
- Use low level APIs to directly read/write frames into the stream.

### Comments about the usage of the library.
- IO stream connection management is not handled by the library.
  If the TCP connection fails, a new IStompConnector must be created.
- Exceptions will be thrown by currently awaited tasks and they will
  also be notified to observers through the OnError method.
- One and only one of the methods OnError or OnCompleted will be 
  invoked. Both methods indicate the end of the incoming messages 
  stream.
- SubscribeAsync returns a Task<IAsyncDisposable> that can be 
  used to unsubscribe at any time by calling DisposeAsync.
  If an unsubscription is not done explicitly, unsubscription is
  done when the connector is disposed or an exception occurs in the
  stream.
- All methods may receive a CancellationToken. Anyway, cancelling 
  before a method is finished may leave the IO stream in an invalid
  state.

## License
Apache License 2.0

## ToDo
- Heartbeat support
- STOMP 1.1 support

[reactive-extensions]: http://msdn.microsoft.com/en-us/data/gg577609.aspx
[stomp12specification]: http://stomp.github.io/stomp-specification-1.2.html