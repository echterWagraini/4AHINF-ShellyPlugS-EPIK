using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using MongoDB.Driver;

namespace BlazorApp1.Data
{
    public class MQTTInfluxDataSercie : BackgroundService
    {
        private Plug currentPlug = null;
        private HiveMQClient client = new();
        private List<Benutzer> user = null;

        public MQTTInfluxDataSercie()
        {
            getUsers();
            connect();
        }

        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            do
            {
                client.OnMessageReceived += (sender, args) =>
                {
                    Console.WriteLine("Message Received: " + args.PublishMessage.PayloadAsString);

                    //InfluxDB Insert
                    string token = "u8WIGv8E39T5zkqfJn-jMI6vQ5frC36rTnsOgzo0ROCUn8Jz__J9Pxn60HoqdAiTM7q8w-q2FE0Jy6-RDX4uOA==";
                    const string bucket = "Shellie";
                    const string org = "Schule";
                    using var client = new InfluxDBClient("http://10.10.1.61:8086", token);

                    using (var writeApi = client.GetWriteApi())
                    {
                        writeApi.WriteRecord("data,shellyid=3CE90ED77318 power=" + args.PublishMessage.PayloadAsString, WritePrecision.Ns, bucket, org);
                    }
                };

                foreach (Benutzer b in user)
                {
                    foreach(Plug p in b.Plugs)
                    {
                        await client.SubscribeAsync("shellies/shellyplug-s-3CE90ED77318/relay/0/power", QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
                    }
                }

                //fired every minute
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }
        public async Task connect()
        {
            var options = new HiveMQClientOptions
            {
                Host = "127.0.0.1",
                Port = 1883
            };
            client = new HiveMQClient(options);

            // Connect
            HiveMQtt.Client.Results.ConnectResult connectResult;

            try
            {
                connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode == ConnAckReasonCode.Success)
                {
                    Console.WriteLine($"Connect successful: {connectResult}");
                }
                else
                {
                    // FIXME: Add ToString
                    Console.WriteLine($"Connect failed: {connectResult}");
                    Environment.Exit(-1);
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine($"Error connecting to the MQTT Broker with the following socket error: {e.Message}");
                Environment.Exit(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error connecting to the MQTT Broker with the following message: {e.Message}");
                Environment.Exit(-1);
            }
        }
        public async void getUsers()
        {
            var client = new MongoClient("mongodb+srv://Flo:shelly123@shelly.ikeunhp.mongodb.net/?retryWrites=true&w=majority");
            var mongodb = client.GetDatabase("BenutzerDatabase");
            var collection = mongodb.GetCollection<Benutzer>("Benutzer");
            var documents = await collection.Find(Builders<Benutzer>.Filter.Empty).ToListAsync();
            user = documents;

        }
    }
}