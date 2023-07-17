using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiabloMpAltDisplay
{
    internal class Debug
    {
        public static void Run()
        {
            var screen = Screen.PrimaryScreen;
            var bmp = new Bitmap(screen.WorkingArea.Width, screen.WorkingArea.Height);
            var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0,0,bmp.Size);
            g.Flush();

            bmp.Save("debug_0.png");

            var bmp2 = new Bitmap(screen.WorkingArea.Width, screen.WorkingArea.Height);
            var g1 = Graphics.FromImage(bmp2);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height;y++)
                {
                    var color = bmp.GetPixel(x, y);

                    var hue = color.GetHue();
                    var sat = color.GetSaturation();

                    var r = (int)(hue / 360f * 255f);
                    var b = (int)(sat * 255f);

                    var newColor = Color.FromArgb(r, 0, b);
                    g1.DrawRectangle(new Pen(newColor), x, y, 1, 1);
                }
            }
            g1.Flush();
            bmp2.Save("debug.png");
        }
    }
}
