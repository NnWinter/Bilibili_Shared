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
        /// 条的宽度比例 (相对于屏幕)
        /// </summary>
        private const float BAR_WIDTH_RATIO = 0.002f;

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
                (int)((barX - (BAR_WIDTH_RATIO / 2)) * screenSize.Width),
                (int)(BARS_SCREEN_POSITION_RATIO_Y.y_top * screenSize.Height),
                (int)(((barX + (BAR_WIDTH_RATIO / 2)) - (barX - (BAR_WIDTH_RATIO / 2))) * screenSize.Width),
                (int)((BARS_SCREEN_POSITION_RATIO_Y.y_bottom - BARS_SCREEN_POSITION_RATIO_Y.y_top) * screenSize.Height)
                );
        }

        public BarStatus(Screen screen)
        {
            var HpZone = GetBarRectangle(screen, BarType.HP);
            var MpZone = GetBarRectangle(screen, BarType.MP);

            var updater = new System.Windows.Forms.Timer();
            updater.Interval = 10;
            updater.Tick += (sender, e) =>
            {
                var bmp = new Bitmap(HpZone.Width, HpZone.Height);
                var g = Graphics.FromImage(bmp);
                g.CopyFromScreen(HpZone.Location, Point.Empty, HpZone.Size);
                var hp = GetBarStatus(bmp, BarType.HP);
                if (hp != float.NaN)
                {
                    Hp = hp;
                }
                bmp = new Bitmap(MpZone.Width, MpZone.Height);
                g = Graphics.FromImage(bmp);
                g.CopyFromScreen(MpZone.Location, Point.Empty, MpZone.Size);
                var mp = GetBarStatus(bmp, BarType.MP);
                if (mp != float.NaN)
                {
                    Mp = mp;
                }
            };
        }

        float GetBarStatus(Bitmap bmp, BarType bar)
        {

        }

        public enum BarType
        {
            HP,
            MP
        }
    }
}
