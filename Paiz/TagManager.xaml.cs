using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;

namespace Paiz
{
    /// <summary>
    /// Interaction logic for TagManager.xaml
    /// </summary>
    public partial class TagManager : UserControl
    {
        DBActions DB;
        List<TagItem> FilteredTagItems = new();
        bool Filtered = false;
        List<TagItem> AllTags = new();
        ImageItem MediaFile;
        List<TagItem> MediaTags = new();
        int PageIndex = 1;          //Index of Page
        readonly int TagsperPage = 50;
        double PageAmount = 0.0;
        bool Alltags = false;
        List<RelationshipItem> ParentRelationshipTags = new();
        List<RelationshipItem> SiblingRelationshipTags = new();
        List<string> ImageSources = new();
        TagSuggestionSearchBar SiblingSearch;
        TagSuggestionSearchBar ParentSearch;
        TagSuggestionSearchBar MainTagEdit;

        public TagManager(DBActions _DB)
        {
            InitializeComponent();

            Alltags = true;
            DB = _DB;
            AllTags = DB.GetAllTags();

            Initailize();
        }

        public TagManager(ImageItem II, DBActions _DB)
        {

            InitializeComponent();

            Alltags = false;
            MediaFile = II;
            DB = _DB;
            MediaTags = DB.GetFileTags(MediaFile.tag_list);            

            Initailize();
        }

        public void Initailize()
        {
            AddTagRelationshipUI();
            if (PageAmount <= 1)
            {
                BottomNavGrid.Visibility = Visibility.Collapsed;
                TagGrid.RowDefinitions[0].Height = new GridLength(0);
                TagGrid.RowDefinitions[3].Height = new GridLength(0);
            }
        }

        private void AddTagRelationshipUI()
        {
            SiblingSearch = new TagSuggestionSearchBar(DB) { Height = 30, VerticalAlignment = VerticalAlignment.Top, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(4), TabIndex = 2 };
            ParentSearch = new TagSuggestionSearchBar(DB) { Height = 30, VerticalAlignment = VerticalAlignment.Top, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(4), TabIndex = 10 };
            MainTagEdit = new TagSuggestionSearchBar(DB) { Height = 30, VerticalAlignment = VerticalAlignment.Top, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(4), TabIndex = 1 };
            SiblingSearch.PreviewKeyDown += new KeyEventHandler(ChildSearch_Enter);
            ParentSearch.PreviewKeyDown += new KeyEventHandler(ParentSearch_Enter);
            MainTagEdit.PreviewKeyDown += new KeyEventHandler(MainTagEdit_Enter);
            Grid.SetColumn(SiblingSearch, 1);
            Grid.SetColumn(ParentSearch, 1);
            Grid.SetColumn(MainTagEdit, 1);
            SiblingAddArea.Children.Add(SiblingSearch);
            ParentAddArea.Children.Add(ParentSearch);
            MainTagEditArea.Children.Add(MainTagEdit);

            CategoryFilter.Items.Add("All");
            foreach (string cat in CategoryItem.CategoryList)
            {
                TagEditCategory.Items.Add(cat);
                CategoryFilter.Items.Add(cat);
            }
            CategoryFilter.SelectedItem = "All";
        }

