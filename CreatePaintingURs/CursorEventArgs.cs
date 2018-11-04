// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    public class CursorEventArgs : RoutedEventArgs
    {
        #region Data

        KinectCursor _cursor;
        int _timestamp;

        #endregion

        public CursorEventArgs(KinectCursor cursor, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            _cursor = cursor;
            _timestamp = Environment.TickCount;
        }

        #region Properties
        public KinectCursor Cursor { get { return _cursor; } }

        public int Timestamp { get { return _timestamp; } }

        #endregion
    }
}
