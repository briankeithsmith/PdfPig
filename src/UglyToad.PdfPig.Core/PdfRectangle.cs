﻿namespace UglyToad.PdfPig.Core
{
    using System;

    /// <summary>
    /// A rectangle in a PDF file. 
    /// </summary>
    /// <remarks>
    /// PDF coordinates are defined with the origin at the lower left (0, 0).
    /// The Y-axis extends vertically upwards and the X-axis horizontally to the right.
    /// Unless otherwise specified on a per-page basis, units in PDF space are equivalent to a typographic point (1/72 inch).
    /// </remarks>
    public struct PdfRectangle
    {
        /// <summary>
        /// Top left point of the rectangle.
        /// </summary>
        public PdfPoint TopLeft { get; }

        /// <summary>
        /// Top right point of the rectangle.
        /// </summary>
        public PdfPoint TopRight { get; }

        /// <summary>
        /// Bottom right point of the rectangle.
        /// </summary>
        public PdfPoint BottomRight { get; }

        /// <summary>
        /// Bottom left point of the rectangle.
        /// </summary>
        public PdfPoint BottomLeft { get; }

        /// <summary>
        /// Centroid point of the rectangle.
        /// </summary>
        public PdfPoint Centroid
        {
            get
            {
                var t = GetT();

                var cosT = Math.Cos(t);
                var sinT = Math.Sin(t);

                var cx = Math.Min(BottomRight.X, Math.Min(TopLeft.X, Math.Min(BottomLeft.X, TopRight.X)))
                         + (Width * cosT + Height * sinT) / 2.0;
                var cy = Math.Min(BottomRight.Y, Math.Min(TopLeft.Y, Math.Min(BottomLeft.Y, TopRight.Y)))
                         + (Height * cosT + Width * sinT) / 2.0;

                return new PdfPoint(cx, cy);
            }
        }

        private double width;
        /// <summary>
        /// Width of the rectangle.
        /// </summary>
        public double Width
        {
            get
            {
                if (double.IsNaN(width))
                {
                    GetWidthHeight();
                }

                return width;
            }
        }

        private double height;
        /// <summary>
        /// Height of the rectangle.
        /// </summary>
        public double Height
        {
            get
            {
                if (double.IsNaN(height))
                {
                    GetWidthHeight();
                }

                return height;
            }
        }

        /// <summary>
        /// Rotation angle of the rectangle. Counterclockwise, in degrees.
        /// </summary>
        public double Rotation
        {
            get
            {
                var rotation = GetT() * 180 / Math.PI;

                return rotation;
            }
        }

        /// <summary>
        /// Area of the rectangle.
        /// </summary>
        public double Area => Math.Abs(Width * Height);

        /// <summary>
        /// Left. This value is only valid if the rectangle is not rotated, check <see cref="Rotation"/>.
        /// </summary>
        public double Left => TopLeft.X < TopRight.X ? TopLeft.X : TopRight.X;

        /// <summary>
        /// Top. This value is only valid if the rectangle is not rotated, check <see cref="Rotation"/>.
        /// </summary>
        public double Top => TopLeft.Y > BottomLeft.Y ? TopLeft.Y : BottomLeft.Y;

        /// <summary>
        /// Right. This value is only valid if the rectangle is not rotated, check <see cref="Rotation"/>.
        /// </summary>
        public double Right => BottomRight.X > BottomLeft.X ? BottomRight.X : BottomLeft.X;

        /// <summary>
        /// Bottom. This value is only valid if the rectangle is not rotated, check <see cref="Rotation"/>.
        /// </summary>
        public double Bottom => BottomRight.Y < TopRight.Y ? BottomRight.Y : TopRight.Y;

        /// <summary>
        /// Create a new <see cref="PdfRectangle"/>.
        /// </summary>
        /// <param name="bottomLeft">Bottom left point of the rectangle.</param>
        /// <param name="topRight">Top right point of the rectangle.</param>
        public PdfRectangle(PdfPoint bottomLeft, PdfPoint topRight) :
            this(bottomLeft.X, bottomLeft.Y, topRight.X, topRight.Y)
        { }

        /// <summary>
        /// Create a new <see cref="PdfRectangle"/>.
        /// </summary>
        /// <param name="x1">Bottom left point's x coordinate of the rectangle.</param>
        /// <param name="y1">Bottom left point's y coordinate of the rectangle.</param>
        /// <param name="x2">Top right point's x coordinate of the rectangle.</param>
        /// <param name="y2">Top right point's y coordinate of the rectangle.</param>
        public PdfRectangle(short x1, short y1, short x2, short y2) :
            this((double)x1, y1, x2, y2)
        { }

        /// <summary>
        /// Create a new <see cref="PdfRectangle"/>.
        /// </summary>
        /// <param name="x1">Bottom left point's x coordinate of the rectangle.</param>
        /// <param name="y1">Bottom left point's y coordinate of the rectangle.</param>
        /// <param name="x2">Top right point's x coordinate of the rectangle.</param>
        /// <param name="y2">Top right point's y coordinate of the rectangle.</param>
        public PdfRectangle(double x1, double y1, double x2, double y2) :
            this(new PdfPoint(x1, y2), new PdfPoint(x2, y2), new PdfPoint(x1, y1), new PdfPoint(x2, y1))
        { }

        /// <summary>
        /// Create a new <see cref="PdfRectangle"/>.
        /// </summary>
        public PdfRectangle(PdfPoint topLeft, PdfPoint topRight, PdfPoint bottomLeft, PdfPoint bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;

            BottomLeft = bottomLeft;
            BottomRight = bottomRight;

            width = double.NaN;
            height = double.NaN;
        }

        /// <summary>
        /// Creates a new <see cref="PdfRectangle"/> which is the current rectangle moved in the x and y directions relative to its current position by a value.
        /// </summary>
        /// <param name="dx">The distance to move the rectangle in the x direction relative to its current location.</param>
        /// <param name="dy">The distance to move the rectangle in the y direction relative to its current location.</param>
        /// <returns>A new rectangle shifted on the y axis by the given delta value.</returns>
        public PdfRectangle Translate(double dx, double dy)
        {
            return new PdfRectangle(TopLeft.Translate(dx, dy), TopRight.Translate(dx, dy),
                                    BottomLeft.Translate(dx, dy), BottomRight.Translate(dx, dy));
        }

        private double GetT()
        {
            double t;
            if (!BottomRight.Equals(BottomLeft))
            {
                t = Math.Atan2(BottomRight.Y - BottomLeft.Y, BottomRight.X - BottomLeft.X);
            }
            else
            {
                // handle the case where both bottom points are identical
                t = Math.Atan2(TopLeft.Y - BottomLeft.Y, TopLeft.X - BottomLeft.X) - Math.PI / 2;
            }

            return t;
        }

        private void GetWidthHeight()
        {
            var bx = Math.Max(Math.Abs(BottomRight.X - TopLeft.X), Math.Abs(BottomLeft.X - TopRight.X));
            var by = Math.Max(Math.Abs(TopRight.Y - BottomLeft.Y), Math.Abs(TopLeft.Y - BottomRight.Y));

            var t = GetT();

            var cosT = Math.Cos(t);
            var sinT = Math.Sin(t);
            var cosSqSinSqInv = 1.0 / (cosT * cosT - sinT * sinT);

            width = cosSqSinSqInv * (bx * cosT - by * sinT);
            height = cosSqSinSqInv * (-bx * sinT + by * cosT);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{TopLeft}, {Width}, {Height}]";
        }
    }
}
