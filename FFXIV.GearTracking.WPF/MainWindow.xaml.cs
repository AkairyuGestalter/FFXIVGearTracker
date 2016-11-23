using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FFXIV.GearTracking.Core;
using FFXIV.GearTracking.Simulation;
using System.IO;

namespace FFXIV.GearTracking.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            LoadData();
            InitializeComponent();
            //ItemsGrid.ItemsSource = Common.gearDictWPF;
            //ItemsGrid.AutoGenerateColumns
        }

        private bool LoadData()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
            {
                return Common.Load(Properties.Settings.Default.SaveFile);
            }
            else if (File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFXIVGearTracker.DAT")))
            {
                return Common.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFXIVGearTracker.DAT"));
            }
            return false;
        }

        private void JobSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsGrid != null)
            {
                (ItemsGrid.ItemsSource as CollectionView).Refresh();
            }
        }

        private void GearListView_Filter(object sender, FilterEventArgs e)
        {
            Item i = (Item)e.Item;
            try
            {
                if (i.canEquip.Contains(Common.activeChar.currentJob))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
            catch
            {
                e.Accepted = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Jobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            object parent = ((CheckBox)sender).TemplatedParent;
        }
    }
}
