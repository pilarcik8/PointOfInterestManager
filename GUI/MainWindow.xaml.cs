using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PoiManager.Model;
using PoiManager.Controller;
using Microsoft.Win32;

namespace GUI
{
    // Vygenerovane / upravene pomocou AI - Kapitola 67
    // View - iba zbiera vstupy, vola controller a zobrazuje vysledky
    public partial class MainWindow : Window
    {
        private readonly PoiController _controller;
        private readonly ObservableCollection<PointOfInterest> _results = new();
        private PointOfInterest? _editTarget;

        public MainWindow(PoiModel.TreeType treeType)
        {
            InitializeComponent();

            _controller = new PoiController(treeType);
            Title = $"POI Manager - {treeType}";
            TreeTypeText.Text = $"Strom: {treeType}";
            ResultsGrid.ItemsSource = _results;
            SetStatus($"Nová evidencia ({treeType}).");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void ShowResults(IEnumerable<PointOfInterest> pois, string header)
        {
            _results.Clear();
            foreach (var poi in pois)
            {
                _results.Add(poi);
            }
            ResultsHeader.Text = $"{header} - počet: {_results.Count}";
            SetStatus($"{header}: nájdených {_results.Count} bodov.");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void ClearResults()
        {
            _results.Clear();
            ResultsHeader.Text = "Výsledky";
            ClearEditForm();
        }

        private void SetStatus(string text)
        {
            StatusText.Text = $"[{DateTime.Now:HH:mm:ss}] {text}";
        }

        // Chyby z controllera (zly vstup, chyba suboru) zobrazime pouzivatelovi
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void OperationTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Udalost bubla aj z vnorenych ovladacich prvkov
            if (e.OriginalSource != OperationTabs)
            {
                return;
            }

            // Editovat / mazat sa da len z vysledkov vyhladavania v danej zalozke
            if (OperationTabs.SelectedIndex is 3 or 5)
            {
                ClearResults();
            }
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationTabs.SelectedIndex != 3)
            {
                return;
            }

            if (ResultsGrid.SelectedItem is not PointOfInterest poi)
            {
                ClearEditForm();
                return;
            }

            _editTarget = poi;
            EditSelectedText.Text = $"Editujete bod ID {poi.Id}.";
            EditNameBox.Text = poi.Name;
            EditDescBox.Text = poi.Description;
            EditLatBox.Text = poi.Latitude.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            EditLonBox.Text = poi.Longitude.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            EditAddrBox.Text = poi.Address ?? string.Empty;
            EditForm.IsEnabled = true;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void ClearEditForm()
        {
            _editTarget = null;
            EditSelectedText.Text = "Vyberte bod v tabuľke výsledkov.";
            EditNameBox.Text = EditDescBox.Text = EditLatBox.Text = EditLonBox.Text = EditAddrBox.Text = string.Empty;
            EditForm.IsEnabled = false;
        }

        // ---- Panel nastroje ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "CSV súbory (*.csv)|*.csv|Všetky súbory (*.*)|*.*" };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            Run(() =>
            {
                _controller.Import(dialog.FileName);
                ShowResults(_controller.GetAll(), "Všetky body po importe");
            });
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "CSV súbory (*.csv)|*.csv", FileName = "poi.csv" };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            Run(() =>
            {
                _controller.Export(dialog.FileName);
                SetStatus($"Exportované do {dialog.FileName}.");
            });
        }

        private void ChooseGeneratorFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Vyberte priečinok pre vygenerovaný CSV súbor"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            var fileName = Path.GetFileName(GeneratorPathBox.Text);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "generated_data.csv";
            }

            GeneratorPathBox.Text = Path.Combine(dialog.FolderName, fileName);
            SetStatus($"Vybraný priečinok: {dialog.FolderName}.");
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 71
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            Run(() =>
            {
                if (!int.TryParse(GeneratorCountBox.Text, out var count))
                {
                    throw new FormatException("Počet záznamov musí byť celé číslo.");
                }

                PoiController.GenerateTestData(GeneratorPathBox.Text, count);
                SetStatus($"Vygenerovaných {count} záznamov do {Path.GetFullPath(GeneratorPathBox.Text)}.");
            });
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            Run(() => ShowResults(_controller.GetAll(), "Všetky body"));
        }

        // ---- 1. Vyhladanie na pozicii ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67

        private void FindAtPosition_Click(object sender, RoutedEventArgs e)
        {
            Run(() => ShowResults(
                _controller.FindAtPosition(PosLat1Box.Text, PosLon1Box.Text, PosLat2Box.Text, PosLon2Box.Text),
                "Body v obdĺžniku"));
        }

        // ---- 2. Vyhladanie v okoli ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67

        private void FindAround_Click(object sender, RoutedEventArgs e)
        {
            Run(() => ShowResults(
                _controller.FindAround(AroundLatBox.Text, AroundLonBox.Text, AroundRadiusBox.Text),
                "Body v okolí"));
        }

        // ---- 3. Novy bod ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            Run(() =>
            {
                _controller.Create(CreateNameBox.Text, CreateDescBox.Text, CreateLatBox.Text, CreateLonBox.Text,
                    CreateAddrBox.Text);
                CreateNameBox.Text = CreateDescBox.Text = CreateLatBox.Text = CreateLonBox.Text = CreateAddrBox.Text =
                    string.Empty;
                ShowResults(_controller.GetAll(), "Všetky body");
                SetStatus("Bod bol vytvorený.");
            });
        }

        // ---- 4. Editacia ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void EditSearch_Click(object sender, RoutedEventArgs e)
        {
            Run(() =>
            {
                ClearEditForm();
                ShowResults(
                    _controller.FindAround(EditSearchLatBox.Text, EditSearchLonBox.Text, EditSearchRadiusBox.Text),
                    "Body na editáciu");
            });
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void EditSave_Click(object sender, RoutedEventArgs e)
        {
            if (_editTarget == null)
            {
                return;
            }

            Run(() =>
            {
                _controller.Update(_editTarget, EditNameBox.Text, EditDescBox.Text, EditLatBox.Text, EditLonBox.Text,
                    EditAddrBox.Text);
                // PointOfInterest nema notifikacie zmien, tabulku treba prekreslit
                ResultsGrid.Items.Refresh();
                SetStatus($"Bod ID {_editTarget.Id} bol upravený.");
            });
        }

        // ---- 5. Vyhladanie podla nazvu ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void FindByName_Click(object sender, RoutedEventArgs e)
        {
            Run(() => ShowResults(
                _controller.FindByName(NameLat1Box.Text, NameLon1Box.Text, NameLat2Box.Text, NameLon2Box.Text,
                    NameTextBox.Text),
                "Body podľa názvu"));
        }

        // ---- 6. Vyradenie ----
        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void DeleteSearch_Click(object sender, RoutedEventArgs e)
        {
            Run(() => ShowResults(
                _controller.FindAround(DeleteLatBox.Text, DeleteLonBox.Text, DeleteRadiusBox.Text),
                "Body na vyradenie"));
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 67
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsGrid.SelectedItem is not PointOfInterest poi)
            {
                MessageBox.Show(this, "Najprv vyberte bod v tabuľke.", "Vyradenie", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show(this, $"Naozaj vymazať bod ID {poi.Id} '{poi.Name}'?", "Vyradenie",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            Run(() =>
            {
                _controller.Delete(poi);
                _results.Remove(poi);
                ResultsHeader.Text = $"Body na vyradenie - počet: {_results.Count}";
                SetStatus($"Bod ID {poi.Id} bol vymazaný.");
            });
        }
    }
}