        private Grid AddTagRelationshipDisplay(RelationshipItem rel)
        {
            Grid RelationshipDisplayGrid = new();
            ColumnDefinition ColDefA = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition ColDefB = new() { Width = new GridLength(30, GridUnitType.Pixel) };
            ColumnDefinition ColDefC = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition ColDefD = new() { Width = new GridLength(30, GridUnitType.Pixel) };
            ColumnDefinition ColDefE = new() { Width = new GridLength(30, GridUnitType.Pixel) };
            RelationshipDisplayGrid.ColumnDefinitions.Add(ColDefA);
            RelationshipDisplayGrid.ColumnDefinitions.Add(ColDefB);
            RelationshipDisplayGrid.ColumnDefinitions.Add(ColDefC);
            RelationshipDisplayGrid.ColumnDefinitions.Add(ColDefD);
            RelationshipDisplayGrid.ColumnDefinitions.Add(ColDefE);

            TextBlock TagRelationshipDisplay = new() { Text = rel.ChildAliasTag, TextAlignment = TextAlignment.Center, Margin = new Thickness(1) };
            TextBlock TransitionDisplay = new() { Text = " -> ", TextAlignment = TextAlignment.Center, FontWeight = FontWeights.UltraBold, Margin = new Thickness(1) };
            TextBlock ParentDisplay = new() { Text = rel.ParentPreferredTag, TextAlignment = TextAlignment.Center, Margin = new Thickness(1) };
            Button SwapButton = new() { Content = " <-> ", Tag = rel, HorizontalContentAlignment = HorizontalAlignment.Center, Margin = new Thickness(1) };
            SwapButton.Click += new RoutedEventHandler(SwapRelationship_Click);
            Button RemoveButton = new() { Content = " X ", Tag = rel, HorizontalContentAlignment = HorizontalAlignment.Center, Margin = new Thickness(1) };
            RemoveButton.Click += new RoutedEventHandler(ParentRemove_Click);

            Grid.SetColumn(TagRelationshipDisplay, 0);
            Grid.SetColumn(TransitionDisplay, 1);
            Grid.SetColumn(ParentDisplay, 2);
            Grid.SetColumn(SwapButton, 3);
            Grid.SetColumn(RemoveButton, 4);

            RelationshipDisplayGrid.Children.Add(TagRelationshipDisplay);
            RelationshipDisplayGrid.Children.Add(TransitionDisplay);
            RelationshipDisplayGrid.Children.Add(ParentDisplay);
            RelationshipDisplayGrid.Children.Add(SwapButton);
            RelationshipDisplayGrid.Children.Add(RemoveButton);

            return RelationshipDisplayGrid;
        }

        private Grid AddSourcesDisplay(string source)
        {
            int SourceID = ImageSources.IndexOf(source);
            Grid SourcesDisplayGrid = new();
            ColumnDefinition ColDefA = new() { Width = new GridLength(30, GridUnitType.Pixel) };
            ColumnDefinition ColDefB = new() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition ColDefC = new() { Width = new GridLength(60, GridUnitType.Pixel) };
            ColumnDefinition ColDefD = new() { Width = new GridLength(60, GridUnitType.Pixel) };
            ColumnDefinition ColDefE = new() { Width = new GridLength(60, GridUnitType.Pixel) };
            SourcesDisplayGrid.ColumnDefinitions.Add(ColDefA);
            SourcesDisplayGrid.ColumnDefinitions.Add(ColDefB);
            SourcesDisplayGrid.ColumnDefinitions.Add(ColDefC);
            SourcesDisplayGrid.ColumnDefinitions.Add(ColDefD);
            SourcesDisplayGrid.ColumnDefinitions.Add(ColDefE);

            Label SourceIDDisplay = new() { Content = (SourceID + 1).ToString(), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(1) };
            TextBox SourceEditBox = new() { Text = source, Margin = new Thickness(1), Visibility = Visibility.Hidden };
            SourceEditBox.PreviewKeyDown += new KeyEventHandler(SourceEditBox_previewkeydown);
            TextBlock SourceDisplay = new() { Tag = source, TextAlignment = TextAlignment.Center, Margin = new Thickness(1) };
            Hyperlink link = new(new Run(source)) { Tag = source };
            link.Click += new RoutedEventHandler(source_click);
            SourceDisplay.Inlines.Add(link);
            CheckBox PrimaryCheckbox = new() { IsChecked = SourceID==0?true:false, Margin = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Center };
            PrimaryCheckbox.Checked += new RoutedEventHandler(PrimaryCheckbox_checked);
            Button EditButton = new() { Content = "Edit", Tag = source, HorizontalContentAlignment = HorizontalAlignment.Center, Margin = new Thickness(1) };
            EditButton.Click += new RoutedEventHandler(Source_edit);
            Button RemoveButton = new() { Content = "Remove", Tag = source, HorizontalContentAlignment = HorizontalAlignment.Center, Margin = new Thickness(1) };
            RemoveButton.Click += new RoutedEventHandler(Source_remove);

            Grid.SetColumn(SourceIDDisplay, 0);
            Grid.SetColumn(SourceDisplay, 1);
            Grid.SetColumn(SourceEditBox, 1);
            Grid.SetColumn(PrimaryCheckbox, 2);
            Grid.SetColumn(EditButton, 3);
            Grid.SetColumn(RemoveButton, 4);

            SourcesDisplayGrid.Children.Add(SourceIDDisplay);
            SourcesDisplayGrid.Children.Add(SourceDisplay);
            SourcesDisplayGrid.Children.Add(SourceEditBox);
            SourcesDisplayGrid.Children.Add(PrimaryCheckbox);
            SourcesDisplayGrid.Children.Add(EditButton);
            SourcesDisplayGrid.Children.Add(RemoveButton);

            return SourcesDisplayGrid;
        }

