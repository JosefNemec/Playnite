using System.IO;

namespace Playnite.SDK.Metadata
{
    /// <summary>
    /// Represents metadata file.
    /// </summary>
    public class MetadataFile
    {
        /// <summary>
        /// Indicates whether metadata holds some content.
        /// </summary>
        public bool HasContent => !string.IsNullOrEmpty(FileName) && Content != null;

        /// <summary>
        /// Indicates whether there's some source information for the file (content or URL).
        /// </summary>
        public bool HasImageData => HasContent || !string.IsNullOrEmpty(OriginalUrl);

        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets file content.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets original source url.
        /// </summary>
        public string OriginalUrl { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        public MetadataFile() { }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="url">Source URL.</param>
        public MetadataFile(string url)
        {
            FileName = Path.GetFileName(url);
            OriginalUrl = url;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="data">File content.</param>
        /// <param name="originalUrl">Source url.</param>
        public MetadataFile(string name, byte[] data, string originalUrl = null)
        {
            FileName = name;
            Content = data;
            OriginalUrl = originalUrl;
        }
    }
}
