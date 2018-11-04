// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using Microsoft.Kinect.Samples.KinectPaint;


namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Control displays available images to load, allowing the user to choose one.
    /// </summary>
    public partial class LoadPopup : UserControl
    {
        #region Data

        const int FilesToDisplay = 16;
        MainWindow _main;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="main">A reference to the main window of the app</param>
        public LoadPopup(MainWindow main)
        {
            _main = main;

            List<string> files = new List<string>(Directory.GetFiles(App.PhotoFolder));
            ArchivedImage[] images = new ArchivedImage[files.Count > FilesToDisplay ? FilesToDisplay : files.Count];

            files.Sort((x, y) =>
                {
                    DateTime xd = File.GetLastWriteTime(x);
                    DateTime yd = File.GetLastWriteTime(y);

                    if (xd < yd) return 1;
                    if (xd > yd) return -1;
                    return 0;
                });

            for (int i = 0; i < files.Count && i < FilesToDisplay; i++)
                images[i] = new ArchivedImage(files[i]);

            AvailableImages = new ReadOnlyCollection<ArchivedImage>(images);

            InitializeComponent();
        }

        #region Properties

        /// <summary>
        /// The list of images that can be loaded
        /// </summary>
        public ReadOnlyCollection<ArchivedImage> AvailableImages { get; private set; }

        /// <summary>
        /// The image the user selected
        /// </summary>
        public ArchivedImage SelectedImage { get; set; }

        #endregion

        #region Event Handlers

        // When the user selects an image to load, just load it so it's an easy 1-step process.
        private void OnSelectedImageChanged(object sender, SelectionChangedEventArgs args)
        {
            _main.LoadingFinished();
        }

        // Canceling simply involves nulling out the popup window to make it go away.
        private void OnCancel(object sender, RoutedEventArgs args)
        {
            _main.CurrentPopup = null;
        }

        #endregion
    }
}
