﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiabloMpAltDisplay
{
    internal class BarStatus
    {
        public float Hp { get; private set; } = float.NaN;
        public float Mp { get; private set; } = float.NaN;

        /// <summary>
        /// 血条和蓝条相对于屏幕的坐标上界和下界比例
        /// </summary>
        private static readonly (float y_top, float y_bottom) BARS_SCREEN_POSITION_RATIO_Y = new(0.86f, 0.978f);
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
        private const int BAR_WIDTH = 6;

        /// <summary>
        /// 饱和度阈值
        /// </summary>
        private const float BAR_SAT_THRESHOLD = 0.5f;

        /// <summary>
        /// 计算时取平均数
        /// </summary>
        private const int SMOOTH_SIZE = 5;

        /// <summary>
        /// 每次更新间隔(ms)
        /// </summary>
        private const int UPDATE_INTERVAL = 10;

        /// <summary>
        /// 获得血条的位置
        /// </summary>
        /// <param name="screen">用于获得尺寸的屏幕</param>
        /// <returns>血条的矩形范围</returns>
        public static Rectangle GetBarRectangle(Size size, BarType barType)
        {
            var barX = barType == BarType.HP ? HP_BAR_SCREEN_POSITION_RATIO_X : MP_BAR_SCREEN_POSITION_RATIO_X;
            return new Rectangle(
                (int)((barX * size.Width)-(BAR_WIDTH/2)),
                (int)(BARS_SCREEN_POSITION_RATIO_Y.y_top * size.Height),
                (int)(BAR_WIDTH),
                (int)((BARS_SCREEN_POSITION_RATIO_Y.y_bottom - BARS_SCREEN_POSITION_RATIO_Y.y_top) * size.Height)
                );
        }

        private System.Timers.Timer Updater { get; init; }

        private Rectangle HpZone { get; init; }
        private Rectangle MpZone { get; init; }

        public BarStatus(Size size)
        {
            HpZone = GetBarRectangle(size, BarType.HP);
            MpZone = GetBarRectangle(size, BarType.MP);

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
            var sats = GetBarSats(bmp);
            var values = GetValues(sats);

            return GetValue(values);
        }

        float[] GetBarSats(Bitmap bmp)
        {
            float[] avgSats = new float[bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                var sats = new float[bmp.Width];
                for (int x = 0; x < bmp.Width; x++)
                {
                    sats[x] = bmp.GetPixel(x, y).GetSaturation();
                }
                avgSats[y] = sats.Average();
            }
            return avgSats;
        }

        float[] GetValues(float[] sats)
        {
            float[] smoothedArray = new float[sats.Length];

            for (int i = 0; i < sats.Length; i++)
            {
                float sum = 0;
                int count = 0;

                for (int j = Math.Max(0, i - SMOOTH_SIZE + 1); j <= i; j++)
                {
                    sum += sats[j];
                    count++;
                }

                smoothedArray[i] = sum / count;
            }

            return smoothedArray;
        }

        float GetValue(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > BAR_SAT_THRESHOLD)
                {
                    return 1 - ((float)i / values.Length);
                }
            }
            return 0;
        }

        public enum BarType
        {
            HP,
            MP
        }
    }
}