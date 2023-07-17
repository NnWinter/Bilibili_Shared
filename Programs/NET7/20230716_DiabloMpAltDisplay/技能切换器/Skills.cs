using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 技能切换器
{
    internal class Skills
    {
        private static readonly Vector2 SKILL_START = new Vector2(0.4375f, 0.3451f);
        private static readonly Vector2 SKILL_PADDING = new Vector2(0.0329f, 0.0670f);

        private static readonly Vector2 BAR_START = new Vector2(0.4183f, 0.9306f);
        private static readonly float BAR_PADDING = 0.0328f;

        public static Point[,] GetSkillLocs(Screen screen)
        {
            var skills = new Point[6, 6];
            var screenSize = GetScreenSize(screen);

            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    var pos = (SKILL_START + new Vector2(x, y) * SKILL_PADDING) * screenSize;
                    skills[x, y] = new Point((int)pos.X, (int)pos.Y);
                }
            }
            return skills;
        }
        public static Point[] GetBarLocs(Screen screen)
        {
            var screenSize = GetScreenSize(screen);

            var bars = new Point[6];
            for (int i = 0; i < 6; i++)
            {
                var pos = (BAR_START + new Vector2(i, 0) * BAR_PADDING) * screenSize;
                bars[i] = new Point((int)pos.X, (int)pos.Y);
            }
            return bars;
        }
        private static Vector2 GetScreenSize(Screen screen)
        {
            return new Vector2(screen.Bounds.Width, screen.Bounds.Height);
        }
    }
}
