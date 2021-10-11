using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using IqdbApi;
using Microsoft.WindowsAPICodePack.Shell;
using BooruSharp.Search.Post;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for MediaTagPage.xaml
    /// </summary>
    public partial class MediaTagPage : UserControl
    {
        private int index;
        private readonly List<ImageItem> MediaPaths = new();
        private List<TagItem> MediaTags = new();
        private readonly List<List<Run>> TagList = new();
        private readonly int ResultsPerpage = 102;
        private readonly string[] VideoTypes = { ".avi", ".gif", ".mkv", ".mp4", ".mpg", ".webm", ".wmv" };
        private readonly DBActions DB;
        private readonly TagSuggestionSearchBar SearchBar;
        private readonly string LargeThumbLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\LargeThumbs\\";

        public MediaTagPage(ref List<ImageItem> mediapaths, DBActions DBInstance, int imageindex)
        {
            InitializeComponent();

            DB = DBInstance;
            MediaPaths = mediapaths;
            index = imageindex;

            MediaTags = DB.GetFileTags(MediaPaths[index].tag_list);

            foreach (string cat in CategoryItem.CategoryList)
            {
                _ = Categories.Items.Add(cat);
            }
            Categories.SelectedIndex = 14;

            SearchBar = new TagSuggestionSearchBar(DB);
            SearchBar.PreviewKeyDown += new KeyEventHandler(SearchBar_PreviewKeyDown);
            Grid.SetColumn(SearchBar, 1);
            _ = SearchBarGrid.Children.Add(SearchBar);

            DB.UpdateLastFileOpened(MediaPaths[index].path);

            UpdateTagList();

            UpdateImageDetails(MediaPaths[index]);

            LoadImage(MediaPaths[index].path);

        }

        private void LoadImage(string path)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (File.Exists(path))
            {
                if (TagPagePlayer.MediaFile != "")
                {
                    TagPagePlayer.Unload();
                }
                if (VideoTypes.Contains(Path.GetExtension(path)))   //@"Y:\Media\JPG\kzncx2.jpg")
                {
                    try
                    {
                        ImageDisplay.Visibility = Visibility.Hidden;
                        TagPagePlayer.Visibility = Visibility.Visible;

                        TagPagePlayer.MediaFile = path;                 // @"Y:\Media\MP4 - Clips\1080P_4000K_103997252.mp4";
                        TagPagePlayer.IsPlaying = false;
                        TagPagePlayer.Start();
                        TagPagePlayer.Position = new TimeSpan(0, 0, 0);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write("Error Loading Video '" + path + "' - " + ex.Message);
                        TagPagePlayer.MediaFile = "";
                    }
                }
                else
                {
                    try
                    {
                        ImageDisplay.Visibility = Visibility.Visible;
                        TagPagePlayer.Visibility = Visibility.Hidden;

                        ImageDisplay.Source = BitmapImageFromFile(path);
                    }
                    catch(Exception ex)
                    {
                        Logger.Write("Error Loading Image '" + path + "' - " + ex.Message);
                        ImageDisplay.Source = null;
                    }                        
                }                    
            }
        }

        private void UpdateImageDetails(ImageItem MediaFile)
        {
            PrimarySourceAddTextBox.Text = "";
            ImageDetails.Text = "Name: " + Path.GetFileNameWithoutExtension(MediaFile.path) + Environment.NewLine;
            ImageDetails.Text += "H x W: " + MediaFile.height.ToString() + " x " + MediaFile.width.ToString() + Environment.NewLine;
            ImageDetails.Text += "Date Modified: " + MediaFile.date_modfied.ToShortDateString() + Environment.NewLine;
            ImageDetails.Text += "Date Added: " + MediaFile.date_added.ToShortDateString() + Environment.NewLine + Environment.NewLine;
            ImageDetails.ToolTip = Path.GetFileNameWithoutExtension(MediaFile.path);

            if (MediaFile.primary_source == "empty")
            {
                PrimarySourceAddTextBox.Visibility = Visibility.Collapsed;
            }
            else if (MediaFile.primary_source != "")
            {
                PrimarySourceAddTextBox.Visibility = Visibility.Collapsed;
                Hyperlink source = new(new Run(MediaFile.primary_source)) { Tag = MediaFile.primary_source };
                source.Click += new RoutedEventHandler(Source_click);
                ImageDetails.Inlines.Add(source);
            }
            else
            {
                PrimarySourceAddTextBox.Visibility = Visibility.Visible;
            }
        }

        private void Source_click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink Link = sender as Hyperlink;
                //Logger.Write("Opening link " + Link.Tag.ToString() + " for image " + MediaPaths[index].path);
                System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", Link.Tag.ToString());
            }
            catch (Exception ex)
            {
                Logger.Write("Error Opening source link for image " + MediaPaths[index].path + " - " + ex.Message);
            }
        }

        private void UpdateTagList()
        {
            TagList.Clear();
            TagDisplay.Inlines.Clear();

            foreach (string cat in CategoryItem.CategoryList)
            {
                FormRunLists(cat, (SolidColorBrush)new BrushConverter().ConvertFromString(CategoryItem.CategoryByName[cat].Color), MediaTags);
            }

            foreach (List<Run> ListofRuns in TagList)
            {
                foreach (Run R in ListofRuns)
                {
                    TagDisplay.Inlines.Add(R);
                }
            }

            RatingDisplay(DB.GetRating(MediaPaths[index].path));
        }

        private void FormRunLists(string category, SolidColorBrush Color, List<TagItem> tags)
        {
            bool addfirst = false;
            List<Run> RunList = new();
            foreach (TagItem tag in tags)
            {
                if (tag.CategoryID == CategoryItem.CategoryByName[category].CategoryID)
                {
                    if (!addfirst)
                    {
                        RunList.Add(new Run(CategoryItem.CategoryByName[category].DisplayName + ":" + Environment.NewLine) { FontWeight = FontWeights.Bold, Foreground = Color });
                        addfirst = true;
                    }
                    RunList.Add(new Run("\t" + tag.Name + Environment.NewLine) { Foreground = Color, ToolTip = tag.Description });
                }
            }
            if (RunList.Count > 0)
            {
                RunList.Add(new Run(Environment.NewLine));
            }
            TagList.Add(RunList);
        }

        private static BitmapImage BitmapImageFromFile(string path)
        {
            BitmapImage bi = new();

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

                return bi;
            }
            catch (Exception ex)
            {
                Logger.Write("Error Generating bitmap for image " + path + " - " + ex.Message);
                return bi;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PreviousPage();
        }

        private void NextVideoButton_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        private void NextPage()
        {
            index++;
            if (index > MediaPaths.Count - 1)
            {
                index = 0;
            }
            ChangePage();
        }

        private void PreviousPage()
        {
            index--;
            if (index < 0)
            {
                index = MediaPaths.Count - 1;
            }
            ChangePage();
        }

        private void ChangePage()
        {
            try
            {
                Logger.Write("Changing Page to file " + MediaPaths[index].path);
                LoadImage(MediaPaths[index].path);
                MediaTags = DB.GetFileTags(MediaPaths[index].tag_list);
                UpdateTagList();
                UpdateImageDetails(MediaPaths[index]);
            }
            catch (Exception ex)
            {
                Logger.Write("Error Changing Page to file " + MediaPaths[index].path + " - " + ex.Message);
            }

            TabItem arg = (TabItem)Parent;
            arg.Header = "File: " + (((index + 1) % ResultsPerpage) == 0 ? ResultsPerpage.ToString() : ((index + 1) % ResultsPerpage).ToString()) + " / " + ResultsPerpage + " [" + (index + 1).ToString() + " / " + MediaPaths.Count.ToString() + "]";

            DB.UpdateLastFileOpened(MediaPaths[index].path);
        }

        private void EditTagsButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Write("Opening Tag Manager Tab for " + MediaPaths[index].path);
            TagManager TM = new(MediaPaths[index], DB);
            TabItem NewTab = new()
            {
                Header = Path.GetFileNameWithoutExtension(MediaPaths[index].path) + " Tags",
                Content = TM
            };

            NewTab.MouseDown += new MouseButtonEventHandler(TabItem_MouseDown);
            TabControl TB = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(this)))) as TabControl;
            TB.Items.Add(NewTab);
            NewTab.Focus();
        }

        private void Star1_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(1);
            UpdateRating(1);
        }

        private void Star2_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(2);
            UpdateRating(2);
        }

        private void Star3_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(3);
            UpdateRating(3);
        }

        private void Star4_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(4);
            UpdateRating(4);
        }

        private void Star5_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(5);
            UpdateRating(5);
        }

        private void Star6_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(6);
            UpdateRating(6);
        }

        private void Star7_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(7);
            UpdateRating(7);
        }

        private void Star8_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(8);
            UpdateRating(8);
        }

        private void Star9_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(9);
            UpdateRating(9);
        }

        private void Star10_Click(object sender, RoutedEventArgs e)
        {
            RatingDisplay(10);
            UpdateRating(10);
        }

        private void UpdateRating(int rating)
        {
            DB.UpdateRating(MediaPaths[index].path, rating);
            if (MediaPaths[index].rating < 1)
            {
                MediaPaths[index].rating = rating;
            }
        }

        private void RatingDisplay(int rating)
        {
            switch (rating)
            {
                case 1:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gray;
                    StarPoly3.Fill = Brushes.Gray;
                    StarPoly4.Fill = Brushes.Gray;
                    StarPoly5.Fill = Brushes.Gray;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 2:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gray;
                    StarPoly4.Fill = Brushes.Gray;
                    StarPoly5.Fill = Brushes.Gray;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 3:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gray;
                    StarPoly5.Fill = Brushes.Gray;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 4:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gray;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 5:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 6:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gold;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 7:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gold;
                    StarPoly7.Fill = Brushes.Gold;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 8:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gold;
                    StarPoly7.Fill = Brushes.Gold;
                    StarPoly8.Fill = Brushes.Gold;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 9:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gold;
                    StarPoly7.Fill = Brushes.Gold;
                    StarPoly8.Fill = Brushes.Gold;
                    StarPoly9.Fill = Brushes.Gold;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
                case 10:
                    StarPoly1.Fill = Brushes.Gold;
                    StarPoly2.Fill = Brushes.Gold;
                    StarPoly3.Fill = Brushes.Gold;
                    StarPoly4.Fill = Brushes.Gold;
                    StarPoly5.Fill = Brushes.Gold;
                    StarPoly6.Fill = Brushes.Gold;
                    StarPoly7.Fill = Brushes.Gold;
                    StarPoly8.Fill = Brushes.Gold;
                    StarPoly9.Fill = Brushes.Gold;
                    StarPoly10.Fill = Brushes.Gold;
                    break;
                default:
                    StarPoly1.Fill = Brushes.Gray;
                    StarPoly2.Fill = Brushes.Gray;
                    StarPoly3.Fill = Brushes.Gray;
                    StarPoly4.Fill = Brushes.Gray;
                    StarPoly5.Fill = Brushes.Gray;
                    StarPoly6.Fill = Brushes.Gray;
                    StarPoly7.Fill = Brushes.Gray;
                    StarPoly8.Fill = Brushes.Gray;
                    StarPoly9.Fill = Brushes.Gray;
                    StarPoly10.Fill = Brushes.Gray;
                    break;
            }
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AddTag(SearchBar.Text, Categories.SelectedItem.ToString());
        }

        private void AddTag(string SearchContents, string Category)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SearchContents))
                {
                    Logger.Write("Adding Tag '" + SearchContents + "' for File '" + MediaPaths[index].path);
                    MediaPaths[index].tag_list = DB.AddTag(MediaPaths[index].path, SearchContents, Category);
                    AddingTagUIUpdate();
                }
            }
            catch(Exception ex)
            {
                Logger.Write("Error Adding Tag '" + SearchContents + "' for File '" + MediaPaths[index].path + "' - " + ex.Message);
            }
        }

        private void AddTags(string SearchContents, string Category)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SearchContents))
                {
                    Logger.Write("Adding Tag '" + SearchContents + "' for File '" + MediaPaths[index].path);
                    MediaPaths[index].tag_list = DB.AddTag(MediaPaths[index].path, SearchContents, Category);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error Adding Tag '" + SearchContents + "' for File '" + MediaPaths[index].path + "' - " + ex.Message);
            }
        }

        private void AddingTagUIUpdate()
        {
            MediaTags = DB.GetFileTags(MediaPaths[index].tag_list);
            UpdateTagList();
            SearchBar.Text = "";
            Categories.SelectedIndex = 14;
        }

        private void SearchBar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchBar.GetSelected();
                AddTag(SearchBar.Text, Categories.SelectedItem.ToString());
                SearchBar.Focus();
            }
        }

        private async void IQDBButton_Click(object sender, RoutedEventArgs e)
        {
            IIqdbClient api = new IqdbClient();
            using var fs = new FileStream(MediaPaths[index].path, FileMode.Open);
            var searchResults = await api.SearchFile(fs);
        }

        private void ContextGetPath_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(MediaPaths[index].path);
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Random RNG = new();
            index = RNG.Next(0, MediaPaths.Count - 1);
            ChangePage();
        }

        private void ContextGetRule34_Click(object sender, RoutedEventArgs e)
        {
            string[] filecontents = MediaPaths[index].path.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            string sourceurl;

            if (MediaPaths[index].path.Contains("rule34.xxx"))
            {
                sourceurl = "https://rule34.xxx/index.php?page=post&s=view&id=";
            }
            else if (MediaPaths[index].path.Contains("e621"))
            {
                sourceurl = "https://e621.net/posts/";
            }
            else if (MediaPaths[index].path.Contains("danbooru"))
            {
                sourceurl = "https://danbooru.donmai.us/posts/";
            }
            else if (MediaPaths[index].path.Contains("gelbooru"))
            {
                sourceurl = "https://gelbooru.com/index.php?page=post&s=view&id=";
            }
            else
            {
                sourceurl = @"https://old.reddit.com/";
            }
            sourceurl += Path.GetFileNameWithoutExtension(filecontents[^1]);

            Clipboard.SetDataObject(sourceurl);
        }

        private void PrimarySourceAddTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if(MediaPaths[index].primary_source == "") //if source was empty before
                {
                    if (PrimarySourceAddTextBox.Text.ToLower().Contains("http"))
                    {
                        DB.AddPrimarySource(MediaPaths[index].path, PrimarySourceAddTextBox.Text);
                        MediaPaths[index].primary_source = PrimarySourceAddTextBox.Text;
                    }
                }
                else //if there was an existing source
                {
                    if (PrimarySourceAddTextBox.Text.ToLower().Contains("http"))
                    {
                        string newlink = PrimarySourceAddTextBox.Text;
                        DB.AddPrimarySource(MediaPaths[index].path, newlink);
                        MediaPaths[index].primary_source = newlink;
                    }
                }
                UpdateImageDetails(MediaPaths[index]);
            }
        }

        private void ContextEditSource_Click(object sender, RoutedEventArgs e)
        {
            PrimarySourceAddTextBox.Visibility = Visibility.Visible;
            PrimarySourceAddTextBox.Text = MediaPaths[index].primary_source;
        }

        private void ContextParseTitle_Click(object sender, RoutedEventArgs e)
        {
            string[] parts = MediaPaths[index].path.Split(new string[] { "[", "]", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 2)
            {
                SearchBar.Text = parts[2].Contains("-") ? parts[2].Split("-", StringSplitOptions.TrimEntries)[1] : parts[2].Trim();
                Categories.SelectedIndex = 5;
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1:  //Back button
                    PreviousPage();
                    break;
                case MouseButton.XButton2:  //forward button
                    NextPage();
                    break;
                default:
                    break;
            }
        }

        private void ContextGetIndex_Click(object sender, RoutedEventArgs e)
        {
            int TempIndex = index - 1;
            if (TempIndex > 0)
            {
                MediaTags = DB.CopyAllTags(MediaPaths[TempIndex].path, MediaPaths[index].path);
                UpdateTagList();
            }
        }

        public void CloseProperly()
        {
            if (TagPagePlayer.MediaPlayer.IsOpen)
            {
                TagPagePlayer.CloseProperly();
            }
            if(ImageDisplay.Source != null)
            {
                ImageDisplay.Source = null;
            }
        }

        public void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow AW = (MainWindow)Application.Current.MainWindow;

            AW.TabItem_MouseDown(sender, e);
        }

        private void ContextGetMD5_Click(object sender, RoutedEventArgs e)
        {
            using FileStream fs = new(MediaPaths[index].path, FileMode.Open)
            {
                Position = 0
            };
            using MD5 md5 = MD5.Create();
            Clipboard.SetDataObject(HexStringFromBytes(md5.ComputeHash(fs)));
        }

        private static string HexStringFromBytes(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private void CheckforEmptyMD5()
        {
            if (string.IsNullOrEmpty(MediaPaths[index].md5))
            {
                using FileStream fs = new(MediaPaths[index].path, FileMode.Open)
                {
                    Position = 0
                };
                using MD5 md5 = MD5.Create();
                string md5string = HexStringFromBytes(md5.ComputeHash(fs));
                DB.UpdateMD5(md5string, MediaPaths[index].path);
                MediaPaths[index].md5 = md5string;
            }
        }

        private void ContextRule34MD5_Click(object sender, RoutedEventArgs e)
        {
            CheckforEmptyMD5();
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://rule34.xxx/index.php?page=post&s=list&tags=md5%3a" + MediaPaths[index].md5);
        }

        private void ContextDanbooruMD5_Click(object sender, RoutedEventArgs e)
        {
            CheckforEmptyMD5();
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://danbooru.donmai.us/posts?tags=md5%3A" + MediaPaths[index].md5);
        }

        private void Contexte621MD5_Click(object sender, RoutedEventArgs e)
        {
            CheckforEmptyMD5();
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://e621.net/posts?tags=md5%3a" + MediaPaths[index].md5);
        }

        private void ContextGelbooruMD5_Click(object sender, RoutedEventArgs e)
        {
            CheckforEmptyMD5();
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://gelbooru.com/index.php?page=post&s=list&tags=md5%3a" + MediaPaths[index].md5);
        }

        private void ContextRealbooruMD5_Click(object sender, RoutedEventArgs e)
        {
            CheckforEmptyMD5();
            //System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://realbooru.com/index.php?page=post&s=list&tags=md5%3a" + MediaPaths[index].md5);
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://thh.booru.org/index.php?page=post&s=list&tags=md5%3A" + MediaPaths[index].md5);        
        }

        private void ContextSauceNao_Click(object sender, RoutedEventArgs e)
        {
            string imgururl = DB.GetImgurSource(MediaPaths[index].path);
            if (string.IsNullOrEmpty(imgururl))
            {
                ImgurAPI IA = new();
                imgururl = IA.Upload(MediaPaths[index].path);
                
            }
            if (!string.IsNullOrEmpty(imgururl))
            {
                DB.AddPrimarySource(MediaPaths[index].path, imgururl);
                System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", "https://saucenao.com/search.php?&url=" + imgururl);
            }
            else
            {
                MessageBox.Show("Error Uploading Image to Imgur");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using ShellObject shell = ShellObject.FromParsingName(MediaPaths[index].path);
                shell.Thumbnail.ExtraLargeBitmap.Save(LargeThumbLocation + Path.GetFileNameWithoutExtension(MediaPaths[index].path) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch(Exception ex)
            {
                Logger.Write("Error Attempting to Re-Import Thumbnail");
                Logger.Write(ex.Message);
            }
            
        }

        private async void GetTags()
        {
            if (MediaPaths[index].primary_source.Contains("rule34.xxx"))
            {
                //string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = MediaPaths[index].primary_source[49..];

                BooruSharp.Booru.Rule34 rule34xxx = new();
                SearchResult res = await rule34xxx.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].primary_source.Contains("e621"))
            {
                //string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = MediaPaths[index].primary_source[23..];

                BooruSharp.Booru.E621 furry = new();
                SearchResult res = await furry.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].primary_source.Contains("Danbooru") || MediaPaths[index].primary_source.Contains("danbooru"))
            {
                //string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = MediaPaths[index].primary_source[33..];

                BooruSharp.Booru.DanbooruDonmai danb = new();
                SearchResult res = await danb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].primary_source.Contains("Gelbooru") || MediaPaths[index].primary_source.Contains("gelbooru"))
            {
                //string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = MediaPaths[index].primary_source[51..];

                BooruSharp.Booru.Gelbooru gelb = new BooruSharp.Booru.Gelbooru();
                SearchResult res = await gelb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].primary_source.Contains("realbooru"))
            {
                //string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = MediaPaths[index].primary_source[56..];

                BooruSharp.Booru.Realbooru realb = new BooruSharp.Booru.Realbooru();
                SearchResult res = await realb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].path.Contains("rule34.xxx"))
            {
                string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = name.Split("_")[3];

                BooruSharp.Booru.Rule34 rule34xxx = new();
                SearchResult res = await rule34xxx.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].path.Contains("e621"))
            {
                string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = name.Split("_")[3];

                BooruSharp.Booru.E621 furry = new();
                SearchResult res = await furry.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].path.Contains("Danbooru") || MediaPaths[index].path.Contains("danbooru"))
            {
                string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = name.Split("_")[3];

                BooruSharp.Booru.DanbooruDonmai danb = new();
                SearchResult res = await danb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].path.Contains("Gelbooru") || MediaPaths[index].path.Contains("gelbooru"))
            {
                string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = name.Split("_")[3];

                BooruSharp.Booru.Gelbooru gelb = new();
                SearchResult res = await gelb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }
            else if (MediaPaths[index].path.Contains("realbooru"))
            {
                string name = Path.GetFileNameWithoutExtension(MediaPaths[index].path);
                string id = name.Split("_")[3];

                BooruSharp.Booru.Realbooru realb = new();
                SearchResult res = await realb.GetPostByIdAsync(int.Parse(id));

                TagMap TM = new(res, DB);
                TM.ShowDialog();

                List<Tuple<string, string, string>> Output = TM.Output;
                if (Output.Count > 0)
                {
                    foreach (Tuple<string, string, string> TI in Output)
                    {
                        AddTags(TI.Item1, TI.Item2);
                        if (TI.Item3 != "")
                        {
                            if (DB.GetTagIdfromTagMap(TI.Item3) == -1)
                            {
                                DB.AddRowToTagMapTable(TI.Item3, DB.GetTagid(TI.Item1));
                            }
                        }
                    }
                    AddingTagUIUpdate();
                }
            }            
        }

        private void ContextGetTags(object sender, RoutedEventArgs e)
        {
            GetTags();
        }

        private void GetTagsButton_Click(object sender, RoutedEventArgs e)
        {
            GetTags();
        }
    }
}
