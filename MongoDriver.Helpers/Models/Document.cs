using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDriver.Helpers.Interface.Document;

namespace MongoDriver.Helpers.Models;

public abstract class Document<TKey> : IDocument<TKey> where TKey : struct
{
    public TKey Id { get; set; }

    [BsonExtraElements]
    public BsonDocument ExtraElements { get; set; } = new BsonDocument();

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return Id.Equals(((Document<TKey>)obj).Id);
    }

    public static bool operator ==(Document<TKey>? left, Document<TKey>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Id.Equals(right.Id);
    }

    public static bool operator !=(Document<TKey>? left, Document<TKey>? right) => !(left == right);
}
