using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Axuno.Web
{
    /// <summary>
    /// Generates SVG captcha images
    /// </summary>
    public class CaptchaSvgGenerator : IDisposable
    {
        // use characters well to distinguish by humans
        private readonly char[] _captureChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'k', 'm', 'n', 'o', 'p', 'q', 's', 'u', 'v', 'w', 'x', 'y', 'z' };
        
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="gridColor"></param>
        /// <param name="bgColor"></param>
        /// <param name="textColor"></param>
        public CaptchaSvgGenerator(string text, int width, int height, Color gridColor,
            Color bgColor, Color textColor) : this(text, width, height)
        {
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
        public CaptchaSvgGenerator(string text, int width, int height) : this(text)
        {
            SetDimensions(width, height);
        }

        public CaptchaSvgGenerator(string text)
        {
            Text = !(string.IsNullOrWhiteSpace(text) || text.Length < 2) ? text : GenerateRandomString(2);
            SetDefaults();
            Text = text;
        }

        /// <summary>
        /// Gets the text of the capture image.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Generates a random string of the desired length, but at least with length 2.
        /// </summary>
        /// <param name="length">The number of characters to create.</param>
        /// <returns>Returns the generated random string.</returns>
        public string GenerateRandomString(int length)
        {
            length = length < 2 ? 2 : length;
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
            // For generating random numbers.
            var random = new Random((int)DateTime.Now.Ticks);

            int firstNum;
            int secondNum;

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
                    var result = firstNum * secondNum;
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
        /// Gets or sets the font size of the capture image text in Pt (points)
        /// </summary>
        public float FontSizePt { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> of the capture image.
        /// </summary>
        public string Image => GenerateSvg();

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
            FontFamilyName = "Arial, Helvetica, sans-serif";
            FontSizePt = 20.0F;
            Width = 151;
            Height = 51;
            GridColor = Color.FromArgb(0, 0, 235);
            BackgroundColor = Color.FromArgb(0, 255, 255, 255); // transparent
            TextColor = Color.FromArgb(0, 0, 0);
        }
        
        /// <summary>
        /// Destructor
        /// </summary>
        ~CaptchaSvgGenerator()
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
            { }
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

        /// <summary>
        /// Generates the SVG content
        /// </summary>
        /// <remarks>See hint for accessibility on https://css-tricks.com/accessible-svgs/
        /// </remarks>
        /// <returns>Returns a string with the SVG content</returns>
        private string GenerateSvg()
        {
            var xmlDoc = System.Xml.Linq.XDocument.Parse($@"
<!DOCTYPE svg PUBLIC '-//W3C//DTD SVG 1.1//EN' 'http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd'>
<svg height='{Height}' width='{Width}' xmlns='http://www.w3.org/2000/svg' xml:space='preserve' version='1.1' style='shape-rendering:geometricPrecision; text-rendering:geometricPrecision; image-rendering:optimizeQuality; fill-rule:evenodd; clip-rule:evenodd'>
    <defs>
        <pattern id='smallGrid' width='10' height='10' patternUnits='userSpaceOnUse'>
            <path d='M 10 0 L 0 0 0 10' fill='none' stroke='rgb({GridColor.R}, {GridColor.G}, {GridColor.B})' stroke-width='0.5'/>
        </pattern>
        <pattern id='grid' width='100' height='100' patternUnits='userSpaceOnUse'>
            <rect width='100' height='100' fill='url(#smallGrid)' stroke='red'/>
            <path d='M 100 0 L 0 0 0 100' fill='none' stroke='rgb({GridColor.R}, {GridColor.G}, {GridColor.B})' stroke-width='1'/>
        </pattern>
    </defs>
    <rect height='{Height}' width='{Width}' fill='rgb({BackgroundColor.R}, {BackgroundColor.G}, {BackgroundColor.B})' fill-opacity='{(BackgroundColor.A / 255D).ToString("0.#", CultureInfo.InvariantCulture)}' />
    <rect height='{Height}' width='{Width}' fill='url(#smallGrid)' />
    <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' font-family='{FontFamilyName}' font-size='{FontSizePt.ToString("0.##", CultureInfo.InvariantCulture)}pt'><tspan style='fill:rgb({TextColor.R}, {TextColor.G}, {TextColor.B});'>{Text.Substring(0,1)}</tspan><tspan style='fill:rgb({TextColor.R}, {TextColor.G}, {TextColor.B});'>{Text.Substring(1)}</tspan></text>
</svg>");
            return xmlDoc.ToString(SaveOptions.None);
        }
    }
}