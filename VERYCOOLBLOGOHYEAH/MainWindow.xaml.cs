using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public DateTime postTime;
        public int likes;
        public string liked;
        public string content;
        public string title;
        public bool Editing;
        public int Id;
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
                        ToggleStates(TextElement.FontStyleProperty, FontStyles.Normal, FontStyles.Italic);
                        break;
                    case "Bold":
                        ToggleStates(TextElement.FontWeightProperty, FontWeights.Normal, FontWeights.Bold);
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

            void ToggleStates<T>(DependencyProperty property, T state1, T state2)
            {
                bool isState1 = BlogText.Selection.GetPropertyValue(property) is T t && t.Equals(state1);
                BlogText.Selection.ApplyPropertyValue(property, isState1 ? state2 : state1);
            }
        }

        private void EditPost(object sender, RoutedEventArgs e)
        {
            foreach (var o in PostsList.SelectedItems.OfType<JournalEntry>())
            {
                PostTitle.Text = o.Title;
                BlogText.Document.Blocks.Clear();
                BlogText.AppendText(o.Content);
                likes = o.Likes;
                liked = o.Liked;
                postTime = o.PostTime;
                Id = o.ID;
                Editing = true;
                postBtn.Content = "Save Changes";
                Order_Selected(sender, e);
            }
        }
        private void Order_Selected(object sender, RoutedEventArgs e)
        {
            var (property, direction) = ((ComboBoxItem)Filter.SelectedItem).Name switch
            {
                "Name_minus" => ("Title", ListSortDirection.Descending),
                "Date_plus" => ("PostTime", ListSortDirection.Ascending),
                "Date_minus" => ("PostTime", ListSortDirection.Descending),
                "Likes_plus" => ("Likes", ListSortDirection.Ascending),
                "Likes_minus" => ("Likes", ListSortDirection.Descending),
                _ => ("Title", ListSortDirection.Ascending),
            };
            PostsList.Items.SortDescriptions.Clear();
            PostsList.Items.SortDescriptions.Add(new SortDescription(property, direction));
            PostsList.Items.IsLiveSorting = true;
            PostsList.Items.Refresh();
        }

        private void PostEntry(object sender, RoutedEventArgs e)
        {
            if (DataContext is JournalEntries journalEntries)
            {
                if (Editing)
                {
                    try
                    {
                        if (PostTitle.Text != "")
                        {
                            JournalEntry entry = journalEntries.Items.Single(i => i.ID == Id);
                            entry.Title = PostTitle.Text;
                            entry.Content = new TextRange(BlogText.Document.ContentStart, BlogText.Document.ContentEnd).Text;
                            entry.PostTime = postTime;
                            entry.ID = Id;
                            entry.Likes = likes;
                            entry.Liked = liked;
                            PostsList.Items.Refresh();
                            Editing = false;
                            postBtn.Content = "Post";
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        if (PostTitle.Text != "")
                        {
                            JournalEntry entry = new JournalEntry();
                            entry.Title = PostTitle.Text;
                            entry.Content = new TextRange(BlogText.Document.ContentStart, BlogText.Document.ContentEnd).Text;
                            entry.PostTime = postTime;
                            entry.ID = Id;
                            entry.Likes = 0;
                            entry.Liked = $"♡";
                            Editing = false;
                            postBtn.Content = "Post";
                            journalEntries.Items.Add(entry);
                            PostsList.Items.Refresh();
                        }
                    }
                }
                else
                {
                    if (PostTitle.Text != "")
                    {
                        JournalEntry entry = new JournalEntry();
                        entry.Title = PostTitle.Text;
                        entry.Content = new TextRange(BlogText.Document.ContentStart, BlogText.Document.ContentEnd).Text;
                        entry.PostTime = DateTime.Now;
                        if (entry.Likes > 1)
                        {
                            entry.Likes = 0;
                        }
                        entry.ID = journalEntries.Items.Select(x => x.ID).DefaultIfEmpty().Max() + 1;
                        entry.Liked = $"♡"; ///&#x2665
                        journalEntries.Items.Add(entry);
                        PostsList.Items.Refresh();
                    }
                }
            }
            PostTitle.Clear();
            BlogText.Document.Blocks.Clear();
        }

        private void LikePost(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: JournalEntry entry })
            {
                if (entry.Liked == "♥")
                {
                    entry.Likes--;
                    entry.Liked = "♡";
                }
                else
                {
                    entry.Likes++;
                    entry.Liked = "♥"; ;
                }
                PostsList.Items.Refresh();
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            PostTitle.Clear();
            BlogText.Document.Blocks.Clear();
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is JournalEntries journalEntries)
            {
                foreach (var o in PostsList.SelectedItems.OfType<JournalEntry>())
                {
                    Id = o.ID;
                    JournalEntry entry = journalEntries.Items.Single(i => i.ID == Id);
                    journalEntries.Items.Remove(entry);
                    break;
                }
                PostsList.Items.Refresh();
            }
        }

        private string[] SplitContentToFitTemplate(string content, int maxLength)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < content.Length; i += maxLength)
            {
                int length = Math.Min(maxLength, content.Length - i);
                lines.Add(content.Substring(i, length));
            }

            return lines.ToArray();
        }

        private void SharePost(object sender, RoutedEventArgs e)
        {
            string exportTemplate;
            foreach (var n in PostsList.SelectedItems.OfType<JournalEntry>())
            {
                title = $"Title: {n.Title}";
                content = n.Content;
                string str = n.Likes.ToString();
                string likeReplace = $"Likes: {new string(' ', str.Count())}";
                string likeCount = $"Likes: {n.Likes}";
                string postTimeReplace = $"Creation Time: {new string(' ', n.PostTime.ToShortDateString().ToString().Count())}";
                string postTimeStr = $"Creation Time: {n.PostTime.ToShortDateString()}";
                string exportReplace = $"Export Time: {new string(' ', DateTime.Now.ToShortDateString().ToString().Count())}";
                string exportTimeStr = $"Export Time: {DateTime.Today.ToShortDateString()}";
                //Dictionary<string, int> vars = new Dictionary<string, int>();
                //decimal ugh = 60 / exportReplace.Count();
                //decimal maxLines = Math.Ceiling(ugh * 100) / 100;
                //for (int ec = 0; ec <= maxLines; ec++)
                //{
                //    vars.Add(string.Format("line{0}", i.ToString()), i);
                //    for (int i = 0; i <= exportReplace.Count();)
                //    {
                //        vars["line"]
                //    }
                //}


                if (n.Title.Count() > 52)
                {
                    string[] Titlelines = SplitContentToFitTemplate(n.Title, 52);
                    exportTemplate =
                        $"""
                    ┌────────────────────────────────────────────────────────────────┐
                    │                                                                │
                    │  Creation Time: {n.PostTime:dd.MM.yyyy}                         Likes: {n.Likes,-5}│
                    │  Export Time: {DateTime.Now:dd.MM.yyyy}                                       │
                    │                                                                │
                    │  Title: {Titlelines.First(),-55}│
                    """;

                    foreach (string line in Titlelines.Skip(1))
                    {
                        if (line.Count() < 55)
                        {
                            exportTemplate +=
        @$"
│  {line,-62}";
                            exportTemplate += "|";
                        }
                    }
                    exportTemplate += "\r\n│                                                                │";

                }
                else
                {

                    exportTemplate =
                        $"""
                    ┌────────────────────────────────────────────────────────────────┐
                    │                                                                │
                    │  Creation Time: {n.PostTime:dd.MM.yyyy}                         Likes: {n.Likes,-5}│
                    │  Export Time: {DateTime.Now:dd.MM.yyyy}                                       │
                    │                                                                │
                    │  Title: {n.Title,-55}│
                    │                                                                │
                    """;
                }



                string[] contentLines = SplitContentToFitTemplate(n.Content, 60);

                string filePath = @"C:\Users\user\Downloads\test.txt";
                string last = contentLines.Last();
                foreach (string line in contentLines)
                {
                    if (line.Count() <= 60 && line != last)
                    {
                        exportTemplate +=
    @$"
│  {line,-62}│";
                    }
                    else if (line == last)
                    {
                        string lline = line.Replace("\r\n", "");
                        exportTemplate += @$"{Environment.NewLine}│  {lline,-62}│{Environment.NewLine}";
                    }
                    else
                    {
                        exportTemplate +=
    @$"{Environment.NewLine}│  {line,-62}│";
                    }
                }
                exportTemplate +=
@$"│                                                                │
└────────────────────────────────────────────────────────────────┘";
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                saveFileDialog.InitialDirectory = @"%USERPROFILE%\Documents";
                saveFileDialog.Title = "Export Post as";
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, exportTemplate);
                }
                Process.Start(@"C:\Program Files\Notepad++\notepad++.exe", saveFileDialog.FileName);
            }
        }


        public class JournalEntries
        {
            public ObservableCollection<JournalEntry> Items { get; } = new();
        }

        public class JournalEntry : INotifyPropertyChanged
        {
            private int id;
            public int ID
            {
                get => id;
                set => SetField(ref id, value);
            }

            private DateTime postTime;
            public DateTime PostTime
            {
                get => postTime;
                set => SetField(ref postTime, value);
            }

            private string title;
            public string Title
            {
                get => title;
                set => SetField(ref title, value);
            }

            private string content;
            public string Content
            {
                get => content;
                set => SetField(ref content, value);
            }

            private int likes;
            public int Likes
            {
                get => likes;
                set => SetField(ref likes, value);
            }

            private string liked;
            public string Liked
            {
                get => liked;
                set => SetField(ref liked, value);
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                e.Handled = true;
        }
    }
}
