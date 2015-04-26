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
            InitializeComponent();
            LoadData();
            ItemsGrid.ItemsSource = Common.gearDictWPF;
        }

        private bool LoadData()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SaveFile))
            {
                return Common.Load(Properties.Settings.Default.SaveFile);
            }
            else if (File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFXIVGearTracker.DAT")))
            {
                Common.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFXIVGearTracker.DAT"));
            }
            return true;
        }
    }
}
