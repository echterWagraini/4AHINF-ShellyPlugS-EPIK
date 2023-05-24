using Microsoft.AspNetCore.Components.Authorization;
using MongoDB.Driver;

namespace BlazorApp1.Data
{
    public class NotificationHostedSercie : BackgroundService
    {

        private readonly ILogger _logger;
        private List<Benutzer> user = null;

        public NotificationHostedSercie(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<NotificationHostedSercie>();
        }


        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            do
            {
                if (DateTime.Now.Hour == 16)
                {
                    getUsers();
                    if (user != null)
                    {
                        foreach(Benutzer b in user)
                        {
                            Notification n = new Notification();
                            n.message = "Eine tolle Benachrichtigung";
                            b.Notifications.Add(n);
                            UpdateUser(b);
                        }
                    }
                }
                //fired every one hour
                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }
        public async void getUsers()
        {
            var client = new MongoClient("mongodb+srv://Flo:shelly123@shelly.ikeunhp.mongodb.net/?retryWrites=true&w=majority");
            var mongodb = client.GetDatabase("BenutzerDatabase");
            var collection = mongodb.GetCollection<Benutzer>("Benutzer");
            var documents = await collection.Find(Builders<Benutzer>.Filter.Empty).ToListAsync();
            user = documents;

        }
        public void UpdateUser(Benutzer currentUser)
        {
            var client = new MongoClient("mongodb+srv://Flo:shelly123@shelly.ikeunhp.mongodb.net/?retryWrites=true&w=majority");
            var mongodb = client.GetDatabase("BenutzerDatabase");
            var collection = mongodb.GetCollection<Benutzer>("Benutzer");

            var filter = Builders<Benutzer>.Filter.Eq("Email", currentUser.Email);
            var updater = Builders<Benutzer>.Update.Set("Notifications", currentUser.Notifications);

            var benutzer = collection.Find(filter).FirstOrDefault();

            collection.FindOneAndUpdate(filter, updater);
        }
    }
}