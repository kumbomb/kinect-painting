// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

using Microsoft.Kinect;

using Coding4Fun.Kinect.Wpf;

namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Enumerates types of actions that require a confirmation popup
    /// </summary>
    public enum ActionAwaitingConfirmation
    {
        Close,
        New,
        Load
    }

    /// <summary>
    /// The main application window
    /// </summary>
    public partial class MainWindow : Window
    {
        public double Menu_y_po;
        public double Menu_y_po_init;

        public double Brush_y_po;
        public double Brush_y_po_init;

        bool Menu_on = false;
        bool Brush_on = false;
        DispatcherTimer Mytimer = new DispatcherTimer();
        DispatcherTimer Mytimer2 = new DispatcherTimer();

        public static bool _isTutorialActive;

        public static MainWindow Instance { get; private set; }

        #region Data

        
        Point _pastCursorPosition;
        bool _imageUnsaved;
        FocusingStackPanel _colorpicker;
        bool _isPickingColor;

        #endregion

        public MainWindow()
        {
            //시작하자마자 그리기
            //_isTutorialActive = false;
            ShowCamera = true;
            InitializeComponent();
            SelectedBrush = _availableBrushes[0];
            SelectedColor = _availableColors[300];
            SelectedSize = _availableSizes[4];
            Debug.Assert(Instance == null);
            Instance = this;

            Brush_y_po = PART_SelectBrush.Margin.Top;
            Brush_y_po_init = PART_SelectBrush.Margin.Top;
            Menu_y_po = MENU.Margin.Top;
            Menu_y_po_init = Menu_y_po = MENU.Margin.Top;
        }

        #region Methods

        /// <summary>
        /// Called by LoadPopup after the user has chosen a file to load
        /// </summary>
        public void LoadingFinished()
        {
            LoadedImage = new WriteableBitmap(new BitmapImage(((LoadPopup)CurrentPopup).SelectedImage.Image));
            CurrentPopup = null;
            _imageUnsaved = false;
        }

        /// <summary>
        /// Called by ConfirmationPopup after the user has chosen OK or Cancel
        /// </summary>
        public void ConfirmationFinished()
        {
            ConfirmationPopup popup = (ConfirmationPopup)CurrentPopup;

            CurrentPopup = null;

            ActionAwaitingConfirmation action = (ActionAwaitingConfirmation)popup.UserData;

            switch (action)
            {
                case ActionAwaitingConfirmation.New:
                    if (popup.DidConfirm)
                    {
                        _imageUnsaved = false;
                        CreatePaintableImage();
                    }
                    break;
                case ActionAwaitingConfirmation.Load:
                    if(popup.DidConfirm)
                        CurrentPopup = new LoadPopup(this);
                    break;
                case ActionAwaitingConfirmation.Close:
                    if (popup.DidConfirm)
                        Close();
                    break;
            }
        }
        /// <summary>
        /// Call to hide the UI and begin painting on the canvas
        /// </summary>
        public void StartPainting()
        {
            if (_isTutorialActive)
            {
                return;
            }

            // Make the cursor passive so buttons and stuff don't respond to it
            PART_Cursor.Passive = true;

            // Draw at the current position and start checking for updates until done
            Point pos = PART_Cursor.GetPosition(PART_LoadedImageDisplay);
            Draw(pos, pos, null);
            _pastCursorPosition = pos;
            if (sensor == null)
                CompositionTarget.Rendering += ContinueDrawingStroke;
            else
                sensor.SkeletonFrameReady += ContinueDrawingStroke;
        }

        /// <summary>
        /// Call to show the UI and stop painting on the canvas
        /// </summary>
        public void StopPainting()
        {
            PART_Cursor.Passive = false;
            _imageUnsaved = true;
            if (sensor == null)
                CompositionTarget.Rendering -= ContinueDrawingStroke;
            else
                sensor.SkeletonFrameReady -= ContinueDrawingStroke;
        }

        #endregion

        #region Listbox Contents

        public IEnumerable<Color> AvailableColors { get { return _availableColors; } }
        private Color[] _availableColors = new Color[]
        {
           Colors.White,
           Colors.White,
           Colors.White,
           Colors.White,
           Colors.White,
           (Color)ColorConverter.ConvertFromString("#00000000"),
           (Color)ColorConverter.ConvertFromString("#14ff0000"),
           (Color)ColorConverter.ConvertFromString("#1eff0000"),
           (Color)ColorConverter.ConvertFromString("#28ff0000"),
           (Color)ColorConverter.ConvertFromString("#32ff0000"),
           (Color)ColorConverter.ConvertFromString("#3cff0000"),
           (Color)ColorConverter.ConvertFromString("#46ff0000"),
           (Color)ColorConverter.ConvertFromString("#50ff0000"),
           (Color)ColorConverter.ConvertFromString("#5aff0000"),
           (Color)ColorConverter.ConvertFromString("#64ff0000"),
           (Color)ColorConverter.ConvertFromString("#6eff0000"),
           (Color)ColorConverter.ConvertFromString("#78ff0000"),
           (Color)ColorConverter.ConvertFromString("#82ff0000"),
           (Color)ColorConverter.ConvertFromString("#8cff0000"),
           (Color)ColorConverter.ConvertFromString("#96ff0000"),
           (Color)ColorConverter.ConvertFromString("#a0ff0000"),
           (Color)ColorConverter.ConvertFromString("#aaff0000"),
           (Color)ColorConverter.ConvertFromString("#b4ff0000"),
           (Color)ColorConverter.ConvertFromString("#beff0000"),
           (Color)ColorConverter.ConvertFromString("#c8ff0000"),
           (Color)ColorConverter.ConvertFromString("#d2ff0000"),
           (Color)ColorConverter.ConvertFromString("#dcff0000"),
           (Color)ColorConverter.ConvertFromString("#e6ff0000"),
           (Color)ColorConverter.ConvertFromString("#f0ff0000"),
           (Color)ColorConverter.ConvertFromString("#faff0000"),
           (Color)ColorConverter.ConvertFromString("#ffff0000"),
           (Color)ColorConverter.ConvertFromString("#ffff0a00"),
           (Color)ColorConverter.ConvertFromString("#ffff1400"),
           (Color)ColorConverter.ConvertFromString("#ffff1e00"),
           (Color)ColorConverter.ConvertFromString("#ffff2800"),
           (Color)ColorConverter.ConvertFromString("#ffff3200"),
           (Color)ColorConverter.ConvertFromString("#ffff3c00"),
           (Color)ColorConverter.ConvertFromString("#ffff4600"),
           (Color)ColorConverter.ConvertFromString("#ffff5000"),
           (Color)ColorConverter.ConvertFromString("#ffff5a00"),
           (Color)ColorConverter.ConvertFromString("#ffff6400"),
           (Color)ColorConverter.ConvertFromString("#ffff6900"),
           (Color)ColorConverter.ConvertFromString("#ffff6e00"),
           (Color)ColorConverter.ConvertFromString("#ffff7300"),
           (Color)ColorConverter.ConvertFromString("#ffff7800"),
           (Color)ColorConverter.ConvertFromString("#ffff7d00"),
           (Color)ColorConverter.ConvertFromString("#ffff8200"),
           (Color)ColorConverter.ConvertFromString("#ffff8700"),
           (Color)ColorConverter.ConvertFromString("#ffff8c00"),
           (Color)ColorConverter.ConvertFromString("#ffff9100"),
           (Color)ColorConverter.ConvertFromString("#ffff9600"),
           (Color)ColorConverter.ConvertFromString("#ffff9b00"),
           (Color)ColorConverter.ConvertFromString("#ffffa000"),
           (Color)ColorConverter.ConvertFromString("#ffffa500"),
           (Color)ColorConverter.ConvertFromString("#ffffaa00"),
           (Color)ColorConverter.ConvertFromString("#ffffaf00"),
           (Color)ColorConverter.ConvertFromString("#ffffb400"),
           (Color)ColorConverter.ConvertFromString("#ffffb900"),
           (Color)ColorConverter.ConvertFromString("#ffffbe00"),
           (Color)ColorConverter.ConvertFromString("#ffffc300"),
           (Color)ColorConverter.ConvertFromString("#ffffc800"),
           (Color)ColorConverter.ConvertFromString("#ffffcd00"),
           (Color)ColorConverter.ConvertFromString("#ffffd200"),
           (Color)ColorConverter.ConvertFromString("#ffffd700"),
           (Color)ColorConverter.ConvertFromString("#ffffdc00"),
           (Color)ColorConverter.ConvertFromString("#ffffe100"),
           (Color)ColorConverter.ConvertFromString("#ffffe600"),
           (Color)ColorConverter.ConvertFromString("#ffffeb00"),
           (Color)ColorConverter.ConvertFromString("#fffff000"),
           (Color)ColorConverter.ConvertFromString("#fffff500"),
           (Color)ColorConverter.ConvertFromString("#fffffa00"),
           (Color)ColorConverter.ConvertFromString("#ffffff00"),///한줄 끝
           (Color)ColorConverter.ConvertFromString("#fffaff00"),
           (Color)ColorConverter.ConvertFromString("#fff0ff00"),
           (Color)ColorConverter.ConvertFromString("#ffe6ff00"),
           (Color)ColorConverter.ConvertFromString("#ffdcff00"),
           (Color)ColorConverter.ConvertFromString("#ffd2ff00"),
           (Color)ColorConverter.ConvertFromString("#ffc8ff00"),
           (Color)ColorConverter.ConvertFromString("#ffbeff00"),
           (Color)ColorConverter.ConvertFromString("#ffb4ff00"),
           (Color)ColorConverter.ConvertFromString("#ffaaff00"),
           (Color)ColorConverter.ConvertFromString("#ffa0ff00"),
           (Color)ColorConverter.ConvertFromString("#ff96ff00"),
           (Color)ColorConverter.ConvertFromString("#ff8cff00"),
           (Color)ColorConverter.ConvertFromString("#ff82ff00"),
           (Color)ColorConverter.ConvertFromString("#ff78ff00"),
           (Color)ColorConverter.ConvertFromString("#ff6eff00"),
           (Color)ColorConverter.ConvertFromString("#ff64ff00"),
           (Color)ColorConverter.ConvertFromString("#ff5aff00"),
           (Color)ColorConverter.ConvertFromString("#ff50ff00"),
           (Color)ColorConverter.ConvertFromString("#ff46ff00"),
           (Color)ColorConverter.ConvertFromString("#ff3cff00"),
           (Color)ColorConverter.ConvertFromString("#ff32ff00"),
           (Color)ColorConverter.ConvertFromString("#ff28ff00"),
           (Color)ColorConverter.ConvertFromString("#ff1eff00"),
           (Color)ColorConverter.ConvertFromString("#ff14ff00"),
           (Color)ColorConverter.ConvertFromString("#ff0aff00"),
           (Color)ColorConverter.ConvertFromString("#ff00ff00"),///두줄 끝
           (Color)ColorConverter.ConvertFromString("#ff00ff0a"),
           (Color)ColorConverter.ConvertFromString("#ff00ff14"),
           (Color)ColorConverter.ConvertFromString("#ff00ff1e"),
           (Color)ColorConverter.ConvertFromString("#ff00ff28"),
           (Color)ColorConverter.ConvertFromString("#ff00ff32"),
           (Color)ColorConverter.ConvertFromString("#ff00ff3c"),
           (Color)ColorConverter.ConvertFromString("#ff00ff46"),
           (Color)ColorConverter.ConvertFromString("#ff00ff50"),
           (Color)ColorConverter.ConvertFromString("#ff00ff5a"),
           (Color)ColorConverter.ConvertFromString("#ff00ff64"),
           (Color)ColorConverter.ConvertFromString("#ff00ff6e"),
           (Color)ColorConverter.ConvertFromString("#ff00ff78"),
           (Color)ColorConverter.ConvertFromString("#ff00ff82"),
           (Color)ColorConverter.ConvertFromString("#ff00ff8c"),
           (Color)ColorConverter.ConvertFromString("#ff00ff96"),
           (Color)ColorConverter.ConvertFromString("#ff00ffa0"),
           (Color)ColorConverter.ConvertFromString("#ff00ffaa"),
           (Color)ColorConverter.ConvertFromString("#ff00ffb4"),
           (Color)ColorConverter.ConvertFromString("#ff00ffbe"),
           (Color)ColorConverter.ConvertFromString("#ff00ffc8"),
           (Color)ColorConverter.ConvertFromString("#ff00ffd2"),
           (Color)ColorConverter.ConvertFromString("#ff00ffdc"),
           (Color)ColorConverter.ConvertFromString("#ff00ffe6"),
           (Color)ColorConverter.ConvertFromString("#ff00fff0"),
           (Color)ColorConverter.ConvertFromString("#ff00fffa"),
           (Color)ColorConverter.ConvertFromString("#FF00FFFF"),
           (Color)ColorConverter.ConvertFromString("#FF00F5FF"),
           (Color)ColorConverter.ConvertFromString("#FF00F0FF"),
           (Color)ColorConverter.ConvertFromString("#FF00EBFF"),
           (Color)ColorConverter.ConvertFromString("#FF00E6FF"),
           (Color)ColorConverter.ConvertFromString("#FF00E1FF"),
           (Color)ColorConverter.ConvertFromString("#FF00DCFF"),
           (Color)ColorConverter.ConvertFromString("#FF00D7FF"),
           (Color)ColorConverter.ConvertFromString("#FF00D2FF"),
           (Color)ColorConverter.ConvertFromString("#FF00CDFF"),
           (Color)ColorConverter.ConvertFromString("#FF00C8FF"),
           (Color)ColorConverter.ConvertFromString("#FF00C3FF"),
           (Color)ColorConverter.ConvertFromString("#FF00BEFF"),
           (Color)ColorConverter.ConvertFromString("#FF00B9FF"),
           (Color)ColorConverter.ConvertFromString("#FF00B4FF"),
           (Color)ColorConverter.ConvertFromString("#FF00AFFF"),
           (Color)ColorConverter.ConvertFromString("#FF00AAFF"),
           (Color)ColorConverter.ConvertFromString("#FF00A5FF"),
           (Color)ColorConverter.ConvertFromString("#FF00A0FF"),
           (Color)ColorConverter.ConvertFromString("#FF009BFF"),
           (Color)ColorConverter.ConvertFromString("#FF0096FF"),
           (Color)ColorConverter.ConvertFromString("#FF0091FF"),
           (Color)ColorConverter.ConvertFromString("#FF008CFF"),
           (Color)ColorConverter.ConvertFromString("#FF0087FF"),
           (Color)ColorConverter.ConvertFromString("#FF0082FF"),
           (Color)ColorConverter.ConvertFromString("#FF007DFF"),
           (Color)ColorConverter.ConvertFromString("#FF0078FF"),
           (Color)ColorConverter.ConvertFromString("#FF0073FF"),
           (Color)ColorConverter.ConvertFromString("#FF006EFF"),
           (Color)ColorConverter.ConvertFromString("#FF0069FF"),
           (Color)ColorConverter.ConvertFromString("#FF0064FF"),
           (Color)ColorConverter.ConvertFromString("#FF005FFF"),
           (Color)ColorConverter.ConvertFromString("#FF005AFF"),
           (Color)ColorConverter.ConvertFromString("#FF0055FF"),
           (Color)ColorConverter.ConvertFromString("#FF0050FF"),
           (Color)ColorConverter.ConvertFromString("#FF004BFF"),
           (Color)ColorConverter.ConvertFromString("#FF0046FF"),
           (Color)ColorConverter.ConvertFromString("#FF0041FF"),
           (Color)ColorConverter.ConvertFromString("#FF003CFF"),
           (Color)ColorConverter.ConvertFromString("#FF0037FF"),
           (Color)ColorConverter.ConvertFromString("#FF0032FF"),
           (Color)ColorConverter.ConvertFromString("#FF002DFF"),
           (Color)ColorConverter.ConvertFromString("#FF0028FF"),
           (Color)ColorConverter.ConvertFromString("#FF0023FF"),
           (Color)ColorConverter.ConvertFromString("#FF001EFF"),
           (Color)ColorConverter.ConvertFromString("#FF0019FF"),
           (Color)ColorConverter.ConvertFromString("#FF0014FF"),
           (Color)ColorConverter.ConvertFromString("#FF000FFF"),
           (Color)ColorConverter.ConvertFromString("#FF000AFF"),
           (Color)ColorConverter.ConvertFromString("#FF0005FF"),
           (Color)ColorConverter.ConvertFromString("#FF0000FF"),
           (Color)ColorConverter.ConvertFromString("#FF0000FF"),
           (Color)ColorConverter.ConvertFromString("#FF0500FF"),
           (Color)ColorConverter.ConvertFromString("#FF0A00FF"),
           (Color)ColorConverter.ConvertFromString("#FF0F00FF"),
           (Color)ColorConverter.ConvertFromString("#FF1400FF"),
           (Color)ColorConverter.ConvertFromString("#FF1900FF"),
           (Color)ColorConverter.ConvertFromString("#FF1E00FF"),
           (Color)ColorConverter.ConvertFromString("#FF2300FF"),
           (Color)ColorConverter.ConvertFromString("#FF2800FF"),
           (Color)ColorConverter.ConvertFromString("#FF2D00FF"),
           (Color)ColorConverter.ConvertFromString("#FF3200FF"),
           (Color)ColorConverter.ConvertFromString("#FF3700FF"),
           (Color)ColorConverter.ConvertFromString("#FF3C00FF"),
           (Color)ColorConverter.ConvertFromString("#FF4100FF"),
           (Color)ColorConverter.ConvertFromString("#FF4600FF"),
           (Color)ColorConverter.ConvertFromString("#FF4B00FF"),
           (Color)ColorConverter.ConvertFromString("#FF5000FF"),
           (Color)ColorConverter.ConvertFromString("#FF5500FF"),
           (Color)ColorConverter.ConvertFromString("#FF5A00FF"),
           (Color)ColorConverter.ConvertFromString("#FF5F00FF"),
           (Color)ColorConverter.ConvertFromString("#FF6400FF"),
           (Color)ColorConverter.ConvertFromString("#FF6900FF"),
           (Color)ColorConverter.ConvertFromString("#FF6E00FF"),
           (Color)ColorConverter.ConvertFromString("#FF7300FF"),
           (Color)ColorConverter.ConvertFromString("#FF7800FF"),
           (Color)ColorConverter.ConvertFromString("#FF7D00FF"),
           (Color)ColorConverter.ConvertFromString("#FF8200FF"),
           (Color)ColorConverter.ConvertFromString("#FF8700FF"),
           (Color)ColorConverter.ConvertFromString("#FF8C00FF"),
           (Color)ColorConverter.ConvertFromString("#FF9100FF"),
           (Color)ColorConverter.ConvertFromString("#FF9600FF"),
           (Color)ColorConverter.ConvertFromString("#FF9B00FF"),
           (Color)ColorConverter.ConvertFromString("#FFA000FF"),
           (Color)ColorConverter.ConvertFromString("#FFA500FF"),
           (Color)ColorConverter.ConvertFromString("#FFAA00FF"),
           (Color)ColorConverter.ConvertFromString("#FFAF00FF"),
           (Color)ColorConverter.ConvertFromString("#FFB400FF"),
           (Color)ColorConverter.ConvertFromString("#FFB900FF"),
           (Color)ColorConverter.ConvertFromString("#FFBE00FF"),
           (Color)ColorConverter.ConvertFromString("#FFC300FF"),
           (Color)ColorConverter.ConvertFromString("#FFC800FF"),
           (Color)ColorConverter.ConvertFromString("#FFCD00FF"),
           (Color)ColorConverter.ConvertFromString("#FFD200FF"),
           (Color)ColorConverter.ConvertFromString("#FFD700FF"),
           (Color)ColorConverter.ConvertFromString("#FFDC00FF"),
           (Color)ColorConverter.ConvertFromString("#FFE100FF"),
           (Color)ColorConverter.ConvertFromString("#FFE600FF"),
           (Color)ColorConverter.ConvertFromString("#FFEB00FF"),
           (Color)ColorConverter.ConvertFromString("#FFF000FF"),
           (Color)ColorConverter.ConvertFromString("#FFF500FF"),
           (Color)ColorConverter.ConvertFromString("#FFFA00FF"),
           (Color)ColorConverter.ConvertFromString("#FFFF00FF"),
           (Color)ColorConverter.ConvertFromString("#FFFF00FF"),
           (Color)ColorConverter.ConvertFromString("#FFFF00FA"),
           (Color)ColorConverter.ConvertFromString("#FFFF00F5"),
           (Color)ColorConverter.ConvertFromString("#FFFF00F0"),
           (Color)ColorConverter.ConvertFromString("#FFFF00EB"),
           (Color)ColorConverter.ConvertFromString("#FFFF00E6"),
           (Color)ColorConverter.ConvertFromString("#FFFF00E1"),
           (Color)ColorConverter.ConvertFromString("#FFFF00DC"),
           (Color)ColorConverter.ConvertFromString("#FFFF00D7"),
           (Color)ColorConverter.ConvertFromString("#FFFF00D2"),
           (Color)ColorConverter.ConvertFromString("#FFFF00CD"),
           (Color)ColorConverter.ConvertFromString("#FFFF00C8"),
           (Color)ColorConverter.ConvertFromString("#FFFF00C3"),
           (Color)ColorConverter.ConvertFromString("#FFFF00BE"),
           (Color)ColorConverter.ConvertFromString("#FFFF00B9"),
           (Color)ColorConverter.ConvertFromString("#FFFF00B4"),
           (Color)ColorConverter.ConvertFromString("#FFFF00AF"),
           (Color)ColorConverter.ConvertFromString("#FFFF00AA"),
           (Color)ColorConverter.ConvertFromString("#FFFF00A5"),
           (Color)ColorConverter.ConvertFromString("#FFFF00A0"),
           (Color)ColorConverter.ConvertFromString("#FFFF009B"),
           (Color)ColorConverter.ConvertFromString("#FFFF0096"),
           (Color)ColorConverter.ConvertFromString("#FFFF0091"),
           (Color)ColorConverter.ConvertFromString("#FFFF008C"),
           (Color)ColorConverter.ConvertFromString("#FFFF0087"),
           (Color)ColorConverter.ConvertFromString("#FFFF0082"),
           (Color)ColorConverter.ConvertFromString("#FFFF007D"),
           (Color)ColorConverter.ConvertFromString("#FFFF0078"),
           (Color)ColorConverter.ConvertFromString("#FFFF0073"),
           (Color)ColorConverter.ConvertFromString("#FFFF006E"),
           (Color)ColorConverter.ConvertFromString("#FFFF0069"),
           (Color)ColorConverter.ConvertFromString("#FFFF0064"),
           (Color)ColorConverter.ConvertFromString("#FFFF005F"),
           (Color)ColorConverter.ConvertFromString("#FFFF005A"),
           (Color)ColorConverter.ConvertFromString("#FFFF0055"),
           (Color)ColorConverter.ConvertFromString("#FFFF0050"),
           (Color)ColorConverter.ConvertFromString("#FFFF004B"),
           (Color)ColorConverter.ConvertFromString("#FFfa004B"),
           (Color)ColorConverter.ConvertFromString("#FFf0004B"),
           (Color)ColorConverter.ConvertFromString("#FFe6004B"),
           (Color)ColorConverter.ConvertFromString("#FFdc004B"),
           (Color)ColorConverter.ConvertFromString("#FFd2004B"),
           (Color)ColorConverter.ConvertFromString("#FFc8004B"),
           (Color)ColorConverter.ConvertFromString("#FFbe004B"),
           (Color)ColorConverter.ConvertFromString("#FFb4004B"),
           (Color)ColorConverter.ConvertFromString("#FFaa004B"),
           (Color)ColorConverter.ConvertFromString("#FFa0004B"),
           (Color)ColorConverter.ConvertFromString("#FF96004B"),
           (Color)ColorConverter.ConvertFromString("#FF8c004B"),
           (Color)ColorConverter.ConvertFromString("#FF82004B"),
           (Color)ColorConverter.ConvertFromString("#FF78004B"),
           (Color)ColorConverter.ConvertFromString("#FF6e004B"),
           (Color)ColorConverter.ConvertFromString("#FF64004B"),
           (Color)ColorConverter.ConvertFromString("#FF5a004B"),
           (Color)ColorConverter.ConvertFromString("#FF50004B"),
           (Color)ColorConverter.ConvertFromString("#FF46004B"),
           (Color)ColorConverter.ConvertFromString("#FF3c004B"),
           (Color)ColorConverter.ConvertFromString("#FF32004B"),
           (Color)ColorConverter.ConvertFromString("#FF28004B"),
           (Color)ColorConverter.ConvertFromString("#FF1e004B"),
           (Color)ColorConverter.ConvertFromString("#FF14004B"),
           (Color)ColorConverter.ConvertFromString("#FF0a004B"),
           (Color)ColorConverter.ConvertFromString("#FF00004B"),
           (Color)ColorConverter.ConvertFromString("#FF000041"),
           (Color)ColorConverter.ConvertFromString("#FF00003c"),
           (Color)ColorConverter.ConvertFromString("#FF000037"),
           (Color)ColorConverter.ConvertFromString("#FF000032"),
           (Color)ColorConverter.ConvertFromString("#FF00002d"),
           (Color)ColorConverter.ConvertFromString("#FF000028"),
           (Color)ColorConverter.ConvertFromString("#FF000023"),
           (Color)ColorConverter.ConvertFromString("#FF00001e"),
           (Color)ColorConverter.ConvertFromString("#FF000019"),
           (Color)ColorConverter.ConvertFromString("#FF000014"),
           (Color)ColorConverter.ConvertFromString("#FF00000f"),
           (Color)ColorConverter.ConvertFromString("#FF00000a"),
           (Color)ColorConverter.ConvertFromString("#FF000005"),
           (Color)ColorConverter.ConvertFromString("#FF000000")
        };

        public IEnumerable<double> AvailableSizes { get { return _availableSizes; } }
        private double[] _availableSizes = new double[]
        {
            1.0,
            2.0,
            3.0,
            5.0,
            8.0,
            10.0,
            15.0,
            20.0,
            25.0,
            30.0,
            40.0,
            50.0
        };

        public IEnumerable<BrushSelection> AvailableBrushes { get { return _availableBrushes; } }
        private BrushSelection[] _availableBrushes = new BrushSelection[]
        {
            new BrushSelection(
                new Uri("/KinectPaint;component/Resources/brush_pen.png", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/brush_pen.png", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Marker, 
                ""),
            new BrushSelection(
                new Uri("/KinectPaint;component/Resources/brush_pastel.png", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/brush_pastel.png", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Pastel,
                ""),

                  new BrushSelection(new Uri("/KinectPaint;component/Resources/brush_light.png", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/brush_light.png", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Light,""),

                 new BrushSelection(new Uri("/KinectPaint;component/Resources/brush_butterfly.png", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/brush_butterfly.png", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Butterfly,""),

                 new BrushSelection(new Uri("/KinectPaint;component/Resources/brush_spider.png", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/brush_spider.png", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Spider,""),
                
                new BrushSelection(new Uri("/KinectPaint;component/Resources/", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Test2,""),

 

        new BrushSelection(new Uri("/KinectPaint;component/Resources/", UriKind.RelativeOrAbsolute), 
                new Uri("/KinectPaint;component/Resources/", UriKind.RelativeOrAbsolute), 
                KinectPaintbrush.Test4,"Test4")

            //new BrushSelection(
            //    new Uri("/KinectPaint;component/Resources/brush-unselected.png", UriKind.RelativeOrAbsolute), 
            //    new Uri("/KinectPaint;component/Resources/brush-selected.png", UriKind.RelativeOrAbsolute), 
            //    KinectPaintbrush.Brush, 
            //    "brush"),
            //new BrushSelection(
            //    new Uri("/KinectPaint;component/Resources/eraser-unselected.png", UriKind.RelativeOrAbsolute), 
            //    new Uri("/KinectPaint;component/Resources/eraser-selected.png", UriKind.RelativeOrAbsolute), 
            //    KinectPaintbrush.Eraser, 
            //    "eraser")

        };

        #endregion

        #region Window Properties

        /// <summary>
        /// Gets the Kinect runtime object
        /// </summary>
        public KinectSensor sensor { get; private set; }

        #region SelectedColor

        /// <summary>
        /// The <see cref="SelectedColor" /> dependency property's name.
        /// </summary>
        public const string SelectedColorPropertyName = "SelectedColor";

        /// <summary>
        /// Gets or sets the value of the currently selected color.
        /// This is a dependency property.
        /// </summary>
        public Color SelectedColor
        {
            get 
            {
                return (Color)GetValue(SelectedColorProperty); 
            }
            set { SetValue(SelectedColorProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedColor" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor",
            typeof(Color),
            typeof(MainWindow),
            new UIPropertyMetadata(null));

        #endregion

        #region SelectedSize

        /// <summary>
        /// The <see cref="SelectedSize" /> dependency property's name.
        /// </summary>
        public const string SelectedSizePropertyName = "SelectedSize";

        /// <summary>
        /// Gets or sets the value of the currently selected size.
        /// This is a dependency property.
        /// </summary>
        public double SelectedSize
        {
            get
            {
               
                return (double)GetValue(SelectedSizeProperty);
            }
            set
            {
                SetValue(SelectedSizeProperty, value);

            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedSize" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedSizeProperty = DependencyProperty.Register(
            SelectedSizePropertyName,
            typeof(double),
            typeof(MainWindow),
            new UIPropertyMetadata(0.0));

        #endregion

        #region SelectedBrush

        /// <summary>
        /// The <see cref="SelectedBrush" /> dependency property's name.
        /// </summary>
        public const string SelectedBrushPropertyName = "SelectedBrush";

        /// <summary>
        /// Gets or sets the value of the currently selected brush.
        /// This is a dependency property.
        /// </summary>
        public BrushSelection SelectedBrush
        {
            get
            {
                return (BrushSelection)GetValue(SelectedBrushProperty);
            }
            set
            {
                SetValue(SelectedBrushProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedBrush" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(
            SelectedBrushPropertyName,
            typeof(BrushSelection),
            typeof(MainWindow),
            new UIPropertyMetadata(null));

        #endregion

        #region ShowCamera

        /// <summary>
        /// The <see cref="ShowCamera" /> dependency property's name.
        /// </summary>
        public const string ShowCameraPropertyName = "ShowCamera";

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowCamera" />
        /// property. This is a dependency property.
        /// </summary>
        public bool ShowCamera
        {
            get
            {
                return (bool)GetValue(ShowCameraProperty);
            }
            set
            {
                SetValue(ShowCameraProperty, value);                                
            }
        }

        /// <summary>
        /// Identifies the <see cref="ShowCamera" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowCameraProperty = DependencyProperty.Register(
            ShowCameraPropertyName,
            typeof(bool),
            typeof(MainWindow),
            new UIPropertyMetadata(
                false,
                (s, e) =>
                {
                    var win = (MainWindow)s;

                    if (win.sensor == null)
                    { 
                        return;
                        
                    }
                    //if ((bool)e.NewValue)
                    //{
                    //    win.CameraVisible = true; 
                    //}
                    //else
                    //{
                    //    if (win.sensor == null)
                    //    {
                    //        return;
                    //    }

                    ////    win.CameraVisible = false; 
                    ////    win.PART_KinectVideo.Source = null;
                    //}
                }));

        #endregion

        #region LoadedImage

        /// <summary>
        /// Path to the currently loaded image
        /// </summary>
        public WriteableBitmap LoadedImage
        {
            get { return _loadedImage; }
            set
            {
                _loadedImage = value;

                PART_LoadedImageDisplay.Source = _loadedImage;
            }
        }
        private WriteableBitmap _loadedImage;

        #endregion

        #region CurrentPopup

        /// <summary>
        /// The current popup (load dialog, or confirmation dialog, or tutorial)
        /// </summary>
        public object CurrentPopup
        {
            get { return _currentPopup; }
            set
            {
                _currentPopup = value;

                PART_PopupDisplay.Content = _currentPopup;
            }
        }
        private object _currentPopup;

        #endregion

        #endregion

        #region Button Handlers

        // Called when the user presses the 'New' button
        private void OnNew(object sender, RoutedEventArgs args)
        {
            if (_imageUnsaved)
                CurrentPopup = new ConfirmationPopup("그림이 저장되지 않았습니다!" + Environment.NewLine + "저장하지 않고 새 창을 띄우시겠습니까?", ActionAwaitingConfirmation.New, this);
            else
            {
                _imageUnsaved = false;
                CreatePaintableImage();
            }
        }

        // Called when the user presses the 'Save' button
        private void OnSave(object sender, RoutedEventArgs args)
        {
            LoadedImage.Save(Path.Combine(App.PhotoFolder, "키넥트그림" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".png"), ImageFormat.Png);

            _imageUnsaved = false;

            // 3초 동안 opacity 1.0~ 0.0 으로 애니메이셔닝
            DoubleAnimation saveMessageAnimator = new DoubleAnimation(1.0, 1.0, new Duration(TimeSpan.FromSeconds(2)) , FillBehavior.Stop);
            PART_SaveMessage.BeginAnimation(OpacityProperty, saveMessageAnimator);
        }

        // Called when the user presses the 'Load' button
        private void OnLoad(object sender, RoutedEventArgs args)
        {
            if (_imageUnsaved)
                CurrentPopup = new ConfirmationPopup("그림이 저장되지 않았습니다!" + Environment.NewLine + "저장하지 않고 다른 그림을 불러오시겠습니까?", ActionAwaitingConfirmation.Load, this);
            else
                CurrentPopup = new LoadPopup(this);
        }

        // Called when the user presses the 'Quit' button
        private void OnQuit(object sender, RoutedEventArgs args)
        {
            if (_imageUnsaved)
                CurrentPopup = new ConfirmationPopup("그림이 저장되지 않았습니다!"+Environment.NewLine+"저장하지 않고 종료하시겠습니까?", ActionAwaitingConfirmation.Close, this);
            else
                Close();
        }

        #endregion

        #region Internal

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up the color picker's initial state
            _colorpicker = (FocusingStackPanel)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(PART_ColorPickerListBox, 0), 0), 0);


            _colorpicker.FocusedQuantity = 1;

            try
            {
                if (KinectSensor.KinectSensors.Count > 0)
                {
                    //grab first
                    sensor = KinectSensor.KinectSensors[0];
                }

                if (sensor.Status != KinectStatus.Connected || KinectSensor.KinectSensors.Count == 0)
                {
                    MessageBox.Show("No Kinect connected!"); 
                }

                // Set up the Kinect
                
                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 0.3f,
                    Correction = 0.0f,
                    Prediction = 0.0f,
                    JitterRadius = 1.0f,
                    MaxDeviationRadius = 0.5f
                };

                sensor.SkeletonStream.Enable(parameters);
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.Start();           
            }
            catch (Exception)
            {
                // Failed to set up the Kinect. Show the error onscreen (app will switch to using mouse movement)
                sensor = null;
                PART_ErrorText.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(sensor != null)
                if (sensor.IsRunning)
                {
                    sensor.Stop(); 
                }
            Environment.Exit(0);
        }

        void ContinueDrawingStroke(object sender, EventArgs e)
        {
            Point pos = PART_Cursor.GetPosition(PART_LoadedImageDisplay);
            Point prev = PART_Cursor.GetPreviousPosition(PART_LoadedImageDisplay);
            Draw(prev, pos, _pastCursorPosition);
            _pastCursorPosition = prev;
        }

        private void CreatePaintableImage()
        {
            LoadedImage = new WriteableBitmap(
                (int)PART_PaintCanvas.ActualWidth,
                (int)PART_PaintCanvas.ActualHeight,
                96.0,
                96.0,
                PixelFormats.Pbgra32,
                null);
        }
        private void Draw(Point from, Point to, Point? past)
        {
            switch (SelectedBrush.Brush)
            {
                case KinectPaintbrush.Eraser:
                    BitmapHelpers.Erase(
                        LoadedImage,
                        from, to,
                        (int)SelectedSize);
                    break;
                case KinectPaintbrush.Marker:
                    BitmapHelpers.Brush(
                        LoadedImage,
                        from, to, past,
                        Color.FromArgb(128, SelectedColor.R, SelectedColor.G, SelectedColor.B),
                        (int)SelectedSize);
                    break;
              
                case KinectPaintbrush.Brush:
                    BitmapHelpers.Brush(
                        LoadedImage,
                        from, to, past,
                        SelectedColor,
                        (int)SelectedSize);
                    break;
                case KinectPaintbrush.Pastel:
                    BitmapHelpers.PastelBrush(
                        LoadedImage,
                        from, to,
                        SelectedColor,
                        (int)SelectedSize * 2);
                    break;
                case KinectPaintbrush.Light:
                     BitmapHelpers.LightBrush(
                        LoadedImage,
                        from, to,
                        SelectedColor,
                        (int)SelectedSize*2);
                    break;
                case KinectPaintbrush.Spider:
                    BitmapHelpers.Spider(
                       LoadedImage,
                       from, to, past,
                       SelectedColor,
                       (int)SelectedSize * 2);
                    break;

                case KinectPaintbrush.Test2:
                    BitmapHelpers.Test2(
                       LoadedImage,
                       from, to,
                       SelectedColor,
                       (int)SelectedSize * 2);
                    break;
                case KinectPaintbrush.Butterfly:
                    BitmapHelpers.Butterfly(
                       LoadedImage,
                       from, to,
                       SelectedColor,
                       (int)SelectedSize * 2);
                    break;
                case KinectPaintbrush.Test4:
                    BitmapHelpers.Test4(
                       LoadedImage,
                       from, to,
                       SelectedColor,
                       (int)SelectedSize * 2);
                    break;

            }
        }

        // Called when a new video frame is ready from the Kinect and the option to display it is turned on.
        public void NuiRuntime_VideoFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                if (ShowCamera)
                {
                    using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                    {
                        if (colorFrame == null)
                        {
                            return;
                        }
                        PART_KinectVideo.Source = colorFrame.ToBitmapSource();
                    }
                }
                else
                    PART_KinectVideo.Source = null;
            }
            catch
            {
                
            }

        }

        private void KinectPaintListBox_CursorEnter(object sender, CursorEventArgs e)
        {
            _isPickingColor = true;

            PART_ColorPickerListBox.Opacity = 1;
        }

        private void KinectPaintListBox_CursorLeave(object sender, CursorEventArgs e)
        {
            PART_ColorPickerListBox.Opacity = 0.1;
            SolidColorBrush brush = new SolidColorBrush((Color)GetValue(MainWindow.SelectedColorProperty));
            PART_Cursor.PART_TEMPO.Fill = brush;
            _isPickingColor = false;
        }
        
        private void PART_PaintCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreatePaintableImage();
        }

        #endregion

        private void ViewColorchange_Click(object sender, RoutedEventArgs e)
        {
            PART_ColorPickerListBox.Visibility = Visibility.Visible;
            PART_SelectSize.Visibility = Visibility.Visible;           
        }

        private void ViewBruchchange_Click(object sender, RoutedEventArgs e)
        {
            PART_SelectBrush.Visibility = Visibility.Visible;
        }

        private void TimerSetting()
        {
            Mytimer.Interval = new TimeSpan(1000);
            Mytimer.Tick += new EventHandler(Menu_move);
            Mytimer.Start();
        }

        private void TimerSetting2()
        {
            Mytimer2.Interval = new TimeSpan(1000);
            Mytimer2.Tick += new EventHandler(Brush_move);
            Mytimer2.Start();
        }

        void Menu_move(object sender, EventArgs e)
        {
            if (Menu_on == true)
            {
                if (Menu_y_po > (Menu_y_po_init+420))
                {
                    Menu_y_po = (Menu_y_po_init + 420);
                    MENU.Margin = new Thickness(0, Menu_y_po, 0, 0);
                    Mytimer.Stop();
                    Mytimer.Tick -= Menu_move;
                }
                else
                {
                    Menu_y_po += 0.3;
                    MENU.Margin = new Thickness(0, Menu_y_po, 0, 0);
                }
            }

            else
            {
                if (Menu_y_po < Menu_y_po_init)
                {
                    Menu_y_po = Menu_y_po_init;
                    MENU.Margin = new Thickness(0, Menu_y_po, 0, 0);
                    Mytimer.Stop();
                    Mytimer.Tick -= Menu_move;
                }
                else
                {
                    Menu_y_po -= 0.3;
                    MENU.Margin = new Thickness(0, Menu_y_po, 0, 0);
                }
            }
        }

        void Brush_move(object sender, EventArgs e)
        {
            if (Brush_on == true)
            {
                if (Brush_y_po < (Brush_y_po_init-450))
                {
                    Brush_y_po = (Brush_y_po_init -450);

                    PART_SelectBrush.Margin = new Thickness(0, Brush_y_po, 0, 0);

                    Mytimer2.Stop();
                    Mytimer2.Tick -= Brush_move;
                }
                else
                {
                    Brush_y_po -= 0.3;
                    PART_SelectBrush.Margin = new Thickness(0, Brush_y_po, 0, 0);
                }
            }
            else
            {
                if (Brush_y_po > Brush_y_po_init)
                {
                    Brush_y_po = Brush_y_po_init;
                    PART_SelectBrush.Margin = new Thickness(0, Brush_y_po, 0, 0);
                    Mytimer2.Stop();
                    Mytimer2.Tick -= Brush_move;
                }
                else
                {
                    Brush_y_po += 0.3;
                    PART_SelectBrush.Margin = new Thickness(0, Brush_y_po, 0, 0);
                }
            }
        }
        private void PART_SelectSize_MouseEnter(object sender, CursorEventArgs e)
        {
            PART_SelectSize.Opacity = 1;
        }

        private void PART_SelectSize_MouseLeave(object sender, CursorEventArgs e)
        {
            PART_SelectSize.Opacity = 0.1;
            PART_Cursor.PART_TEMPO.Width = (double)GetValue(SelectedSizeProperty) * 2.0;
            PART_Cursor.PART_TEMPO.Height = (double)GetValue(SelectedSizeProperty) * 2.0;
        }

        private void MenuPanel_MouseEnter(object sender, CursorEventArgs e)
        {
            Menu_on = true;
            Image_Menu.Visibility = (Visibility.Hidden);
            TimerSetting();
        }

        private void MenuPanel_MouseLeave(object sender, CursorEventArgs e)
        {
            Menu_on = false;
            TimerSetting();
            Image_Menu.Visibility = (Visibility.Visible);
        }


        private void BrushPanel_MouseEnter(object sender, CursorEventArgs e)
        {
            Brush_on = true;
            TimerSetting2();
            // MessageBox.Show("Asdf");
        }

        private void BrushPanel_MouseLeave(object sender, CursorEventArgs e)
        {
            Brush_on = false;
            TimerSetting2();
        }
    }
}