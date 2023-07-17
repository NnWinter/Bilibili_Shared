using System.Numerics;
using 技能切换器;

Bitmap bmp = (Bitmap)Image.FromFile("Sample.png");

var g = Graphics.FromImage(bmp);

var sizeH = 3;

var bar = Skills.GetBarLocs(Screen.PrimaryScreen);
for (int i = 0; i < bar.Length; i++)
{
    var loc = bar[i];
    g.DrawRectangle(new Pen(Color.Red), loc.X - sizeH, loc.Y - sizeH, sizeH * 2, sizeH * 2);
}

g.Flush();

bmp.Save("Debug.png");