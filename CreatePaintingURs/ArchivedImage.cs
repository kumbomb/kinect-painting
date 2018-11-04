// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Represents an image in My Pictures\KinectPaint that can be loaded
    /// </summary>
    public class ArchivedImage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">The file path of the image</param>
        public ArchivedImage(string path)
        {
            CreateTime = File.GetLastWriteTime(path);
            Image = new Uri(path, UriKind.RelativeOrAbsolute);
            CreateTimeString = CreateTime.ToString("yyyy/M/d", CultureInfo.CurrentCulture) + "\n" + CreateTime.ToString("tt h:mm", CultureInfo.CurrentCulture);
        }

        #region Properties

        /// <summary>
        /// The path of the image file
        /// </summary>
        public Uri Image { get; private set; }

        /// <summary>
        /// The time the file was last modified
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// A string representation of the time the file was last modified
        /// </summary>
        public string CreateTimeString { get; private set; }

        #endregion

    }
}
