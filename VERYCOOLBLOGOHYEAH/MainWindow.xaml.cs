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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VERYCOOLBLOGOHYEAH
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            JournalEntries journalEntries = new JournalEntries();
            DataContext = journalEntries;
        }
        private void ChangeFormating(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                switch (menuItem.Name)
                {
                    case "Italic":
                        if (BlogText.Selection.GetPropertyValue(FontStyleProperty).ToString() == "Italic")
                        {
                            BlogText.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                        }
                        else
                        {
                            BlogText.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                        }
                        break;
                    case "Bold":
                        if (BlogText.Selection.GetPropertyValue(FontWeightProperty).ToString() == "Bold")
                        {
                            BlogText.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                        }
                        else
                        {
                            BlogText.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        }
                        break;
                    case "FontSizeUp":
                        BlogText.Selection.ApplyPropertyValue(RichTextBox.FontSizeProperty, Math.Min((BlogText.Selection.GetPropertyValue(TextElement.FontSizeProperty) as double? ?? BlogText.FontSize) + 1.0, 30.0));
                        break;
                    case "FontSizeDown":
                        BlogText.Selection.ApplyPropertyValue(RichTextBox.FontSizeProperty, Math.Max((BlogText.Selection.GetPropertyValue(TextElement.FontSizeProperty) as double? ?? BlogText.FontSize) - 1.0, 8.0));
                        break;
                    default:
                        BlogText.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new BrushConverter().ConvertFromString(menuItem.Name) as SolidColorBrush);
                        break;
                }
            }
        }

        private void Filter_Selected(object sender, RoutedEventArgs e)
        {
            string Sorting = Filter.SelectedItem as string;
            switch (Sorting)
            {
                case "Name_plus":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Content",
                         System.ComponentModel.ListSortDirection.Ascending));
                    PostsList.Items.Refresh();
                    break;
                case "Name_minues":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Content",
                         System.ComponentModel.ListSortDirection.Descending));
                    PostsList.Items.Refresh();
                    break;
                case "Date_plus":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Date",
                         System.ComponentModel.ListSortDirection.Ascending));
                    PostsList.Items.Refresh();
                    break;
                case "Date_minues":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Date",
                         System.ComponentModel.ListSortDirection.Descending));
                    PostsList.Items.Refresh();
                    break;
                case "Likes_plus":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Likes",
                         System.ComponentModel.ListSortDirection.Ascending));
                    PostsList.Items.Refresh();
                    break;
                case "Likes_minues":
                    PostsList.Items.SortDescriptions.Clear();
                    PostsList.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("Likes",
                         System.ComponentModel.ListSortDirection.Descending));
                    PostsList.Items.Refresh();
                    break;
            }
        }

        private void PostEntry(object sender, RoutedEventArgs e)
        {
            if (DataContext is JournalEntries journalEntries)
            {
                JournalEntry entry = new JournalEntry();
                entry.Title = PostTitle.Text;
                entry.Content = new TextRange(BlogText.Document.ContentStart, BlogText.Document.ContentEnd).Text;
                entry.PostTime = DateTime.Now;
                if (entry.Likes > 1)
                {
                    entry.Likes = 0;
                }
                entry.Liked = $"♡"; ///&#x2665
                journalEntries.Items.Add(entry);
                PostsList.Items.Refresh();
            }
        }

        private void LikePost(object sender, RoutedEventArgs e)
        {
            
        }
}
public class JournalEntries
{
    public List<JournalEntry> Items { get; set; } = new List<JournalEntry>();
}
public class JournalEntry
{
    public DateTime PostTime { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; }
    public string Liked { get; set; }

}
}
