using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BusinessLayer.Model;


namespace KlantBestellingen.WPF
{
    /// <summary>
    /// Interaction logic for Bestellingen.xaml
    /// </summary>
    public partial class Bestellingen : Window
    {
        private ObservableCollection<Bestelling> _bestellingen = null;
        public Bestellingen()
        {
            InitializeComponent();
            _bestellingen = new ObservableCollection<Bestelling>(Context.BestellingManager.HaalOp());
            dgBestellingen.ItemsSource = _bestellingen;
            _bestellingen.CollectionChanged += _bestellingen_CollectionChanged;
        }
        private void _bestellingen_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Bestelling bestelling in e.OldItems)
                {
                    Context.BestellingManager.Verwijder(bestelling);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Bestelling bestelling in e.NewItems)
                {
                    Context.BestellingManager.VoegToe(bestelling);
                }
            }
        }
        private void dgBestellingen_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (Key.Delete == e.Key)
            {
                if (!(MessageBox.Show("Zeker dat je de bestelling wenst te verwijderen?", "Bevestig.", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
                {
                    // Cancel Delete.
                    e.Handled = true;
                    return;
                }

                // We moeten een while gebruiken en telkens testen want met foreach treden problemen op omdat de verzameling intussen telkens wijzigt!
                while (grid.SelectedItems.Count > 0)
                {
                    var row = grid.SelectedItems[0];
                    _bestellingen.Remove(row as Bestelling);
                }
            }
        }

        private void Tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbKlantId.Text))
            {
                BtnNieuweBestelling.IsEnabled = true;
            }
            else
            {
                BtnNieuweBestelling.IsEnabled = false;
            }
        }

        private void TbKlantId_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbKlantId.Text))
            {
                BtnNieuweBestelling.IsEnabled = true;
            }
            else
            {
                BtnNieuweBestelling.IsEnabled = false;
            }
        }

        private void Verwijder_Button_Click(object sender, RoutedEventArgs e)
        {
            while (dgBestellingen.SelectedItems.Count > 0)
            {
                var row = dgBestellingen.SelectedItems[0];
                _bestellingen.Remove(row as Bestelling);
            }
        }

        private void BtnNieuweBestelling_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbKlantId?.Text))
            {
                MessageBox.Show("Geef alle bestellinggegevens op!");
                return;
            }

            var bestelling = new Bestelling(0, Context.KlantManager.HaalOp(long.Parse(TbKlantId.Text)), DateTime.Now);
            _bestellingen.Add(bestelling);
        }
    }
}
