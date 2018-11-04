// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Windows.Controls;

namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// The item container used by KinectPaintListBox that responds to hover-over gestures from the Kinect cursor
    /// </summary>
    public class KinectPaintListBoxItem : ListBoxItem
    {
        #region Data

        KinectPaintListBox _listbox;

        #endregion

        public KinectPaintListBoxItem(KinectPaintListBox listBox)
        {
            _listbox = listBox;

            KinectCursor.AddCursorEnterHandler(this, OnCursorEnter);
            KinectCursor.AddCursorLeaveHandler(this, OnCursorLeave);
        }

        #region Internal

        private void OnCursorEnter(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();

            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        private void OnCursorLeave(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();

            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        void Cursor_HoverFinished(object sender, EventArgs e)
        {
            if (_listbox != null)
                _listbox.SelectedIndex = _listbox.ItemContainerGenerator.IndexFromContainer(this);

            ((KinectCursor)sender).HoverFinished -= Cursor_HoverFinished;

        }

        #endregion
    }
}
