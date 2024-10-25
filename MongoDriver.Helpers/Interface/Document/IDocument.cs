using MongoDB.Bson;

namespace MongoDriver.Helpers.Interface.Document
{
    public interface IDocument
    {
        ObjectId Id { get; set; }
    }
}
