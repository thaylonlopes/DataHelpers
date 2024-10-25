using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDriver.Helpers.Interface.Document;

namespace MongoDriver.Helpers.Models
{
    public abstract class Document : IDocument
    {
        [BsonExtraElements]
        public BsonDocument ExtraElements { get; set; }
        public ObjectId Id { get; set; }
    }
}
