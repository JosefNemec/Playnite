using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    // Originally from https://stackoverflow.com/questions/9746538/fastest-safest-file-finding-parsing
    public class SafeFileEnumerator : IEnumerable<FileSystemInfoBase>
    {
        /// <summary>
        /// Starting directory to search from
        /// </summary>
        private DirectoryInfoBase root;

        /// <summary>
        /// Filter pattern
        /// </summary>
        private string pattern;

        /// <summary>
        /// Indicator if search is recursive or not
        /// </summary>
        private SearchOption searchOption;

        /// <summary>
        /// Any errors captured
        /// </summary>
        private IList<Exception> errors;

        /// <summary>
        /// Create an Enumerator that will scan the file system, skipping directories where access is denied
        /// </summary>
        /// <param name="root">Starting Directory</param>
        /// <param name="pattern">Filter pattern</param>
        /// <param name="option">Recursive or not</param>
        public SafeFileEnumerator(string root, string pattern, SearchOption option)
            : this(new DirectoryInfoWrapper(new DirectoryInfo(root)), pattern, option)
        { }

        /// <summary>
        /// Create an Enumerator that will scan the file system, skipping directories where access is denied
        /// </summary>
        /// <param name="root">Starting Directory</param>
        /// <param name="pattern">Filter pattern</param>
        /// <param name="option">Recursive or not</param>
        public SafeFileEnumerator(DirectoryInfoBase root, string pattern, SearchOption option)
            : this(root, pattern, option, new List<Exception>())
        { }

        // Internal constructor for recursive itterator
        private SafeFileEnumerator(DirectoryInfoBase root, string pattern, SearchOption option, IList<Exception> errors)
        {
            if (root == null || !root.Exists)
            {
                throw new ArgumentException("Root directory is not set or does not exist.", "root");
            }
            this.root = root;
            this.searchOption = option;
            this.pattern = String.IsNullOrEmpty(pattern)
                ? "*"
                : pattern;
            this.errors = errors;
        }

        /// <summary>
        /// Errors captured while parsing the file system.
        /// </summary>
        public Exception[] Errors
        {
            get
            {
                return errors.ToArray();
            }
        }

        /// <summary>
        /// Helper class to enumerate the file system.
        /// </summary>
        private class Enumerator : IEnumerator<FileSystemInfoBase>
        {
            // Core enumerator that we will be walking though
            private IEnumerator<FileSystemInfoBase> fileEnumerator;
            // Directory enumerator to capture access errors
            private IEnumerator<DirectoryInfoBase> directoryEnumerator;

            private DirectoryInfoBase root;
            private string pattern;
            private SearchOption searchOption;
            private IList<Exception> errors;

            public Enumerator(DirectoryInfoBase root, string pattern, SearchOption option, IList<Exception> errors)
            {
                this.root = root;
                this.pattern = pattern;
                this.errors = errors;
                this.searchOption = option;

                Reset();
            }

            /// <summary>
            /// Current item the primary itterator is pointing to
            /// </summary>
            public FileSystemInfoBase Current
            {
                get
                {
                    //if (fileEnumerator == null) throw new ObjectDisposedException("FileEnumerator");
                    return fileEnumerator.Current as FileSystemInfoBase;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                Dispose(true, true);
            }

            private void Dispose(bool file, bool dir)
            {
                if (file)
                {
                    if (fileEnumerator != null)
                        fileEnumerator.Dispose();

                    fileEnumerator = null;
                }

                if (dir)
                {
                    if (directoryEnumerator != null)
                        directoryEnumerator.Dispose();

                    directoryEnumerator = null;
                }
            }

            public bool MoveNext()
            {
                // Enumerate the files in the current folder
                if ((fileEnumerator != null) && (fileEnumerator.MoveNext()))
                    return true;

                // Don't go recursive...
                if (searchOption == SearchOption.TopDirectoryOnly) { return false; }

                while ((directoryEnumerator != null) && (directoryEnumerator.MoveNext()))
                {
                    Dispose(true, false);

                    try
                    {
                        fileEnumerator = new SafeFileEnumerator(
                            directoryEnumerator.Current,
                            pattern,
                            SearchOption.AllDirectories,
                            errors
                            ).GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        continue;
                    }

                    // Open up the current folder file enumerator
                    if (fileEnumerator.MoveNext())
                        return true;
                }

                Dispose(true, true);

                return false;
            }

            public void Reset()
            {
                Dispose(true, true);

                // Safely get the enumerators, including in the case where the root is not accessable
                if (root != null)
                {
                    try
                    {
                        fileEnumerator = root.GetFileSystemInfos(pattern, SearchOption.TopDirectoryOnly).AsEnumerable<FileSystemInfoBase>().GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        fileEnumerator = null;
                    }

                    try
                    {
                        directoryEnumerator = root.GetDirectories("*", SearchOption.TopDirectoryOnly).AsEnumerable<DirectoryInfoBase>().GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        directoryEnumerator = null;
                    }
                }
            }
        }

        public IEnumerator<FileSystemInfoBase> GetEnumerator()
        {
            return new Enumerator(root, pattern, searchOption, errors);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
