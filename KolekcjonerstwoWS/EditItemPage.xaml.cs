using KolekcjonerstwoWS.Objects;

namespace KolekcjonerstwoWS;

public partial class EditItemPage : ContentPage
{
    private MainPage mainPage;
    private CollectionPage collectionPage;
    private CollectionItem item;
    private string startName;

    public EditItemPage(MainPage mainPage, CollectionPage collectionPage, CollectionItem item)
    {
        InitializeComponent();

        this.mainPage = mainPage;
        this.collectionPage = collectionPage;
        this.item = item;
        this.startName = item.Name;

        inputName.Text = item.Name;
        inputDescription.Text = item.Description;
        inputPrice.Text = item.Price.ToString();
        inputRate.Text = item.Rate.ToString();
        inputToSell.IsToggled = item.ToSell;

        BindingContext = item;
    }

    private void OnSaveEditClicked(object sender, EventArgs e)
    {
        AsyncOnSaveEditClicked();
    }

    private async Task AsyncOnSaveEditClicked()
    {
        item.Name = inputName.Text;
        item.Description = inputDescription.Text;

        try
        {
            item.Price = Int32.Parse(inputPrice.Text);
        }
        catch (Exception)
        {
            DisplayAlert("B³¹d", "Cena musi byæ liczb¹ natuarln¹!", "OK");
            return;
        }

        try
        {
            item.Rate = Int32.Parse(inputRate.Text);
        }
        catch (Exception)
        {
            DisplayAlert("B³¹d", "Ocena musi byæ liczb¹ naturaln¹ z przednia³u od 0 do 10!", "OK");
            return;
        }

        item.ToSell = inputToSell.IsToggled;

        List<CollectionItem> items = new List<CollectionItem>(collectionPage.Collection.Items);
        items.Remove(item);
        
        if(startName != inputName.Text)
        {
            foreach (CollectionItem colItem in items)
            {
                if (item.Name == colItem.Name && (!await DisplayAlert("Ostrze¿enie", "Przedmiot o takiej nazwie ju¿ istnieje. Czy chcesz kontynuowaæ?", "Tak", "Nie")))
                    return;
                else
                    break;
            }
        }

        collectionPage.UpdateView();
        mainPage.SaveCollections();
        Navigation.PopAsync();
    }
}