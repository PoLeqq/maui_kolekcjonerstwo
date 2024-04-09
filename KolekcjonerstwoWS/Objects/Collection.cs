namespace KolekcjonerstwoWS.Objects;

public class Collection
{
    public string Name { get; set; }
    public List<CollectionItem> Items { get; set; }

    public Collection(string name, List<CollectionItem> items)
    {
        Name = name;
        Items = items;
    }
}
