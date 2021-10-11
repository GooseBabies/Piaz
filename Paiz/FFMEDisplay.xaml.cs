using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Timers;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for FFMEDisplay.xaml
    /// </summary>
    public partial class FFMEDisplay : UserControl
    {
        private bool IgnoreValueChanged = false;
        private DependencyObject PreviousParent;
        private int SkipAmount = 20;
        private Window FullScreenWindow;
        
        private readonly Timer MouseNoMoveTimer = new Timer();
        private readonly static TimeSpan ZeroTime = new TimeSpan(0, 0, 0);

        public string MediaFile { get; set; } = "";
        public double Volume { get; set; } = 0.08;
        public TimeSpan Position { get; set; } = ZeroTime;
        public bool IsPlaying { get; set; } = false;
        public bool IsFullscreen { get; set; } = false;

        public FFMEDisplay()
        {
            InitializeComponent();

            MouseNoMoveTimer.Interval = 3000;
            MouseNoMoveTimer.Elapsed += new ElapsedEventHandler(MouseTimer_Elapsed);
            MouseNoMoveTimer.Enabled = true;
        }

        public async void Start()
        {
            await MediaPlayer.Open(new Uri(MediaFile));

            //if (IsPlaying)
            //{
            //    await MediaPlayer.Play();
            //    PlayPauseToggle(true);
            //}
            //else
            //{
            //    await MediaPlayer.Stop();
            //    PlayPauseToggle(false);
            //}

            VideoProgress.Maximum = MediaPlayer.PlaybackEndTime.Value.TotalSeconds;

            if (MediaPlayer.PlaybackEndTime.Value.TotalSeconds < 121)
            {
                SkipAmount = 5;
            }
                        
            VolumeSlider.Value = MediaPlayer.Volume = Volume;
            MediaPlayer.Position = Position;
            MediaPlayer.LoopingBehavior = Unosquare.FFME.Common.MediaPlaybackState.Play;
        }

        public async void CloseProperly()
        {
            MediaPlayer.IsMuted = true;
            await MediaPlayer.Stop();
            await MediaPlayer.Close();
            MediaPlayer.IsMuted = false;
            //MediaPlayer.Dispose();
        }

        public async void Unload()
        {
            await MediaPlayer.Stop();
            MediaFile = "";
            IsPlaying = false;
            Position = ZeroTime;
            await MediaPlayer.Close();
        }

        private void MouseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => { VideoControls.Visibility = Visibility.Hidden; });
            }
            catch { }
        }

        public void PlayPauseToggle(bool playing)
        {
            if (playing)
            {
                Play.Visibility = Visibility.Hidden;
                Pause.Visibility = Visibility.Visible;
                Pause2.Visibility = Visibility.Visible;
                IsPlaying = true;
            }
            else
            {
                Play.Visibility = Visibility.Visible;
                Pause.Visibility = Visibility.Hidden;
                Pause2.Visibility = Visibility.Hidden;
                IsPlaying = false;
            }
        }

        public void MuteToggle(bool muted)
        {
            if (muted)
            {
                mutesym.Visibility = Visibility.Visible;
                bar1sym.Visibility = Visibility.Hidden;
                bar2sym.Visibility = Visibility.Hidden;
            }
            else
            {
                mutesym.Visibility = Visibility.Hidden;
                bar1sym.Visibility = Visibility.Visible;
                bar2sym.Visibility = Visibility.Visible;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!MediaPlayer.IsPlaying)
            {
                MediaPlayer.Play();
                PlayPauseToggle(true);
            }
            else
            {
                MediaPlayer.Pause();
                PlayPauseToggle(false);
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.IsPlaying)
            {
                MediaPlayer.Stop();
                PlayPauseToggle(false);
            }
        }

        private void SkipB_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Seek(MediaPlayer.Position.Subtract(new TimeSpan(0, 0, SkipAmount)));
        }

        private void SkipF_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Seek(MediaPlayer.Position.Add(new TimeSpan(0, 0, SkipAmount)));
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.LoopingBehavior == Unosquare.FFME.Common.MediaPlaybackState.Play)
            {
                MediaPlayer.LoopingBehavior = Unosquare.FFME.Common.MediaPlaybackState.Stop;
                RepeatPoly.Fill = Brushes.Gray;
            }
            else
            {
                MediaPlayer.LoopingBehavior = Unosquare.FFME.Common.MediaPlaybackState.Play;
                RepeatPoly.Fill = Brushes.RoyalBlue;
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.IsMuted)
            {
                MuteToggle(MediaPlayer.IsMuted = false);
            }
            else
            {
                MuteToggle(MediaPlayer.IsMuted = true);
            }
        }

        private void Volume_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer != null)
            {
                Volume = MediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        private void Fullscren_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFullscreen) //go fullscreen
            {
                FullScreenClose.Visibility = Visibility.Visible;
                FullScreenOpen.Visibility = Visibility.Hidden;

                FullScreenWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    WindowState = WindowState.Maximized,
                    ResizeMode = ResizeMode.NoResize
                };

                Grid WindowGrid = new Grid();
                FullScreenWindow.Content = WindowGrid;

                Yeet.RemoveFromParent(this, out DependencyObject parent, out _); //remove the user control from current parent
                PreviousParent = parent;

                Yeet.AddToParent(this, WindowGrid); //add the user control to full screen window

                IsFullscreen = true;                

                FullScreenWindow.Show();
            }
            else //exit fullscreen
            {
                FullScreenClose.Visibility = Visibility.Hidden;
                FullScreenOpen.Visibility = Visibility.Visible;

                Yeet.RemoveFromParent(this, out _, out _); //remove the user control from current parent

                Yeet.AddToParent(this, PreviousParent);

                IsFullscreen = false;

                FullScreenWindow.Close();
                FullScreenWindow = null;
            }
        }

        private void Media_PositionChanged(object sender, Unosquare.FFME.Common.PositionChangedEventArgs e)
        {
            mediatime.Text = MediaPlayer.Position.ToString("hh\\:mm\\:ss");
            Position = MediaPlayer.Position;

            IgnoreValueChanged = true;
            VideoProgress.Value = e.Position.TotalSeconds;
            IgnoreValueChanged = false;

            RemainingMediaTime.Text = MediaPlayer.RemainingDuration.HasValue ? MediaPlayer.RemainingDuration.Value.ToString("hh\\:mm\\:ss") : "";
        }

        private void VideoProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IgnoreValueChanged)
            {
                MediaPlayer.Seek(new TimeSpan(0, 0, (int)VideoProgress.Value));
            }
        }

        private void Media_MediaEnded(object sender, EventArgs e)
        {
            if (MediaPlayer.LoopingBehavior == Unosquare.FFME.Common.MediaPlaybackState.Stop)
            {
                PlayPauseToggle(false);
            }
        }

        private async void MediaPlayer_MediaReady(object sender, EventArgs e)
        {
            VolumeSlider.Value = MediaPlayer.Volume = Volume;
            //MediaPlayer.IsMuted = true;
            //MuteToggle(true);

            if (IsPlaying)
            {
                await MediaPlayer.Play();
                PlayPauseToggle(true);
            }
            else
            {
                await MediaPlayer.Stop();
                PlayPauseToggle(false);
            }

            await MediaPlayer.Seek(Position);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            VideoControls.Visibility = Visibility.Visible;
            MouseNoMoveTimer.Start();
        }

        private void MediaPlayer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //Need this bubble event up from MediaPlayer Element to FFMEDisplay Usercontrol
            OnPreviewKeyUp(e);
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                Fullscren_Click(Fullscren, new RoutedEventArgs());
            }
            else if(e.Key == Key.Pause)
            {
                Play_Click(sender, e);
            }
        }
    }

    public static class Yeet
    {
        public static void AddToParent(this UIElement child, DependencyObject parent, int? index = null)
        {
            if (parent == null)
                return;

            if (parent is Grid grid)
                if (index == null)
                    grid.Children.Add(child);
                else
                    grid.Children.Insert(index.Value, child);
            //else if (parent is Panel panel)
            //    if (index == null)
            //        panel.Children.Add(child);
            //    else
            //        panel.Children.Insert(index.Value, child);
            //else if (parent is Decorator decorator)
            //    decorator.Child = child;
            //else if (parent is Window window)
            //    window.Content = child;
            //else if (parent is ContentPresenter contentPresenter)
            //    contentPresenter.Content = child;
            //else if (parent is ContentControl contentControl)
            //    contentControl.Content = child;
        }

        public static bool RemoveFromParent(this UIElement child, out DependencyObject parent, out int? index)
        {
            parent = VisualTreeHelper.GetParent(child);

            index = null;

            if (parent == null)
                return false;

            if (parent is Grid grid)
            {
                if (grid.Children.Contains(child))
                {
                    index = grid.Children.IndexOf(child);
                    grid.Children.Remove(child);
                    return true;
                }
            }
            //else if (parent is Panel panel)
            //{
            //    if (panel.Children.Contains(child))
            //    {
            //        index = panel.Children.IndexOf(child);
            //        panel.Children.Remove(child);
            //        return true;
            //    }
            //}
            //else if (parent is Decorator decorator)
            //{
            //    if (decorator.Child == child)
            //    {
            //        decorator.Child = null;
            //        return true;
            //    }
            //}
            //else if (parent is Window window)
            //{
            //    if (window.Content == child)
            //    {
            //        window.Content = null;                    
            //        return true;
            //    }
            //}
            //else if (parent is ContentPresenter contentPresenter)
            //{
            //    if (contentPresenter.Content == child)
            //    {
            //        contentPresenter.Content = null;
            //        return true;
            //    }
            //}
            //else if (parent is ContentControl contentControl)
            //{
            //    if (contentControl.Content == child)
            //    {
            //        contentControl.Content = null;
            //        return true;
            //    }
            //}

            return false;
        }
    }
}
