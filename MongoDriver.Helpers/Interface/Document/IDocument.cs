namespace MongoDriver.Helpers.Interface.Document
{
    public interface IDocument<TKey> : IDocument where TKey : struct
    {
        public TKey Id { get; set; }
    }

    public interface IDocument { }
}
