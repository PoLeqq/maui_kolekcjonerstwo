namespace KolekcjonerstwoWS.Objects;

public class CollectionItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    private int _price;
    public int Price
    {
        get
        {
            return _price;
        }
        set
        {
            if (value < 0) value = 0;
            _price = value;
        }
    }
    private int _rate;
    public int Rate
    {
        get
        {
            return _rate;
        }
        set
        {
            if (value > 10) value = 10;
            if (value < 0) value = 0;
            _rate = value;
        }
    }
    public bool Sold { get; set; } 
    public bool ToSell { get; set; }
}
