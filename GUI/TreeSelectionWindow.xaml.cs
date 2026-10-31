using System.Windows;
using PoiManager.Model;

namespace GUI
{
    // Dialog pri spusteni - pouzivatel zvoli strom, ktory sa pocas behu nemeni
    // Vygenerovane / upravene pomocou AI - Kapitola 69
    public partial class TreeSelectionWindow : Window
    {
        public PoiModel.TreeType SelectedTreeType { get; private set; }

        public TreeSelectionWindow()
        {
            InitializeComponent();
        }

        // Vygenerovane / upravene pomocou AI - Kapitola 69
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (KDTreeRadio.IsChecked == true)
            {
                SelectedTreeType = PoiModel.TreeType.KDTree;
            }
            else if (HybridQTreeRadio.IsChecked == true)
            {
                SelectedTreeType = PoiModel.TreeType.HybridQTree;
            }
            else
            {
                SelectedTreeType = PoiModel.TreeType.QTree;
            }

            DialogResult = true;
        }
    }
}