        private void UpdateUI()
        {
            if (MediaFile != null || AllTags.Count > 0) //If image has at least one tag
            {
                ParentRelationshipTags.Clear();
                SiblingRelationshipTags.Clear();
                ImageSources.Clear();
                TagList.ClearValue(ItemsControl.ItemsSourceProperty); //clear the grid for updating
                if (MediaFile != null)
                {
                    TagList.ItemsSource = MediaTags;

                    foreach (TagItem tg in MediaTags)
                    {
                        foreach (string parenttag in DB.GetParentTags(tg.Name))
                        {
                            if (!string.IsNullOrWhiteSpace(parenttag))
                            {
                                ParentRelationshipTags.Add(new RelationshipItem() { ParentPreferredTag = parenttag, ChildAliasTag = tg.Name });
                            }
                        }
                        SiblingRelationshipTags.AddRange(DB.GetSiblingTagRelationshipsContaining(tg.Name));
                    }
                }
                else
                {
                    //AllTags.Clear();
                    //for (int index = ((PageIndex - 1) * TagsperPage); (index <= (TagsperPage * PageIndex) - 1) && (index <= (Filtered ? FilteredTagIDs : TagNames).Count - 1); index++)
                    //{
                    //    //AllTags.Add(new TagItem() { Name = (Filtered ? FilteredTagIDs : TagNames)[index], Color = "White" });
                    //}

                    int TagListUpperLimit = (Filtered ? FilteredTagItems : AllTags).Count;

                    if (((TagsperPage * PageIndex) - 1) > TagListUpperLimit)
                    {
                        TagList.ItemsSource = (Filtered ? FilteredTagItems : AllTags).GetRange(((PageIndex - 1) * TagsperPage), TagListUpperLimit);
                    }
                    else
                    {
                        TagList.ItemsSource = (Filtered ? FilteredTagItems : AllTags).GetRange(((PageIndex - 1) * TagsperPage), ((TagsperPage * PageIndex) - 1));
                    }

                    for (int index = ((PageIndex - 1) * TagsperPage); (index <= (TagsperPage * PageIndex) - 1) && (index <= (Filtered ? FilteredTagItems : AllTags).Count - 1); index++)
                    {
                        foreach (string parenttag in DB.GetParentTags((Filtered ? FilteredTagItems : AllTags)[index].Name))
                        {
                            if (!string.IsNullOrWhiteSpace(parenttag))
                            {
                                ParentRelationshipTags.Add(new RelationshipItem() { ParentPreferredTag = parenttag, ChildAliasTag = (Filtered ? FilteredTagItems : AllTags)[index].Name });
                            }
                        }
                        SiblingRelationshipTags.AddRange(DB.GetSiblingTagRelationshipsContaining((Filtered ? FilteredTagItems : AllTags)[index].Name));                        
                    }
                    PageNumber.Text = PageIndex.ToString();
                    PageCount.Content = "/" + Math.Ceiling(PageAmount).ToString() + " (" + (Filtered ? FilteredTagItems : AllTags).Count.ToString() + ")";
                }
            }
            UpdateRelationshipDisplay();

            if (MediaFile != null)
            {
                ImageSources = DB.GetSources(MediaFile.path);
            }
            UpdateSourcesDisplay();
        }

        private void UpdateRelationshipDisplay()
        {
            ParentTagList.Children.Clear();
            SiblingTagList.Children.Clear();
            foreach (RelationshipItem RI in ParentRelationshipTags)
            {
                ParentTagList.Children.Add(AddTagRelationshipDisplay(RI));
            }
            foreach (RelationshipItem RI in SiblingRelationshipTags)
            {
                SiblingTagList.Children.Add(AddTagRelationshipDisplay(RI));
            }
        }

        private void UpdateSourcesDisplay()
        {
            SourcesList.Children.Clear();
            foreach(string url in ImageSources)
            {
                SourcesList.Children.Add(AddSourcesDisplay(url));
            }
            SourceURL.Text = "";
            SourcePrimary.IsChecked = false;
        }

