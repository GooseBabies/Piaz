using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using IqdbApi;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using Microsoft.WindowsAPICodePack.Shell;
using Hardcodet.Wpf.TaskbarNotification;
using AdonisUI.Controls;
using System.Windows.Media.Animation;
using BooruSharp.Search.Post;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        enum OrderBY
        {
            Hash,
            FilePath,
            FileName,
            DateAdded,
            DateModified,
            Height,
            Width,
            Rating,
            ID
        }

        readonly string[] FileLocations = { @"Y:\Media\AVI", @"Y:\Media\GIF", @"Y:\Media\JPEG", @"Y:\Media\JPG", @"Y:\Media\MKV", @"Y:\Media\MP4", @"Y:\Media\MP4 - Clips", @"Y:\Media\MPG", @"Y:\Media\PNG", @"Y:\Media\WEBM", @"Y:\Media\WMV" };
        readonly string[] AcceptedTypes = { ".jpeg", ".jpg", ".png", ".gif", ".avi", ".mkv", ".mp4", ".mpg", ".webm", ".wmv" };
        readonly string[] VideoTypes = { ".avi", ".mkv", ".mp4", ".mpg", ".webm", ".wmv" };
        //readonly string ThumbLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\Thumbs\\";
        readonly string LargeThumbLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\LargeThumbs\\";
        readonly DBActions DB = new DBActions();

        double PageAmount = 0.0;
        int PageIndex = 1;
        readonly int ResultsPerPage = 72;    //102 fits a perfect grid on both my monitors
        //bool Filtered = false;
        //bool MainGridItemsHidden = false;

        List<ImageItem> ImagePaths = new();
        List<ImageItem> FilteredImagePaths;
        CategoryItem Categories = new();

        //BackgroundWorker NewFilesWorker;
        BackgroundWorker BooruAutoTaggerWorker = new();
        //readonly BackgroundWorker ExportDatabaseWorker = new();
        readonly ThumbnailCreator ThumbMaker = new();
        readonly TagSuggestionSearchBar SearchBar;
        TaskbarIcon tbi;

        public MainWindow()
        {
            InitializeComponent();

            tbi = new TaskbarIcon
            {
                Icon = new Icon("PP.ico"),
                Visibility = Visibility.Visible
            };
            tbi.TrayMouseDoubleClick += new RoutedEventHandler(tbi_TrayMouseDoubleClick);

            BooruAutoTaggerWorker.DoWork += BooruAutoTaggerWorker_DoWork;
            BooruAutoTaggerWorker.ProgressChanged += BooruAutoTaggerWorker_ProgressChanged;
            BooruAutoTaggerWorker.WorkerReportsProgress = true;

            //Connect to the Database
            DB.SQLiteConnect();

            SearchBar = new TagSuggestionSearchBar(DB)
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 133, 133, 133))
            };
            SearchBar.PreviewKeyUp += new KeyEventHandler(SearchBar_PreviewKeyUp);
            SearchBar.MultipleTagsMode = true;
            SearchBar.Height = 26;
            SearchBar.Width = double.NaN;
            SearchBar.VerticalContentAlignment = VerticalAlignment.Center;
            SearchBar.Margin = new Thickness(10, 0, 10, 0);
            Grid.SetColumn(SearchBar, 0);
            Grid howie = AcceptSearch.Parent as Grid;
            howie.Children.Add(SearchBar);
            PasswordBox.Focus();
        }

        #region Initialization

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // Calls all the Functions to Initialize the Program
            //

            //Get all important image information needed to set up main grid for each image
            FilteredImagePaths = ImagePaths = DB.GetAllImageItems((int)OrderBY.ID, false);
            SearchCount.Content = FilteredImagePaths.Count.ToString();

            //ImportData();

            // Add all controls to main tab
            CreateThumbnailGridSkeleton();

            //Populate main tab controls with appropriate thumnail and filenames based on current page
            PopulateThumbnailGrid(FilteredImagePaths);

            DupeComparison.Header = "Dupes [" + DB.GetDupesCount() + "]";

            //Maximize the window
            WindowState = WindowState.Maximized;
        }

        private void CreateThumbnailGridSkeleton()
        {
            //
            // Create Controls on main tab to hold each files thumbnail and filename
            //
            for (int i = 0; i <= ResultsPerPage - 1; i++)
            {
                //Create Grid for Thumbnail and Filename (Maybe Turn into a User control later?)
                Grid ResultGrid = new() { Margin = new Thickness(15, 8, 15, 8) };

                //Define sizes row sizes for thumbnail item and Filename item
                RowDefinition rd1 = new() { Height = new GridLength(180, GridUnitType.Pixel) };  //Thumbnail
                //RowDefinition rd2 = new() { Height = new GridLength(60, GridUnitType.Pixel) };   //Filename
                ResultGrid.RowDefinitions.Add(rd1);
                //ResultGrid.RowDefinitions.Add(rd2);

                //Define column size for both tumbnail and filename
                ColumnDefinition cd1 = new() { Width = new GridLength(180, GridUnitType.Pixel) };
                ResultGrid.ColumnDefinitions.Add(cd1);

                //Create filename item and set its placement in the grid
                //TextBlock Filename = new() { Text = "", TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Foreground = System.Windows.Media.Brushes.White };
                //Grid.SetRow(Filename, 1);
                //Grid.SetColumn(Filename, 0);
                //ResultGrid.Children.Add(Filename);

                //Create button which holds thumbnail and its click event handler
                Button ThumbnailLink = new() { Background = System.Windows.Media.Brushes.Black };
                ThumbnailLink.Click += new RoutedEventHandler(Thumbnail_Click);

                //Create image item which wil hold the thumbnail, set image items as button's content
                System.Windows.Controls.Image Thumbnail = new System.Windows.Controls.Image() { Width = 240, Height = 200, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                Thumbnail.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(Thumbnail_MouseEnterEvent);
                Thumbnail.Stretch = System.Windows.Media.Stretch.Uniform;
                ThumbnailLink.Content = Thumbnail;

                //Create border for button, add button into border, set borders placement in the grid
                Border ThumbnailBorder = new(){ BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(2) };
                ThumbnailBorder.Child = ThumbnailLink;
                Grid.SetRow(ThumbnailBorder, 0);
                Grid.SetColumn(ThumbnailBorder, 0);
                _ = ResultGrid.Children.Add(ThumbnailBorder);

                Canvas TitleMarquee = new() { ClipToBounds = true, Background = System.Windows.Media.Brushes.Black, Opacity = 0.6, Width = 174, Height = 18, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0,0,0,2) };
                Grid.SetRow(TitleMarquee, 0);
                Grid.SetColumn(TitleMarquee, 0);
                _ = ResultGrid.Children.Add(TitleMarquee);

                TextBlock TitleBlock = new() { Text = "", Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(2, 0, 2, 0) };
                TitleMarquee.Children.Add(TitleBlock);

                //Create Textblock to display rating and set its placement in the grid
                TextBlock RatingDisplay = new() { Width = 14, Height = 16, Foreground = System.Windows.Media.Brushes.White, Background = System.Windows.Media.Brushes.Black, FontWeight = FontWeights.Bold, Text = "", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 6, 6, 0), TextAlignment = TextAlignment.Center, Visibility = Visibility.Collapsed };
                Grid.SetRow(RatingDisplay, 0);
                Grid.SetColumn(RatingDisplay, 0);
                _ = ResultGrid.Children.Add(RatingDisplay);
                //ResultsPanel.Children.Add

                TextBlock VideoDisplay = new() { Width = 0, Height = 16, Foreground = System.Windows.Media.Brushes.White, Background = System.Windows.Media.Brushes.Black, FontWeight = FontWeights.Bold, Text = "", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(6, 6, 6, 0), TextAlignment = TextAlignment.Center, Visibility = Visibility.Collapsed };
                Grid.SetRow(VideoDisplay, 0);
                Grid.SetColumn(VideoDisplay, 0);
                _ = ResultGrid.Children.Add(VideoDisplay);
                //add the newly created grid into the overall wrappanel
                _ = ResultsPanel.Children.Add(ResultGrid);
            }
        }

        private void Thumbnail_MouseEnterEvent(object sender, DependencyPropertyChangedEventArgs e)
        {
            

            System.Windows.Controls.Image a = (System.Windows.Controls.Image)sender;
            Button b = (Button)a.Parent;
            Border c = (Border)b.Parent;
            Grid d = (Grid)c.Parent;
            Canvas TM = (Canvas)d.Children[1];
            TextBlock TB = (TextBlock)TM.Children[0];

            if (a.IsMouseDirectlyOver && (TB.ActualWidth > TM.ActualWidth))
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = -TB.ActualWidth + 170;
                doubleAnimation.To = TM.ActualWidth;
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
                doubleAnimation.Duration = new Duration(TimeSpan.Parse("0:0:10"));
                TB.BeginAnimation(Canvas.RightProperty, doubleAnimation);
            }
            else
            {
                TB.BeginAnimation(Canvas.RightProperty, null);
            }
        }

        //private void ImportData()
        //{
        //    //
        //    // Currently Commented out Function
        //    // Imports booru links from a texts file and adds them into the database for the corresponding image
        //    //
        //    WindowState = WindowState.Minimized;
        //    Logger.Write("Importing External File Data");

        //    string ImportData = "";
        //    string FileToRead = @"C:\Users\Chris\Downloads\export 11-14.txt";
        //    using (StreamReader Reader = new StreamReader(FileToRead))
        //    {
        //        ImportData = Reader.ReadToEnd();
        //    }
        //    string[] DataLines = ImportData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 1; i <= DataLines.Length - 1; i++)
        //    {
        //        string[] LineColumns = DataLines[i].Split(new string[] { "||" }, StringSplitOptions.None);
        //        if (DB.CheckName(LineColumns[1]))
        //        {
        //            DB.UpdateRatingBasedOnName(LineColumns[1], int.Parse(LineColumns[2]));
        //            Logger.Write("Setting Rating to " + LineColumns[2] + " for file: " + LineColumns[1]);
        //        }
        //    }
        //    foreach (ImageThumbnailItem image in ImagePaths)
        //    {
        //        DB.UpdateIqdb(DB.GetHash(image.path));
        //        Logger.Write("Mark IQDB as found for file: " + image.path);
        //    }
        //}

        #endregion

        #region New Files

        private void CheckforNewImages(bool perform)
        {
            //DB Check if appropriate time to run initialization
            if (perform)
            {
                List<FileInfo> NewMedia = new();
                List<Tuple<FileInfo, string[]>> newHashes = new();
                foreach (string folder in FileLocations)
                {
                    if (Directory.Exists(folder))
                    {
                        //NewImages.AddRange(new DirectoryInfo(folder).GetFiles("*.*").Where(s => (AcceptedTypes.Contains(s.Extension)) && (s.LastWriteTime.CompareTo(DateTime.Now.AddMonths(-3)) > 0)).ToList());
                        NewMedia.AddRange(new DirectoryInfo(folder).GetFiles("*.*", SearchOption.AllDirectories).Where(s => AcceptedTypes.Contains(s.Extension)).ToList());
                    }
                }

                foreach (FileInfo file in NewMedia)
                {
                    if (DB.GetHash(file.FullName) == "") //path not in database
                    {
                        newHashes.Add(new Tuple<FileInfo, string[]>(file, GenerateHash(file.FullName)));
                    }
                }

                newHashes = newHashes.OrderBy(c => c.Item2[0]).ToList();

                NewMedia.Clear();

                foreach(Tuple<FileInfo, string[]> HashItem in newHashes)
                {

                    if (DB.CheckDuplicateHash(HashItem.Item2[0])) //Check if Hash is already in Database, this means this is a new File that is the exact same Image or it is a renames file
                    {
                        Logger.Write("Hash Duplicate of " + HashItem.Item1.Name + " already in database");
                        if (!File.Exists(DB.GetPath(HashItem.Item2[0]))) //If the old filename of the hash doesn't exist anymore it most likely had its Filename changed
                        {
                            DB.UpdatePathComponents(HashItem.Item2[0], HashItem.Item1.FullName, HashItem.Item1.Name, HashItem.Item1.Extension); //Update the Filename information in the Database
                            Logger.Write("Hash file already in Database doesn't exist anymore, this file will replace: " + HashItem.Item1.Name);
                        }
                        else  //If the old filename does still exist this new file is an exact duplicate of another file already in the databse so we can delete it
                        {
                            File.Delete(HashItem.Item1.FullName);
                            Logger.Write("Deleting new file: " + HashItem.Item1.Name);
                        }
                    }
                    else if (DB.CheckIfHashHasBeenDeletedBefore(HashItem.Item2[0])) //if we've deleted a file with the same hash before ignore the new file
                    {
                        Logger.Write(HashItem.Item1.Name + " has been deleted from Database before; Ignored");
                        continue;
                    }
                    else
                    {
                        DB.InsertIntoImageData(HashItem.Item2[0], HashItem.Item1.FullName, HashItem.Item1.Name, HashItem.Item1.Extension, DateTime.Now, HashItem.Item1.LastWriteTime, 0, 0, HashItem.Item2[1]);
                        Logger.Write("Importing: " + HashItem.Item1.Name + " into database");

                        if(HashItem.Item1.Length > 8192000)
                        {
                            DB.UpdateIqdb(HashItem.Item1.FullName);
                        }

                        if (HashItem.Item1.Name.Contains("rule34.xxx"))
                        {
                            string[] filecontents = HashItem.Item1.Name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            string sourceurl = "";
                            sourceurl = "https://rule34.xxx/index.php?page=post&s=view&id=";
                            sourceurl += Path.GetFileNameWithoutExtension(filecontents[^1]);
                            DB.AddPrimarySource(HashItem.Item1.FullName, sourceurl);
                        }
                        else if (HashItem.Item1.Name.Contains("e621"))
                        {
                            string[] filecontents = HashItem.Item1.Name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            string sourceurl = "";
                            sourceurl = "https://e621.net/posts/";
                            sourceurl += Path.GetFileNameWithoutExtension(filecontents[^1]);
                            DB.AddPrimarySource(HashItem.Item1.FullName, sourceurl);
                        }
                        else if (HashItem.Item1.Name.Contains("Danbooru") || HashItem.Item1.Name.Contains("danbooru"))
                        {
                            string[] filecontents = HashItem.Item1.Name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            string sourceurl = "";
                            sourceurl = "https://danbooru.donmai.us/posts/";
                            sourceurl += Path.GetFileNameWithoutExtension(filecontents[^1]);
                            DB.AddPrimarySource(HashItem.Item1.FullName, sourceurl);
                        }

                        if (VideoTypes.Contains(HashItem.Item1.Extension))
                        {
                            using ShellObject shell = ShellObject.FromParsingName(HashItem.Item1.FullName);
                            double l = 0;
                            var dur = shell.Properties.System.Media.Duration.ValueAsObject;
                            if (dur != null)
                            {
                                l = TimeSpan.FromTicks((long)(ulong)dur).TotalSeconds;
                            }

                            int Duration = (int)Math.Floor(l);

                            float rateob = 0.0F;
                            var rate = shell.Properties.System.Video.FrameRate.ValueAsObject;
                            if (rate != null)
                            {
                                var ratefl = (float)(uint)rate;
                                rateob = ratefl / (float)1000;
                            }

                            var frameheight = shell.Properties.System.Video.FrameHeight.ValueAsObject;
                            int frameh = 0;
                            if (frameheight != null)
                            {
                                frameh = (int)(uint)frameheight;
                            }

                            var framewidth = shell.Properties.System.Video.FrameWidth.ValueAsObject;
                            int framew = 0;
                            if (framewidth != null)
                            {
                                framew = (int)(uint)framewidth;
                            }

                            var audio = shell.Properties.System.Audio.ChannelCount.ValueAsObject;
                            int audioob = 0;
                            if (audio != null)
                            {
                                audioob = (int)(uint)audio;
                            }

                            DB.Updatephash(HashItem.Item1.FullName, new byte[40]);
                            DB.UpdateCorrelated(HashItem.Item1.FullName);
                            DB.UpdateHeightAndWidth(HashItem.Item1.FullName, frameh, framew);
                            DB.UpdateVideoItems(HashItem.Item1.FullName, true, audioob > 0, Duration, rateob);

                            ImportThumbnail(HashItem.Item1.FullName, shell);
                        }
                        else if(HashItem.Item1.Extension == ".gif")
                        {
                            //string filePath = @"Y:\Media\GIF\yah-buoy_Zelda_fbnjwl.gif";
                            int animationDuration = 0;
                            bool animated = false;
                            int height = 0;
                            int width = 0;
                            float rate = 0.0F;

                            using (var image = System.Drawing.Image.FromFile(HashItem.Item1.FullName))
                            {
                                height = image.Height;
                                width = image.Width;

                                if (ImageAnimator.CanAnimate(image))
                                {
                                    //Get frames  
                                    var dimension = new System.Drawing.Imaging.FrameDimension(image.FrameDimensionsList[0]);
                                    int frameCount = image.GetFrameCount(dimension);
                                    var minimumFrameDelay = (1000.0 / 60);
                                    var duration = 0;                                        

                                    for (int i = 0; i < frameCount; i++)
                                    {

                                        var delayPropertyBytes = image.GetPropertyItem(20736).Value;
                                        var frameDelay = BitConverter.ToInt32(delayPropertyBytes, i * 4) * 10;
                                        // Minimum delay is 16 ms. It's 1/60 sec i.e. 60 fps
                                        duration += (frameDelay < minimumFrameDelay ? (int)minimumFrameDelay : frameDelay);
                                        rate = (frameDelay == 0 ? 0 : 1 / frameDelay);
                                    }

                                    animationDuration = (duration / 1000);
                                    rate = (float)frameCount / ((float)duration / (float)1000);
                                    animated = true;
                                }
                            }

                            DB.UpdateVideoItems(HashItem.Item1.FullName, animated, false, animationDuration, rate);

                            DB.UpdateHeightAndWidth(HashItem.Item1.FullName, height, width);
                            //Logger.Write("Importing HxW: " + height.ToString() + "x" + width.ToString() + " for image: " + HashItem.Item1.Name + " into database");

                            using(ShellObject shell = ShellObject.FromParsingName(HashItem.Item1.FullName))
                            {
                                ImportThumbnail(HashItem.Item1.FullName, shell);
                            }
                                
                            //Logger.Write("Generating Thumbnail for " + HashItem.Item1.Name);

                            GeneratePHash(HashItem.Item1.FullName);
                            //Logger.Write("Generating PHash for " + HashItem.Item1.Name);
                        }
                        else
                        {
                            try
                            {
                                using var imageStream = File.OpenRead(HashItem.Item1.FullName);
                                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                                DB.UpdateHeightAndWidth(HashItem.Item1.FullName, decoder.Frames[0].PixelHeight, decoder.Frames[0].PixelWidth);
                                //Logger.Write("Importing HxW: " + decoder.Frames[0].PixelHeight.ToString() + "x" + decoder.Frames[0].PixelWidth.ToString() + " for image: " + HashItem.Item1.Name + " into database");
                            }
                            catch { }

                            DB.UpdateVideoItems(HashItem.Item1.FullName, false, false, 0, 0);

                            using (ShellObject shell = ShellObject.FromParsingName(HashItem.Item1.FullName))
                            {
                                ImportThumbnail(HashItem.Item1.FullName, shell);
                            }

                            GeneratePHash(HashItem.Item1.FullName);
                            //Logger.Write("Generating PHash for " + HashItem.Item1.Name);
                        }
                        
                    }
                }
                tbi.ShowBalloonTip("Import", "Import Completed", BalloonIcon.None);
                newHashes.Clear();
            }
        }

        public static string[] GenerateHash(string videopath)
        {
            using FileStream fs = new(videopath, FileMode.Open)
            {
                Position = 0
            };
            using SHA256 sha = SHA256.Create();
            string hashstring = HexStringFromBytes(sha.ComputeHash(fs));
            using MD5 md5 = MD5.Create();
            string md5string = HexStringFromBytes(md5.ComputeHash(fs));
            return new string[] { hashstring, md5string };
        }

        public static string CalculateMD5(string videopath)
        {
            using FileStream fs = new FileStream(videopath, FileMode.Open)
            {
                Position = 0
            };
            using SHA256 sha = SHA256.Create();
            return HexStringFromBytes(sha.ComputeHash(fs));
        }

        private static string HexStringFromBytes(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }

            return sb.ToString();
        }

        public void GenerateThumbnail(string filepath)
        {
            string filename = Path.GetFileNameWithoutExtension(filepath);
            if (Path.Combine(LargeThumbLocation, filename).Length < 260)
            {
                if (!File.Exists(Path.Combine(LargeThumbLocation, filename)))
                {
                    using Stream sourceStream = ThumbMaker.CreateThumbnailStream(200, filepath, Format.Jpeg);
                    if (sourceStream != null)
                    {
                        if (!Directory.Exists(LargeThumbLocation))
                        {
                            return;
                        }
                        using FileStream fs = new FileStream(Path.Combine(LargeThumbLocation, filename + ".jpg"), FileMode.Create, FileAccess.Write);
                        if (sourceStream.Position != 0)
                        {
                            sourceStream.Position = 0;
                        }
                        sourceStream.CopyTo(fs);
                    }
                    else
                    {
                        Logger.Write("Thumbnail Generation Error: Sourcestream Not Found for image: " + filename);
                    }
                }
                else
                {
                    Logger.Write("Thumbnail already exists for image: " + filename + " deleting and trying again.");
                    File.Delete(Path.Combine(LargeThumbLocation, filename));
                    GenerateThumbnail(filepath);
                }
            }
        }

        public void ImportThumbnail(string filepath, ShellObject shell)
        {
            try
            {
                shell.Thumbnail.ExtraLargeBitmap.Save(LargeThumbLocation + Path.GetFileNameWithoutExtension(filepath) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                //shell.Thumbnail.LargeBitmap.Save(ThumbLocation + Path.GetFileNameWithoutExtension(file.FullName) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);                                    
                //shell.Thumbnail.
            }
            catch (Exception ex)
            {
                GenerateThumbnail(filepath);
                Logger.Write("error getting shell thumbnail for " + filepath);
                Logger.Write(ex.Message);
            }
        }

        private void GeneratePHash(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Bitmap BMP = (Bitmap)System.Drawing.Image.FromFile(path);
                    Digest a = ImagePhash.ComputeDigest(BMP.ToLuminanceImage());
                    DB.Updatephash(path, a.Coefficients);
                    BMP.Dispose();
                }
                else
                {
                    Logger.Write("Phash Generation Error: File Doesn't Exist: " + path);
                }
            }
            catch(Exception ex)
            {
                Logger.Write("Phash Generation Error");
                Logger.Write(ex.Message);
            }
            
        }

        private void ImportFiles_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Starting Manual Import");
            WindowState = WindowState.Minimized;

            CheckforNewImages(true);

            //ImagePaths = DB.GetAllImageThumbnailItems((int)OrderBY.DateModified, false);
            //ImagePaths = DB.GetAllPaths((int)OrderBY.DateModified, false);
            FilteredImagePaths = ImagePaths = DB.GetAllImageItems((int)OrderBY.ID, false);
            SearchCount.Content = FilteredImagePaths.Count.ToString();

            PopulateThumbnailGrid(FilteredImagePaths);
        }

        #endregion

        #region Update UI

        private void PopulateThumbnailGrid(List<ImageItem> FileList)
        {
            int Counter = 0;
            PageAmount = Math.Ceiling(FileList.Count / (double)ResultsPerPage);

            if (FileList.Count > 0)
            {
                ResultsPanel.Visibility = Visibility.Visible;
                Chickens.Visibility = Visibility.Hidden;

                BottomPageCount.Content = " / " + PageAmount.ToString() + " (" + FileList.Count.ToString() + ")";
                BottomPagenumber.Text = PageIndex.ToString();
                for (int ResultIndex = ((PageIndex - 1) * ResultsPerPage); (ResultIndex <= (ResultsPerPage * PageIndex) - 1) && (ResultIndex <= FileList.Count - 1); ResultIndex++)
                {
                    string thumbname = Path.GetFileNameWithoutExtension(FileList[ResultIndex].path);
                    bool isTagged = FileList[ResultIndex].tag_list.Length>1;
                    if (File.Exists(FileList[ResultIndex].path))
                    {
                        if (ResultsPanel.Children[Counter] is Grid g)
                        {
                            //if (g.Children[0] is TextBlock j)
                            //{
                            //    //j.Text = Path.GetFileNameWithoutExtension(FileList[ResultIndex].path);
                            //}
                            if (g.Children[0] is Border k)
                            {
                                if (isTagged && FileList[ResultIndex].primary_source != "")
                                {
                                    k.BorderBrush = System.Windows.Media.Brushes.Green;
                                }
                                else if (isTagged && FileList[ResultIndex].primary_source == "")
                                {
                                    k.BorderBrush = System.Windows.Media.Brushes.LightBlue;
                                }
                                else if (!isTagged && FileList[ResultIndex].primary_source != "")
                                {
                                    k.BorderBrush = System.Windows.Media.Brushes.Yellow;
                                }
                                else
                                {
                                    k.BorderBrush = System.Windows.Media.Brushes.Red;
                                }
                                if (k.Child is Button l)
                                {
                                    l.Tag = FileList[ResultIndex].path;
                                    if (l.Content is System.Windows.Controls.Image m)
                                    {
                                        m.Source = File.Exists(Path.Combine(LargeThumbLocation, thumbname + ".jpg"))
                                            ? BitmapImageFromFile(Path.Combine(LargeThumbLocation, thumbname + ".jpg"), 140)
                                            : BitmapImageFromFile(Path.Combine(LargeThumbLocation, "MissingThumb.jpg"), 140);
                                    }

                                }
                            }
                            if(g.Children[1] is Canvas c)
                            {
                                if(c.Children[0] is TextBlock f)
                                {
                                    f.Text = Path.GetFileNameWithoutExtension(FileList[ResultIndex].path);
                                }
                            }
                            if (g.Children[2] is TextBlock n)
                            {
                                if (FileList[ResultIndex].rating > 0)
                                {
                                    n.Visibility = Visibility.Visible;
                                    n.Text = FileList[ResultIndex].rating.ToString();
                                }
                                else
                                {
                                    n.Visibility = Visibility.Collapsed;
                                }
                            }
                            if(g.Children[3] is TextBlock q)
                            {
                                if (FileList[ResultIndex].video)
                                {
                                    q.Visibility = Visibility.Visible;
                                    q.Text = "\u25B6";
                                    q.Width = 14;
                                    if (FileList[ResultIndex].sound)
                                    {
                                        q.Text += "\u266B"; //\u266B
                                        q.Width += 8;
                                    }
                                }
                                else
                                {
                                    q.Text = "";
                                    q.Visibility = Visibility.Hidden;
                                }
                            }

                        }
                        Counter++;
                        ResultsPanel.Children[Counter - 1].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        DB.DeleteAllImageTags(FileList[ResultIndex].path);
                        DB.DeleteFromImageData(FileList[ResultIndex].path);
                    }
                }
                if (Counter < ResultsPerPage)
                {
                    for (int r = Counter; r <= ResultsPerPage - 1; r++)
                    {
                        ResultsPanel.Children[r].Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                ResultsPanel.Visibility = Visibility.Hidden;
                Chickens.Visibility = Visibility.Visible;
            }
        }

        private static BitmapImage BitmapImageFromFile(string path, int height)
        {
            BitmapImage bi = new();

            try
            {

                bi.BeginInit();
                bi.DecodePixelHeight = height;
                bi.UriSource = new Uri(path);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();

                bi.Freeze();

                return bi;
            }
            catch (Exception)
            {
                //Error.WriteToLog(ex);
                return bi;
            }
        }

        #endregion

        #region Navigation

        private void Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            Button chosenTag = sender as Button;
            string filePath = chosenTag.Tag.ToString();

            Logger.Write("Opening Media page for: " + filePath);

            MediaTagPage SubPage;

            int fileindex = 0;

            fileindex = FilteredImagePaths.IndexOf(FilteredImagePaths.Where(x => x.path == filePath).First()) + 1;
            SubPage = new MediaTagPage(ref FilteredImagePaths, DB, fileindex - 1);

            string filename = Path.GetFileNameWithoutExtension(filePath);

            TabItem NewTab = new()
            {
                Header = "File: " + ImageCountString(fileindex),
                Content = SubPage
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabBar.Items.Add(NewTab);
            NewTab.IsSelected = true;
        }

        private void NextPage_Clicked(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        private void PrevPage_Clicked(object sender, RoutedEventArgs e)
        {
            PreviousPage();
        }

        private void FirstPage_Clicked(object sender, RoutedEventArgs e)
        {
            FirstPage();
        }

        private void LastPage_Clicked(object sender, RoutedEventArgs e)
        {
            LastPage();
        }

        private void NextPage()
        {
            PageIndex++;    //increment page index
            if (PageIndex > PageAmount) //make sure page index didn't go over max number of pages
            {
                PageIndex = (int)Math.Ceiling(PageAmount);    //if page index went over max number of pages set page index back to max number of pages
            }
            else
            {
                PopulateThumbnailGrid(FilteredImagePaths);
                Scroller.ScrollToTop();
            }
        }

        private void PreviousPage()
        {
            PageIndex--;
            if (PageIndex < 1)
            {
                PageIndex = 1;
            }
            else
            {
                PopulateThumbnailGrid(FilteredImagePaths);
                Scroller.ScrollToTop();
            }
        }

        private void FirstPage()
        {
            if (PageIndex != 1)
            {
                PageIndex = 1;
                PopulateThumbnailGrid(FilteredImagePaths);
                Scroller.ScrollToTop();
            }
        }

        private void LastPage()
        {
            if (PageIndex != (int)Math.Ceiling(PageAmount))
            {
                PageIndex = (int)Math.Ceiling(PageAmount);
                PopulateThumbnailGrid(FilteredImagePaths);
                Scroller.ScrollToTop();
            }
        }

        private void LastVideoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FilteredImagePaths = ImagePaths;
            SearchBar.Text = "";
            ImageItem ImageFile = FilteredImagePaths.Where(x => x.path == DB.GetLastFileOpened()).First();
            int fileindex = FilteredImagePaths.IndexOf(ImageFile) + 1;
            MediaTagPage SubPage = new(ref FilteredImagePaths, DB, fileindex - 1);

            Logger.Write("Opening Media page for Last File Opened: " + ImageFile.path);

            TabItem NewTab = new()
            {
                Header = "File: " + ImageCountString(fileindex),
                Content = SubPage
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabBar.Items.Add(NewTab);
            NewTab.Focus();
        }

        private string ImageCountString(int fileindex)
        {
            return fileindex.ToString() + " / " + FilteredImagePaths.Count.ToString() +  " [" + ((fileindex % ResultsPerPage) == 0 ? ResultsPerPage.ToString() : (fileindex % ResultsPerPage).ToString()) + " / " + (FilteredImagePaths.Count < ResultsPerPage ? FilteredImagePaths.Count.ToString() : ResultsPerPage.ToString()) + "]";
        }

        private void FirstUntaggedItem_Click(object sender, RoutedEventArgs e)
        {
            ImageItem ImageFile = FilteredImagePaths.Where(x => x.tag_list.Length < 2).First();

            Logger.Write("Opening Media page for First Untagged Item: " + ImageFile.path);

            int TempIndex = PageIndex;

            int fileindex = FilteredImagePaths.IndexOf(ImageFile) + 1;

            PageIndex = (int)Math.Ceiling(fileindex / (double)ResultsPerPage);
            try
            {
                if (PageIndex < (int)Math.Ceiling(PageAmount) && PageIndex > 0)
                {
                    PopulateThumbnailGrid(FilteredImagePaths);
                    Scroller.ScrollToTop();
                }
                else
                {
                    PageIndex = TempIndex;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error moving to first page with an Untagged item. - " + ex.Message);
            }
        }


        private void FirstUnratedItem_Click(object sender, RoutedEventArgs e)
        {
            ImageItem ImageFile = FilteredImagePaths.Where(x => x.rating == 0).First();

            Logger.Write("Opening Media page for First Unrated Item: " + ImageFile.path);

            int TempIndex = PageIndex;

            int fileindex = FilteredImagePaths.IndexOf(ImageFile) + 1;

            PageIndex = (int)Math.Ceiling(fileindex / (double)ResultsPerPage);
            try
            {
                if (PageIndex < (int)Math.Ceiling(PageAmount) && PageIndex > 0)
                {
                    PopulateThumbnailGrid(FilteredImagePaths);
                    Scroller.ScrollToTop();
                }
                else
                {
                    PageIndex = TempIndex;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error moving to first page with an Unrated Item. - " + ex.Message);
            }
        }

        private void FirstUnLinkedItem_Click(object sender, RoutedEventArgs e)
        {
            ImageItem ImageFile = FilteredImagePaths.Where(x => x.primary_source == "").First();

            Logger.Write("Opening Media page for First Unlinked Item: " + ImageFile.path);

            int TempIndex = PageIndex;

            int fileindex = FilteredImagePaths.IndexOf(ImageFile) + 1;

            PageIndex = (int)Math.Ceiling(fileindex / (double)ResultsPerPage);
            try
            {
                if (PageIndex < (int)Math.Ceiling(PageAmount) && PageIndex > 0)
                {
                    PopulateThumbnailGrid(FilteredImagePaths);
                    Scroller.ScrollToTop();
                }
                else
                {
                    PageIndex = TempIndex;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error moving to first page. - " + ex.Message);
            }
        }

        private void RandomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Random RNG = new();
            MediaTagPage SubPage;

            int Rando;
            Rando = RNG.Next(0, FilteredImagePaths.Count - 1);
            SubPage = new MediaTagPage(ref FilteredImagePaths, DB, Rando);

            Logger.Write("Opening Media page for Random File: " + FilteredImagePaths[Rando].path);

            int fileindex = Rando + 1;

            TabItem NewTab = new()
            {
                Header = "File: " + ImageCountString(fileindex),
                Content = SubPage
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabBar.Items.Add(NewTab);
            NewTab.Focus();
        }

        private void BottomPagenumber_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int TempIndex = PageIndex;

                try
                {
                    PageIndex = Convert.ToInt32(BottomPagenumber.Text);
                    if (PageIndex > PageAmount) //make sure page index didn't go over max number of pages
                    {
                        PageIndex = TempIndex;    //if page index went over max number of pages set page index back to max number of pages
                    }
                    else
                    {
                        PopulateThumbnailGrid(FilteredImagePaths);
                        Scroller.ScrollToTop();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message);
                    PageIndex = TempIndex;
                }
            }
        }

        private void BottomPagenumber_GotMouseCapture(object sender, MouseEventArgs e)
        {
            BottomPagenumber.SelectAll();
        }

        #endregion

        #region Functional Tab Navigation

        private void TagManagerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Opening All Tags Tag Manager");
            TagManager TM = new(DB);
            TabItem NewTab = new()
            {
                Header = "All Tags",
                Content = TM
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabBar.Items.Add(NewTab);
            NewTab.Focus();
        }

        private void Slideshow_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Opening Slideshow");
            Slideshow SS = new(FilteredImagePaths, DB);
            SS.Show();
        }

        private void DupeComparison_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Opening Dupes Comparison");
            DupesPage DP = new(DB.GetDupes(), DB);
            TabItem NewTab = new()
            {
                Header = "Dupes",
                Content = DP
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabBar.Items.Add(NewTab);
            NewTab.Focus();
        }

        #endregion

        #region Search Functions

        private void AcceptSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            PageIndex = 1;
            if (SearchBar.Text == "")
            {
                FilteredImagePaths = ImagePaths;
                SearchCount.Content = FilteredImagePaths.Count.ToString();
                PopulateThumbnailGrid(FilteredImagePaths);                
            }
            else
            {
                FilteredImagePaths = DB.ComplexSearch(SearchBar.Text);
                //Logger.Write("Searched for: " + SearchBar.Text);
                SearchCount.Content = FilteredImagePaths.Count.ToString();
                PopulateThumbnailGrid(FilteredImagePaths);
            }
        }

        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (SearchBar.Selected_Index == -1)
                {
                    SearchBar.ClosePopup();
                    Search();
                }
                else
                {
                    SearchBar.GetSelected();
                }
            }
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Search bar reset");
            SearchBar.Text = "";
            QuickSearchFilename.Text = "";
            //SearchBar.Background = System.Windows.Media.Brushes.White;
            Search();
        }

        #endregion

        #region Tools

        private void IQDBItem_Click(object sender, RoutedEventArgs e)
        {
            IQDBSearch();
        }

        private async void IQDBSearch()
        {
            Logger.Write("IQDB Search Started");
            int limit = 300;
            List<ImageItem> imgs = DB.GetNonIQDBImages();
            IqdbApi.Models.SearchResult searchResults = new IqdbApi.Models.SearchResult();
            IIqdbClient api = new IqdbClient();
            if (imgs.Count > 0)
            {
                if (imgs.Count < 300)
                {
                    limit = imgs.Count;
                }
                for (int index = 0; index < limit; index++)
                {
                    try
                    {
                        if (imgs[index].height < 7500 & imgs[index].width < 7500 & !VideoTypes.Contains(Path.GetExtension(imgs[index].path)))
                        {
                            int Match = -1;
                            using (var fs = new FileStream(imgs[index].path, FileMode.Open))
                            {
                                if(fs.Length <= 8192000)
                                {
                                    searchResults = await api.SearchFile(fs);
                                }
                                else
                                {
                                    Logger.Write("File Too Large");
                                }
                                
                            }
                            if (!searchResults.IsFound)
                            {
                                Logger.Write("Not on IQDB");
                            }
                            else
                            {
                                for (int r = 0; r <= searchResults.Matches.Count - 1; r++)
                                {
                                    if (searchResults.Matches[r].Source == IqdbApi.Enums.Source.Danbooru)
                                    {
                                        Match = r;
                                        break;
                                    }
                                }
                                if (Match == -1)
                                {
                                    Logger.Write("Not on IQBD");
                                }
                                else if (searchResults.Matches[Match].Similarity < 70)
                                {
                                    Logger.Write("Similarity Score Too Low");
                                }
                                else
                                {
                                    Logger.Write("Http:" + searchResults.Matches[Match].Url);

                                    string otherprimary = DB.GetPrimarySource(imgs[index].hash);
                                    if (otherprimary.Contains("e621"))
                                    {
                                        DB.AddNonPrimarySource(imgs[index].path, "https:" + searchResults.Matches[Match].Url);
                                        DB.UpdateBooruTagged(imgs[index].path, true);
                                    }
                                    else if(otherprimary == "")
                                    {
                                        DB.AddPrimarySource(imgs[index].path, "https:" + searchResults.Matches[Match].Url);
                                        DB.UpdateBooruTagged(imgs[index].path, true);
                                    }
                                    else if(otherprimary.Contains("danbooru"))
                                    {
                                        //DB.AddRowToSourcesTable(imgs[index].path, "https:" + searchResults.Matches[Match].Url, false);
                                    }
                                    else
                                    {
                                        DB.AddPrimarySource(imgs[index].path, "https:" + searchResults.Matches[Match].Url);
                                        DB.UpdateBooruTagged(imgs[index].path, true);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Write(imgs[index].path + ":");
                        Logger.Write("IQDB Exception: " + e.Message);
                    }
                    DB.UpdateIqdb(imgs[index].path);
                    IQDBItem.Header = "IQDB (" + (index + 1).ToString() + @" / " + imgs.Count.ToString() + ")";
                }

                tbi.ShowBalloonTip("IDQB", "IQDB Completed", BalloonIcon.None);
            }
            else
            {
                IQDBItem.Header = "IQDB (0 / 0)";
            }

        }

        private void DupesProcessing_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Started Dupes Processing");
            WindowState = WindowState.Minimized;
            List<ImageItem> UncorrelatedImages = DB.GetUncorrelatedImages();
            List<ImageItem> CorrelatedImages = DB.GetCorrelatedImages();
            for (int i = 0; i < UncorrelatedImages.Count; i++)
            {
                if (CorrelatedImages.Count > 0)
                {
                    for (int j = 0; j < CorrelatedImages.Count; j++)
                    {
                        if (UncorrelatedImages[i].phash == null)
                        {
                            GeneratePHash(UncorrelatedImages[i].path);
                        }
                        if (CorrelatedImages[j].phash == null)
                        {
                            GeneratePHash(CorrelatedImages[j].path);
                        }
                        if (UncorrelatedImages[i].phash == null)
                        {
                            Logger.Write(UncorrelatedImages[i].path + " phash is null");
                            break;
                        }
                        if (CorrelatedImages[j].phash == null)
                        {
                            Logger.Write(CorrelatedImages[j].path + " phash is null");
                            break;
                        }
                        float score = ImagePhash.GetCrossCorrelation(UncorrelatedImages[i].phash, CorrelatedImages[j].phash);
                        if (score > 0.97)
                        {
                            if (!DB.CheckifDupeExists(UncorrelatedImages[i].hash, CorrelatedImages[j].hash))
                            {
                                Logger.Write("Duplicate found between: " + UncorrelatedImages[i].path + " & " + CorrelatedImages[j].path + " with score: " + score.ToString());
                                DB.AddRowToDupesTable(UncorrelatedImages[i].hash, CorrelatedImages[j].hash, score);
                            }
                        }
                    }
                }
                DB.UpdateCorrelated(UncorrelatedImages[i].path);
                CorrelatedImages.Add(UncorrelatedImages[i]);
            }

            tbi.ShowBalloonTip("Dupes", "Dupes Processed", BalloonIcon.None);
        }

        private void BooruAuto_Click(object sender, RoutedEventArgs e)
        {
            BooruAutoTaggerWorker.RunWorkerAsync();
        }
        private void BooruAutoTaggerWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BooruAuto.Header = "Auto Booru Tag (" + (e.ProgressPercentage + 1).ToString() + @" / 300)";
        }

        private async void BooruAutoTaggerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Logger.Write("Started Booru Auto Tagger");
            List<ImageItem> NonAutoTaggedImages = DB.GetBooruUntaggedItems();

            for (int i = 0; i < NonAutoTaggedImages.Count; i++)
            {
                if (NonAutoTaggedImages[i].sources.Count > 0)
                {
                    foreach (string source in NonAutoTaggedImages[i].sources)
                    {
                        SearchResult res;
                        if (source.Contains("danbooru") || source.Contains("Danbooru"))
                        {
                            try
                            {
                                string id = source[33..];

                                BooruSharp.Booru.DanbooruDonmai danb = new();
                                res = await danb.GetPostByIdAsync(int.Parse(id));

                                res.Tags.Append("Hentai");
                            }
                            catch(Exception ex)
                            {
                                Logger.Write("Error Getting Tags from source: " + source);
                                continue;
                            }
                        }
                        else if (source.Contains("e621"))
                        {
                            try
                            {
                                string id = source[23..];

                                BooruSharp.Booru.E621 furry = new();
                                res = await furry.GetPostByIdAsync(int.Parse(id));

                                res.Tags.Append("Hentai");
                                res.Tags.Append("Furry");
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Error Getting Tags from source: " + source);
                                continue;
                            }
                        }
                        else if (source.Contains("rule34.xxx"))
                        {
                            try
                            {
                                string id = source[49..];

                                BooruSharp.Booru.Rule34 rule34xxx = new();
                                res = await rule34xxx.GetPostByIdAsync(int.Parse(id));

                                res.Tags.Append("Hentai");
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Error Getting Tags from source: " + source);
                                continue;
                            }
                        }
                        else if (source.Contains("realbooru"))
                        {
                            try
                            {
                                string id = source[56..];

                                BooruSharp.Booru.Realbooru realb = new BooruSharp.Booru.Realbooru();
                                res = await realb.GetPostByIdAsync(int.Parse(id));
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Error Getting Tags from source: " + source);
                                continue;
                            }
                        }
                        else if (source.Contains("Gelbooru") || source.Contains("gelbooru"))
                        {
                            try
                            {
                                string id = source[51..];

                                BooruSharp.Booru.Gelbooru gelb = new BooruSharp.Booru.Gelbooru();
                                res = await gelb.GetPostByIdAsync(int.Parse(id));

                                res.Tags.Append("Hentai");
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Error Getting Tags from source: " + source);
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        foreach (string Tag in res.Tags)
                        {
                            int sourcenumber = MediaTagPage.GetBooruUrl(source);
                            int PodoboTagId = DB.GetTagIdfromTagMap(Tag, sourcenumber);
                            if (PodoboTagId == -1) //If Tag Currently isn't in Booru Tag map
                            {
                                if(!DB.CheckIfProcessingCompleted(Tag, sourcenumber)) //If We Haven't already processed this tag (can be processed but not in Map if Ignored)
                                {
                                    if (!DB.CheckIfPostProcessingExists(NonAutoTaggedImages[i].hash, Tag, sourcenumber))
                                    {
                                        DB.AddRowToPostprocessingTable(NonAutoTaggedImages[i].hash, Tag, sourcenumber);
                                    }
                                }                                
                            }
                            else
                            {
                                DB.AddTagByID(NonAutoTaggedImages[i].path, PodoboTagId);
                            }
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                }

                DB.UpdateBooruTagged(NonAutoTaggedImages[i].path, true);
                await Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => { BooruAuto.Header = "Auto Booru Tag (" + (i + 1).ToString() + @" / " + NonAutoTaggedImages.Count.ToString() + ")"; }));
                
            }
            tbi.ShowBalloonTip("Booru Auto Tagger", "Booru Auto Tagger Completed", BalloonIcon.None);
        }

        private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Exporting Database");
            DB.Backup();
        }

        #endregion

        #region General Window Functions

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WindowState = WindowState.Minimized;
            }
            else if(e.Key == Key.F8)
            {
                PasswordBox.Text = "";
                Menubar.Visibility = Visibility.Hidden;
                TabBar.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Focus();
                WindowState = WindowState.Minimized;
            }
        }

        private void tbi_TrayMouseDoubleClick(object sender, RoutedEventArgs args)
        {
            if(WindowState == WindowState.Minimized)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            else
            {
                Show();
            }
        }

        #endregion

        #region Closing Functions

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AlwaysClosingJobs();
            ClosingJob();
            tbi.Dispose();
            //DB.Disconnect();
            DB.SQLiteClose();
        }

        private void AlwaysClosingJobs()
        {
            WindowState = WindowState.Minimized;
            //TrayIcon.BalloonTipTitle = "Closing Job 1";
            //TrayIcon.BalloonTipText = "Retroactivley Adding Parent Tags";
            //TrayIcon.ShowBalloonTip(10000, "Closing Job 1", "Retroactivley Adding Parent Tags", System.Windows.Forms.ToolTipIcon.Info);
            Logger.Write("Retroactivley Adding Parent Tags");

            List<RelationshipItem> ParentTags = DB.GetRetroParentTagRelationships();
            foreach (RelationshipItem RI in ParentTags)
            {
                List<ImageItem> TaggedImages = DB.GetImagesWithSpecificTag(RI.ChildAliasId);
                foreach (ImageItem II in TaggedImages)
                {
                    string childtag = DB.GetTagName(RI.ChildAliasId);
                    DB.CheckForParentTags(childtag, II.path);
                    Logger.Write("Adding Parent Tags for " + childtag + " in image: " + II.path);
                    DB.UpdateParentRetro(RI.ChildAliasId, RI.ParentPreferredId);
                }
            }

            //TrayIcon.BalloonTipTitle = "Closing Job 2";
            //TrayIcon.BalloonTipText = "Retroactivley Adding Sibling Tags";
            Logger.Write("Retroactivley Adding Sibling Tags");

            List<RelationshipItem> SiblingTags = DB.GetRetroSiblingTagRelationships();
            foreach (RelationshipItem RI in SiblingTags)
            {
                List<ImageItem> TaggedImages = DB.GetImagesWithSpecificTag(RI.ChildAliasId);
                foreach (ImageItem II in TaggedImages)
                {
                    string tag = DB.GetTagName(RI.ChildAliasId);
                    DB.ReplaceOldSiblingTag(tag, II.path);
                    Logger.Write("Replacing Old Sibling Tag: " + tag + " in image: " + II.path);                    
                }
                List<RelationshipItem> SiblingedParentTags = DB.GetParentTagRelationshipsContaining(DB.GetTagName(RI.ChildAliasId));
                //Get parents of old sibling tag that was updated to new sibling tag
                foreach (RelationshipItem RI2 in SiblingedParentTags)
                {
                    if (RI2.ChildAliasId == RI.ChildAliasId)
                    {
                        //if old sibling tag is equal to child of parent relationship
                        //Update Child tag to be New Sibling
                        Logger.Write("Updating Child Tag from " + DB.GetTagName(RI2.ChildAliasId) + " to " + DB.GetTagName(RI.ParentPreferredId));
                        DB.UpdateChildTag(RI2.ChildAliasId, RI.ParentPreferredId, RI2.ParentPreferredId);
                    }
                    else if (RI2.ParentPreferredId == RI.ChildAliasId)
                    {
                        //if old sibling tag is equal to parent of parent relationship
                        //Update Parent tag to be New Sibling
                        Logger.Write("Updating Parent Tag from " + DB.GetTagName(RI2.ParentPreferredId) + " to " + DB.GetTagName(RI.ParentPreferredId));
                        DB.UpdateParentTag(RI2.ChildAliasId, RI2.ParentPreferredId, RI.ParentPreferredId);
                    }
                }
                DB.UpdateSiblingRetro(RI.ChildAliasId, RI.ParentPreferredId);
            }

            //TrayIcon.BalloonTipTitle = "Closing Job 7";
            //TrayIcon.BalloonTipText = "Adding 'delete' Images to Trash";
            Logger.Write("Adding 'delete' Images to Trash");

            List<string> ToTrash = DB.SearchByExactTag("delete");
            foreach (string file in ToTrash)
            {
                if (!DB.CheckIfHashHasBeenDeletedBefore(DB.GetHash(file)))
                {
                    DB.AddRowToDeletedImagesTable(DB.GetHash(file));
                    Logger.Write("Adding " + file + " to Trash");
                }
            }
        }

        private void ClosingJob()
        {
            WindowState = WindowState.Minimized;
            int JobtoPerform = DB.GetClosingJob();
            switch (JobtoPerform)
            {
                case 1:
                    //1. Clear Empty tags from Images tag_list
                    DB.RemoveBrokenTagsfromImages();
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 2:
                    //2. Remove Duplicate Sources
                    //Logger.Write("Removing Duplicate Sources");
                    //List<Tuple<string, string>> NonPrimaryDanbooruSources = DB.get
                    //foreach (Tuple<string, string> danbooru in NonPrimaryDanbooruSources)
                    //{
                    //    string otherprimary = DB.GetPrimarySource(danbooru.Item1);
                    //    if (otherprimary == danbooru.Item2)
                    //    {
                    //        DB.RemoveSource(danbooru.Item1, danbooru.Item2);
                    //    }
                    //    else if (otherprimary == "")
                    //    {
                    //        DB.UpdatePrimary(danbooru.Item1, danbooru.Item2, true);
                    //    }
                    //    else if (!otherprimary.Contains("e621"))
                    //    {
                    //        DB.ChangePrimarySource(danbooru.Item1, otherprimary, danbooru.Item2);
                    //    }

                    //}
                    //DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 3:
                    //3. Process need phashes
                    //TrayIcon.BalloonTipTitle = "Closing Job 3";
                    //TrayIcon.BalloonTipText = "Calculating Missing PHashes";
                    Logger.Write("Calculating Missing PHashes");

                    List<string> needphash = DB.GetImagesWithoutphash();
                    foreach (string p in needphash)
                    {
                        Logger.Write("Generating PHash for: " + p);
                        GeneratePHash(p);
                    }
                    DB.UpdateClosingJob(7);
                    break;
                case 4:
                    //4.Check for deleted files
                    //TrayIcon.BalloonTipTitle = "Closing Job 4";
                    //TrayIcon.BalloonTipText = "Checking for Deleted Files";
                    Logger.Write("Checking for Deleted Files");

                    List<string> files = DB.GetAllPaths(0, true);
                    foreach (string file in files)
                    {
                        if (!File.Exists(file))
                        {
                            Logger.Write(file + " has been deleted");
                            string hash = DB.GetHash(file);
                            DB.UpdateDeleted(hash);
                            Logger.Write("Added " + file + " to deleted Table");
                            DB.DeleteDupe(hash);
                            Logger.Write("Removed instances of " + file + " from dupes table");
                            DB.DeleteAllImageTags(file);
                            Logger.Write("Deleteing all tags from: " + file);
                            DB.DeleteFromImageData(file);
                            Logger.Write("Delete row from image_data for file: " + file);
                        }
                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 5:
                    //5. Double Check Tag Count
                    //TrayIcon.BalloonTipTitle = "Closing Job 5";
                    //TrayIcon.BalloonTipText = "Double-Check Tag Count";
                    Logger.Write("Double Checking Tag Count");

                    List<int> TagIDs = DB.GetAllTagIDs();
                    foreach (int ID in TagIDs)
                    {
                        int CalculatedTagCount = DB.CalculateTagCount(ID);
                        if (CalculatedTagCount != DB.GetTagCount(ID))
                        {
                            Logger.Write("Tag Count is inaccurate. Changing Count to " + CalculatedTagCount + " for " + DB.GetTagName(ID));
                            DB.UpdateTagCount(ID, CalculatedTagCount);
                        }
                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 6:
                    //6. Remove Broken Sibling Relationship Tags
                    //TrayIcon.BalloonTipTitle = "Closing Job 6";
                    //TrayIcon.BalloonTipText = "Removing Broken Sibling Relationships";
                    Logger.Write("Removing Broken Sibling Relationships");

                    List<RelationshipItem> BrokenSiblings = DB.GetBrokenSiblingRelationships();
                    foreach (RelationshipItem sib in BrokenSiblings)
                    {
                        Logger.Write("Deleting Sibling relationship - Alias: " + DB.GetTagName(sib.ChildAliasId) + " & Preferred: " + DB.GetTagName(sib.ParentPreferredId));
                        DB.DeleteSiblingRelationship(sib.ChildAliasId, sib.ParentPreferredId);
                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 7:
                    //7. Remove Broken Parent Relationship Tags
                    //TrayIcon.BalloonTipTitle = "Closing Job 7";
                    //TrayIcon.BalloonTipText = "Removing Broken Parent Relationships";
                    Logger.Write("Removing Broken Parent Relationships");

                    List<RelationshipItem> BrokenParents = DB.GetBrokenParentRelationships();
                    foreach (RelationshipItem rel in BrokenParents)
                    {
                        Logger.Write("Deleting Parent relationship - Child: " + DB.GetTagName(rel.ChildAliasId) + " & Parent: " + DB.GetTagName(rel.ParentPreferredId));
                        DB.DeleteParentRelationship(rel.ChildAliasId, rel.ParentPreferredId);
                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 8:
                    //8. Delete files older than 30 days from deleted_images Table
                    //TrayIcon.BalloonTipTitle = "Closing Job 8";
                    //TrayIcon.BalloonTipText = "Deleting Files in Trash";
                    Logger.Write("Deleting Files in Trash");

                    List<string> ToDelete = DB.GetImagesToDelete();
                    foreach (string file in ToDelete)
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Logger.Write("Deleting " + file);
                        }
                        DB.UpdateDeleted(DB.GetHash(file));
                        Logger.Write("Added " + file + " to deleted Table");
                        DB.DeleteDupe(DB.GetHash(file));
                        Logger.Write("Removed instances of " + file + " from dupes table");
                        DB.DeleteAllImageTags(file);
                        Logger.Write("Deleteing all tags from: " + file);
                        DB.DeleteFromImageData(file);
                        Logger.Write("Delete row from image_data for file: " + file);
                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 9:
                    //9. Database Export
                    //TrayIcon.BalloonTipTitle = "Closing Job 9";
                    //TrayIcon.BalloonTipText = "Exporting Database";
                    Logger.Write("Exporting Database");

                    if (DateTime.Now.Hour < 21)
                    {
                        //DB.SQLiteClose();
                        DB.Backup();
                        DB.UpdateClosingJob(JobtoPerform + 1);
                    }
                    break;
                case 10:
                    //10. Rotate Image Tagged with rotatecw
                    //TrayIcon.BalloonTipTitle = "Closing Job 10";
                    //TrayIcon.BalloonTipText = "Rotating Images CW";
                    Logger.Write("Rotating Images CW");
                    System.Drawing.Imaging.ImageFormat formatcw = System.Drawing.Imaging.ImageFormat.Jpeg;

                    List<string> RotateFilesCW = DB.SearchByExactTag("rotatecw");

                    foreach (string image in RotateFilesCW)
                    {
                        using (System.Drawing.Image img = System.Drawing.Image.FromFile(image))
                        {
                            switch (Path.GetExtension(image))
                            {
                                case ".jpg":
                                    formatcw = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".jpeg":
                                    formatcw = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".png":
                                    formatcw = System.Drawing.Imaging.ImageFormat.Png;
                                    break;
                                default:
                                    break;
                            }
                            //rotate the picture by 90 degrees and re-save the picture
                            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            img.Save(image, formatcw);
                            Logger.Write("Rotating " + image + " CW");
                        }
                        //need to calculate new hash
                        string[] hasharray = GenerateHash(image);
                        DB.UpdateHash(hasharray[0], image);
                        DB.UpdateMD5(hasharray[1], image);
                        Logger.Write("Updateing Hash for " + image);
                        //need to update thumbnail
                        GenerateThumbnail(image);
                        Logger.Write("Generating Thumbnail for " + image);
                        //need to update PHash
                        GeneratePHash(image);
                        Logger.Write("Generating PHash for " + image);
                        DB.RemoveTag(image, DB.GetTagid("rotatecw"));

                    }
                    DB.UpdateClosingJob(JobtoPerform + 1);
                    break;
                case 11:
                    //11. Rotate Image Tagged with rotateccw
                    //TrayIcon.BalloonTipTitle = "Closing Job 11";
                    //TrayIcon.BalloonTipText = "Rotating Images CCW";
                    Logger.Write("Rotating Images CCW");
                    System.Drawing.Imaging.ImageFormat formatccw = System.Drawing.Imaging.ImageFormat.Jpeg;

                    List<string> RotateFilesCCW = DB.SearchByExactTag("rotateccw");

                    foreach (string image in RotateFilesCCW)
                    {
                        using (System.Drawing.Image img = System.Drawing.Image.FromFile(image))
                        {
                            switch (Path.GetExtension(image))
                            {
                                case ".jpg":
                                    formatccw = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".jpeg":
                                    formatccw = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;
                                case ".png":
                                    formatccw = System.Drawing.Imaging.ImageFormat.Png;
                                    break;
                                default:
                                    break;
                            }
                            //rotate the picture by 90 degrees and re-save the picture
                            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            img.Save(image, formatccw);
                            Logger.Write("Rotating " + image + " CCW");
                        }
                        //need to calculate new hash
                        string[] hasharray = GenerateHash(image);
                        DB.UpdateHash(hasharray[0], image);
                        DB.UpdateMD5(hasharray[1], image);
                        Logger.Write("Updateing Hash for " + image);
                        //need to update thumbnail
                        GenerateThumbnail(image);
                        Logger.Write("Generating Thumbnail for " + image);
                        //need to update PHash
                        GeneratePHash(image);
                        Logger.Write("Generating PHash for " + image);
                        DB.RemoveTag(image, DB.GetTagid("rotateccw"));

                    }
                    DB.UpdateClosingJob(1);
                    break;
                default:
                    break;
            }
        }

        #endregion

        private void TextFFME_Click(object sender, RoutedEventArgs e)
        {
            ////Logger.Write("Opening Dupes Comparison");
            //FFMEDisplay FD = new FFMEDisplay
            //{
            //    Mediafile = @"Y:\Grabber\kami otaku_3a35b1efa4a200efa7221c3ee0f2974f_rule34.xxx_4357820.gif",
            //    Volume = 0.02,
            //    Position = new TimeSpan(0, 0, 0),
            //    IsPlaying = true
            //};
            //ClosableTab NewTab = new ClosableTab
            //{
            //    Title = "FFME",
            //    Content = FD
            //};
            //TabBar.Items.Add(NewTab);
            //NewTab.Focus();

            //using (ShellObject shell = ShellObject.FromParsingName(@"Y:\Media\WEBM\3ac723f41cc29c6872a83c674beec94c.webm"))
            //{
            //    var dur = shell.Properties.System.Media.Duration;
            //    double l = TimeSpan.FromTicks((long)(ulong)dur.ValueAsObject).TotalSeconds;
            //    int Duration = (int)Math.Floor(l);

            //    var rate = shell.Properties.System.Video.FrameRate;
            //    var ratefl = (float)(uint)rate.ValueAsObject;
            //    var rateob = ratefl / 1000;

            //    var frameheight = shell.Properties.System.Video.FrameHeight;
            //    var heightob = (int)(uint)frameheight.ValueAsObject;

            //    var framewidth = shell.Properties.System.Video.FrameWidth;
            //    var widthob = (int)(uint)framewidth.ValueAsObject;

            //    var audio = (int)(uint)shell.Properties.System.Audio.ChannelCount.ValueAsObject;

            //    //shell.Thumbnail.LargeBitmap.Save(@"Y:\testy.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            //}


            //WindowState = WindowState.Minimized;

            //List<string> paths = DB.GetAllPaths(0, false);

            //foreach (string path in paths)
            //{
            //    try
            //    {
            //        using (ShellObject shell = ShellObject.FromParsingName(path))
            //        {
            //            shell.Thumbnail.ExtraLargeBitmap.Save(LargeThumbLocation + Path.GetFileNameWithoutExtension(path) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            //        }
            //    }
            //    catch
            //    {
            //        //GenerateThumbnail(path);
            //        Logger.Write("error getting shell thumnail for " + path);
            //    }

            //}

            //List <ImageItem> un = DB.GetUncorrelatedImages();
            //foreach(ImageItem h in un)
            //{
            //    DB.Updatephash(h.sha_id, new byte[40]);
            //    DB.UpdateCorrelated(h.sha_id);
            //}

            //string filePath = @"Y:\Media\GIF\yah-buoy_Zelda_fbnjwl.gif";
            //List<ImageThumbnailItem> gifs = DB.ComplexSearch("$extension='.gif'");
            //int animationDuration = 0;
            //bool animated = false;
            //bool loop = false;
            //float rate = 0.0F;

            //for(int i = 0; i < ImagePaths.Count; i++)
            //{
            //    DB.UpdateID(ImagePaths[i].path, i+1);
            //}

            //BooruSharp.Booru.E621 furry = new BooruSharp.Booru.E621();
            //BooruSharp.Search.Post.SearchResult res = await furry.GetPostByIdAsync(2485940);

            //string url = "https://e621.net/posts/2485940.json";

            //ImgurAPI howie = new ImgurAPI();
            //howie.Testarino();

            //List<string> listy = DB.GetEmptySources("Danbooru");

            //foreach (string path in listy)
            //{
            //    if (path.ToLower().Contains("danbooru"))
            //    {
            //        string[] filecontents = path.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            //        string sourceurl = "https://danbooru.donmai.us/posts/";
            //        sourceurl += Path.GetFileNameWithoutExtension(filecontents[^1]);
            //        DB.AddRowToSourcesTable(path, sourceurl, true);
            //    }
            //}

            //DB.SQLiteConnect();
            ////DB.CreateSQLiteTables();

            //WindowState = WindowState.Minimized;
            //Logger.Write("Importing External File Data");

            //string ImportData = "";
            //string FileToRead = @"C:\Users\Chris\AppData\Roaming\Paiz\Backupphashes.txt";
            //using (StreamReader Reader = new StreamReader(FileToRead))
            //{
            //    ImportData = Reader.ReadToEnd();
            //}
            //string[] DataLines = ImportData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 2; i <= DataLines.Length - 1; i++)
            //{
            //    string[] LineColumns = DataLines[i].Split(new string[] { "||" }, StringSplitOptions.None);

            //    DB.Updatephash(DB.GetPath(LineColumns[0]), GetBytes(LineColumns[1]));

            //    //DB.TagDataInsert(int.Parse(LineColumns[0]), LineColumns[1], LineColumns[2], int.Parse(LineColumns[3]), int.Parse(LineColumns[4]));

            //    //DB.SiblingDataInsert(int.Parse(LineColumns[0]), int.Parse(LineColumns[1]), LineColumns[2] == "True");

            //    //DB.ParentDataInsert(int.Parse(LineColumns[0]), int.Parse(LineColumns[1]), LineColumns[2] == "True");

            //    //DB.DupesInsert(LineColumns[1], LineColumns[2], float.Parse(LineColumns[3]), LineColumns[4] == "True");

            //    //DB.DeletedImagesInsert(LineColumns[1], LineColumns[2] == "True", DateTime.Parse(LineColumns[3]));

            //    //DB.BooruTagMapInsert(LineColumns[0], int.Parse(LineColumns[1]));

            //    //string[] sources = DB.ImageDataSelectSources(LineColumns[0]);
            //    //string source;
            //    //if (sources.Length < 1)
            //    //{
            //    //    source = " " + LineColumns[1] + " ";
            //    //}
            //    //else
            //    //{
            //    //    if (LineColumns[2] == "True")
            //    //    {
            //    //        source = " " + LineColumns[1] + " ";
            //    //        foreach (string s in sources)
            //    //        {
            //    //            source += s + " ";
            //    //        }
            //    //    }
            //    //    else
            //    //    {
            //    //        source = " ";
            //    //        foreach (string s in sources)
            //    //        {
            //    //            source += s + " ";
            //    //        }
            //    //        source += LineColumns[1] + " ";
            //    //    }
            //    //}

            //    //DB.ImageDataUpdateSources(LineColumns[0], source);

            //    //DB.ImageDataInsert(LineColumns[0], LineColumns[1], LineColumns[2], LineColumns[3], DateTime.Parse(LineColumns[4]), DateTime.Parse(LineColumns[5]), int.Parse(LineColumns[6]), int.Parse(LineColumns[7]), int.Parse(LineColumns[8]), LineColumns[9], LineColumns[17] == "True" ? true : false, LineColumns[11], LineColumns[10] == "True" ? true : false, LineColumns[12] == "True" ? true : false, LineColumns[13] == "True" ? true : false, int.Parse(LineColumns[14]), float.Parse(LineColumns[15]), LineColumns[16]);
            //}



            //DB.SQLiteClose();

            //List<ImageItem> hh = DB.GetCorrelatedImages();
            //Bitmap BMP = (Bitmap)System.Drawing.Image.FromFile(hh[0].path);
            //Digest a = ImagePhash.ComputeDigest(BMP.ToLuminanceImage());
            //BMP.Dispose();
            //var q = ImagePhash.GetCrossCorrelation(hh[0].phash, hh[1].phash);

            //DB.Connect();

            //DB.ExportPhashes();

            //DB.Disconnect();
        }

        public static byte[] GetBytes(string value)
        {
            return value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => byte.Parse(s)).ToArray();
        }

        public void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Pressed)
            {
                TabItem TI = sender as TabItem;
                if(e.Source is TabItem)
                {
                    if(TI.Header.ToString() != "Main")
                    {
                        if (TI.Content is MediaTagPage mp)
                        {
                            mp.CloseProperly();
                        }

                        TabBar.Items.Remove(TI);
                    }                    
                }                
            }
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if(TabBar.SelectedIndex > 0)
            {
                if (TabBar.SelectedItem is TabItem ti)
                {
                    if(ti.Content is MediaTagPage mp)
                    {
                        mp.CloseProperly();
                    }
                    TabBar.Items.Remove(ti);
                }                
            }
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(PasswordBox.Text == "7410")
            {
                PasswordBox.Text = "";
                Menubar.Visibility = Visibility.Visible;
                TabBar.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Hidden;
            }
        }

        private void VideoSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBar.Text = "$video";
            QuickSearchFilename.Text = "";
            Search();
        }

        private void QuickSearchFilename_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SearchBar.Text = "$name" + QuickSearchFilename.Text;
                Search();
            }            
        }

        private void QuickNotTagged_Click(object sender, RoutedEventArgs e)
        {
            SearchBar.Text = "$!tagged";
            QuickSearchFilename.Text = "";
            Search();
        }

        private void QuickSearchDuration_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(DurationCombo.SelectionBoxItem.ToString() != "~")
                {
                    SearchBar.Text = "$duration" + DurationCombo.SelectionBoxItem.ToString() + QuickSearchDuration.Text;
                    Search();
                }
                else
                {
                    int duration = int.Parse(QuickSearchDuration.Text);
                    int positivemodifier = 0;
                    int negativemodifier = 0;
                    if (duration == 0)
                    {
                        positivemodifier = 0;
                        negativemodifier = 0;
                    }
                    else if(duration == 1)
                    {
                        positivemodifier = 1;
                        negativemodifier = 0;
                    }
                    else if(duration > 1 & duration < 5)
                    {
                        positivemodifier = negativemodifier = 2;
                    }
                    else if (duration > 4 & duration < 60)
                    {
                        positivemodifier = negativemodifier = 3;
                    }
                    else if (duration > 59 & duration < 180)
                    {
                        positivemodifier = negativemodifier = 5;
                    }
                    else if (duration > 179 & duration < 600)
                    {
                        positivemodifier = negativemodifier = 20;
                    }
                    else if (duration > 599 & duration < 1800)
                    {
                        positivemodifier = negativemodifier = 60;
                    }
                    else if (duration > 1799)
                    {
                        positivemodifier = negativemodifier = 180;
                    }
                    SearchBar.Text = "$duration between " + (duration - negativemodifier).ToString() + " and " + (duration + positivemodifier).ToString();
                    Search();
                }
            }
        }
    }

}
