// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using System.Windows;




namespace Microsoft.Kinect.Samples.KinectPaint
{
    public class KinectPaintButton : Button
    {
        #region Data

        IInvokeProvider _invoker;

        #endregion

        public KinectPaintButton()
        {
            KinectCursor.AddCursorEnterHandler(this, OnCursorEnter);
            KinectCursor.AddCursorLeaveHandler(this, OnCursorLeave);
            _invoker = (IInvokeProvider)UIElementAutomationPeer.CreatePeerForElement(this).GetPattern(PatternInterface.Invoke);
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

        private void Cursor_HoverFinished(object sender, EventArgs e)
        {
            _invoker.Invoke();
           
            ((KinectCursor)sender).HoverFinished -= Cursor_HoverFinished;
        }
        #endregion
    }
}
