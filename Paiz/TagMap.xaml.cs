using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdonisUI.Controls;
using BooruSharp.Search.Post;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for TagMap.xaml
    /// </summary>
    public partial class TagMap : AdonisWindow
    {
        SearchResult BooruResults;
        DBActions DB;
        public List<Tuple<string, string, string>> Output = new();

        public TagMap(SearchResult PostResults, DBActions DBinstance)
        {
            InitializeComponent();

            BooruResults = PostResults;
            DB = DBinstance;

            Title = "Tag Map - " + PostResults.ID.ToString();

            UpdateUI();
        }

        public void UpdateUI()
        {

            //Add Hentai & Furry tags to add to each image

            Grid aa = new() { Margin = new Thickness(2) };
            ColumnDefinition pcd1 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition pcd2 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition pcd3 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition pcd4 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition pcd5 = new() { Width = new GridLength(80, GridUnitType.Pixel) };
            aa.ColumnDefinitions.Add(pcd1);
            aa.ColumnDefinitions.Add(pcd2);
            aa.ColumnDefinitions.Add(pcd3);
            aa.ColumnDefinitions.Add(pcd4);
            aa.ColumnDefinitions.Add(pcd5);

            CheckBox CB0 = new();
            Grid.SetColumn(CB0, 0);
            aa.Children.Add(CB0);
            CB0.IsChecked = true;

            TextBlock HentaiDisplay = new() { Text = "Hentai" };
            Grid.SetColumn(HentaiDisplay, 3);
            aa.Children.Add(HentaiDisplay);

            TagPanel.Children.Add(aa);

            Grid bb = new() { Margin = new Thickness(2) };
            ColumnDefinition ucd1 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition ucd2 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition ucd3 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition ucd4 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition ucd5 = new() { Width = new GridLength(80, GridUnitType.Pixel) };
            bb.ColumnDefinitions.Add(ucd1);
            bb.ColumnDefinitions.Add(ucd2);
            bb.ColumnDefinitions.Add(ucd3);
            bb.ColumnDefinitions.Add(ucd4);
            bb.ColumnDefinitions.Add(ucd5);

            CheckBox CB1 = new();
            Grid.SetColumn(CB1, 0);
            bb.Children.Add(CB1);

            TextBlock FurryDisplay = new() { Text = "Furry" };
            Grid.SetColumn(FurryDisplay, 3);
            bb.Children.Add(FurryDisplay);

            TagPanel.Children.Add(bb);

            foreach (string Tag in BooruResults.Tags)
            {
                Grid a = new() { Margin = new Thickness(2) } ;
                ColumnDefinition cd1 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
                ColumnDefinition cd2 = new() { Width = new GridLength(1, GridUnitType.Star) };
                ColumnDefinition cd3 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
                ColumnDefinition cd4 = new() { Width = new GridLength(1, GridUnitType.Star) };
                ColumnDefinition cd5 = new() { Width = new GridLength(80, GridUnitType.Pixel) };
                a.ColumnDefinitions.Add(cd1);
                a.ColumnDefinitions.Add(cd2);
                a.ColumnDefinitions.Add(cd3);
                a.ColumnDefinitions.Add(cd4);
                a.ColumnDefinitions.Add(cd5);

                CheckBox CB = new();
                Grid.SetColumn(CB, 0);
                a.Children.Add(CB);

                TextBlock BooruDisplay = new() { Text = Tag };
                Grid.SetColumn(BooruDisplay, 1);
                a.Children.Add(BooruDisplay);

                Label arrw = new() { Content = "->" };
                Grid.SetColumn(arrw, 2);
                a.Children.Add(arrw);

                TagSuggestionSearchBar TSSB = new(DB) { MultipleTagsMode = false, Tag = Tag, VerticalContentAlignment = VerticalAlignment.Center };
                TSSB.PreviewKeyDown += new KeyEventHandler(TSSB_Enter);
                Grid.SetColumn(TSSB, 3);
                a.Children.Add(TSSB);
                int tagid = DB.GetTagIdfromTagMap(Tag, GetBooruUrl(BooruResults.Source));

                ComboBox CCB = new();
                foreach (string cat in CategoryItem.CategoryList)
                {
                    CCB.Items.Add(cat);
                }
                CCB.SelectedIndex = 14;
                Grid.SetColumn(CCB, 4);
                a.Children.Add(CCB);

                if (tagid != -1)
                {
                    TSSB.Text = DB.GetTagName(tagid);
                    CCB.SelectedValue = CategoryItem.CategoryByID[DB.GetTagCateory(tagid)].Category;
                    CB.IsChecked = true;
                }

                TagPanel.Children.Add(a);
            }

            Grid zz = new() { Margin = new Thickness(2) };
            ColumnDefinition zcd1 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition zcd2 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition zcd3 = new() { Width = new GridLength(16, GridUnitType.Pixel) };
            ColumnDefinition zcd4 = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition zcd5 = new() { Width = new GridLength(80, GridUnitType.Pixel) };
            zz.ColumnDefinitions.Add(zcd1);
            zz.ColumnDefinitions.Add(zcd2);
            zz.ColumnDefinitions.Add(zcd3);
            zz.ColumnDefinitions.Add(zcd4);
            zz.ColumnDefinitions.Add(zcd5);

            Button AddTags = new() { Content = "Add Tags", VerticalContentAlignment = VerticalAlignment.Center };
            AddTags.Click += new RoutedEventHandler(AddTags_Click);
            Grid.SetColumn(AddTags, 3);
            zz.Children.Add(AddTags);

            TagPanel.Children.Add(zz);

            //Add Buttons to Submit All tags
        }

        private void TSSB_Enter(object sender, KeyEventArgs e)
        {
            TagSuggestionSearchBar TBB = (TagSuggestionSearchBar)sender;
            if (e.Key == Key.Return)
            {
                Grid a = (Grid)TBB.Parent;
                ComboBox b = (ComboBox)a.Children[4];
                CheckBox c = (CheckBox)a.Children[0];
                TBB.GetSelected();
                int tagcategory = DB.GetTagCateory(DB.GetTagid(TBB.Text));
                b.SelectedValue = CategoryItem.CategoryByID[tagcategory].Category;
                c.IsChecked = true;

            }
        }

        private void AddTags_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < TagPanel.Children.Count - 1; i++)
            {
                if (TagPanel.Children[i] is Grid j)
                {
                    if (j.Children[0] is CheckBox k)
                    {
                        if ((bool)k.IsChecked)
                        {
                            if (j.Children.Count < 3)
                            {
                                if (j.Children[1] is TextBlock l)
                                {
                                    Output.Add(new Tuple<string, string, string>(l.Text, "Meta", ""));
                                }
                            }
                            else if (j.Children[3] is TagSuggestionSearchBar n)
                            {
                                TextBlock o = (TextBlock)j.Children[1];
                                ComboBox m = (ComboBox)j.Children[4];
                                Output.Add(new Tuple<string, string, string>(n.Text, m.SelectedValue.ToString(), o.Text));
                            }
                        }
                    }
                }
            }
            Close();
        }
        private int GetBooruUrl(string url)
        {
            if (url.Contains("danbooru") || url.Contains("Danbooru"))
            {
                return (int)BooruUrl.Danbooru;
            }
            else if (url.Contains("e621"))
            {
                return (int)BooruUrl.e621;
            }
            else if (url.Contains("rule34.xxx"))
            {
                return (int)BooruUrl.rule34;
            }
            else if (url.Contains("realbooru"))
            {
                return (int)BooruUrl.realbooru;
            }
            else if (url.Contains("Gelbooru") || url.Contains("gelbooru"))
            {
                return (int)BooruUrl.Gelbooru;
            }
            else
            {
                return -1;
            }
        }
    }
}

enum BooruUrl
{
    Danbooru,   //0
    e621,       //1
    rule34,     //2
    Gelbooru,   //3
    realbooru   //4
}
