using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace VERYCOOLBLOGOHYEAH;

public class RichTextBoxHelper : DependencyObject
{
    private static HashSet<Thread> _recursionProtectionXaml = new HashSet<Thread>();

    public static string GetDocumentXaml(DependencyObject obj)
    {
        return (string)obj.GetValue(DocumentXamlProperty);
    }

    public static void SetDocumentXaml(DependencyObject obj, string value)
    {
        _recursionProtectionXaml.Add(Thread.CurrentThread);
        obj.SetValue(DocumentXamlProperty, value);
        _recursionProtectionXaml.Remove(Thread.CurrentThread);
    }

    public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
        "DocumentXaml",
        typeof(string),
        typeof(RichTextBoxHelper),
        new FrameworkPropertyMetadata(
            "",
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (obj, e) =>
            {
                if (_recursionProtectionXaml.Contains(Thread.CurrentThread))
                    return;

                var richTextBox = (RichTextBox)obj;

                // Parse the XAML to a document (or use XamlReader.Parse())

                try
                {
                    var doc = (FlowDocument)XamlReader.Parse(GetDocumentXaml(richTextBox));

                    // Set the document
                    richTextBox.Document = doc;
                }
                catch (Exception)
                {
                    richTextBox.Document = new FlowDocument();
                }

                // When the document changes update the source
                richTextBox.TextChanged += (obj2, e2) =>
                {
                    SetDocumentXaml(richTextBox, XamlWriter.Save(richTextBox.Document));
                };
            }
        )
    );
}