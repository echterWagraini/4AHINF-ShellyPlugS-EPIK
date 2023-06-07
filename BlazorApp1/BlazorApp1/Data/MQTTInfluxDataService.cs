using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MongoDB.Driver;
using System.Globalization;

namespace BlazorApp1.Data
{
    public class MQTTInfluxDataService : BackgroundService
    {
        private Plug currentPlug = null;
        private HiveMQClient client = new();
        private List<Benutzer> user = new List<Benutzer>();
        private List<Plug> subbedPlugs = new List<Plug>();
        public MQTTInfluxDataService()
        {

        }
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("EXECUTE");

            connectToMQTT();

            do
            {
                getUsers();

                if (user != null)
                {
                    foreach (Benutzer u in user)
                    {
                        if (u.Plugs != null)
                        {
                            foreach (Plug p in u.Plugs)
                            {
                                if (!subbedPlugs.Contains(p))
                                {
                                    subscribeToMQTT(p.id);
                                    subbedPlugs.Add(p);
                                    Console.WriteLine("sub");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to get plugs");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to get users");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }
        public void insertIntoInflux(string content, string id)
        {
            string token = "u8WIGv8E39T5zkqfJn-jMI6vQ5frC36rTnsOgzo0ROCUn8Jz__J9Pxn60HoqdAiTM7q8w-q2FE0Jy6-RDX4uOA==";
            const string bucket = "Shellies";
            const string orgid = "74dfbae189580d3d";
            using var client = new InfluxDBClient("http://localhost:8086/", token);

            using (var writeApi = client.GetWriteApi())
            {
                //writeApi.WriteRecord($"data,shellyid={id} power={content}", WritePrecision.Ns, bucket, orgid);
                var CurrentCultureInfo = new CultureInfo("en", false);
                CurrentCultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                CurrentCultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
                Thread.CurrentThread.CurrentUICulture = CurrentCultureInfo;
                Thread.CurrentThread.CurrentCulture = CurrentCultureInfo;
                CultureInfo.DefaultThreadCurrentCulture = CurrentCultureInfo;

                var point = PointData.Measurement("data")
                                     .Tag("shellyid", id)
                                     .Field("power", float.Parse(content, CurrentCultureInfo))
                                     .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                writeApi.WritePoint(point, bucket, orgid);
                Console.WriteLine("POWER: " + float.Parse(content, CurrentCultureInfo));

                Console.WriteLine("in insertInflux");
            }
        }
        public async void subscribeToMQTT(string id)
        {
            client.OnMessageReceived += (sender, args) =>
            {
                Console.WriteLine("Message Received " + id + ": " + args.PublishMessage.PayloadAsString);

                //InfluxDB Insert
                insertIntoInflux(args.PublishMessage.PayloadAsString, id);
            };
            Console.WriteLine($"Subscribing to {id}");
            try
            {
                var result = await client.SubscribeAsync("shellies/shellyplug-s-" + id + "/relay/0/power", QualityOfService.ExactlyOnceDelivery);
                Console.WriteLine(result.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to subscribe to MQTT once");
            }

        }
        public async Task connectToMQTT()
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
            try
            {
                var client = new MongoClient("mongodb+srv://Flo:shelly123@shelly.ikeunhp.mongodb.net/?retryWrites=true&w=majority");
                Console.WriteLine("In get Users");
                var mongodb = client.GetDatabase("BenutzerDatabase");
                var collection = mongodb.GetCollection<Benutzer>("Benutzer");
                var documents = await collection.Find(Builders<Benutzer>.Filter.Empty).ToListAsync();
                Console.WriteLine("FORTNITE");
                user = documents;
            }
            catch (Exception e)
            {
                Console.WriteLine("irgendwos");
            }
        }
    }
}