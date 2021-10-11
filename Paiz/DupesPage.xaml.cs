using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for DupesPage.xaml
    /// </summary>
    public partial class DupesPage : UserControl
    {
        int index = 0;
        List<DupesItem> ImagePaths = new List<DupesItem>();
        List<ImageItem> DeletedImages = new List<ImageItem>();
        ImageItem ImageFile;
        ImageItem ImageFile2;
        BitmapImage ImageInstance;
        BitmapImage ImageInstance2;
        int VisibleImage = 1;
        readonly DBActions DB;

        public DupesPage(List<DupesItem> imagepaths, DBActions DB_)
        {
            InitializeComponent();

            DB = DB_;
            ImagePaths = imagepaths;

            if(imagepaths.Count > 0)
            {
                ImageFile = DB.GetImage(DB.GetPath(imagepaths[index].hash1));
                ImageFile2 = DB.GetImage(DB.GetPath(imagepaths[index].hash2));

                UpdateImageDetails(ImageFile);

                try
                {
                    if (File.Exists(ImageFile.path))
                    {
                        ImageInstance = BitmapImageFromFile(ImageFile.path, ImageFile.height);
                        DupesDisplay.Source = ImageInstance;
                    }
                    if (File.Exists(ImageFile2.path))
                    {
                        ImageInstance2 = BitmapImageFromFile(ImageFile2.path, ImageFile2.height);
                        DupesDisplay2.Source = ImageInstance2;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error initializing media fullcreen. - " + ex.Message);
                }
            }
            else
            {
                ImageDetails.Text = "No Duplicates Found";
            }            
        }

        private void UpdateImageDetails(ImageItem II)
        {
            ImageDetails.Text = "Name: " + Path.GetFileName(II.path) + Environment.NewLine;
            ImageDetails.Text += "Extension: " + Path.GetExtension(II.path).ToUpper() + Environment.NewLine;
            ImageDetails.Text += "H x W: " + II.height.ToString() + " x " + II.width.ToString() + Environment.NewLine;
            ImageDetails.Text += "Date Modified: " + II.date_modfied.ToShortDateString() + Environment.NewLine;
            ImageDetails.Text += "Date Added: " + II.date_added.ToShortDateString() + Environment.NewLine;
            if (DB.CheckIfHashHasBeenDeletedBefore(II.path))
            {
                ImageDetails.Text += "* marked for deletion *" + Environment.NewLine;
            }
            ImageDetails.ToolTip = Path.GetFileName(II.path);
            Hyperlink link = new Hyperlink(new Run(II.primary_source)) { Tag = II.primary_source };
            link.Click += new RoutedEventHandler(link_click);
            ImageDetails.Inlines.Add(link);
        }

        private void link_click(object sender, RoutedEventArgs e)
        {
            Hyperlink HL = (Hyperlink)sender;
            System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", HL.Tag.ToString());
        }

        private static BitmapImage BitmapImageFromFile(string path, int height)
        {
            BitmapImage bi = new BitmapImage();

            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    bi.BeginInit();
                    bi.DecodePixelHeight = height;
                    bi.StreamSource = fs;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                }

                bi.Freeze();

                return bi;
            }
            catch (Exception)
            {
                //Error.WriteToLog(ex);
                return BitmapImageFromFileOnError(path);
                //return bi;
            }
        }

        private static BitmapImage BitmapImageFromFileOnError(string path)
        {
            BitmapImage bi = new BitmapImage();

            try
            {
                Stream stream = new MemoryStream();  // Create new MemoryStream  
                Bitmap bitmap = new Bitmap(path);  // Create new Bitmap (System.Drawing.Bitmap) from the existing image file (albumArtSource set to its path name)  
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);  // Save the loaded Bitmap into the MemoryStream - Png format was the only one I tried that didn't cause an error (tried Jpg, Bmp, MemoryBmp)  
                bitmap.Dispose();  // Dispose bitmap so it releases the source image file

                bi.BeginInit();
                bi.StreamSource = stream;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                stream.Close();

                return bi;
            }
            catch (Exception)
            {
                bi.UriSource = null;
                bi = null;
                return bi;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void NextVideoButton_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Next()
        {
            index++;
            if (index > ImagePaths.Count - 1)
            {
                index = 0;
            }
            ChangePage();
        }

        private void Previous()
        {
            index--;
            if (index < 0)
            {
                index = ImagePaths.Count - 1;
            }
            ChangePage();
        }

        private void ChangePage()
        {
            //GC.Collect();
            try
            {
                if(!DB.CheckIfHashHasBeenDeletedBefore(DB.GetPath(ImagePaths[index].hash1)) & !DB.CheckIfHashHasBeenDeletedBefore(DB.GetPath(ImagePaths[index].hash2)))
                {
                    ImageFile = DB.GetImage(DB.GetPath(ImagePaths[index].hash1));
                    ImageFile2 = DB.GetImage(DB.GetPath(ImagePaths[index].hash2));

                    UpdateImageDetails(ImageFile);
                    if (File.Exists(ImageFile.path))
                    {
                        ImageInstance = BitmapImageFromFile(ImageFile.path, ImageFile.height);
                        DupesDisplay.Source = ImageInstance;
                    }
                    if (File.Exists(ImageFile2.path))
                    {
                        ImageInstance2 = BitmapImageFromFile(ImageFile2.path, ImageFile2.height);
                        DupesDisplay2.Source = ImageInstance2;
                    }
                    VisibleImage = 1;
                    ChangeImageVisibility();
                }
                else
                {
                    DB.UpdateDupesProcessed(ImagePaths[index].hash1, ImagePaths[index].hash2);
                    Next();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing media fullcreen. - " + ex.Message);
            }

            TabItem arg = (TabItem)Parent;
            arg.Header = "Dupes (" + (index + 1).ToString() + " / " + ImagePaths.Count.ToString() + ")";
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(ImagePaths.Count > 0)
            {
                if (VisibleImage == 1)
                {
                    if (e.Delta < 0)
                    {
                        VisibleImage = 2;
                        ChangeImageVisibility();
                    }
                }
                else if (VisibleImage == 2)
                {
                    if (e.Delta > 0)
                    {
                        VisibleImage = 1;
                        ChangeImageVisibility();
                    }
                }
            }            
        }

        private void ChangeImageVisibility()
        {
            if (VisibleImage == 1)
            {
                MediaBorder.Visibility = Visibility.Visible;
                DupesDisplay.Visibility = Visibility.Visible;
                MediaBorder2.Visibility = Visibility.Hidden;
                DupesDisplay2.Visibility = Visibility.Hidden;
                UpdateImageDetails(ImageFile);
            }
            else if (VisibleImage == 2)
            {
                MediaBorder.Visibility = Visibility.Hidden;
                DupesDisplay.Visibility = Visibility.Hidden;
                MediaBorder2.Visibility = Visibility.Visible;
                DupesDisplay2.Visibility = Visibility.Visible;
                UpdateImageDetails(ImageFile2);
            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //mark dupes table as processed
            DB.UpdateDupesProcessed(ImageFile.hash, ImageFile2.hash);
            //move to next image set
            Next();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string pathtodelete = "";
            string pathtokeep = "";
            if (VisibleImage == 1)
            {
                pathtodelete = ImageFile2.path;
                pathtokeep = ImageFile.path;
            }
            else if (VisibleImage == 2)
            {
                pathtodelete = ImageFile.path;
                pathtokeep = ImageFile2.path;
            }
            DB.UpdateDupesProcessed(ImageFile.hash, ImageFile2.hash);
            //add other images hash to deleted_images table
            DB.CheckIfHashHasBeenDeletedBefore(pathtodelete);
            DB.AddRowToDeletedImagesTable(pathtodelete);

            //Copy tags and booru link and rating from to be deleted file if it contains no tags or booru link or rating
            if (ImageFile.hash == pathtodelete && !string.IsNullOrEmpty(ImageFile.tag_list))
            {
                if (string.IsNullOrEmpty(ImageFile2.tag_list))
                {
                    DB.UpdateTagList(pathtokeep, ImageFile.tag_list);
                }
                if (string.IsNullOrEmpty(ImageFile2.primary_source))
                {
                    DB.AddPrimarySource(pathtokeep, ImageFile.primary_source);
                }
                if (ImageFile2.rating < 1)
                {
                    DB.UpdateRating(ImageFile2.path, ImageFile.rating);
                }
            }
            else if (ImageFile2.hash == pathtodelete && !string.IsNullOrEmpty(ImageFile2.tag_list))
            {
                if (string.IsNullOrEmpty(ImageFile.tag_list))
                {
                    DB.UpdateTagList(pathtokeep, ImageFile2.tag_list);
                }
                if (string.IsNullOrEmpty(ImageFile.primary_source))
                {
                    DB.AddPrimarySource(pathtokeep, ImageFile2.primary_source);
                }
                if (ImageFile.rating < 1)
                {
                    DB.UpdateRating(ImageFile.path, ImageFile2.rating);
                }
            }
            //move to next image set
            Next();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabItem arg = (TabItem)Parent;
            arg.Header = "Dupes (" + (index + 1).ToString() + " / " + ImagePaths.Count.ToString() + ")";
        }
    }
}
