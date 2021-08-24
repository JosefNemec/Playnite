using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class MarkupConverter : SDK.Data.IMarkupConverter
    {
        public string MarkdownToHtml(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown);
        }
    }
}
