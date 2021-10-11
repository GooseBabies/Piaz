using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for TagSuggestionSearchBar.xaml
    /// </summary>
    public partial class TagSuggestionSearchBar : UserControl
    {
        public string Text
        {
            get { return SearchBar.Text; }
            set { SearchBar.Text = value; }
        }

        public int Selected_Index
        {
            get { return SelectedIndex; }
        }

        public bool IgnoreSuggestions { get; set; }
        public bool MultipleTagsMode { get; set; }

        Color HardGray = Color.FromRgb(37, 37, 37);
        Color HighlightGray = Color.FromRgb(69, 69, 69);

        readonly Popup codePopup;
        List<TagItem> Filtered_Tags = new();
        private int SelectedIndex = -1;
        Border SuggestionBorder;
        StackPanel SuggestionPanel;
        readonly DBActions DB;
        private int TextStartLocation = 0;
        private string TextTagSubstring = "";
        private readonly List<TagItem> SpecialSearchTags = new();

        private readonly string[] SpecialSearchItems = { "$rating", "$name", "$extension", "$date_added", "$date_modified", "$path", "$height", "$width", "$source", "$video", "$sound", "$duration", "$!tagged", "$resolution" };

        public TagSuggestionSearchBar(DBActions _DB)
        {
            InitializeComponent();

            DB = _DB;
            codePopup = new Popup()
            {
                PlacementTarget = SearchBar,
                Placement = PlacementMode.Bottom,
                StaysOpen = false
            };
            MultipleTagsMode = false;

            foreach (string st in SpecialSearchItems)
            {
                SpecialSearchTags.Add(new TagItem { Display = st, Name = st });
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBar.Text == "")
            {
                TextStartLocation = 0;
                codePopup.IsOpen = false;
            }
            //try
            //{
            //    if (!IgnoreSuggestions && !MultipleTagsMode)
            //    {
            //        string TypedInput = SearchBar.Text;
            //        SelectedIndex = -1;

            //        if (TypedInput != "" && TypedInput != " ")
            //        {
            //            Filtered_Tags = DB.TagSuggestions(SearchBar.Text, 8);
            //            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray) };
            //            SuggestionPanel = new StackPanel();

            //            foreach (TagItem sugg in Filtered_Tags)
            //            {
            //                TextBlock tb = new TextBlock
            //                {
            //                    Text = sugg.Display,
            //                    Background = new SolidColorBrush(HardGray),
            //                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(sugg.Color),
            //                    Padding = new Thickness(4, 2, 4, 2)
            //                };
            //                SuggestionPanel.Children.Add(tb);
            //            }
            //            SuggestionBorder.Child = SuggestionPanel;
            //            codePopup.Child = SuggestionBorder;

            //            if (Filtered_Tags.Count == 1 && Filtered_Tags[0].Name == TypedInput)
            //            {
            //                //codePopup.IsOpen = false;
            //            }
            //            else if (Filtered_Tags.Count > 0)
            //            {
            //                codePopup.IsOpen = true;
            //            }
            //            else
            //            {
            //                codePopup.IsOpen = false;
            //            }
            //        }
            //        else
            //        {
            //            codePopup.IsOpen = false;
            //        }
            //    }
            //    else if (MultipleTagsMode)
            //    {
            //        if (SearchBar.Text == "")
            //        {
            //            TextStartLocation = 0;
            //            codePopup.IsOpen = false;
            //        }
            //        if (SearchBar.Text.Length < TextStartLocation)
            //        {
            //            TextStartLocation = SearchBar.Text.Length;
            //        }
            //        TextTagSubstring = SearchBar.Text[TextStartLocation..]; //Start at TextStartLocation
            //        SelectedIndex = -1;
            //        if (TextTagSubstring.StartsWith(" ") || TextTagSubstring.StartsWith("&") || TextTagSubstring.StartsWith("|") || TextTagSubstring.StartsWith("!") || TextTagSubstring.StartsWith("~") || TextTagSubstring.StartsWith("(") || TextTagSubstring.StartsWith(")"))
            //        {
            //            TextStartLocation += 1;
            //            codePopup.IsOpen = false;
            //        }
            //        else if (TextTagSubstring == "")
            //        {
            //            codePopup.IsOpen = false;
            //        }
            //        else if (TextTagSubstring.StartsWith("$"))
            //        {
            //            Filtered_Tags = SpecialSearchTags.Where(c => c.Name.StartsWith(TextTagSubstring)).ToList();
            //            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray), BorderThickness = new Thickness(2) };
            //            SuggestionPanel = new StackPanel();

            //            foreach (TagItem sugg in Filtered_Tags)
            //            {
            //                TextBlock tb = new()
            //                {
            //                    Text = sugg.Name,
            //                    //Background = Brushes.WhiteSmoke,
            //                    //Foreground = Brushes.Black,
            //                    Padding = new Thickness(4, 2, 4, 2)
            //                };
            //                SuggestionPanel.Children.Add(tb);
            //            }
            //            SuggestionBorder.Child = SuggestionPanel;
            //            codePopup.Child = SuggestionBorder;

            //            if (Filtered_Tags.Count > 1)
            //            {
            //                codePopup.IsOpen = true;
            //            }
            //            else if (Filtered_Tags.Count == 1)
            //            {
            //                if (Filtered_Tags[0].Name == TextTagSubstring)
            //                {
            //                    TextStartLocation = SearchBar.Text.Length;
            //                    codePopup.IsOpen = false;
            //                }
            //            }
            //            else
            //            {
            //                TextStartLocation = SearchBar.Text.Length;
            //                codePopup.IsOpen = false;
            //            }
            //        }
            //        else
            //        {
            //            Filtered_Tags = DB.TagSuggestions(TextTagSubstring, 8);
            //            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray) };
            //            SuggestionPanel = new StackPanel();

            //            foreach (TagItem sugg in Filtered_Tags)
            //            {
            //                TextBlock tb = new TextBlock
            //                {
            //                    Text = sugg.Display,
            //                    Background = new SolidColorBrush(HardGray),
            //                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(CategoryList.Where(z => z.Category == sugg.Category).First().Color),
            //                    Padding = new Thickness(4, 2, 4, 2)
            //                };
            //                SuggestionPanel.Children.Add(tb);
            //            }
            //            SuggestionBorder.Child = SuggestionPanel;
            //            codePopup.Child = SuggestionBorder;

            //            if (Filtered_Tags.Count > 0)
            //            {
            //                codePopup.IsOpen = true;
            //            }
            //            else
            //            {
            //                //TextStartLocation = SearchBar.Text.Length;
            //                codePopup.IsOpen = false;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Write("Search Bar Error: " + ex.Message);
            //}
        }

        //private void SearchBar_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    //if (e.Key == Key.Down)
        //    //{
        //    //    if (Filtered_Tags.Count > 0)
        //    //    {
        //    //        TextBlock temptb;// = new TextBlock();

        //    //        SelectedIndex++;
        //    //        if (SelectedIndex <= Filtered_Tags.Count - 1)
        //    //        {
        //    //            temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex];
        //    //            temptb.Background = new SolidColorBrush(HighlightGray);
        //    //        }
        //    //        else if (SelectedIndex >= Filtered_Tags.Count)
        //    //        {
        //    //            SelectedIndex = Filtered_Tags.Count - 1;
        //    //        }

        //    //        if (SelectedIndex > 0)
        //    //        {
        //    //            temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex - 1];
        //    //            temptb.Background = new SolidColorBrush(HardGray);
        //    //        }
        //    //    }
        //    //}
        //    //else if (e.Key == Key.Up)
        //    //{
        //    //    if (Filtered_Tags.Count > 0)
        //    //    {
        //    //        TextBlock temptb;// = new TextBlock();

        //    //        SelectedIndex--;
        //    //        if (SelectedIndex >= 0)
        //    //        {
        //    //            temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex];
        //    //            temptb.Background = new SolidColorBrush(HighlightGray);
        //    //        }
        //    //        else if (SelectedIndex < 0)
        //    //        {
        //    //            SelectedIndex = -1;
        //    //        }

        //    //        if (SelectedIndex < Filtered_Tags.Count - 1)
        //    //        {
        //    //            temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex + 1];
        //    //            temptb.Background = new SolidColorBrush(HardGray);
        //    //        }
        //    //    }
        //    //}
        //    //else if (e.Key == Key.Tab)
        //    //{
        //    //    if (SelectedIndex >= 0)
        //    //    {
        //    //        if (MultipleTagsMode)
        //    //        {
        //    //            SearchBar.Text = SearchBar.Text.Replace(TextTagSubstring, Filtered_Tags[SelectedIndex].Name);
        //    //            SelectedIndex = -1;
        //    //            SearchBar.CaretIndex = SearchBar.Text.Length;
        //    //            SearchBar.Focus();
        //    //        }
        //    //        else
        //    //        {
        //    //            SearchBar.Text = Filtered_Tags[SelectedIndex].Name;
        //    //            SelectedIndex = -1;
        //    //            SearchBar.CaretIndex = SearchBar.Text.Length;
        //    //            SearchBar.Focus();
        //    //        }
        //    //    }
        //    //    codePopup.IsOpen = false;
        //    //}
        //    //else if (e.Key == Key.Escape)
        //    //{
        //    //    if (codePopup.IsOpen)
        //    //    {
        //    //        codePopup.IsOpen = false;
        //    //    }
        //    //}
        //}

        public void GetSelected()
        {
            if (SelectedIndex >= 0)
            {
                if (MultipleTagsMode)
                {
                    SearchBar.Text = SearchBar.Text.Replace(TextTagSubstring, Filtered_Tags[SelectedIndex].Name);
                    SelectedIndex = -1;
                    SearchBar.CaretIndex = SearchBar.Text.Length;
                    SearchBar.Focus();
                }
                else
                {
                    SearchBar.Text = Filtered_Tags[SelectedIndex].Name;
                    SelectedIndex = -1;
                    SearchBar.CaretIndex = SearchBar.Text.Length;
                    SearchBar.Focus();
                }
            }
            codePopup.IsOpen = false;
        }

        public void ClosePopup()
        {
            if (codePopup.IsOpen)
            {
                codePopup.IsOpen = false;
            }
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchBar.Select(0, 0);
        }

        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (Filtered_Tags.Count > 0)
                {
                    TextBlock temptb;// = new TextBlock();

                    SelectedIndex++;
                    if (SelectedIndex <= Filtered_Tags.Count - 1)
                    {
                        temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex];
                        temptb.Background = new SolidColorBrush(HighlightGray);
                    }
                    else if (SelectedIndex >= Filtered_Tags.Count)
                    {
                        SelectedIndex = Filtered_Tags.Count - 1;
                    }

                    if (SelectedIndex > 0)
                    {
                        temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex - 1];
                        temptb.Background = new SolidColorBrush(HardGray);
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                if (Filtered_Tags.Count > 0)
                {
                    TextBlock temptb;// = new TextBlock();

                    SelectedIndex--;
                    if (SelectedIndex >= 0)
                    {
                        temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex];
                        temptb.Background = new SolidColorBrush(HighlightGray);
                    }
                    else if (SelectedIndex < 0)
                    {
                        SelectedIndex = -1;
                    }

                    if (SelectedIndex < Filtered_Tags.Count - 1)
                    {
                        temptb = (TextBlock)SuggestionPanel.Children[SelectedIndex + 1];
                        temptb.Background = new SolidColorBrush(HardGray);
                    }
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (SelectedIndex >= 0)
                {
                    if (MultipleTagsMode)
                    {
                        SearchBar.Text = SearchBar.Text.Replace(TextTagSubstring, Filtered_Tags[SelectedIndex].Name);
                        SelectedIndex = -1;
                        SearchBar.CaretIndex = SearchBar.Text.Length;
                        SearchBar.Focus();
                    }
                    else
                    {
                        SearchBar.Text = Filtered_Tags[SelectedIndex].Name;
                        SelectedIndex = -1;
                        SearchBar.CaretIndex = SearchBar.Text.Length;
                        SearchBar.Focus();
                    }
                }
                codePopup.IsOpen = false;
            }
            else if (e.Key == Key.Escape)
            {
                if (codePopup.IsOpen)
                {
                    codePopup.IsOpen = false;
                }
            }
            else if(e.Key == Key.Return)
            {
                TextStartLocation = SearchBar.Text.Length;
            }
            else
            {
                try
                {
                    if (!IgnoreSuggestions && !MultipleTagsMode)
                    {
                        string TypedInput = SearchBar.Text;
                        SelectedIndex = -1;

                        if (TypedInput != "" && TypedInput != " ")
                        {
                            Filtered_Tags = DB.TagSuggestions(SearchBar.Text, 8);
                            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray) };
                            SuggestionPanel = new StackPanel();

                            foreach (TagItem sugg in Filtered_Tags)
                            {
                                TextBlock tb = new()
                                {
                                    Text = sugg.Display,
                                    Background = new SolidColorBrush(HardGray),
                                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(sugg.Color),
                                    Padding = new Thickness(4, 2, 4, 2)
                                };
                                SuggestionPanel.Children.Add(tb);
                            }
                            SuggestionBorder.Child = SuggestionPanel;
                            codePopup.Child = SuggestionBorder;

                            if (Filtered_Tags.Count == 1 && Filtered_Tags[0].Name == TypedInput)
                            {
                                //codePopup.IsOpen = false;
                            }
                            else if (Filtered_Tags.Count > 0)
                            {
                                codePopup.IsOpen = true;
                            }
                            else
                            {
                                codePopup.IsOpen = false;
                            }
                        }
                        else
                        {
                            codePopup.IsOpen = false;
                        }
                    }
                    else if (MultipleTagsMode)
                    {
                        //if (SearchBar.Text == "")
                        //{
                        //    TextStartLocation = 0;
                        //    codePopup.IsOpen = false;
                        //}
                        if (SearchBar.Text.Length < TextStartLocation)
                        {
                            TextStartLocation = SearchBar.Text.Length;
                        }
                        TextTagSubstring = SearchBar.Text[TextStartLocation..]; //Start at TextStartLocation
                        SelectedIndex = -1;
                        if (TextTagSubstring.StartsWith(" ") || TextTagSubstring.StartsWith("&") || TextTagSubstring.StartsWith("|") || TextTagSubstring.StartsWith("!") || TextTagSubstring.StartsWith("~") || TextTagSubstring.StartsWith("(") || TextTagSubstring.StartsWith(")"))
                        {
                            TextStartLocation += 1;
                            codePopup.IsOpen = false;
                        }
                        else if (TextTagSubstring == "")
                        {
                            codePopup.IsOpen = false;
                        }
                        else if (TextTagSubstring.StartsWith("$"))
                        {
                            Filtered_Tags = SpecialSearchTags.Where(c => c.Name.StartsWith(TextTagSubstring)).ToList();
                            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray), BorderThickness = new Thickness(2) };
                            SuggestionPanel = new StackPanel();

                            foreach (TagItem sugg in Filtered_Tags)
                            {
                                TextBlock tb = new()
                                {
                                    Text = sugg.Name,
                                    //Background = Brushes.WhiteSmoke,
                                    //Foreground = Brushes.Black,
                                    Padding = new Thickness(4, 2, 4, 2)
                                };
                                SuggestionPanel.Children.Add(tb);
                            }
                            SuggestionBorder.Child = SuggestionPanel;
                            codePopup.Child = SuggestionBorder;

                            if (Filtered_Tags.Count > 1)
                            {
                                codePopup.IsOpen = true;
                            }
                            else if (Filtered_Tags.Count == 1)
                            {
                                if (Filtered_Tags[0].Name == TextTagSubstring)
                                {
                                    TextStartLocation = SearchBar.Text.Length;
                                    codePopup.IsOpen = false;
                                }
                            }
                            else
                            {
                                //TextStartLocation = SearchBar.Text.Length;
                                codePopup.IsOpen = false;
                            }
                        }
                        else
                        {
                            Filtered_Tags = DB.TagSuggestions(TextTagSubstring, 8);
                            SuggestionBorder = new Border() { BorderBrush = new SolidColorBrush(HardGray) };
                            SuggestionPanel = new StackPanel();

                            foreach (TagItem sugg in Filtered_Tags)
                            {
                                TextBlock tb = new TextBlock
                                {
                                    Text = sugg.Display,
                                    Background = new SolidColorBrush(HardGray),
                                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(CategoryItem.CategoryByName[sugg.Category].Color),
                                    Padding = new Thickness(4, 2, 4, 2)
                                };
                                SuggestionPanel.Children.Add(tb);
                            }
                            SuggestionBorder.Child = SuggestionPanel;
                            codePopup.Child = SuggestionBorder;

                            if (Filtered_Tags.Count > 0)
                            {
                                codePopup.IsOpen = true;
                            }
                            else
                            {
                                //TextStartLocation = SearchBar.Text.Length;
                                codePopup.IsOpen = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write("Search Bar Error: " + ex.Message);
                }
            }
        }

        private void SearchBar_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            codePopup.IsOpen = false;
        }
    }
}
