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
            Bitmap bmp = (Bitmap)Image.FromFile("sample_A.png");

            var selectionArea = BarStatus.GetBarRectangle(bmp.Size, BarStatus.BarType.HP);

            // 创建一个新的Bitmap，将选区复制到新的图像中
            Bitmap selectedRegion = new Bitmap(selectionArea.Width, selectionArea.Height);
            using (Graphics g = Graphics.FromImage(selectedRegion))
            {
                g.DrawImage(bmp, new Rectangle(0, 0, selectionArea.Width, selectionArea.Height), selectionArea, GraphicsUnit.Pixel);
            }

            selectedRegion.Save("selected_region.png");


            float[] satBar = new float[selectedRegion.Height];
            for (int y = 0; y < selectedRegion.Height; y++)
            {
                float[] sats = new float[selectedRegion.Width];
                for (int x = 0; x < selectedRegion.Width; x++)
                {
                    sats[x] = selectedRegion.GetPixel(x, y).GetSaturation();
                }
                float avg = sats.Average();
                satBar[y] = avg;
            }

            for (int i = 0; i < satBar.Length; i++)
            {
                Console.WriteLine(satBar[i]);
            }
        }
    }
}
