// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    public enum KinectPaintbrush
    {
        Brush,
        Marker,
        Eraser,
        Pastel,
        Light,
        Spider,
        Test2,
        Butterfly,
        Test4
    }
    public class BrushSelection
    {
        public BrushSelection(Uri icon, Uri iconSelected, KinectPaintbrush brush, string friendlyName)
        {
            Icon = icon;
            IconSelected = iconSelected;
            Brush = brush;
            FriendlyName = friendlyName;
        }

        #region Properties

        /// <summary>
        /// URI of the icon representing the brush
        /// </summary>
        public Uri Icon { get; private set; }

        /// <summary>
        /// URI of the icon representing the brush when the tool is selected
        /// </summary>
        public Uri IconSelected { get; private set; }

        /// <summary>
        /// The type of brush
        /// </summary>
        public KinectPaintbrush Brush { get; private set; }

        /// <summary>
        /// The user-friendly name of the brush
        /// </summary>
        public string FriendlyName { get; private set; }

        #endregion
    }
}
