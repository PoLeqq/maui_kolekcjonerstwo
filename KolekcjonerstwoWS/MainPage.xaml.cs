using KolekcjonerstwoWS.Objects;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KolekcjonerstwoWS
{
    public partial class MainPage : ContentPage
    {
        private string dataFilePath;

        private List<Collection> collections;

        public MainPage()
        {
            InitializeComponent();

            dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "collections.txt");

            LoadCollectionsAsync();
        }

        private async Task LoadCollectionsAsync()
        {
            try
            {
                await LoadFromFile();
                UpdateListView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas ładowania kolekcji: {ex.Message}");
            }
        }

        private void OnRemoveCollectionButtonClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            Collection collection = button.BindingContext as Collection;

            if (collection != null)
            {
                collections.Remove(collection);
                SaveCollections();

                UpdateListView();
            }
        }

        private void UpdateListView()
        {
            listView.ItemsSource = null;
            listView.ItemsSource = collections;
        }

        public async Task SaveCollections()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var collection in collections)
                {
                    sb.AppendLine($"{collection.Name}");
                    foreach (var item in collection.Items)
                    {
                        sb.AppendLine($"{item.Id},{item.Name},{item.Description},{item.Price},{item.Rate},{item.Sold},{item.ToSell}");
                    }
                }

                using (StreamWriter writer = new StreamWriter(dataFilePath))
                {
                    await writer.WriteAsync(sb.ToString());
                }

                Debug.WriteLine($"Zapisano do pliku: {dataFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas zapisu do pliku: {ex.Message}");
                Debug.WriteLine($"Próbowano utworzyć plik: {dataFilePath}");
            }
        }

        public async Task LoadFromFile()
        {
            try
            {
                if (!File.Exists(dataFilePath))
                {
                    using (File.Create(dataFilePath)) { }
                }

                List<Collection> loadedCollections = new List<Collection>();
                using (StreamReader reader = new StreamReader(dataFilePath))
                {
                    string line;
                    Collection currentCollection = null;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        if (!line.Contains(","))
                        {
                            if (currentCollection != null)
                            {
                                loadedCollections.Add(currentCollection);
                            }
                            currentCollection = new Collection(line.Trim(), new List<CollectionItem>());
                        }
                        else
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 7)
                            {
                                int id = int.Parse(parts[0]);
                                string name = parts[1];
                                string description = parts[2];
                                int price = int.Parse(parts[3]);
                                int rate = int.Parse(parts[4]);
                                bool sold = bool.Parse(parts[5]);
                                bool toSell = bool.Parse(parts[6]);
                                currentCollection.Items.Add(new CollectionItem
                                {
                                    Id = id,
                                    Name = name,
                                    Description = description,
                                    Price = price,
                                    Rate = rate,
                                    Sold = sold,
                                    ToSell = toSell
                                });
                            }
                        }
                    }
                    if (currentCollection != null)
                    {
                        loadedCollections.Add(currentCollection);
                    }
                }

                collections = loadedCollections;

                Debug.WriteLine("Załadowano z pliku!");
            }
            catch (Exception ex)
            {
                collections = new List<Collection>();
                Debug.WriteLine($"Błąd podczas odczytu z pliku: {ex.Message}");
            }
        }

        private void OnShowCollectionButtonClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Collection collection = button.BindingContext as Collection;

            if (collection != null)
                Navigation.PushAsync(new CollectionPage(collection, this));
        }

        private void OnAddCollectionButtonClicked(object sender, EventArgs e)
        {
            string collectionName = inputCollectionName.Text;
            inputCollectionName.Text = "";

            collections.Add(new Collection(collectionName, new List<CollectionItem>()));
            SaveCollections();
            UpdateListView();
        }
    }

    
}