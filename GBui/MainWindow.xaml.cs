﻿using GB.Emulator;
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
        public List<RomListItem> Roms { get; set; }
        public string SelectedTItle { get; set; }

        public bool Playing { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += LoadLibrary;
        }

        //Load roms from config.
        private void LoadLibrary(object sender, RoutedEventArgs e)
        {
            Roms = CatBoyConfig.DeserialiseLibrary();
            foreach (var item in Roms)
            {
                this.Library.Items.Add(item);
            }
        }

        private void AddRomButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".gb";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true && openFileDlg.FileName.EndsWith(".gb"))
            {
                var selectedRom = new RomListItem(openFileDlg.FileName);
                this.Library.Items.Add(selectedRom);
                Roms.Add(selectedRom);
                CatBoyConfig.SaveLibrary(Roms);
            }
            else
            {
                MessageBox.Show("You haven't found a Game Boy ROM.");
            }
        }

        private void PlaySelectedGame(object sender, RoutedEventArgs e)
        {
            if (Playing)
            {
                return;
            }
            var rom = this.Library.SelectedItem as RomListItem;
            if (rom is not null)
            {
                Playing = true;
                GameLaunch(rom.Path);
            }
            else
            {
                MessageBox.Show("Please select the game you wish to play before pressing play.");
            }
        }

        [STAThread]
        void GameLaunch(string path)
        {
            
            using (var game = new Game1(path))
            {
                game.Run();
                
                game.Disposed += Game_Disposed;
            }
                
        }

        private void Game_Disposed(object sender, EventArgs e)
        {
            Playing = false;
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming soon...");
        }

        private void Library_Selected(object sender, RoutedEventArgs e)
        {
            var rom = this.Library.SelectedItem as RomListItem;
            var item = this.Library.SelectedItem as ComboBoxItem;
            SelectedTitleLabel.Content = rom?.RomTitle ?? "";
        }

        private void RomCartColor_Changed(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class RomListItem
    {
        //default ctor for json deserialiser.
        public RomListItem()
        {

        }
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
