using MQTTBasicWrapper;
using System.Threading;

public class Program
{
    public static async Task Main(string[] args)
    {
        Broker broker = new();

        await broker.Start();

        string serverIp = "localhost";

        Client subscriber = new(serverIp);
        await subscriber.Connect();

        Client publisher = new(serverIp);
        await publisher.Connect();

        await subscriber.Subscribe("test/topic");

        // Imposta il subrscriber per ricevere un messaggio
        subscriber.ReceiveMessage();

        Console.WriteLine("Enter message to publish:");
        string? message = Console.ReadLine();
        await publisher.Publish("test/topic", message);

        await broker.Stop();

    }
}
