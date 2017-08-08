using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace FFMEVideoPlayer.Player
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl, INotifyPropertyChanged
    {
        private DelegateCommand m_PauseCommand = null;
        private DelegateCommand m_PlayCommand = null;
        private DelegateCommand m_StopCommand = null;

        public DelegateCommand PauseCommand
        {
            get
            {
                if (m_PauseCommand == null)
                    m_PauseCommand = new DelegateCommand(() => { Media.Pause(); });

                return m_PauseCommand;
            }
        }
        public DelegateCommand PlayCommand
        {
            get
            {
                if (m_PlayCommand == null)
                    m_PlayCommand = new DelegateCommand(()=> {
                        Media.Play();
                    });
                return m_PlayCommand;
            }
        }
        public DelegateCommand StopCommand
        {
            get
            {
                if (m_StopCommand == null)
                    m_StopCommand = new DelegateCommand(() => { Media.Stop(); });

                return m_StopCommand;
            }
        }
        public Visibility ButtonPauseVisible { get; set; }
        public Visibility ButtonPlayVisible { get; set; }
        public Visibility ProgressDowndloadVisible { get; set; }
        public Visibility SeekBarVisibility { get; set; } = Visibility.Visible;
        private Visibility commandGridVisibility;
        public Visibility CommandGridVisibility {
            get { return commandGridVisibility; }
            set
            {
                commandGridVisibility = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(CommandGridVisibility)));
                    Task.Run(() => {
                        if (commandGridVisibility == Visibility.Visible)
                        {
                            lock (new object()) { 
                            if (IsSetVisibility)
                                return;
                            }
                            IsSetVisibility = true;
                            Thread.Sleep(5000);
                            CommandGridVisibility = Visibility.Collapsed;
                            IsSetVisibility = false;
                        }
                    });
            }
        }
        public double DownloagProcess { get; set; }

        private readonly Dictionary<string, Action> PropertyUpdaters;
        private readonly Dictionary<string, string[]> PropertyTriggers;
        public PlayerControl()
        {
            this.Dispatcher.BeginInvoke((Action)delegate {
                InitializeComponent();
                CommandGridVisibility = Visibility.Collapsed;

                //必须在为播放器指定uri之前设置FFmpegDirectory的值
                //它引用一些依赖的.dll和.exe文件，该文件在../bin/DLL/中
                Unosquare.FFME.MediaElement.FFmpegDirectory = Directory.GetCurrentDirectory() + "\\DLL\\";

            
                //当视频播放器的属性发生改变时，如开始播放，停止播放
                //同时修改由播放状态决定的属性值
                Media.PropertyChanged += Media_PropertyChanged;

                this.Unloaded += PlayerControl_Unloaded;

                //鼠标移动
                Media.MouseMove += Media_MouseMove;
                //鼠标退出
                Media.MouseLeave += Media_MouseLeave;
            });

            PropertyUpdaters = new Dictionary<string, Action>
            {
                { nameof(ButtonPauseVisible), () => { ButtonPauseVisible =Media.CanPause && Media.IsPlaying ? Visibility.Visible : Visibility.Collapsed; } },
                { nameof(ButtonPlayVisible), () => { ButtonPlayVisible = Media.IsOpen && Media.IsPlaying == false && Media.HasMediaEnded == false ? Visibility.Visible : Visibility.Collapsed; } },
                { nameof(ProgressDowndloadVisible), () => { ProgressDowndloadVisible = Media.IsOpen && Media.HasMediaEnded == false  && ((Media.DownloadProgress > 0d && Media.DownloadProgress < 0.95) || Media.IsLiveStream) ? Visibility.Visible : Visibility.Hidden; } },
                { nameof(DownloagProcess),()=> { DownloagProcess=Media.DownloadProgress; } },
                { nameof(SeekBarVisibility), () => { SeekBarVisibility = Media.IsSeekable ? Visibility.Visible : Visibility.Hidden; } },
            };
            PropertyTriggers = new Dictionary<string, string[]>
            {
                { nameof(Media.IsOpen), PropertyUpdaters.Keys.ToArray() },
                { nameof(Media.IsOpening), PropertyUpdaters.Keys.ToArray() },
                { nameof(Media.MediaState), PropertyUpdaters.Keys.ToArray() },
                { nameof(Media.HasMediaEnded), PropertyUpdaters.Keys.ToArray() },
                { nameof(Media.DownloadProgress),  PropertyUpdaters.Keys.ToArray() },
                { nameof(Media.IsSeekable),  PropertyUpdaters.Keys.ToArray() },
            };
        }

        private void PlayerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Media.Close();
        }

        private bool IsSetVisibility = false;
        private void Media_MouseLeave(object sender, MouseEventArgs e)
        {
            Task.Run(()=> {
                Thread.Sleep(3000);
                CommandGridVisibility = Visibility.Collapsed;
            });
        }

        private void Media_MouseMove(object sender, MouseEventArgs e)
        {
            CommandGridVisibility = Visibility.Visible;
        }

        private void Media_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyTriggers.ContainsKey(e.PropertyName) == false) return;
            foreach (var propertyName in PropertyTriggers[e.PropertyName])
            {
                if (PropertyUpdaters.ContainsKey(propertyName) == false)
                    continue;

                PropertyUpdaters[propertyName]?.Invoke();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (SourceStr != null)
            {
                //指定播放路径
                Media.Source = new Uri(SourceStr);
                //开始播放
                Media.Play();
            }
        }

        public static readonly DependencyProperty SourceStrProperty = DependencyProperty.Register("SourceStr", typeof(string), typeof(PlayerControl));
        public string SourceStr
        {
            get { return (string)GetValue(SourceStrProperty); }
            set { SetValue(SourceStrProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
