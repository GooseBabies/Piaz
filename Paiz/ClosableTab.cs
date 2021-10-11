using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Paiz
{
    class ClosableTab : TabItem
    {
        public ClosableTab()
        {
            // Create an instance of the usercontrol
            ClosableHeader closableTabHeader = new ClosableHeader();
            // Assign the usercontrol to the tab header
            //AdonisUI.ResourceLocator.AddAdonisResources(Resources);
            //Style = Resources["Styles.ToolbarButton"] as Style;
            this.Header = closableTabHeader;            

            closableTabHeader.button_close.MouseEnter += new MouseEventHandler(button_close_MouseEnter);
            closableTabHeader.button_close.MouseLeave += new MouseEventHandler(button_close_MouseLeave);
            closableTabHeader.button_close.Click += new RoutedEventHandler(Button_close_Click);
            closableTabHeader.label_TabTitle.SizeChanged += new SizeChangedEventHandler(Label_TabTitle_SizeChanged);
        }

        public string Title
        {
            set
            {
                ((ClosableHeader)this.Header).label_TabTitle.Content = value;
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((ClosableHeader)this.Header).button_close.Visibility = Visibility.Visible;
            //((ClosableHeader)this.Header).Background = new SolidColorBrush(Color.FromRgb(81, 104, 183));
            //((ClosableHeader)this.Header).label_TabTitle.Background = new SolidColorBrush(Color.FromRgb(81, 104, 183));
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            //((ClosableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
            //((ClosableHeader)this.Header).Background = new SolidColorBrush(Color.FromRgb(42, 43, 52));
            //((ClosableHeader)this.Header).label_TabTitle.Background = new SolidColorBrush(Color.FromRgb(42, 43, 52));
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ((ClosableHeader)this.Header).button_close.Visibility = Visibility.Visible;
            //((ClosableHeader)this.Header).Background = new SolidColorBrush(Colors.LightBlue);
            //((ClosableHeader)this.Header).label_TabTitle.Background = new SolidColorBrush(Colors.LightBlue);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                //((ClosableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
                //((ClosableHeader)this.Header).Background = new SolidColorBrush(Colors.DarkGray);
                //((ClosableHeader)this.Header).label_TabTitle.Background = new SolidColorBrush(Colors.DarkGray);
            }
            else
            {
                //((ClosableHeader)this.Header).Background = new SolidColorBrush(Colors.Blue);
                //((ClosableHeader)this.Header).label_TabTitle.Background = new SolidColorBrush(Colors.Blue);
            }
        }

        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((ClosableHeader)this.Header).button_close.Foreground = Brushes.Red;
        }
        // Button MouseLeave - When mouse is no longer over button - change color back to black
        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            ((ClosableHeader)this.Header).button_close.Foreground = Brushes.Black;
        }
        // Button Close Click - Remove the Tab - (or raise
        // an event indicating a "CloseTab" event has occurred)
        public void Button_close_Click(object sender, RoutedEventArgs e)
        {
            if (Content is MediaTagPage mp)
            {
                mp.CloseProperly();
            }

            ((TabControl)Parent).Items.Remove(this);
        }
        // Label SizeChanged - When the Size of the Label changes
        // (due to setting the Title) set position of button properly
        void Label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ClosableHeader)this.Header).button_close.Margin = new Thickness(
               ((ClosableHeader)this.Header).label_TabTitle.ActualWidth + 5, 0, 0, 0);
        }
    }
}
