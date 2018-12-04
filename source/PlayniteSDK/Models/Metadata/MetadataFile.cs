﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Metadata
{
    /// <summary>
    /// Represents metadata file.
    /// </summary>
    public class MetadataFile
    {
        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        public string FileName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file content.
        /// </summary>
        public byte[] Content
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets original source url.
        /// </summary>
        public string OriginalUrl
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        public MetadataFile()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="url">Source URL.</param>
        public MetadataFile(string url)
        {
            FileName = Path.GetFileName(url);
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="data">File content.</param>
        public MetadataFile(string name, byte[] data) : this(name, data, null)
        {            
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="data">File content.</param>
        /// <param name="originalUrl">Source url.</param>
        public MetadataFile(string name, byte[] data, string originalUrl)
        {
            FileName = name;
            Content = data;
            OriginalUrl = originalUrl;
        }
    }
}
