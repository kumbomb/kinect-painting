// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;




namespace Microsoft.Kinect.Samples.KinectPaint
{
    public class KinectPaintListBox : ListBox
    {
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new KinectPaintListBoxItem(this);
        }
    }
}
