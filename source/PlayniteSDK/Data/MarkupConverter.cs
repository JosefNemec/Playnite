using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Data
{
    /// <summary>
    /// Describes markup converter.
    /// </summary>
    public interface IMarkupConverter
    {
        /// <summary>
        /// Converts Markdown markup to HTML.
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        string MarkdownToHtml(string markdown);
    }

    /// <summary>
    /// Represents markup converter.
    /// </summary>
    public class Markup
    {
        private static IMarkupConverter converter;

        internal static void Init(IMarkupConverter textConverter)
        {
            converter = textConverter;
        }

        /// <summary>
        /// Converts Markdown markup to HTML.
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static string MarkdownToHtml(string markdown)
        {
            return converter.MarkdownToHtml(markdown);
        }
    }
}
