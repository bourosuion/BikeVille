namespace BikeVille.BLogic
{
    public class MongoDbConfig
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? CustomerCollectionName { get; set; }
        public string? AdminCollectionName { get; set; }
    }
}
