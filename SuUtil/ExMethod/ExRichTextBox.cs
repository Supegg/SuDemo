using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SuUtil.ExMethod
{
    public static class ExRichTextBox
    {
        public static void AddLine(this RichTextBox rtb, string line, Brush brush = null)
        {
            rtb.Document.Blocks.Add(new Paragraph(new Run(line) { Foreground = brush == null ? Brushes.Black : brush,FontSize=14 }) { LineHeight = 0.1 });
        }
    }
}
