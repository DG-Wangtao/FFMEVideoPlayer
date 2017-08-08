using FFMEVideoPlayer.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace FFMEVideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<PlayerControl> playerControls;
        public  ObservableCollection<PlayerControl> PlayerControls {
            get { return playerControls; }
            set
            {
                playerControls = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(PlayerControls)));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            //PlayerBox.ItemsSource = PlayerControls;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(()=> {
                //PlayerControl player = new PlayerControl() { SourceStr = @"//192.168.17.174:8554/live/main", Width = 400, Height = 400 };
                //lock(new object())
                //{
                //    PlayerControls.Add(player);
                //}
            //});
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
