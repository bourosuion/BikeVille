using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace BikeVille.Models.MongoCredentials
{
    public class AdminCredentials
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [Required]
        public int AdminID { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public string? PasswordHash { get; set; } // Stored in DB
        public string? PasswordSalt { get; set; } // Stored in DB
                                                  // Convert DateTime to string during serialization
        private DateTime _modifiedDate;
        public string ModifiedDate
        {
            get => _modifiedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            set => _modifiedDate = DateTime.Parse(value);
        }
    }
}
