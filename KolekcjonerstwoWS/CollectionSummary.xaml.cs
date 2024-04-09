using KolekcjonerstwoWS.Objects;

namespace KolekcjonerstwoWS;

public partial class CollectionSummary : ContentPage
{
	private Collection collection;

	public CollectionSummary(Collection collection)
	{
		InitializeComponent();

		this.collection = collection;

		InitializeValues();
	}

	private void InitializeValues()
	{
		int soldItemsAmount = 0;
		int toSellItemsAmount = 0;

		foreach(var item in collection.Items)
			if(item.Sold)
                soldItemsAmount++;
			else if (item.ToSell)
                toSellItemsAmount++;

        allItems.Text = $"Wszytkie przedmioty: {collection.Items.Count}";
        ownedItems.Text = $"Posiadane przedmioty: {collection.Items.Count-soldItemsAmount}";
        soldItems.Text = $"Sprzedane przedmioty: {soldItemsAmount}";
        toSellItems.Text = $"Przedmioty \"do sprzedania\": {toSellItemsAmount}";
    }
}