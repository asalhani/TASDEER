// -----------------------------------------------------------------------
// <copyright file="FTPHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TASDEER.FTPHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FTPHelper
    {
         #region Proparties

        public static string FTPUserName { get; set; }
        public static string FTPPassword { get; set; }
        public static string HttpSiteUrl { get; set; }
        public static string HttpAttachmentVirtualDirectory { get; set; }

        #endregion
        #region Constructor

        static FTPHelper()
        {
            FTPUserName = ConfigurationManager.AppSettings.Get("FTPUserName");
            FTPPassword = ConfigurationManager.AppSettings.Get("FTPPassword");
            HttpSiteUrl = ConfigurationManager.AppSettings.Get("HttpSiteUrl");
            HttpAttachmentVirtualDirectory = ConfigurationManager.AppSettings.Get("HttpAttachmentVirtualDirectory");
        }

        #endregion

        delegate FtpDirectoryEntry ParseLine(string lines, string directoryUrl);

        /// <summary>
        /// Find file in a FTP directory
        /// </summary>
        /// <param name="directoryURL">the directory where to search (ftp://{FTPServerURL}/DirectoryName) </param>
        /// <param name="fileToFind">file name (or part of it) to find --> this fucntion use "Contains"</param>
        /// <param name="ftpUser">FTP User name (allow empty string)</param>
        /// <param name="ftpPassword">FTP Passoword (allow empty string)</param>
        /// <returns>returns file name and extention in case the file found, else returns empty string</returns>
        public static string GetFile(string directoryURL, string fileToFind)
        {
            string file = string.Empty;

            try
            {
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(directoryURL));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                while (!reader.EndOfStream)
                {
                    string filename = reader.ReadLine().ToString().ToLower();
                    if (!string.IsNullOrEmpty(fileToFind))
                    {
                        if (filename.Contains(fileToFind.ToLower()))
                        {
                            file = filename.Trim();
                            break;
                        }
                    }

                }

                response.Close();
                responseStream.Close();
                reader.Close();
            }
            catch (Exception ex)
            {
            }

            return file;
        }

        /// <summary>
        /// Check if given directory exist or not
        /// </summary>
        /// <param name="directoryURL">the directory to check (ftp://{FTPServerURL}/DirectoryName)</param>
        /// <param name="ftpUser">FTP User name (allow empty string)</param>
        /// <param name="ftpPassword">FTP Passoword (allow empty string)</param>
        /// <returns>returns True if given directory exist, otherwise, return false</returns>
        public static bool IsFtpDirectoryExists(string directoryURL)
        {
            //bool IsExists = true;
            //try
            //{
            //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryURL);
            //    request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
            //    request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;

            //    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            //}
            //catch (WebException ex)
            //{
            //    IsExists = false;
            //}
            //return IsExists;

            bool directoryExists = true ;

            //var request = (FtpWebRequest)WebRequest.Create(directoryURL);
            //request.Method = WebRequestMethods.Ftp.ListDirectory;
            //request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

            //try
            //{
            //    using (request.GetResponse())
            //    {
            //        directoryExists = true;
            //    }
            //}
            //catch (WebException)
            //{
            //    directoryExists = false;
            //}

            //return directoryExists;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryURL);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    directoryExists =  true; 
                }
                return directoryExists;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        directoryExists =  false;
                    }
                }
                return directoryExists;
            } 
        }

        /// <summary>
        /// Create new directory
        /// </summary>
        /// <param name="directoryURL">The URL of the new directory to be created (ftp://{FTPServerURL}/NewDirectoryName)</param>
        /// <param name="ftpUser">FTP User name (allow empty string)</param>
        /// <param name="ftpPassword">FTP Passoword (allow empty string)</param>
        public static void CreateFtpDirectory(string directoryURL)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryURL);
                request.Credentials = new NetworkCredential(FTPUserName,FTPPassword);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {

                //throw;
            }
        }

        public static void UploadXMLToFTP(string directoryURL, string fileName, string fileContent)
        {
            // check if directory exist, if not, create it
            if (!IsFtpDirectoryExists(directoryURL))
                CreateFtpDirectory(directoryURL);

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Concat(directoryURL, "/", fileName));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

            // Copy the contents of the file to the request stream.
            // StreamReader sourceStream = new StreamReader("testfile.txt");

            byte[] fileContents = Encoding.UTF8.GetBytes(fileContent);

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }

        public static void UploadAttachmentsToFtp(string directoryURL, FileUpload attachmentFile, string fileName)
        {
            if (attachmentFile.HasFile)
            {
                FileInfo fileInfo = new FileInfo(attachmentFile.FileName);
                string fileExtention = fileInfo.Extension;

                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Concat(directoryURL, "/", fileName, fileExtention));
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                byte[] fileContents = attachmentFile.FileBytes;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                response.Close();
            }
        }

        public static bool IsFileExist(string directoryURL, string fileToFind)
        {
            bool isExist = false;

            try
            {
                if (IsFtpDirectoryExists(directoryURL))
                {
                    FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(directoryURL));
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    while (!reader.EndOfStream)
                    {
                        string filename = reader.ReadLine().ToString().ToLower();
                        if (!string.IsNullOrEmpty(fileToFind))
                        {
                            if (filename.Contains(fileToFind.ToLower()))
                            {
                                isExist = true;
                                break;
                            }
                        }

                    }

                    response.Close();
                    responseStream.Close();
                    reader.Close();
                }
                else
                    isExist = false;
            }
            catch (Exception ex)
            {
                return false;
            }

            return isExist;
        }

        /// <summary>
        /// This method downloads the given file name from the FTP server
        /// and returns a byte array containing its contents.
        /// Throws a WebException on encountering a network error.
        /// </summary>
        /// <param name="path">the path of the file to be downloaded (ftp://{FTPServerURL}/NewDirectoryName)</param>
        /// <returns>return array of byte (byte[])</returns>
        public static byte[] DownloadData(string path)
        {
            // Get the object used to communicate with the server.
            WebClient request = new WebClient();

            // Logon to the server using username + password
            request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
            return request.DownloadData(new Uri(path));
        }

        /// <summary>
        /// Returns a directory listing of the current working directory.
        /// </summary>
        public static List<FtpDirectoryEntry> ListDirectory(string directoryUrl)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryUrl);
            request.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            string listing;
            using (FtpWebResponse response = request.GetResponse() as FtpWebResponse)
            {
                StreamReader sr = new StreamReader(response.GetResponseStream(),
                    System.Text.Encoding.UTF8);
                listing = sr.ReadToEnd();
                response.Close();
            }
            return ParseDirectoryListing(listing, directoryUrl);
        }

        // Converts a directory listing to a list of FtpDirectoryEntrys
        protected static List<FtpDirectoryEntry> ParseDirectoryListing(string listing, string directoryUrl)
        {
            ParseLine parseFunction = null;
            List<FtpDirectoryEntry> entries = new List<FtpDirectoryEntry>();
            string[] lines = listing.Split('\n');
            FtpDirectoryFormat format = GuessDirectoryFormat(lines);

            if (format == FtpDirectoryFormat.Windows)
                parseFunction = ParseWindowsDirectoryListing;
            else if (format == FtpDirectoryFormat.Unix)
                parseFunction = ParseUnixDirectoryListing;

            if (parseFunction != null)
            {
                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        FtpDirectoryEntry entry = parseFunction(line, directoryUrl);
                        if (entry.Name != "." && entry.Name != "..")
                            entries.Add(entry);
                    }
                }
            }
            return entries; ;
        }

        // Attempts to determine the directory format.
        protected static FtpDirectoryFormat GuessDirectoryFormat(string[] lines)
        {
            foreach (string s in lines)
            {
                if (s.Length > 10 && Regex.IsMatch(s.Substring(0, 10),
                    "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FtpDirectoryFormat.Unix;
                }
                else if (s.Length > 8 && Regex.IsMatch(s.Substring(0, 8),
                    "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FtpDirectoryFormat.Windows;
                }
            }
            return FtpDirectoryFormat.Unknown;
        }

        // Parses a line from a Windows-format listing
        // 
        // Assumes listing style as:
        // 02-03-04  07:46PM       <DIR>          Append
        protected static FtpDirectoryEntry ParseWindowsDirectoryListing(string text, string directoryUrl)
        {
            FtpDirectoryEntry entry = new FtpDirectoryEntry();

            text = text.Trim();
            string dateStr = text.Substring(0, 8);
            text = text.Substring(8).Trim();
            string timeStr = text.Substring(0, 7);
            text = text.Substring(7).Trim();
            entry.CreateTime = DateTime.Parse(String.Format("{0} {1}", dateStr, timeStr));
            if (text.Substring(0, 5) == "<DIR>")
            {
                entry.IsDirectory = true;
                text = text.Substring(5).Trim();
            }
            else
            {
                entry.IsDirectory = false;
                int pos = text.IndexOf(' ');
                entry.Size = Int64.Parse(text.Substring(0, pos));
                text = text.Substring(pos).Trim();
            }
            entry.Name = text;  // Rest is name

            string dirName = directoryUrl[directoryUrl.Length - 1] == '/' ? directoryUrl.Substring(0, directoryUrl.Length - 1) : directoryUrl;
            dirName = dirName.Substring(dirName.LastIndexOf('/') + 1);
            entry.FileUrl = string.Concat(HttpSiteUrl, HttpAttachmentVirtualDirectory,dirName,"/", entry.Name);
            return entry;
        }

        // Parses a line from a UNIX-format listing
        // 
        // Assumes listing style as:
        // dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys
        protected static FtpDirectoryEntry ParseUnixDirectoryListing(string text, string directoryUrl)
        {
            // Assuming record style as
            // dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys
            FtpDirectoryEntry entry = new FtpDirectoryEntry();
            string processstr = text.Trim();
            entry.Flags = processstr.Substring(0, 9);
            entry.IsDirectory = (entry.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            CutSubstringWithTrim(ref processstr, ' ', 0);   //skip one part
            entry.Owner = CutSubstringWithTrim(ref processstr, ' ', 0);
            entry.Group = CutSubstringWithTrim(ref processstr, ' ', 0);
            CutSubstringWithTrim(ref processstr, ' ', 0);   //skip one part
            entry.CreateTime = DateTime.Parse(CutSubstringWithTrim(ref processstr, ' ', 8));
            entry.Name = processstr;   //Rest of the part is name
            return entry;
        }

        // Removes the token ending in the specified character
        protected static string CutSubstringWithTrim(ref string s, char c, int startIndex)
        {
            int pos = s.IndexOf(c, startIndex);
            if (pos < 0) pos = s.Length;
            string retString = s.Substring(0, pos);
            s = (s.Substring(pos)).Trim();
            return retString;
        }
        //public static void UploadCVInfoXmlToFTP(string directoryURL, string xml, string ftpUser, string ftpPassword)
        //{
        //    // Get the object used to communicate with the server.
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryURL);
        //    request.Method = WebRequestMethods.Ftp.UploadFile;

        //    // This example assumes the FTP site uses anonymous logon.
        //    request.Credentials = new NetworkCredential("spsetupd", "P@ssw0rd1");

        //    // Copy the contents of the file to the request stream.
        //    // StreamReader sourceStream = new StreamReader("testfile.txt");



        //    byte[] fileContents = Encoding.UTF8.GetBytes(xml);

        //    Stream requestStream = request.GetRequestStream();
        //    requestStream.Write(fileContents, 0, fileContents.Length);
        //    requestStream.Close();

        //    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

        //    Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

        //    response.Close();
        //}
    }
    
}
