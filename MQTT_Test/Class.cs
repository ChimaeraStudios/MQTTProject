using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;



namespace MQTTBasics
{
    internal class Broker
    {
        MqttFactory mqttServerFactory = new ();
        MqttServer mqttServer;

        public Broker()
        {
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            mqttServer = mqttServerFactory.CreateMqttServer(mqttServerOptions);
        }

        async public Task Start()
        {
            await mqttServer.StartAsync();
        }

        async public Task Stop()
        {
            await mqttServer.StopAsync();
        }
    }

    internal class Client
    {
        MqttFactory mqttClientFactory = new();
        IMqttClient mqttClient;
        string serverIp;
        bool verbose;

        public Client(string serverIp, bool verbose = true)
        {
            this.serverIp = serverIp;
            this.verbose = verbose;
            mqttClient = mqttClientFactory.CreateMqttClient();
        }

        async public Task Connect()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(serverIp).Build();
            await mqttClient.ConnectAsync(mqttClientOptions);

            if (mqttClient.IsConnected && verbose)
            {
                Console.WriteLine("Connected to server");
            }
            else
            {
                Console.WriteLine("Failed to connect to server");
            }
        }

        async public Task Subscribe(string topic)
        {
            var mqttSubscribeOptions = mqttClientFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topic).Build();
            await mqttClient.SubscribeAsync(mqttSubscribeOptions);

            if (verbose)
            {
                Console.WriteLine($"Subscribed to {topic}");
            }
        }

        public Task<string> ReceiveMessage()
        {
            var tcs = new TaskCompletionSource<string>();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string messagePayload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                if (verbose)
                {
                    Console.WriteLine($"Message received on topic {e.ApplicationMessage.Topic}: {messagePayload}");
                }
                tcs.SetResult(messagePayload);
                return Task.CompletedTask;
            };

            return tcs.Task;
        }


        async public Task Unsubscribe(string topic)
        {
            var mqttUnsubscribeOptions = mqttClientFactory.CreateUnsubscribeOptionsBuilder().WithTopicFilter(topic).Build();
            await mqttClient.UnsubscribeAsync(mqttUnsubscribeOptions);
            if (verbose)
            {
                Console.WriteLine($"Unsubscribed from {topic}");
            }
        }

        async public Task Publish(string topic, string payload)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();

            await mqttClient.PublishAsync(applicationMessage);
        }

        async public Task Disconnect()
        {
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            if (verbose)
            {
                Console.WriteLine("Disconnected from server");
            }
        }
    }
}