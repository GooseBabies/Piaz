using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Library.FFmpegDirectory = @"C:\ffmpeg 4.3.1\bin";
            //Library.FFmpegDirectory = @"C:\ffmpeg 4.4\bin";
            Library.LoadFFmpeg();
            MediaElement.FFmpegMessageLogged += (s, ev) =>
            {
                System.Diagnostics.Debug.WriteLine(ev.Message);
            };
            base.OnStartup(e);
        }
    }
}
