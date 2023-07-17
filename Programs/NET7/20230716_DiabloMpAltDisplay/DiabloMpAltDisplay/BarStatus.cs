using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiabloMpAltDisplay
{
    internal class BarStatus
    {
        public float Hp { get; private set; } = float.NaN;
        public float Mp { get; private set; } = float.NaN;

        /// <summary>
        /// 血条和蓝条相对于屏幕的坐标上界和下界比例
        /// </summary>
        private static readonly (float y_top, float y_bottom) BARS_SCREEN_POSITION_RATIO_Y = new(0.86f, 0.98f);
        /// <summary>
        /// 血条相对于屏幕的 X 坐标比例
        /// </summary>
        private const float HP_BAR_SCREEN_POSITION_RATIO_X = 0.318f;
        /// <summary>
        /// 蓝条相对于屏幕的 X 坐标比例
        /// </summary>
        private const float MP_BAR_SCREEN_POSITION_RATIO_X = 0.682f;

        /// <summary>
        /// 条的宽度
        /// </summary>
        private const int BAR_WIDTH = 3;

        /// <summary>
        /// 条颜色 与 其他颜色的平均值的比例的阈值（超过阈值算作有量）
        /// </summary>
        private const float BAR_COLOR_THRESHOLD = 2f;

        /// <summary>
        /// 每次更新间隔(ms)
        /// </summary>
        private const int UPDATE_INTERVAL = 5000;

        /// <summary>
        /// 获得血条的位置
        /// </summary>
        /// <param name="screen">用于获得尺寸的屏幕</param>
        /// <returns>血条的矩形范围</returns>
        private static Rectangle GetBarRectangle(Screen screen, BarType barType)
        {
            var screenSize = screen.WorkingArea.Size;
            var barX = barType == BarType.HP ? HP_BAR_SCREEN_POSITION_RATIO_X : MP_BAR_SCREEN_POSITION_RATIO_X;
            return new Rectangle(
                (int)((barX * screenSize.Width)),
                (int)(BARS_SCREEN_POSITION_RATIO_Y.y_top * screenSize.Height),
                (int)((barX * screenSize.Width) + BAR_WIDTH),
                (int)((BARS_SCREEN_POSITION_RATIO_Y.y_bottom - BARS_SCREEN_POSITION_RATIO_Y.y_top) * screenSize.Height)
                );
        }

        private System.Timers.Timer Updater { get; init; }

        private Rectangle HpZone { get; init; }
        private Rectangle MpZone { get; init; }

        public BarStatus(Screen screen)
        {
            HpZone = GetBarRectangle(screen, BarType.HP);
            MpZone = GetBarRectangle(screen, BarType.MP);

            Updater = new System.Timers.Timer();
            Updater.Elapsed += Update;
            Updater.Interval = UPDATE_INTERVAL;
            Updater.Start();
        }

        private void Update(object? sender, EventArgs e)
        {
            var bmp = new Bitmap(HpZone.Width, HpZone.Height);
            var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(HpZone.Location, Point.Empty, HpZone.Size);
            var hp = GetBarStatus(bmp, BarType.HP);
            Hp = hp;

            bmp = new Bitmap(MpZone.Width, MpZone.Height);
            g = Graphics.FromImage(bmp);
            g.CopyFromScreen(MpZone.Location, Point.Empty, MpZone.Size);
            var mp = GetBarStatus(bmp, BarType.MP);
            Mp = mp;
        }

        public void Stop()
        {
            if (Updater.Enabled)
            {
                Updater.Stop();
            }
        }

        float GetBarStatus(Bitmap bmp, BarType bar)
        {
            var colors = GetBarAvgChannels(bmp);

            return 0f;
        }

        Color[] GetBarAvgChannels(Bitmap bmp)
        {
            Color[] avgColors = new Color[bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                var colors = new Color[bmp.Width];
                for (int x = 0; x < bmp.Width; x++)
                {
                    colors[x] = bmp.GetPixel(x, y);
                }
                avgColors[y] = Color.FromArgb(
                    (int)colors.Average(c => c.A),
                    (int)colors.Average(c => c.R),
                    (int)colors.Average(c => c.G),
                    (int)colors.Average(c => c.B)
                    );
            }
            return avgColors;
        }

        public enum BarType
        {
            HP,
            MP
        }
    }
}
