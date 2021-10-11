using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for Slideshow.xaml
    /// </summary>
    public partial class Slideshow : Window
    {
        private static DispatcherTimer timerImageChange;
        Random RNG = new Random();
        static int IntervalTimer = 2;
        int Counter = 0;
        bool paused = false;
        List<ImageItem> MediaFiles = new();
        Stack<int> ImageHistory = new Stack<int>();
        static int VideoDuration = IntervalTimer + 1;
        readonly string[] VideoTypes = { ".avi", ".gif", ".mkv", ".mp4", ".mpg", ".webm", ".wmv" };
        DBActions DB;
        System.Windows.Media.MediaPlayer MusicPlayer = new System.Windows.Media.MediaPlayer();
        bool MusicPlayerIsPlaying = false;

        public Slideshow(List<ImageItem> images, DBActions _DB)
        {
            InitializeComponent();

            MediaFiles = images;
            DB = _DB;
            Randomize(ref MediaFiles);

            timerImageChange = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, IntervalTimer)
            };
            timerImageChange.Tick += new EventHandler(TimerImageChange_Tick);
            MusicPlayer.Open(new Uri("WAP.mp3", UriKind.RelativeOrAbsolute));
            MusicPlayer.Volume = 0.1;
            MusicPlayer.MediaEnded += new EventHandler(Music_Ended);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //this.Cursor = Cursors.None;
            //this.Topmost = true;
            PlaySlideShow();
            timerImageChange.IsEnabled = true;
        }

        private void TimerImageChange_Tick(object sender, EventArgs e)
        {
            PlaySlideShow();
        }

        private static BitmapImage BitmapImageFromFile(string path)
        {
            var bi = new BitmapImage();

            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    bi.BeginInit();
                    bi.StreamSource = fs;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                }

                bi.Freeze();
            }
            catch { }

            return bi;
        }

        private void Randomize(ref List<ImageItem> Rand)
        {
            Rand = Rand.OrderBy(x => RNG.Next()).ToList();
        }

        private void PlaySlideShow()
        {
            GC.Collect();
            
            timerImageChange.Stop();
            if (MediaFiles.Count > 0)
            {
                if (Counter > MediaFiles.Count - 1)
                {
                    Counter = 0;
                    Randomize(ref MediaFiles);
                }

                ImageHistory.Push(Counter);
                LoadImage(MediaFiles[Counter].path);

                label.Content = (Counter + 1).ToString() + "/" + MediaFiles.Count.ToString() + " [" + VideoDuration.ToString() + "]";
                Counter++;
            }
        }

        private void LoadImage(string path)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (File.Exists(path))
            {
                if (SlideshowPlayer.IsPlaying)
                {
                    SlideshowPlayer.MediaPlayer.Pause();
                    SlideshowPlayer.Unload();
                }

                if (VideoTypes.Contains(Path.GetExtension(path)))
                {
                    int duration = DB.GetDuration(path);
                    if(duration > IntervalTimer)
                    {
                        IntervalTimer = duration + 2;
                    }
                    myImage.Visibility = Visibility.Hidden;
                    SlideshowPlayer.Visibility = Visibility.Visible;
                    SlideshowPlayer.MediaFile = path;
                    SlideshowPlayer.IsPlaying = true;
                    SlideshowPlayer.Start();
                }
                else
                {
                    IntervalTimer = 3;
                    myImage.Visibility = Visibility.Visible;
                    SlideshowPlayer.Visibility = Visibility.Hidden;
                    myImage.Source = BitmapImageFromFile(path);                    
                }
                timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
                timerImageChange.Start();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.W)
            {
                timerImageChange.Stop();
                MusicPlayer.Stop();
                if (SlideshowPlayer.IsPlaying)
                {
                    SlideshowPlayer.MediaPlayer.Pause();
                    SlideshowPlayer.Unload();
                    SlideshowPlayer.CloseProperly();
                }
                Close();
            }
            else if (e.Key == Key.Space)
            {
                PlaySlideShow();
                timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
            }
            else if (e.Key == Key.Pause)
            {
                if (paused == false)
                {
                    timerImageChange.Stop();
                    paused = true;
                }
                else
                {
                    timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
                    timerImageChange.Start();
                    paused = false;
                }
            }
            else if (e.Key == Key.Left)
            {
                //Goto Previous Image
                timerImageChange.Stop();
                Counter = ImageHistory.Pop();
                Counter = ImageHistory.Pop();
                PlaySlideShow();
                timerImageChange.Start();
            }
            else if (e.Key == Key.Right)
            {
                timerImageChange.Stop();
                Counter++;
                PlaySlideShow();
                timerImageChange.Start();
            }
            else if (e.Key == Key.Up)
            {
                IntervalTimer++;
                timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
            }
            else if (e.Key == Key.Down)
            {
                IntervalTimer--;
                if (IntervalTimer < 1)
                {
                    IntervalTimer = 1;
                }
                timerImageChange.Interval = new TimeSpan(0, 0, IntervalTimer);
            }
            else if (e.Key == Key.M)
            {
                if (MusicPlayerIsPlaying)
                {
                    MusicPlayer.Pause();
                    MusicPlayerIsPlaying = false;
                }
                else
                {
                    MusicPlayer.Play();
                    MusicPlayerIsPlaying = true;
                }
                
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            timerImageChange.Stop();
            MusicPlayer.Stop();
            Close();
        }

        private void Music_Ended(object sender, EventArgs e)
        {
            MusicPlayer.Position = TimeSpan.Zero;
            MusicPlayer.Play();
        }
    }
}
