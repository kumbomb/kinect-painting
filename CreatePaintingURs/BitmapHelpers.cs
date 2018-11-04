// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Threading;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    public static class BitmapHelpers
    {
        #region Static Data

        static int AirbrushDots = 24;
        static int AirbrushDotRadius = 9;
        static int temp;
        private static byte[][] AirbrushDotKernel;

        static Point[] mypoint = new Point[10000];
        static int js=0;

        #endregion

        #region Definitions

        /// <summary>
        /// Represents a bounding box, used here for determining which pixels to consider when processing line segments
        /// </summary>
        private struct BoundingBox
        {
            #region Properties

            /// <summary>
            /// Gets whether the bounding box is bounding any content
            /// </summary>
            public bool HasContent { get; private set; }

            /// <summary>
            /// The left edge of the bounding box
            /// </summary>
            public int Left { get; private set; }

            /// <summary>
            /// The right edge of the bounding box
            /// </summary>
            public int Right { get; private set; }

            /// <summary>
            /// The top edge of the bounding box
            /// </summary>
            public int Top { get; private set; }

            /// <summary>
            /// The bottom edge of the bounding box
            /// </summary>
            public int Bottom { get; private set; }

            /// <summary>
            /// The width of the bounding box
            /// </summary>
            public int Width { get; private set; }

            /// <summary>
            /// The height of the bounding box
            /// </summary>
            public int Height { get; private set; }


            #endregion

            #region Methods

            /// <summary>
            /// Adds a point that the bounding box must contain
            /// </summary>
            /// <param name="x">X coordinate of the point</param>
            /// <param name="y">Y coordinate of the point</param>
            /// <param name="radius">Radius of the point</param>
            public void AddPoint(int x, int y, int radius)
            {
                if (!HasContent)
                {
                    Left = x - radius;
                    Right = x + radius + 1;
                    Top = y - radius;
                    Bottom = y + radius + 1;
                    Width = Height = 2 * radius;
                    HasContent = true;
                }
                else
                {
                    if (x - radius < Left)
                        Left = x - radius;
                    if (x + radius + 1 > Right)
                        Right = x + radius + 1;
                    if (y - radius < Top)
                        Top = y - radius;
                    if (y + radius + 1 > Bottom)
                        Bottom = y + radius + 1;
                    Width = Right - Left;
                    Height = Bottom - Top;
                }
            }

            /// <summary>
            /// Clips this bounding box against a larger bounding region
            /// </summary>
            /// <param name="clipregion">The bounding box of the clipping region</param>
            public void Clip(BoundingBox clipregion)
            {
                if (Left < clipregion.Left)
                    Left = clipregion.Left;
                if (Left > clipregion.Right)
                    Left = clipregion.Right;
                if (Right < clipregion.Left)
                    Right = clipregion.Left;
                if (Right > clipregion.Right)
                    Right = clipregion.Right;
                if (Top < clipregion.Top)
                    Top = clipregion.Top;
                if (Top > clipregion.Bottom)
                    Top = clipregion.Bottom;
                if (Bottom < clipregion.Top)
                    Bottom = clipregion.Top;
                if (Bottom > clipregion.Bottom)
                    Bottom = clipregion.Bottom;
                Width = Right - Left;
                Height = Bottom - Top;
            }

            #endregion
        }

        /// <summary>
        /// Represents a line segment, and optimizes the calculation of its nearest distance to multiple points
        /// </summary>
        private struct LineSegment
        {
            #region Data

            int fromx, fromy, tox, toy;     // Stores the 'to' and 'from' points as integers

            int bx, by, bxx, bxy, byy, L2;  // Stores intermediate calculations, so they aren't needlessly repeated

            #endregion

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="_from">The starting point of the line segment</param>
            /// <param name="_to">The ending point of the line segment</param>
            public LineSegment(Point _from, Point _to)
            {
                fromx = (int)_from.X;
                fromy = (int)_from.Y;
                tox = (int)_to.X;
                toy = (int)_to.Y;
                bx = tox - fromx;
                by = toy - fromy;
                bxx = bx * bx;
                bxy = bx * by;
                byy = by * by;
                L2 = bxx + byy;
            }

            #region Methods

            /// <summary>
            /// Returns the squared distance between the line segment and the point represented by x,y
            /// </summary>
            /// <param name="x">X coordinate of the point</param>
            /// <param name="y">Y coordinate of the point</param>
            /// <returns></returns>
            public int DistanceSquared(int x, int y)
            {
                int nx, ny;

                int ay = y - fromy;
                int ax = x - fromx;

                // Compute the point along the line nearest to the current pixel (represented by nx,ny)                    
                if(L2 != 0)
                {
                    nx = (bxx * ax + bxy * ay) / L2;
                    ny = (bxy * ax + byy * ay) / L2;

                    // If the nearest point lies beyond the 'to' point, snap n to 'to'
                    if (nx * nx + ny * ny > L2)
                    {
                        nx = bx;
                        ny = by;
                    }
                    else
                    {
                        int bnx = bx - nx;
                        int bny = by - ny;

                        // If the nearest point lies before the 'from' point, snap n to 'from'
                        if (bnx * bnx + bny * bny > L2)
                            nx = ny = 0;
                    }
                }
                else
                    nx = ny = 0;

                // Return the squared distance between n and the given point
                int dx = nx - ax;
                int dy = ny - ay;

                return dx * dx + dy * dy;
            }

            /// <summary>
            /// Interpolates along the line segment
            /// </summary>
            /// <param name="i">The step index to interpolate to</param>
            /// <param name="steps">The number of steps</param>
            /// <param name="x">The returned x coordinate</param>
            /// <param name="y">The returned y coordinate</param>
            public void Interpolate(int i, int steps, out int x, out int y)
            {
                x = fromx + bx * i / steps;
                y = fromy + by * i / steps;
            }

            #endregion
        }

        #endregion

        #region Constructor

        static BitmapHelpers()
        {
            AirbrushDotKernel = new byte[2 * AirbrushDotRadius + 1][];
            
            for(int i = 0;i < 2 * AirbrushDotRadius + 1;i++)
              AirbrushDotKernel[i] = new byte[2 * AirbrushDotRadius + 1];

            int x,y;
            for(int i = 0;i < AirbrushDotKernel.Length;i++)
            {
                x = i - AirbrushDotRadius;
                for (int j = 0; j < AirbrushDotKernel.Length; j++)
                {
                    y = j - AirbrushDotRadius;
                    AirbrushDotKernel[i][j] = (byte)(128 - (int)(128.0 * Math.Min(1, Math.Sqrt(x * x + y * y) / (double)AirbrushDotRadius)));
                }
            }
        }

        #endregion

        public static unsafe void Erase(WriteableBitmap bmp, Point from, Point to, int size)
        {
            if (bmp == null) return;

            bmp.Lock();
            int sizesize = size * size;
            LineSegment linesegment = new LineSegment(from, to);
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size);
            segmentbounds.Clip(bitmapbounds);
            Int32* start = (Int32*)bmp.BackBuffer.ToPointer();

            start += segmentbounds.Left;

            for (int y = segmentbounds.Top; y < segmentbounds.Bottom; y++)
            {
                Int32* pixel = start + bmp.BackBufferStride / sizeof(Int32) * y;

                for (int x = segmentbounds.Left; x < segmentbounds.Right; x++)
                {
                    if (linesegment.DistanceSquared(x, y) <= sizesize)
                        *pixel = 0;

                    pixel++;
                }
            }

            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        public static unsafe void Brush(WriteableBitmap bmp, Point from, Point to, Point? previous, Color color, int size)
        {
            if (bmp == null) return;

            
            bmp.Lock();
            int sizesize = size * size;
            uint flatcolor = (uint)((int)color.A << 24) + (uint)((int)color.R << 16) + (uint)((int)color.G << 8) + color.B;

            LineSegment linesegment = new LineSegment(from, to);
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();

            start += segmentbounds.Left;

            if (previous.HasValue)
            {
                LineSegment previoussegment = new LineSegment(previous.Value, from);

                for (int y = segmentbounds.Top; y < segmentbounds.Bottom; y++)
                {
                    UInt32* pixel = start + bmp.BackBufferStride / sizeof(UInt32) * y;

                    for (int x = segmentbounds.Left; x < segmentbounds.Right; x++)
                    {
                        if (linesegment.DistanceSquared(x, y) <= sizesize && previoussegment.DistanceSquared(x, y) > sizesize)
                        {
                            if (color.A == 255)
                                *pixel = flatcolor;
                            else
                                WriteAlphaBlended(pixel, color);
                        }

            
                        pixel++;
                    }
                }
            }
            else
            {
             
                for (int y = segmentbounds.Top; y < segmentbounds.Bottom; y++)
                {
                    UInt32* pixel = start + bmp.BackBufferStride / sizeof(UInt32) * y;

                    for (int x = segmentbounds.Left; x < segmentbounds.Right; x++)
                    {
                        if (linesegment.DistanceSquared(x, y) <= sizesize)
                        {
                            if (color.A == 255)
                                *pixel = flatcolor;
                            else
                                WriteAlphaBlended(pixel, color);
                        }

                 
                        pixel++;
                    }
                }
            }



            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

    
        public static unsafe void PastelBrush(WriteableBitmap bmp, Point from, Point to, Color color, int size)
        {
            Random r = new Random();
            AirbrushDots = 500;
            AirbrushDotRadius = 2;

            if (bmp == null) return;

            bmp.Lock();

            
            LineSegment segment = new LineSegment(from, to);

            
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size + AirbrushDotRadius);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size + AirbrushDotRadius);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();
            int stride = bmp.BackBufferStride / sizeof(UInt32);
            // Move from 'from' to 'to' along timestep intervals, with one dot painted per interval
            for (int i = 0; i < (int)((AirbrushDots*size)/30); i++)
            {
                int x, y;
                segment.Interpolate(i, (int)((AirbrushDots * size) / 30), out x, out y);

                int dist = r.Next() % size;
                double angle = r.NextDouble() * 2 * Math.PI;

                double dx = Math.Cos(angle) * dist;
                double dy = Math.Sqrt(dist * dist - dx * dx);
                if (angle > Math.PI) dy = -dy;

                int bx = x + (int)dx;
                int by = y + (int)dy;

                BoundingBox dotbounds = new BoundingBox();

                dotbounds.AddPoint(bx, by, AirbrushDotRadius);
                dotbounds.Clip(bitmapbounds);

                for (int k = dotbounds.Top, row = 0; k < dotbounds.Bottom; k++, y++, row++)
                    for (int j = dotbounds.Left, col = 0; j < dotbounds.Right; j++, col++)
                        WriteAlphaBlended(start + stride * k + j, Color.FromArgb(AirbrushDotKernel[row][col], color.R, color.G, color.B));
            }

            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        /// <summary>
        /// Paints on a pbgra32 WriteableBitmap with a stylized airbrush
        /// </summary>
        /// <param name="bmp">The bitmap to modify</param>
        /// <param name="from">The starting point of the stroke</param>
        /// <param name="to">The end point of the stroke</param>
        /// <param name="color">The color of the stroke</param>
        /// <param name="size">The size of the stroke</param>
        public static unsafe void LightBrush(WriteableBitmap bmp, Point from, Point to, Color color, int size)
        {
            Random r = new Random();
            AirbrushDots = 800;
            AirbrushDotRadius = 2;

            if (bmp == null) return;

            bmp.Lock();

            // Create a line segment representation
            LineSegment segment = new LineSegment(from, to);

            // Get a bounding box for the painted area
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size + AirbrushDotRadius);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size + AirbrushDotRadius);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();
            int stride = bmp.BackBufferStride / sizeof(UInt32);
            // Move from 'from' to 'to' along timestep intervals, with one dot painted per interval
            for (int i = 0; i < (int)((AirbrushDots * size) / 50); i++)
            {
                int x, y;
                segment.Interpolate(i, (int)((AirbrushDots * size) / 50), out x, out y);

                int dist = r.Next() % size;
                double angle = r.NextDouble() * 2 * Math.PI;

                double dx = Math.Sin(angle) * dist + Math.Cos(angle) * dist;
                double dy = Math.Sin(angle) * dist + Math.Cos(angle) * dist;
                if (angle > Math.PI) dy = -dy;

                int bx = x + (int)dx;
                int by = y + (int)dy;

                if (i > (int)((AirbrushDots * size) / 50*3/7))
                {
                    if (i < (int)((AirbrushDots * size) / 50 * 4 / 7))
                        color = Colors.Brown;
                    else if (i < (int)((AirbrushDots * size) / 50 * 5 / 7))
                        color = Colors.Red;
                    else if (i < (int)((AirbrushDots * size) / 50 * 6 / 7))
                        color = Colors.Green;
                    else if (i < (int)((AirbrushDots * size) / 50 ))
                        color = Colors.Yellow;
                }

                BoundingBox dotbounds = new BoundingBox();

                dotbounds.AddPoint(bx, by, AirbrushDotRadius);
                dotbounds.Clip(bitmapbounds);

                for (int k = dotbounds.Top, row = 0; k < dotbounds.Bottom; k++, y++, row++)
                    for (int j = dotbounds.Left, col = 0; j < dotbounds.Right; j++, col++)
                        WriteAlphaBlended(start + stride * k + j, Color.FromArgb(AirbrushDotKernel[row][col], color.R, color.G, color.B));
            }


            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        public static unsafe void Spider(WriteableBitmap bmp, Point from, Point to, Point? previous, Color color, int size)
        {
            if (bmp == null) return;
            bmp.Lock();

            size = size / 2;

            mypoint[js].X = from.X;
            mypoint[js].Y = from.Y;

            // Intermediate storage of the square of the size
            int sizesize = size * size;
            uint flatcolor = (uint)((int)color.A << 24) + (uint)((int)color.R << 16) + (uint)((int)color.G << 8) + color.B;

            // Create a line segment representation to compare distance to
            LineSegment linesegment = new LineSegment(from, to);

            // Get a bounding box for the line segment
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size);
            segmentbounds.Clip(bitmapbounds);

            // Get a pointer to the back buffer (we use an int pointer here, since we can safely assume a 32-bit pixel format)
            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();

            // Move the starting pixel to the x offset
            start += segmentbounds.Left;

            if (previous.HasValue)
            {
                LineSegment previoussegment = new LineSegment(previous.Value, from);

                //  PrePoint[Funccount++] = previous.Value;

                // Loop through the relevant portion of the image and figure out which pixels need to be erased
                for (int y = segmentbounds.Top; y < segmentbounds.Bottom; y++)
                {
                    UInt32* pixel = start + bmp.BackBufferStride / sizeof(UInt32) * y;

                    for (int x = segmentbounds.Left; x < segmentbounds.Right; x++)
                    {
                        if (linesegment.DistanceSquared(x, y) <= sizesize && previoussegment.DistanceSquared(x, y) > sizesize)
                        {
                            if (color.A == 255)
                                *pixel = flatcolor;
                            else
                                WriteAlphaBlended(pixel, color);
                        }

                        // Move to the next pixel
                        pixel++;
                    }
                }
            }
            else
            {
                // Loop through the relevant portion of the image and figure out which pixels need to be erased
                for (int y = segmentbounds.Top; y < segmentbounds.Bottom; y++)
                {
                    UInt32* pixel = start + bmp.BackBufferStride / sizeof(UInt32) * y;

                    for (int x = segmentbounds.Left; x < segmentbounds.Right; x++)
                    {
                        if (linesegment.DistanceSquared(x, y) <= sizesize)
                        {
                            if (color.A == 255)
                                *pixel = flatcolor;
                            else
                                WriteAlphaBlended(pixel, color);
                        }

                        // Move to the next pixel
                        pixel++;
                    }
                }
            }


            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
            int num = 0;
            for (int jsfor = 0; jsfor < js; jsfor++)
            {

                if (Math.Abs(mypoint[jsfor].X - to.X) < size * 30 & Math.Abs(mypoint[jsfor].Y - to.Y) < size * 30 & num < 2)
                {
                    Brush(bmp, mypoint[jsfor], to, previous, color, size);
                    num++;
                }
            }
            js++;
            if (js == 10000)
            {
                js = 0;
            }
      
        }

        public static unsafe void Test2(WriteableBitmap bmp, Point from, Point to, Color color, int size)
        {
            Random r = new Random();
            AirbrushDots = 500;
            AirbrushDotRadius = 2;

            if (bmp == null) return;

            bmp.Lock();

            // Create a line segment representation
            LineSegment segment = new LineSegment(from, to);

            // Get a bounding box for the painted area
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size + AirbrushDotRadius);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size + AirbrushDotRadius);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();
            int stride = bmp.BackBufferStride / sizeof(UInt32);
            // Move from 'from' to 'to' along timestep intervals, with one dot painted per interval
            for (int i = 0; i < AirbrushDots; i++)
            {
                int x, y;
                segment.Interpolate(i, AirbrushDots, out x, out y);

                int dist = r.Next() % size;
                double angle = r.NextDouble() * 2 * Math.PI;

                double dx = Math.Cos(angle) * dist;
                double dy = Math.Sqrt(dist * dist - dx * dx);
                if (angle > Math.PI) dy = -dy;

                int bx = x + (int)dx;
                int by = y + (int)dy;
                    //세로로 rainbow
                if (i < 50)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Red;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Red;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Red;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Red;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Red;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 50 && i < 100)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Orange;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Orange;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Orange;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Orange;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Orange;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 100 && i < 150)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Yellow;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Yellow;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Yellow;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Yellow;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Yellow;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 150 && i < 200)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Green;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Green;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Green;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Green;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Green;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 200 && i < 250)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Blue;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Blue;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Blue;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Blue;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Blue;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 250 && i < 300)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.Indigo;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.Indigo;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.Indigo;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.Indigo;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.Indigo;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }
                if (i >= 300 && i < 350)
                {
                    if ((i % 5) == 0)
                    {
                        color = Colors.BlueViolet;
                        // color = Color.FromArgb(20, 132, 200, 249);
                        bx = x + 0;
                        by = y + 0;

                    }
                    if ((i % 5) == 1)
                    {
                        color = Colors.BlueViolet;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y + 3;
                    }
                    if ((i % 5) == 2)
                    {
                        color = Colors.BlueViolet;
                        //color = Color.FromArgb(15, 133, 199, 247);
                        bx = x + 0;
                        by = y - 3;
                    }
                    if ((i % 5) == 3)
                    {
                        color = Colors.BlueViolet;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y + 6;
                    }
                    if ((i % 5) == 4)
                    {
                        color = Colors.BlueViolet;
                        //color = Color.FromArgb(10, 124, 201, 255);
                        bx = x + 0;
                        by = y - 6;
                    }
                }

                BoundingBox dotbounds = new BoundingBox();

                dotbounds.AddPoint(bx, by, AirbrushDotRadius);
                dotbounds.Clip(bitmapbounds);

                for (int k = dotbounds.Top, row = 0; k < dotbounds.Bottom; k++, y++, row++)
                    for (int j = dotbounds.Left, col = 0; j < dotbounds.Right; j++, col++)
                        WriteAlphaBlended(start + stride * k + j, Color.FromArgb(AirbrushDotKernel[row][col], color.R, color.G, color.B));
            }

            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        public static unsafe void Butterfly(WriteableBitmap bmp, Point from, Point to, Color color, int size)
        {
            AirbrushDots = 2000;
            AirbrushDotRadius = 3;
            Random r = new Random();

            if (bmp == null) return;

            bmp.Lock();

            // Create a line segment representation
            LineSegment segment = new LineSegment(from, to);

            // Get a bounding box for the painted area
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, 1500);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, 1500);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();
            int stride = bmp.BackBufferStride / sizeof(UInt32);
            // Move from 'from' to 'to' along timestep0 intervals, with one dot painted per interval
            int ran;

            if (size < 10)
            {
                ran=(r.Next() % (300 - size * 20)) + 1;
            }
            else if (size < 20)
            {
                ran=(r.Next() % (450 - size * 20)) + 1;
            }

            else if (size < 30)
            {
                ran = (r.Next() % (600 - size * 30)) + 1;
            }
            else if (size < 40)
            {
                ran = (r.Next() % (1400 - size * 20)) + 1;
            }
            else
            {
                ran = (r.Next() % (3200 - size * 20)) + 1;
            }
     
            for (int i = 0; i < AirbrushDots; i++)
            {
                int x, y;

                temp = i;
                segment.Interpolate(i, AirbrushDots, out x, out y);

                int bx = x + temp;
                int by = y - (temp * temp) / ran;

                if (temp % 4 == 0)
                {
                    if (ran % 2 == 0)
                    {
                        bx = x + (temp * temp) / ran;
                        by = y - temp;
                    }
                    else
                    {
                        bx = x + temp;
                        by = y - (temp * temp) / ran;
                    }
                }
                else if (temp % 4 == 1)
                {
                    if (ran % 2 == 0)
                    {
                        bx = x + (temp * temp) / ran;
                        by = y + temp;
                    }
                    else
                    {
                        bx = x + temp;
                        by = y + (temp * temp) / ran;
                    }
                }
                else if (temp % 4 == 2)
                {
                    if (ran % 2 == 0)
                    {
                        bx = x - (temp * temp) / ran;
                        by = y + temp;
                    }
                    else
                    {
                        bx = x - temp;
                        by = y + (temp * temp) / ran;
                    }
                }
                else if (temp % 4 == 3)
                {
                    if (ran % 2 == 0)
                    {
                        bx = x - (temp * temp) / ran;
                        by = y - temp;
                    }
                    else
                    {
                        bx = x - temp;
                        by = y - (temp * temp) / ran;
                    }
                }

                if (Math.Sqrt((bx - x) * (bx - x) + (by - y) * (by - y)) > size * 10)
                    continue;



                BoundingBox dotbounds = new BoundingBox();

                dotbounds.AddPoint(bx, by, AirbrushDotRadius);
                dotbounds.Clip(bitmapbounds);

                for (int k = dotbounds.Top, row = 0; k < dotbounds.Bottom; k++, y++, row++)
                    for (int j = dotbounds.Left, col = 0; j < dotbounds.Right; j++, col++)
                        WriteAlphaBlended(start + stride * k + j, Color.FromArgb(AirbrushDotKernel[row][col], color.R, color.G, color.B));
            }

            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        public static unsafe void Test4(WriteableBitmap bmp, Point from, Point to, Color color, int size)
        {
            AirbrushDots = 1000;
            AirbrushDotRadius = 4;

            if (bmp == null) return;
            bmp.Lock();
            // Create a line segment representation
            LineSegment segment = new LineSegment(from, to);

           //  Get a bounding box for the painted area
            BoundingBox bitmapbounds = new BoundingBox();
            BoundingBox segmentbounds = new BoundingBox();

            bitmapbounds.AddPoint(0, 0, 0);
            bitmapbounds.AddPoint(bmp.PixelWidth - 1, bmp.PixelHeight - 1, 0);

            segmentbounds.AddPoint((int)from.X, (int)from.Y, size+500 + AirbrushDotRadius);
            segmentbounds.AddPoint((int)to.X, (int)to.Y, size +500+ AirbrushDotRadius);
            segmentbounds.Clip(bitmapbounds);

            UInt32* start = (UInt32*)bmp.BackBuffer.ToPointer();
            int stride = bmp.BackBufferStride / sizeof(UInt32);
            // Move from 'from' to 'to' along timestep intervals, with one dot painted per interval
            for (int i = 0; i < AirbrushDots; i++)
            {
                int x, y;
                segment.Interpolate(i, AirbrushDots, out x, out y);

                //int dist =size;
                //double angle =2 * Math.PI;

                //double dx = Math.Cos(angle) * dist;
                //double dy = Math.Sqrt(dist * dist - dx * dx);
                //if (angle > Math.PI) dy = -dy;

                int bx = x;
                int by = y;

                if(i%4==0)
                {
                    bx = x+500;
                    by = y+500;
                }
                else if(i%4==1)
                {
                    bx = x -500;
                    by = y+500;
                }
                else if(i%4==2)
                {
                    bx = x +500;
                    by = y -500;
                }
                else if (i % 4 == 3)
                {
                    bx = x-500;
                    by = y-500;
                }

              
                //switch (i % 4)
                //{
                //    case 0:
                //       bx= 
                //}

                BoundingBox dotbounds = new BoundingBox();

                dotbounds.AddPoint(bx, by, AirbrushDotRadius);
                dotbounds.Clip(bitmapbounds);

                for (int k = dotbounds.Top, row = 0; k < dotbounds.Bottom; k++, y++, row++)
                    for (int j = dotbounds.Left, col = 0; j < dotbounds.Right; j++, col++)
                        WriteAlphaBlended(start + stride * k + j, Color.FromArgb(AirbrushDotKernel[row][col], color.R, color.G, color.B));
            }

            bmp.AddDirtyRect(new Int32Rect(segmentbounds.Left, segmentbounds.Top, segmentbounds.Width, segmentbounds.Height));
            bmp.Unlock();
        }

        // Alpha blends a color with its destination pixel using the standard formula
        private static unsafe void WriteAlphaBlended(UInt32 *pixel, Color c)
        {
            byte* component = (byte*)pixel;
            ushort alpha = (ushort)c.A;
            component[3] += (byte)(((ushort)(255 - component[3]) * alpha) / 255);
            component[2] = (byte)(((ushort)component[2] * (255 - alpha) + (ushort)c.R * alpha) / 255);
            component[1] = (byte)(((ushort)component[1] * (255 - alpha) + (ushort)c.G * alpha) / 255);
            component[0] = (byte)(((ushort)component[0] * (255 - alpha) + (ushort)c.B * alpha) / 255);
        }


    }
}
