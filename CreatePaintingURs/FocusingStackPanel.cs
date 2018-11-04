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
    /// <summary>
    /// A Grid customized to work like a StackPanel that assigns equal space to each child item except for the one in "focus"
    /// </summary>
    public class FocusingStackPanel : Grid
    {
        #region Properties
        public int FocusedIndex 
        {
            get { return _focusedIndex; }
            set
            {
                if (_focusedIndex == value) return;

                _focusedIndex = value;

                ApplyFocus();
            }
        }
        private int _focusedIndex;

        public double FocusedQuantity 
        {
            get { return _focusedQuantity; }
            set
            {
                if (_focusedQuantity == value) return;

                _focusedQuantity = value;

                ApplyFocus();
            }
        }
        private double _focusedQuantity;

        #endregion

        #region Internal

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            if (Children.Count != RowDefinitions.Count)
                FixRowDefinitions();

            return base.MeasureOverride(constraint);
        }

        private void ApplyFocus()
        {
            if (Children.Count != RowDefinitions.Count)
                FixRowDefinitions();

            for (int i = 0; i < Children.Count; i++)
                RowDefinitions[i].Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);

            RowDefinitions[FocusedIndex].Height = new System.Windows.GridLength(FocusedQuantity, System.Windows.GridUnitType.Star);
        }
        private void FixRowDefinitions()
        {
            RowDefinitions.Clear();

            for (int i = 0; i < Children.Count; i++)
            {
                RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1.0, System.Windows.GridUnitType.Star) });
                SetRow(Children[i], i);
            }
        }

        #endregion
    }
}