        private void NextPageEvent()
        {
            int TempIndex = PageIndex;

            try
            {
                PageIndex++;    //increment page index
                if (PageIndex > PageAmount) //make sure page index didn't go over max number of pages
                {
                    PageIndex = (int)Math.Ceiling(PageAmount);    //if page index went over max number of pages set page index back to max number of pages
                }
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moving to next page. - " + ex.Message);
                //Error.WriteToLog(ex);
                PageIndex = TempIndex;
            }
        }

        private void PrevPageEvent()
        {
            int TempIndex = PageIndex;

            try
            {
                PageIndex--;
                if (PageIndex < 1)
                {
                    PageIndex = 1;
                }
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moving to next page. - " + ex.Message);
                //Error.WriteToLog(ex);
                PageIndex = TempIndex;
            }
        }

        private void FirstPageEvent()
        {
            int TempIndex = PageIndex;
            try
            {
                if (PageIndex != 1)
                {
                    PageIndex = 1;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moving to first page. - " + ex.Message);
                //Error.WriteToLog(ex);
            }
        }

        private void LastPageEvent()
        {
            try
            {
                if (PageIndex != (int)Math.Ceiling(PageAmount))
                {
                    PageIndex = (int)Math.Ceiling(PageAmount);
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moving to first page. - " + ex.Message);
                //Error.WriteToLog(ex);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            TagItem tag = (TagItem)TagList.SelectedItem;
            int tagid = DB.GetTagid(tag.Name);
            List<string> ImagePaths;
            List<RelationshipItem> ParentRelationships = DB.GetParentTagRelationshipsContaining(tag.Name);
            List<RelationshipItem> SiblingRelationships = DB.GetSiblingTagRelationshipsContaining(tag.Name);

            if (Alltags)
            {
                ImagePaths = DB.SearchByExactTag(tag.Name);
                foreach (string path in ImagePaths)
                {
                    DB.RemoveTag(path, tagid);
                }
                DB.DeleteTagData(tagid);
                foreach (RelationshipItem RI in ParentRelationships)
                {
                    DB.DeleteParentRelationship(RI.ChildAliasTag, RI.ParentPreferredTag);
                }
                foreach (RelationshipItem RI in SiblingRelationships)
                {
                    DB.DeleteSiblingRelationship(RI.ChildAliasTag, RI.ParentPreferredTag);
                }
                AllTags.Remove(tag);
            }
            else
            {
                DB.RemoveTag(MediaFile.path, tagid);
                if (DB.GetTagCount(tagid) < 1)      //if the only instance of this tag was just removed
                {
                    DB.DeleteTagData(tagid);        //then remove the tag id row
                    foreach (RelationshipItem RI in ParentRelationships)
                    {
                        DB.DeleteParentRelationship(RI.ChildAliasId, RI.ParentPreferredId);
                    }
                    foreach (RelationshipItem RI in SiblingRelationships)
                    {
                        DB.DeleteSiblingRelationship(RI.ChildAliasId, RI.ParentPreferredId);
                    }
                }
                MediaTags.Remove(tag);
            }
            UpdateUI();
            MainTagEdit.Text = "";
            TagIDLabel.Content = "#";
            TagCountLabel.Content = "0";
            TagEditCategory.SelectedItem = "General";
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            TagItem oldtag = (TagItem)TagList.SelectedItem;
            int oldtagid = DB.GetTagid(oldtag.Name);
            int oldcatid = DB.GetTagCateory(oldtagid);
            string oldcat = CategoryItem.CategoryByID[oldcatid].Category;
            string newtag = MainTagEdit.Text;
            int newtagid = DB.GetTagid(newtag);
            int newtagcatergory = DB.GetTagCateory(newtagid);
            List<string> ImagePaths;
            bool edited = false;
            List<RelationshipItem> SiblingRelationships = DB.GetSiblingTagRelationshipsContaining(oldtag.Name);

            if (newtag == oldtag.Name)   //If the tags are the same update the categories if its new
            {
                if (oldcat != TagEditCategory.SelectedItem.ToString())
                {
                    DB.UpdateTagCategory(CategoryItem.CategoryByName[TagEditCategory.SelectedItem.ToString()].CategoryID, oldtagid);
                    edited = true;
                }
            }
            else //if the tags are different
            {
                if (newtagid == -1)  //if new tag doesn't exist, were just changing text of tag (ex fixing a spelling error)
                {
                    //DB.InsertIntoTag_Data(newtag, DB.GetCategory_id(d.SelectedItem.ToString()));
                    if (SiblingRelationships.Count > 0)
                    {
                        foreach (RelationshipItem sib in SiblingRelationships)
                        {
                            if (oldtag.Name == sib.ChildAliasTag) //If Tag Being updated is a old sibling, tag_display needs to be updated as well
                            {
                                DB.UpdateTag(newtag, newtag + "Displays as: " + sib.ParentPreferredTag, oldtagid);
                            }
                        }
                    }
                    DB.UpdateTag(newtag, oldtagid);
                    newtagid = oldtagid;
                }
                else if (newtagid == oldtagid)
                {
                    //
                }
                else //if new tag already exists
                {
                    List<RelationshipItem> ParentRelationships = DB.GetParentTagRelationshipsContaining(oldtag.Name);
                    ImagePaths = DB.SearchByExactTag(oldtag.Name);
                    foreach (RelationshipItem RI in ParentRelationships)
                    {
                        if (RI.ChildAliasId == oldtagid) //if the old tag is the child in the parent relationship
                        {
                            DB.UpdateChildTag(RI.ChildAliasId, newtagid, RI.ParentPreferredId);
                        }
                        else if (RI.ParentPreferredId == oldtagid) //if the old tag is the parent in the parent relationship
                        {
                            DB.UpdateParentTag(RI.ChildAliasId, RI.ParentPreferredId, newtagid);
                        }
                    }
                    foreach (RelationshipItem RI in SiblingRelationships)
                    {
                        if (RI.ChildAliasId == oldtagid) //if old tag is the one to be updated
                        {
                            DB.UpdateAliasTag(RI.ChildAliasId, newtagid, RI.ParentPreferredId);
                            DB.UpdateTagDisplay(newtag + " Displays as: " + RI.ParentPreferredTag, newtagid);
                        }
                        else if (RI.ParentPreferredId == oldtagid) // if new tag is the one to be updates
                        {
                            DB.UpdatePreferredTag(RI.ChildAliasId, RI.ParentPreferredId, newtagid);
                        }
                    }
                    foreach (string path in ImagePaths) //update all tags with new tag
                    {
                        DB.EditTag(path, oldtagid, newtagid);
                    }

                    DB.DeleteTagData(oldtagid);
                }

                edited = true;
            }
            if (edited)
            {
                MainTagEdit.Background = Brushes.CornflowerBlue;
            }
        }

        private void ParentRemove_Click(object sender, RoutedEventArgs e)
        {
            Button a = (Button)sender;
            RelationshipItem RI = (RelationshipItem)a.Tag;
            Grid b = (Grid)a.Parent;

            if (RelationshipTabControl.SelectedIndex == 0)
            {
                DB.DeleteParentRelationship(RI.ChildAliasTag, RI.ParentPreferredTag);
                ParentRelationshipTags.Remove(RI);
                ParentTagList.Children.Remove(b);
            }
            else
            {
                DB.DeleteSiblingRelationship(RI.ChildAliasTag, RI.ParentPreferredTag);
                SiblingRelationshipTags.Remove(RI);
                SiblingTagList.Children.Remove(b);
            }
        }

        private void SwapRelationship_Click(object sender, RoutedEventArgs e)
        {
            Button a = (Button)sender;
            RelationshipItem RI = (RelationshipItem)a.Tag;
            Grid b = (Grid)a.Parent;

            SwapRelationship(b, RI);
        }

        private void SwapRelationship(Grid uiele, RelationshipItem RelationtoSwap)
        {
            if (RelationshipTabControl.SelectedIndex == 0)
            {
                //DB.DeleteParentRelationship(RI.ChildAliasTag, RI.ParentPreferredTag);
                //ParentRelationshipTags.Remove(RI);
                //ParentTagList.Children.Remove(b);
            }
            else
            {
                string newpreferred = RelationtoSwap.ChildAliasTag;
                string newalias = RelationtoSwap.ParentPreferredTag;
                int newpreferredid = RelationtoSwap.ChildAliasId;
                int newaliasid = RelationtoSwap.ParentPreferredId;

                DB.SwapSiblingRelationship(RelationtoSwap.ChildAliasTag, RelationtoSwap.ParentPreferredTag);
                SiblingRelationshipTags.Remove(RelationtoSwap);
                RelationshipItem NewRI = new RelationshipItem() { ChildAliasId = newaliasid, ChildAliasTag = newalias, ParentPreferredId = newpreferredid, ParentPreferredTag = newpreferred };
                SiblingRelationshipTags.Add(NewRI);
                SiblingTagList.Children.Remove(uiele);
                SiblingTagList.Children.Add(AddTagRelationshipDisplay(NewRI));

                List<RelationshipItem> SiblingedParentTags = DB.GetParentTagRelationshipsContaining(NewRI.ChildAliasTag);
                //Get parents of old sibling tag that was updated to new sibling tag
                foreach (RelationshipItem RI2 in SiblingedParentTags)
                {
                    if (RI2.ChildAliasId == NewRI.ChildAliasId)
                    {
                        //if old sibling tag is equal to child of parent relationship
                        //Update Child tag to be New Sibling
                        Logger.Write("Updating Child Tag from " + DB.GetTagName(RI2.ChildAliasId) + " to " + DB.GetTagName(NewRI.ParentPreferredId));
                        DB.UpdateChildTag(RI2.ChildAliasId, NewRI.ParentPreferredId, RI2.ParentPreferredId);
                    }
                    else if (RI2.ParentPreferredId == NewRI.ChildAliasId)
                    {
                        //if old sibling tag is equal to parent of parent relationship
                        //Update Parent tag to be New Sibling
                        Logger.Write("Updating Parent Tag from " + DB.GetTagName(RI2.ParentPreferredId) + " to " + DB.GetTagName(NewRI.ParentPreferredId));
                        DB.UpdateParentTag(RI2.ChildAliasId, RI2.ParentPreferredId, NewRI.ParentPreferredId);
                    }
                }
                //if new preferred tag is an alias is another sibling relationship
                //swap the other sibling relationship

                //List<RelationshipItem> AliasSiblingsofSwappedRelationship = DB.GetSiblingTagRelationshipsContaining(NewRI.ParentPreferredTag);
                //foreach (RelationshipItem RI3 in AliasSiblingsofSwappedRelationship)
                //{
                //    if (RI3.ChildAliasTag == NewRI.ParentPreferredTag)
                //    {
                //        string newpreferred2 = RI.ChildAliasTag;
                //        string newalias2 = RI.ParentPreferredTag;
                //        int newpreferredid2 = RI.ChildAliasId;
                //        int newaliasid2 = RI.ParentPreferredId;

                //        DB.SwapSiblingRelationship(RI3.ChildAliasTag, RI3.ParentPreferredTag);
                //        SiblingRelationshipTags.Remove(RI3);
                //        RelationshipItem NewRI2 = new RelationshipItem() { ChildAliasId = newaliasid2, ChildAliasTag = newalias2, ParentPreferredId = newpreferredid2, ParentPreferredTag = newpreferred2 };
                //        SiblingRelationshipTags.Add(NewRI2);
                //        //SiblingTagList.Children.Remove(b);
                //        //SiblingTagList.Children.Add(AddTagRelationshipDisplay(NewRI));
                //    }
                //}
            }
        }

        private void NextPage_Clicked(object sender, RoutedEventArgs e)
        {
            NextPageEvent();
        }

        private void PrevPage_Clicked(object sender, RoutedEventArgs e)
        {
            PrevPageEvent();
        }

        private void FirstPage_Clicked(object sender, RoutedEventArgs e)
        {
            FirstPageEvent();
        }

        private void LastPage_Clicked(object sender, RoutedEventArgs e)
        {
            LastPageEvent();
        }

        private void PageNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int TempIndex = PageIndex;

                try
                {
                    PageIndex = Convert.ToInt32(PageNumber.Text);
                    if (PageIndex > PageAmount) //make sure page index didn't go over max number of pages
                    {
                        PageIndex = TempIndex;    //if page index went over max number of pages set page index back to max number of pages
                    }
                    else
                    {
                        UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write("Error moving to requested page: " + PageNumber.Text + " - " + ex.Message);
                    PageIndex = TempIndex;
                }
            }
        }

        private void ParentSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ParentSearch.GetSelected();
                AddParent_Click(sender, e);
            }
        }

