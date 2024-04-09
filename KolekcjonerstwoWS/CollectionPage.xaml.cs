using KolekcjonerstwoWS.Objects;
using System.Diagnostics;
using System.Text;

namespace KolekcjonerstwoWS;

public partial class CollectionPage : ContentPage
{
    public string CollectionName { 
        get
        {
            return Collection.Name;
        }
    }
    public Collection Collection;

    private MainPage MainPage;

    public CollectionPage(Collection collection, MainPage mainPage)
    {
        InitializeComponent();
        Collection = collection;
        MainPage = mainPage;

        BindingContext = this;
        UpdateView();
    }

    public void UpdateView()
    {
        collectionItems.ItemsSource = null;

        var groupedItems = Collection.Items
            .GroupBy(i => i.Sold)
            .OrderBy(g => g.Key) 
            .SelectMany(g => g.OrderBy(i => i.Id)) 
            .ToList();

        Collection.Items.Clear();
        foreach (var groupedItem in groupedItems)
            Collection.Items.Add(groupedItem);

        collectionItems.ItemsSource = Collection.Items;
    }

    private void OnAddItemButtonClicked(object sender, EventArgs e)
    {
        AsyncOnAddItemButtonClicked();
    }

    private void OnSellButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int itemId)
        {
            var itemToSell = Collection.Items.FirstOrDefault(item => item.Id == itemId);
            if (itemToSell != null)
            {
                Collection.Items.Remove(itemToSell);

                Collection.Items.Add(itemToSell);

                itemToSell.Sold = true;

                MainPage.SaveCollections();
                UpdateView();
            }
        }
    }

    private void OnSummaryButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CollectionSummary(Collection));
    }

    private async void OnExportButtonClicked(object sender, EventArgs e)
    {
        try
        {
            string fileName = $"KolekcjonerstwoWS-{Collection.Name}-{Guid.NewGuid()}.txt";
            string downloadsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(downloadsFolderPath, fileName);

            await ExportCollectionToFile(filePath);
            await DisplayAlert("Sukces", $"Kolekcja zosta³a pomyœlnie wyeksportowana do pliku. SprawdŸ folder \"dokumenty\", szukaj pliku {fileName}", "OK");
            Debug.WriteLine(filePath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"Wyst¹pi³ b³¹d podczas eksportu kolekcji: {ex.Message}", "OK");
        }
    }

    private async Task ExportCollectionToFile(string filePath)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Collection.Name}");
            foreach (var item in Collection.Items)
                sb.AppendLine($"{item.Id},{item.Name},{item.Description},{item.Price},{item.Rate},{item.Sold},{item.ToSell}");

            await File.WriteAllTextAsync(filePath, sb.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas zapisu kolekcji do pliku: {ex.Message}");
        }
    }

    private async void OnImportButtonClicked(object sender, EventArgs e)
    {
        try
        {
            FileResult file = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz plik do importu"
            });

            int addedItems = -1;
            if (file != null)
            {
                addedItems++;

                string importedData = string.Empty;
                using (Stream stream = await file.OpenReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        importedData = await reader.ReadToEndAsync();
                    }
                }

                string[] lines = importedData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                int nextId = Collection.Items.Count > 0 ? Collection.Items.Max(item => item.Id) + 1 : 0;

                foreach (var line in lines)
                {
                    string[] fields = line.Split(',');
                    if (fields.Length >= 7)
                    {
                        string name = fields[1];
                        string description = fields[2];
                        int price = int.Parse(fields[3]);
                        int rate = int.Parse(fields[4]);
                        bool sold = bool.Parse(fields[5]);
                        bool toSell = bool.Parse(fields[6]);

                        addedItems++;

                        Collection.Items.Add(new CollectionItem
                        {
                            Id = nextId++,
                            Name = name,
                            Description = description,
                            Price = price,
                            Rate = rate,
                            Sold = sold,
                            ToSell = toSell
                        });
                    }
                }

                UpdateView();
                MainPage.SaveCollections();
            }

            if(addedItems == -1)
                await DisplayAlert("B³¹d", "Wyst¹pi³ nieznany b³¹d podczas odczytu pliku", "OK");
            else if(addedItems == 0)
                await DisplayAlert("Ostrze¿enie", "Plik zosta³ odczytany, jednak nie dodano ¿adnych elementów. Upewnij siê, ¿e jest to poprawny plik", "OK");
            else
                await DisplayAlert("Sukces", $"Dane zosta³y pomyœlnie zaimportowane. Iloœæ nowych przedmiotów: {addedItems}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"Wyst¹pi³ b³¹d podczas importu danych: {ex.Message}", "OK");
        }
    }

    private async Task AsyncOnAddItemButtonClicked()
    {
        CollectionItem item = new CollectionItem();
        if (Collection.Items.Count == 0)
            item.Id = 0;
        else
            item.Id = Collection.Items.ElementAt(Collection.Items.Count-1).Id+1;

        item.Name = inputItemName.Text;
        item.Description = inputItemDescription.Text;
        try
        {
            item.Price = Int32.Parse(inputItemPrice.Text);
        } catch (Exception)
        {
            DisplayAlert("B³¹d", "Cena musi byæ liczb¹ natuarln¹!", "OK");
            return;
        }
        
        try
        {
            item.Rate = Int32.Parse(inputItemRate.Text);
        } catch (Exception)
        {
            DisplayAlert("B³¹d", "Ocena musi byæ liczb¹ naturaln¹ z przednia³u od 0 do 10!", "OK");
            return;
        }

        foreach(CollectionItem colItem in Collection.Items)
        {
            if (item.Name == colItem.Name && (!await DisplayAlert("Ostrze¿enie", "Przedmiot o takiej nazwie ju¿ istnieje. Czy chcesz kontynuowaæ?", "Tak", "Nie")))
                return;
            else
                break;
        }

        Collection.Items.Add(item);

        UpdateView();
        MainPage.SaveCollections();

        inputItemName.Text = "";
        inputItemDescription.Text = "";
        inputItemPrice.Text = "";
        inputItemRate.Text = "";
    }

    private void OnItemEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int itemId)
        {
            var itemToEdit = Collection.Items.FirstOrDefault(item => item.Id == itemId);
            if (itemToEdit != null)
            {
                var editPage = new EditItemPage(MainPage, this, itemToEdit);
                Navigation.PushAsync(editPage);
            }
        }
    }

    private void OnItemRemoveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int itemId)
        {
            var itemToRemove = Collection.Items.FirstOrDefault(item => item.Id == itemId);
            if (itemToRemove != null)
            {
                Collection.Items.Remove(itemToRemove);
                UpdateView();
                MainPage.SaveCollections();
            }
        }
    }
}