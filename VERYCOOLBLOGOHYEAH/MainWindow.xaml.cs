using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace VERYCOOLBLOGOHYEAH;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public JournalEntries journalEntries = new JournalEntries();

    public MainWindow()
    {
        InitializeComponent();
        journalEntries.Items.Load();
        DataContext = journalEntries;
        PostsList.ItemsSource = journalEntries.Items.Local.ToObservableCollection();
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

    private void Order_Selected(object sender, SelectionChangedEventArgs e)
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

    private void SaveEntry(object sender, RoutedEventArgs e)
    {
        if (journalEntries.CurrentEntry != null && !journalEntries.Items.Local.Contains(journalEntries.CurrentEntry))
        {
            if (journalEntries.CurrentEntry.Title != null)
            {
                //TextRange range = new TextRange(BlogText.Document.ContentStart, BlogText.Document.ContentEnd);
                //journalEntries.CurrentEntry = new()
                //{
                //    Title = PostTitle.Text,
                //    Content = XamlWriter.Save(BlogText.Document),
                //};
                journalEntries.CurrentEntry.PostTime = DateTime.Now;
                journalEntries.CurrentEntry.Liked = "♡"; ///&#x2665
                journalEntries.CurrentEntry.Likes = 0;
                journalEntries.Items.Add(journalEntries.CurrentEntry);
            }

            journalEntries.SaveChanges();
        }
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
            //PostsList.Items.Refresh();
            //journalEntries.Items.Update(entry);
            journalEntries.SaveChanges();
        }
    }

    private void NewBtn_Click(object sender, RoutedEventArgs e)
    {
        journalEntries.CurrentEntry = new();
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        RemoveEntry();
    }

    private void RemoveEntry()
    {
        journalEntries.Items.Remove(journalEntries.CurrentEntry);
        journalEntries.SaveChanges();
        //PostsList.Items.Refresh();
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

    private IEnumerable<string> SmartSplit(string input, int maxLength)
    {
        int i = 0;
        while (i + maxLength < input.Length)
        {
            int index = input.LastIndexOf(' ', i + maxLength);
            if (index <= 0)
            {
                index = maxLength;
            }
            yield return input.Substring(i, index - i);

            i = index + 1;
        }

        yield return input.Substring(i);
    }
    private void SharePost(object sender, RoutedEventArgs e)
    {
        string exportTemplate;
        var n = journalEntries.CurrentEntry;
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
        try
        {
            if (n.Title.Length > 52)
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
                    if (line.Length < 55)
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


            IEnumerable<string> contentLines = SmartSplit(n.TextContent, 60);
            try
            {
                string last = contentLines.Last();
                foreach (string line in contentLines)
                {
                    if (line.Length <= 60 && line != last)
                    {
                        string lline = line.Replace("\r", "");
                        string llline = lline.Replace("\n", "");
                        exportTemplate += @$"{Environment.NewLine}│  {llline,-62}│";
                    }
                    else if (line == last)
                    {
                        string lline = line.Replace("\r", "");
                        string llline = lline.Replace("\n", "");
                        exportTemplate += @$"{Environment.NewLine}│  {llline,-62}│";
                    }
                    else
                    {
                        string lline = line.Replace("\r", "");
                        string llline = lline.Replace("\n", "");
                        exportTemplate += @$"{Environment.NewLine}│  {llline,-62}│";
                    }
                }
            }
            catch (InvalidOperationException)
            {
                string last = $"{Environment.NewLine}│  {-62}│";
            }
            exportTemplate += $"{Environment.NewLine}│                                                                │{Environment.NewLine}└────────────────────────────────────────────────────────────────┘";

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
        catch (NullReferenceException)
        {
            MessageBox.Show("ERROR: THIS BITCH'S EMPTY");
        }
    }
    public class JournalEntries : DbContext, INotifyPropertyChanged
    {
        public DbSet<JournalEntry> Items { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=stupid;Trusted_Connection=True;MultipleActiveResultSets=True;Trust Server Certificate=True;App=BlogApp");
        }

        private JournalEntry? currentEntry;
        public JournalEntry? CurrentEntry
        {
            get => currentEntry;
            set
            {
                SetField(ref currentEntry, value);
            }
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
            set
            {
                SetField(ref content, value);
                OnPropertyChanged(nameof(TextContent));
            }
        }

        public string TextContent
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Content))
                    return "";

                MemoryStream buffer = new MemoryStream();
                var document = (FlowDocument)XamlReader.Parse(Content);
                var range = new TextRange(document.ContentStart, document.ContentEnd);
                range.Save(buffer, DataFormats.Text);
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
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
}