        private void ChildSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SiblingSearch.GetSelected();
                AddSibling_Click(sender, e);
            }
        }

        private void MainTagEdit_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                MainTagEdit.GetSelected();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            TagListSearch();
        }

        public void TagListSearch()
        {
            if (string.IsNullOrWhiteSpace(TagSearch.Text))
            {
                Filtered = false;
                //CategoryFilter.SelectedItem = "All";
                PageIndex = 1;
                PageAmount = (double)AllTags.Count / TagsperPage;
                UpdateUI();
            }
            else
            {
                Filtered = true;
                //CategoryFilter.SelectedItem = "All";
                FilteredTagItems = DB.TagItemSearch(TagSearch.Text);
                PageIndex = 1;
                PageAmount = (double)FilteredTagItems.Count / TagsperPage;
                UpdateUI();
            }
        }

        private void TagSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TagListSearch();
            }
        }

        private void AddParent_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ParentSearch.Text) && !string.IsNullOrWhiteSpace(MainTagEdit.Text))
            {
                string preferred = DB.GetPreferredTag(ParentSearch.Text);

                DB.AddParentTag(MainTagEdit.Text, preferred == "" ? ParentSearch.Text : preferred);
                ParentRelationshipTags.Add(new RelationshipItem() { ChildAliasTag = MainTagEdit.Text, ParentPreferredTag = preferred == "" ? ParentSearch.Text : preferred });
                UpdateRelationshipDisplay();
            }
            if (RelationshipTabControl.SelectedIndex != 0)
            {
                RelationshipTabControl.SelectedIndex = 0;
            }
            ParentSearch.Text = "";
            ParentSearch.Focus();
        }

        private void AddSibling_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SiblingSearch.Text) && !string.IsNullOrWhiteSpace(MainTagEdit.Text))
            {
                string prefferedcheck = DB.GetPreferredTag(MainTagEdit.Text);
                if (prefferedcheck == "")
                {
                    DB.AddSiblingTag(MainTagEdit.Text, SiblingSearch.Text);
                    SiblingRelationshipTags.Add(new RelationshipItem() { ChildAliasTag = MainTagEdit.Text, ParentPreferredTag = SiblingSearch.Text });
                    UpdateRelationshipDisplay();
                    if (RelationshipTabControl.SelectedIndex != 1)
                    {
                        RelationshipTabControl.SelectedIndex = 1;
                    }
                }
                else
                {
                    MessageBox.Show(MainTagEdit.Text + " is already a alias tag to preferred tag: " + prefferedcheck, "Error Adding Sibling Relationship");
                }
            }
            
            SiblingSearch.Text = "";
            SiblingSearch.Focus();
        }

        private void TagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagList.SelectedItem != null)
            {
                TagItem Tag = (TagItem)TagList.SelectedItem;
                int tagid = DB.GetTagid(Tag.Name);
                int catid = DB.GetTagCateory(tagid);
                //TagEditCategory.Background = Brushes.White;
                TagEditCategory.SelectedItem = CategoryItem.CategoryByID[catid].Category;
                MainTagEdit.IgnoreSuggestions = true;
                MainTagEdit.Text = Tag.Name;
                MainTagEdit.IgnoreSuggestions = false;
                TagIDLabel.Content = tagid;
                TagCountLabel.Content = DB.GetTagCount(tagid);
            }
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TagSearch.Text = "";
            if (CategoryFilter.SelectedItem.ToString() == "All")
            {
                Filtered = false;
                PageIndex = 1;
                PageAmount = (double)AllTags.Count / TagsperPage;
                UpdateUI();
            }
            else
            {
                Filtered = true;
                FilteredTagItems = DB.GetAllTagIdsInCategory(CategoryFilter.SelectedItem.ToString());
                PageIndex = 1;
                PageAmount = (double)FilteredTagItems.Count / TagsperPage;
                UpdateUI();
            }
        }

        private void SourceAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                AddSource_Click(sender, e);
            }
        }

        private void AddSource_Click(object sender, RoutedEventArgs e)
        {
            if (SourceURL.Text.ToLower().Contains("http"))
            {
                if(SourcePrimary.IsChecked == true)
                {
                    DB.AddPrimarySource(MediaFile.path, SourceURL.Text);
                    DB.UpdateBooruTagged(MediaFile.path, false);
                    ImageSources.Insert(0, SourceURL.Text);
                }
                else
                {
                    if (DB.GetPrimarySource(MediaFile.path) == "")
                    {
                        DB.AddPrimarySource(MediaFile.path, SourceURL.Text);
                        DB.UpdateBooruTagged(MediaFile.path, false);
                    }
                    else
                    {
                        DB.AddNonPrimarySource(MediaFile.path, SourceURL.Text);
                        DB.UpdateBooruTagged(MediaFile.path, false);
                    }
                    ImageSources.Add(SourceURL.Text);
                }
                UpdateSourcesDisplay();
            }
        }

        private void source_click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink Link = sender as Hyperlink;
                Logger.Write("Opening link " + Link.Tag.ToString() + " for image " + MediaFile.path);
                System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", Link.Tag.ToString());
            }
            catch (Exception ex)
            {
                Logger.Write("Error Opening source link for image " + MediaFile.path + " - " + ex.Message);
            }
        }

        private void Source_edit(object sender, RoutedEventArgs e)
        {
            if(sender is Button a)
            {
                if(a.Parent is Grid b)
                {
                    if(b.Children[2] is TextBox c)
                    {
                        if(c.Visibility == Visibility.Visible)
                        {
                            if(b.Children[1] is TextBlock d)
                            {
                                ImageSources.Remove(d.Tag.ToString());
                                ImageSources.Insert(0, c.Text);
                                DB.UpdateSpecificSource(MediaFile.path, d.Tag.ToString(), c.Text);
                                //c.Visibility = Visibility.Hidden;
                                UpdateSourcesDisplay();
                            }
                        }
                        else
                        {
                            c.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            else if (sender is TextBox f)
            {
                if(f.Parent is Grid g)
                {
                    if (g.Children[1] is TextBlock h)
                    {
                        ImageSources.Remove(h.Tag.ToString());
                        ImageSources.Insert(0, f.Text);
                        DB.UpdateSpecificSource(MediaFile.path, h.Tag.ToString(), f.Text);
                        //f.Visibility = Visibility.Hidden;
                        UpdateSourcesDisplay();
                    }
                }
            }
        }

        private void Source_remove(object sender, RoutedEventArgs e)
        {
            if (ImageSources.Count > 1)
            {
                if(sender is Button a)
                {
                    if (a.Parent is Grid b)
                    {
                        if (b.Children[3] is CheckBox c)
                        {
                            TextBox d = (TextBox)b.Children[2];
                            string url = d.Text;
                            ImageSources.Remove(url);
                            DB.RemoveSource(MediaFile.hash, url);
                        }
                    }
                }
            }
            else
            {
                ImageSources.Remove(MediaFile.primary_source);
                DB.RemoveSource(MediaFile.hash, MediaFile.primary_source);
            }
            UpdateSourcesDisplay();
        }

        private void PrimaryCheckbox_checked(object sender, RoutedEventArgs e)
        {
            CheckBox NewPrimaryCheckbox = (CheckBox)sender; //Clicked Checkbox
            TextBox OldPrimaryUrl = new();
            TextBox NewPrimaryUrl = new();
            bool NoOldPrimary = true;
            foreach(UIElement a in SourcesList.Children)
            {
                if(a is Grid b)
                {
                    if(b.Children[3] is CheckBox OldPrimaryCheckbox)
                    {
                        if(OldPrimaryCheckbox != NewPrimaryCheckbox & (bool)OldPrimaryCheckbox.IsChecked)
                        {
                            NoOldPrimary = false;
                            OldPrimaryUrl = (TextBox)b.Children[2];
                        }
                        else if(OldPrimaryCheckbox == NewPrimaryCheckbox)
                        {
                            NewPrimaryUrl = (TextBox)b.Children[2];
                        }
                    }
                }
            }
            if (NoOldPrimary)
            {
                DB.AddPrimarySource(MediaFile.hash, NewPrimaryUrl.Text);
                DB.UpdateBooruTagged(MediaFile.path, false);
            }
            else
            {
                DB.ChangePrimarySource(MediaFile.path, NewPrimaryUrl.Text);
            }
            ImageSources.Remove(NewPrimaryUrl.Text);
            ImageSources.Insert(0, NewPrimaryUrl.Text);
            UpdateSourcesDisplay();
        }

        private void SourceEditBox_previewkeydown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                Source_edit(sender, e);
            }
        }
    }
}
