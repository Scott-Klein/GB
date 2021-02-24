using GB.Emulator;
using GBemu;
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
//using GB.Emulator;
namespace GBui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += LoadLibrary;
        }

        //Load roms from config.
        private void LoadLibrary(object sender, RoutedEventArgs e)
        {
            //this.Library.Items.
        }

        private void AddRomButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".gb";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true && openFileDlg.FileName.EndsWith(".gb"))
            {
                this.Library.Items.Add(new RomListItem(openFileDlg.FileName));
            }
            else
            {
                MessageBox.Show("Hello, world!");
            }
        }

        private void PlaySelectedGame(object sender, RoutedEventArgs e)
        {
            var rom = this.Library.SelectedItem as RomListItem;
            if (rom is not null)
            {
                GameLaunch(rom.Path);
            }
            else
            {
                MessageBox.Show("Please select the game you wish to play before pressing play.");
            }
        }

        [STAThread]
        static void GameLaunch(string path)
        {
            using (var game = new Game1(path))
                game.Run();
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class RomListItem
    {
        public RomListItem(string title)
        {
            Path = title;
            RomTitle = "Game Boy Game";
            cart = new Cartridge(Path);
            RomTitle = cart.Info.Name;
            Type = cart.Info.Type;
        }
        Cartridge cart;
        public CartridgeType Type { get; set; }
        public string Path { get; set; }
        public string RomTitle { get; set; }
    }
}
