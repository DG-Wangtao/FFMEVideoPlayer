# 目录
- [如何使用我的Demo](#如何使用我的demo)
    - [说明](#说明)
    - [下载、编译并运行](#下载编译并运行)
    - [Demo的结构](#demo的结构)
        - [MainWindow](#mainwindow)
        - [PlayerController](#playercontroller)
            - [先贴出只实现播放功能的最简单的代码](#先贴出只实现播放功能的最简单的代码)
            - [PlayerControl.xaml](#playercontrol_xaml)
            - [PlayerControl.xaml.cs](#playercontrol_xaml_cs)
            - [MediaElement](#mediaelement)
            - [demo中其他内容](#demo中其他内容)
- [如何使用FFME](#如何使用ffme)
    - [下载所需内容](#下载所需内容)
        - [下载源码](#下载源码)
        - [下载一个压缩包](#下载一个压缩包)
        - [新建一个WPF应用程序](#新建一个wpf应用程序)
        - [编译下载的源码](#编译下载的源码)
        - [在新的WPF应用程序中添加引用](#在新的wpf应用程序中添加引用)
        - [编辑MainWindow](#编辑mainwindow)
        - [MediaElement其他方法与属性](#mediaelement其他方法与属性)
- [感谢](#感谢)

这是一个WPF中播放rtsp等流视频文件的示例，使用的方式是使用ffmpeg封装MediaElement来实现的。封装的方法是unosquare做的，我只是把他的成果拿来用罢了，找了好久才找到在wpf中播放rtsp流（当然vlc、nvlc、winform控件和流转bitmap的方式也能实现，但都没有这个效率高而且它不依赖任何其他内容），封装的源码请查看：https://github.com/unosquare/ffmediaelement  
# 如何使用我的Demo
## 说明
为了方遍别人和我以后更方便地使用demo，我将生成地dll文件和所需其他文件也传到了gihub上，这使得我的文件显得很臃肿，看起来很不专业，但方遍简洁最重要不是么，哈哈。
## 下载编译并运行
此次我使用Visual Stdio2015进行编程工作，只要你下载下这整个文件（zip或者clone），然后双击`.sln`文件打开解决方案，加载完成`Rebuild`一下，然后点击`Start`按钮即可，首先vs会先下载一些NuGet包，配置完成后会顺利运行，此时你就会看到展示了四个视频直播地窗口。
## Demo的结构
这是一个非常简单的`UserController`的建立与使用的示例
### MainWindow
MainWindow只是展示了四个`PlayerControl`用户控件而已，`PlayerControl`是我自己写好的播放器控件，它的`SourceStr`属性为依赖项属性，从父窗口获取要播放的rtsp流地址。
### PlayerController
#### 先贴出只实现播放功能的最简单的代码
PlayerControl.xaml:
```
<UserControl x:Class="FFMEVideoPlayer.Player.PlayerControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             xmlns:local="clr-namespace:FFMEVideoPlayer.Player" 
             Loaded="UserControl_Loaded"
             xmlns:player="clr-namespace:Unosquare.FFME;assembly=ffme"
             mc:Ignorable="d" Name="playerController">
    </UserControl.Resources>
    <Grid>
        <player:MediaElement Name="Media"/>
    </Grid>
</UserControl>
```
PlayerControl.xaml.cs
```
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
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
                InitializeComponent();
                //必须在为播放器指定uri之前设置FFmpegDirectory的值
                //它引用一些依赖的.dll和.exe文件，该文件在../bin/DLL/中
                Unosquare.FFME.MediaElement.FFmpegDirectory = Directory.GetCurrentDirectory() + "\\DLL\\";
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

    }
}

```
`PlayerControl`调用`ffme`封装好的`MediaElement`来实现播放功能。经过封装的`MediaElement`被集成到了我所引用的`ffme.dll`文件中，而`ffmpeg.dll`在本程序中也是不可缺少的引用。
所以引用M`ediaElement`的方式如下：
#### PlayerControl_xaml
PlayerControl.xaml文件
* 首先引入ffme.dll的命名空间： 
```
   xmlns:player="clr-namespace:Unosquare.FFME;assembly=ffme"
```
* 在Grid中添加MediaElementk控件：  
```
<player:MediaElement Name="Media" HorizontalAlignment="Right" VerticalAlignment="Stretch"/>
```
xaml中定义的其他内容只是控制播放/停止等命令的按钮而已，若不需要可以删除掉，不影响播放功能。

#### PlayerControl_xaml_cs
PlayerControl.xaml.cs
* 注册一个依赖项属性，获取父窗体提供的播放地址
```
 public static readonly DependencyProperty SourceStrProperty = DependencyProperty.Register("SourceStr", typeof(string), typeof(PlayerControl));
        public string SourceStr
        {
            get { return (string)GetValue(SourceStrProperty); }
            set { SetValue(SourceStrProperty, value); }
        }
```
* 在构造函数中首先添加一句：
```
 Unosquare.FFME.MediaElement.FFmpegDirectory = Directory.GetCurrentDirectory() + "\\DLL\\";
```
这是为了要添加一些播放所需的dll和exe文件，位置在/bin/Debug/DLL，它必须在设置播放器`Source`属性之前执行。

* 而后在控件的`Loaded`事件里为`Media`控件的`Source`属性赋值并令其播放。
```
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
```
此时就可以实现最简单的播放功能了。

#### MediaElement
MediaElement有`Play()`、`Close()`、`Dispose()`、`Pause()`、`Stop()`等方法，有`IsPlaying`、`IsOpen`、`IsOpening`、`MediaState`、`HasAudio`等属性，我们可以通过他们来判断和控制播放状态。
#### demo中其他内容
定义`DelegateCommand`命令实现停止、播放功能，定义公有属性监控当前播放状态，当`MediaElement`播放状态发生改变时`Media_PropertyChanged`事件被触发来更新监控状态的属性的值。
知道了这些内容，如何定义自已的界面以及功能就随君所欲了。

# 如何使用FFME
如果想要按照[unosquare](https://github.com/unosquare/ffmediaelement)的指引来做播放器的话，首先要做的就是下载并安装Visual Stuido 2017来使用C#7.0，否则是无法编译成功的。
## 下载所需内容
### 下载源码
从[这里](https://github.com/unosquare/ffmediaelement)下载unosquare/ffmediaelement整个项目源码，里面有使用FFmpeg封装MediaElemnt的源码，也有使用封装得到的dll的Exmaple，我便是按照他的教程来做的。
### 下载一个压缩包
从[这里](https://ffmpeg.zeranoe.com/builds/win32/shared/ffmpeg-3.3.2-win32-shared.zip)下载文件并解压。
### 新建一个WPF应用程序
在VS中新建一个你的播放器WPF程序，我取的名字为`FFMEVideoPlayer`。建立完成之后打开其所在文件夹，依次导航到`FFMEVideoPlayer\FFMEVideoPlayer\bin\Debug`下，新建一个文件夹命名为`DLL`，当然你也可以取别的名字。而后将刚才下载并解压得到的`ffmpeg-3.3.2-win32-shared`文件夹中`bin`文件夹下的3个exe文件和8个dll文件全部复制到`DLL`下。
### 编译下载的源码
下载下载unosquare/ffmediaelemen完成之后使用VS2017打开`Unosquare.FFME.sln`解决方案，重新生`Unosquare.FFME`项目，生成成功之后打开其所在文件夹，找到`bin/Debug`或者`bin/Release`下生成的`ffme.dll`和`ffmpeg.dll`文件，复制到你的WPF应用程序的`bin/DLL`文件夹下。
### 在新的WPF应用程序中添加引用
在刚才新建的WPF应用程序中添加引用，引用刚刚复制到`bin/DLL`文件夹中的`ffme.dll`和`ffmpeg.dll`文件。
### 编辑MainWindow
在MainWindow.xaml中:
* 添加命名空间：
```
xmlns:player="clr-namespace:Unosquare.FFME;assembly=ffme"
```
* 注册Loaded事件：
```
Loaded="Window_Loaded"
```
* 添加MediaElement控件
```
<player:MediaElement Name="Media"/>
```

在MainWindow.xaml.cs中:
* 修改Loaded事件：
```
  private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         	//必须在为播放器指定uri之前设置FFmpegDirectory的值
            //它引用一些依赖的.dll和.exe文件，文件在../bin/DLL/中
            Unosquare.FFME.MediaElement.FFmpegDirectory = Directory.GetCurrentDirectory() + "\\DLL\\";
        	//http://ivi.bupt.edu.cn/hls/cctv1hd.m3u8是cctv1直播地址
 			Media.Source = new Uri("http://ivi.bupt.edu.cn/hls/cctv1hd.m3u8);
             //开始播放
            Media.Play();
		}
```
此时运行你的程序，稍等一会就回看到cctv1的直播节目了。

### MediaElement其他方法与属性
[这里](#mediaelement)简要说了下`MediaElement`的几个方法，具体内容可以在` Media.Play();`的` Play`上右键转到定义查看其他属性、方法和事件。

# 感谢
感谢[Unosquare Labs](https://github.com/unosquare)提供的解决方案


