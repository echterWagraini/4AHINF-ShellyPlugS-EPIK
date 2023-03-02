using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlazorApp1
{
    public class Benutzer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Benutzername { get; set; }
        public string Email { get; set; }
        public string Passwort { get; set; }
        public int MaxStromVerbrauch { get; set; }
    }
}
