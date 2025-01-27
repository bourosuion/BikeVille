using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BikeVille.Models.MongoCredentials
{
    public class CustomerCredentials
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int CustomerID { get; set; }
        [Required]
        public string? CompanyName { get; set; }
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        private DateTime _modifiedDate;
        public string ModifiedDate
        {
            get => _modifiedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            set => _modifiedDate = DateTime.Parse(value);
        }
    }
}
