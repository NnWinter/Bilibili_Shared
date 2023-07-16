using DiabloMpAltDisplay;

var hpBar = Consts.GetBarRectangle(Screen.PrimaryScreen, Consts.BarType.HP);
var mpBar = Consts.GetBarRectangle(Screen.PrimaryScreen, Consts.BarType.MP);

Thread.Sleep(5000);

Bitmap bmp = new Bitmap(hpBar.Width, hpBar.Height);
Graphics g = Graphics.FromImage(bmp);
g.CopyFromScreen(hpBar.Location, Point.Empty, hpBar.Size);
bmp.Save("hp.bmp");

Bitmap bmp2 = new Bitmap(mpBar.Width, mpBar.Height);
Graphics g2 = Graphics.FromImage(bmp2);
g2.CopyFromScreen(mpBar.Location, Point.Empty, mpBar.Size);
bmp2.Save("mp.bmp");