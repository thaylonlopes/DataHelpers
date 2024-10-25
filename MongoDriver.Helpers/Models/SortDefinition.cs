namespace MongoDriver.Helpers.Models
{
    public class SortDefinition
    {
        public string Field { get; set; }
        public bool Ascending { get; set; }

        public SortDefinition(string field, bool ascending = true)
        {
            Field = field;
            Ascending = ascending;
        }
    }

}
