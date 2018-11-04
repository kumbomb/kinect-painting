================================================================================

KINECTPAINT – READ ME
Copyright © Microsoft Corporation. 
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
=============================
OVERVIEW  

This paint sample allows you to paint on a canvas using a pen, paintbrush, airbrush, and eraser.
It allows you to select different brush sizes and colors, and to save and load.

Use your right hand to control the cursor. Hover over a button for a second to press it.

To paint, hold your left hand up above your shoulder. 

If the Kinect is not plugged in when you start the app, it will use the mouse instead.


=============================
SAMPLE LANGUAGE IMPLEMENTATIONS     
 
This sample is available in the following language implementations:
     C#
	 Visual Basic

=============================
FILES   

VisibleIfPresentConverter.cs - Converts a non-null value into Visibility.Visible, and a null value into Visibility.Collapsed
ConfirmationPopup.xaml - The popup dialog giving the user a yes/no option
LoadPopup.xaml - The popup dialog displaying the 10 most recent images created with KinectPaint
CursorStyles.xaml - A collection of styles relating to the cursor
GlobalStyles.xaml - A collection of styles used across the entire app UI
TutorialStyles.xaml - Styles used by the tutorial page
App.xaml - The basic application class
ArchivedImage.cs - Represents an image saved in My Pictures/KinectPaint that can be loaded
BitmapHelpers.cs - A set of helper functions for painting to a WriteableBitmap
BrushSelection.cs - Represents a brush that can be selected to paint with
CursorEventArgs.cs - Defines event parameters for the KinectCursor.CursorEnter and KinectCursor.CursorLeave events
FocusingStackPanel.cs - A panel similar to StackPanel that gives all children equal space except the "focused" object
KinectCursor.xaml - The cursor control that displays on the screen. Also responsible for firing CursorEnter and CursorLeave events
KinectPaintButton.cs - Derivative of Button that activates when the Kinect cursor hovers over it.
KinectPaintCheckBox.cs - Derivative of CheckBox that toggles when the Kinect cursor hovers over it.
KinectPaintListBox.cs - Derivative of ListBox that selects an item when the Kinect cursor hovers over it.
KinectPaintListBoxItem.cs - Derivative of ListBoxItem that allows KinectPaintListBox to work.
MainWindow.xaml - Top-level application UI and interaction logic
Tutorial.xaml - The tutorial UI


=============================
PREREQUISITES   

None


=============================
BUILDING THE SAMPLE   

To build the sample using the command prompt:
---------------------------------------------
1. Open the Command Prompt window and navigate to the KinectPaint directory.
2. Type msbuild KinectPaint.sln

To build the sample using Visual Studio (preferred method):
-----------------------------------------------------------
1. In Windows Explorer, navigate to the KinectPaint directory.
2. Double-click the icon for the .sln (solution) file to open the file in Visual Studio.
3. In the Build menu, select Build Solution. The application will be built in the default \Debug or \Release directory.

=============================
RUNNING THE SAMPLE   
 
To run the sample:
------------------
1. Navigate to the directory that contains the new executable, using the command prompt or Windows Explorer.
2. Type KinectPaint.exe at the command line, or double-click the icon for KinectPaint to launch it from Windows Explorer.
