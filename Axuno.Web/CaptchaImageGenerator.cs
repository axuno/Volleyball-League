using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Axuno.Web
{
    /// <summary>
    /// Generates captcha images
    /// </summary>
    public class CaptchaImageGenerator
    {
        // use characters well to distinguish by humans
        private readonly char[] _captureChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'k', 'm', 'n', 'o', 'p', 'q', 's', 'u', 'v', 'w', 'x', 'y', 'z' };
        
        // For generating random numbers.
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fontFamilyName"></param>
        /// <param name="gridColor"></param>
        /// <param name="bgColor"></param>
        /// <param name="textColor"></param>
        public CaptchaImageGenerator(string text, int width, int height, string fontFamilyName, Color gridColor,
            Color bgColor, Color textColor) : this(text, width, height)
        {
            SetFontFamilyName(fontFamilyName);
            GridColor = gridColor;
            BackgroundColor = bgColor;
            TextColor = textColor;
        }

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public CaptchaImageGenerator(string text, int width, int height)
        {
            Text = string.IsNullOrEmpty(text) ? " " : text;
            SetDimensions(width, height);
            SetDefaults();
        }

        /// <summary>
        /// Gets the text of the capture image.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Set the captcha text with a random string.
        /// </summary>
        /// <param name="length">The number of characters to create.</param>
        /// <returns>Returns the generated random string.</returns>
        public string GenerateRandomString(int length)
        {
            var random = new Random();
            var s = "";
            for (var i = 0; i < length; i++)
                s = string.Concat(s, _captureChars[random.Next(0, _captureChars.Length)]);

            return s;
        }

        /// <summary>
        /// Set the captcha text with a math task.
        /// </summary>
        /// <param name="calcRule">1:add, 2:subtract, 3:multiply, 4:divide, 5:random</param>
        /// <returns>Returns the result of the math task.</returns>
        public int SetTextWithMathCalc(int calcRule = 1)
        {
            var random = new Random((int)DateTime.Now.Ticks);

            int firstNum;
            int secondNum;
            int result;

            if (calcRule >= 5 ) calcRule = random.Next(1, 5);
            if (calcRule < 0) calcRule = 1;
            
            switch (calcRule)
            {
                case 1: // add
                    firstNum = random.Next(10, 90);
                    secondNum = random.Next(1, 9);
                    Text = string.Format($"{firstNum} + {secondNum}");
                    return firstNum + secondNum;
                case 2: // subtract
                    firstNum = random.Next(10, 90) / 10 * 10;
                    secondNum = random.Next(1, 9);
                    Text = string.Format($"{firstNum} – {secondNum}");
                    return firstNum - secondNum;
                case 3: // multiply
                    firstNum = random.Next(2, 6);
                    secondNum = random.Next(1, 9);
                    Text = string.Format($"{firstNum} * {secondNum}");
                    return firstNum * secondNum;
                default: // divide
                    firstNum = random.Next(2, 9);
                    secondNum = random.Next(1, 9);
                    result = firstNum * secondNum;
                    Text = string.Format($"{result} / {secondNum}");
                    return result / secondNum;
            }
        }

        /// <summary>
        /// Gets or sets the static string which can be used for a session key.
        /// </summary>
        public static string CaptchaSessionKeyName { get; set; } = "CaptchaImageText";

        /// <summary>
        /// Gets or sets the font family of the capture image text.
        /// </summary>
        public string FontFamilyName { get; set; }

        /// <summary>
        /// Gets the <see cref="Bitmap"/> of the capture image.
        /// </summary>
        public Bitmap Image => GenerateImage();

        /// <summary>
        /// Get or sets the width of the capture image.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the capture image. 
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the grid color of the capture image. 
        /// </summary>
        public Color GridColor { get; set; }

        /// <summary>
        /// Gets or sets the background color of the capture image. 
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the text color of the capture image. 
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Gets or sets whether the text should be warped or not.
        /// </summary>
        public bool WarpText { get; set; } = false;

        /// <summary>
        /// Gets or sets whether noise should be added to the capture image.
        /// </summary>
        public bool AddNoise { get; set; } = false;

        private void SetDefaults()
        {
            FontFamilyName = "Arial";
            GridColor = Color.FromArgb(0xC3, 0xD2, 0xEB);
            BackgroundColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
            TextColor = Color.FromArgb(0xB4, 0xC9, 0xEB);
        }
        
        /// <summary>
        /// Destructor
        /// </summary>
        ~CaptchaImageGenerator()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
                Image.Dispose();
        }

        private void SetDimensions(int width, int height)
        {
            // Check the width and height.
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height,
                    "Argument out of range, must be greater than zero.");
            Width = width;
            Height = height;
        }

        private void SetFontFamilyName(string fontFamilyName)
        {
            // If the named font is not installed, default to a system font.
            try
            {
                var font = new Font(fontFamilyName, 12F);
                FontFamilyName = fontFamilyName;
                font.Dispose();
            }
            catch
            {
                FontFamilyName = FontFamily.GenericSerif.Name;
            }
        }

        /// <summary>
        /// Finds a font size which fits into a given width and height.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="textToFit"></param>
        /// <param name="sizeToUse"></param>
        /// <param name="fontToTry"></param>
        /// <returns>The font, scaled if it was necessary.</returns>
        private static Font FindFontSizeToFit(Graphics g, string textToFit, Size sizeToUse, Font fontToTry)
        {
            // Find out what the current size of the string in this font is
            var realSize = g.MeasureString(textToFit, fontToTry);
            if ((realSize.Width <= sizeToUse.Width) && (realSize.Height <= sizeToUse.Height))
            {
                return fontToTry;
            }

            // Either width or height is too big...
            // Usually either the height ratio or the width ratio
            // will be less than 1. Work them out...
            var heightScaleRatio = sizeToUse.Height / realSize.Height;
            var widthScaleRatio = sizeToUse.Width / realSize.Width;

            // We'll scale the font by the one which is furthest out of range...
            float scaleRatio = (heightScaleRatio < widthScaleRatio) ? heightScaleRatio : widthScaleRatio;
            float scaleFontSize = fontToTry.Size * scaleRatio;

            // Retain whatever the style was in the old font...
            var fontToTryStyle = fontToTry.Style;

            // Get rid of the old non working font...
            fontToTry.Dispose();

            // Tell the caller to use this newer smaller font.
            return new Font(fontToTry.FontFamily, scaleFontSize, fontToTryStyle, GraphicsUnit.Pixel);
        }

        private Bitmap GenerateImage()
        {
            // Create a new 32-bit bitmap image.
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            // Create a graphics object for drawing.
            var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;  // performance vs. quality!
            var rect = new Rectangle(0, 0, Width, Height);

            //HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallGrid, Color.FromArgb(196, 215, 246), Color.White);
            var hatchBrush = new HatchBrush(HatchStyle.LargeGrid, GridColor, BackgroundColor);
            g.FillRectangle(hatchBrush, rect);

            var font = FindFontSizeToFit(g, Text, new Size(Width, Height), new Font(FontFamilyName, Width));

            // Set up the text format.
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Create a path using the text and warp it randomly.
            var path = new GraphicsPath();
            path.AddString(Text, font.FontFamily, (int) font.Style, font.Size, rect, format);
            var v = 4F;
            PointF[] points =
            {
                new PointF(_random.Next(rect.Width) / v, _random.Next(rect.Height) / v),
                new PointF(rect.Width - _random.Next(rect.Width) / v, _random.Next(rect.Height) / v),
                new PointF(_random.Next(rect.Width) / v, rect.Height - _random.Next(rect.Height) / v),
                new PointF(rect.Width - _random.Next(rect.Width) / v, rect.Height - _random.Next(rect.Height) / v)
            };
            var matrix = new Matrix();
            matrix.Translate(0F, 0F);
            if (WarpText) path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

            // Draw the text.
            hatchBrush = new HatchBrush(HatchStyle.Percent40, TextColor, TextColor);
            g.FillPath(hatchBrush, path);

            // Add some random noise v1
            if (false)
            {
                var m = Math.Max(rect.Width, rect.Height);
                for (var i = 0; i < (int) (rect.Width * rect.Height / 30F); i++)
                {
                    var x = _random.Next(rect.Width);
                    var y = _random.Next(rect.Height);
                    var w = _random.Next(m / 50);
                    var h = _random.Next(m / 50);
                    g.FillEllipse(hatchBrush, x, y, w, h);
                }
            }

            //add some random noise v2
            if (AddNoise)
            {
                int i;
                var pen = new Pen(TextColor);
                
                for (i = 1; i < 10; i++)
                {
                    pen.Color = Color.FromArgb(
                        (_random.Next(0, 255)),
                        (_random.Next(0, 255)),
                        (_random.Next(0, 255)));

                    var r = _random.Next(0, (Width / 3));
                    var x = _random.Next(0, Width);
                    var y = _random.Next(0, Height);

                    var j = x - r;
                    var k = y - r;
                    g.DrawEllipse(pen, j, k, r, r);
                }
            }

            // Clean up.
            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();

            // Set the image.
            return bitmap;
        }
    }
}