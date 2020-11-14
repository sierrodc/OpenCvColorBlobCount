using System;
using OpenCvSharp;
using System.Linq;

namespace CountObjects
{
    public record InterestingColor(string Name, InputArray LowerHsv, InputArray UpperHsv, Scalar DestColor)
    {
        public int BlobsFound { get; set; }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("USAGE: CountObject.exe image.jpg");
                return;
            }
            var filePath = args[0];
            if(!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} doesn't exist");
                return;
            }

            // define range of colors
            var interestingColors = new InterestingColor[]
            {
                // hsv in open cv: 0-255, 0-255, 0-255
                new InterestingColor("DarkB", GetOpenCvHsvColor(0, 20, 0), GetOpenCvHsvColor(360, 100, 35), new Scalar(255, 0, 0)),
                new InterestingColor("Light",GetOpenCvHsvColor(20, 40, 20), GetOpenCvHsvColor( 55, 100, 80 ), new Scalar(0, 255, 0))
            };

            var image = new Mat(filePath, ImreadModes.Color); // real image in BGR color
            var image_hsv = image.CvtColor(ColorConversionCodes.BGR2HSV);

            foreach (var ic in interestingColors)
            {
                Mat colorFilter = new Mat(); // crete filter
                Cv2.InRange(image_hsv, ic.LowerHsv, ic.UpperHsv, colorFilter);

                // show mask
                using (new Window(ic.Name, WindowMode.KeepRatio, colorFilter))
                {
                    Cv2.WaitKey(1000);
                }
                var p = new SimpleBlobDetector.Params()
                {
                    //FilterByCircularity = true,
                    MinDistBetweenBlobs = 5,
                    MinArea = 5,
                    FilterByConvexity = false,
                    FilterByInertia = false,
                    FilterByColor = true,
                    BlobColor = 255
                };
                var detector = OpenCvSharp.SimpleBlobDetector.Create(p);
                var keypoints = detector.Detect(colorFilter);
                // draw points on original image
                foreach (var kp in keypoints)
                {
                    Cv2.Circle(image, (int)kp.Pt.X, (int)kp.Pt.Y, (int)10, ic.DestColor, thickness: 1);
                }

                ic.BlobsFound = keypoints.Length;
            }

            // show origina image with detected points
            var title = string.Join(',', interestingColors.Select(ic => $"{ic.Name}={ic.BlobsFound}"));
            using (new Window(title, WindowMode.KeepRatio, image))
            {
                Cv2.WaitKey();
            }

        }

        /// <summary>
        /// Create a opencv HSV array from "standard" hsv values
        /// </summary>
        private static InputArray GetOpenCvHsvColor(int h, int s, int v)
        {
            return InputArray.Create<int>(new int[] { h * 255 / 360, s * 255 / 100, v * 255 / 100 });
        }
    }
}
