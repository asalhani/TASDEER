// -----------------------------------------------------------------------
// <copyright file="FtpDirectoryEntry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TASDEER.FTPHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FtpDirectoryEntry
    {
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsDirectory { get; set; }
        public Int64 Size { get; set; }
        public string Group { get; set; } // UNIX only
        public string Owner { get; set; }
        public string Flags { get; set; }
        public string FileUrl { get; set; }
    }

    public enum FtpDirectoryFormat
    {
        Unix,
        Windows,
        Unknown
    }
}
