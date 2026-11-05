using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace GUI
{
    public partial class GenerateDataWindow : Window
    {
        public string OutputPath { get; private set; } = string.Empty;
        public int RecordCount { get; private set; }
        public double MinLatitude { get; private set; }
        public double MinLongitude { get; private set; }
        public double MaxLatitude { get; private set; }
        public double MaxLongitude { get; private set; }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        public GenerateDataWindow()
        {
            InitializeComponent();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Vyberte priečinok pre vygenerovaný CSV súbor"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            var fileName = Path.GetFileName(OutputPathBox.Text);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "generated_data.csv";
            }

            OutputPathBox.Text = Path.Combine(dialog.FolderName, fileName);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OutputPathBox.Text))
            {
                ShowValidationError("Zadajte cestu k výstupnému CSV súboru.");
                return;
            }

            if (!int.TryParse(CountBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count < 1)
            {
                ShowValidationError("Počet bodov musí byť kladné celé číslo.");
                return;
            }

            if (!TryParseCoordinate(MinLatitudeBox.Text, out double minLatitude) ||
                !TryParseCoordinate(MinLongitudeBox.Text, out double minLongitude) ||
                !TryParseCoordinate(MaxLatitudeBox.Text, out double maxLatitude) ||
                !TryParseCoordinate(MaxLongitudeBox.Text, out double maxLongitude))
            {
                ShowValidationError("Hranice oblasti musia byť platné čísla.");
                return;
            }

            if (minLatitude < -90 || maxLatitude > 90 || minLatitude > maxLatitude)
            {
                ShowValidationError("Zemepisná šírka musí byť v rozsahu -90 až 90 a minimum nesmie byť väčšie ako maximum.");
                return;
            }

            if (minLongitude < -180 || maxLongitude > 180 || minLongitude > maxLongitude)
            {
                ShowValidationError("Zemepisná dĺžka musí byť v rozsahu -180 až 180 a minimum nesmie byť väčšie ako maximum.");
                return;
            }

            OutputPath = OutputPathBox.Text.Trim();
            RecordCount = count;
            MinLatitude = minLatitude;
            MinLongitude = minLongitude;
            MaxLatitude = maxLatitude;
            MaxLongitude = maxLongitude;
            DialogResult = true;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        private static bool TryParseCoordinate(string text, out double coordinate)
        {
            string normalized = text.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate)
                   && double.IsFinite(coordinate);
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 75
        private void ShowValidationError(string message)
        {
            MessageBox.Show(this, message, "Neplatné vstupy", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
