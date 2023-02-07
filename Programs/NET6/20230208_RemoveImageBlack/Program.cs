using RemoveImageBlack;
using System.Numerics;

const float MIN_BRIGHT = 0.05f;      // 最小有效亮度
const float DIST = 10f;              // 点集最大有效距离
const string OUT_DIR = "Cropped";   // 保存路径

// 创建输出文件夹
if (!Directory.Exists(OUT_DIR)) { Directory.CreateDirectory(OUT_DIR); }

// 获取图像 < filename, bitmap >
var imgDic = IO.GetImages(args);

// 处理并保存新图像
foreach (var imgP in imgDic)
{
    // 文件信息
    var filename = imgP.Key;
    var img = imgP.Value;

    // 用于储存点集范围
    var result = new Dictionary<int, Rectangle>(); // 静态数据储存 (应ref) [注: Vector2 是 struct 类型，默认为值比较]
    var gid = 0; // 组标

    // 获取符合条件的图像亮度图
    var map = GetBrightnessMap(img, MIN_BRIGHT);

    // 迭代 map 中每一个点
    for (int y = 0; y < map.GetLength(1); y++)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            // 只对有效坐标查找点集
            if (map[x, y])
            {
                var loc = new Vector2(x, y); // 当前像素的坐标
                // 去重，跳过已属于某个点集的点。
                if (!IsPointInResult(loc, result))
                {
                    // 将相邻点递归到 result 中
                    GetClosePoints(loc, ref map, ref gid, DIST, ref result);
                    // 组标加一
                    gid++;
                }
            }
        }
    }

    // DEBUG - 保存矩形图
    SaveRectMap(img,result);

    // 获取尺寸最大的矩形
    var rect = result.Values.MaxBy(x=>x.Size.Width * x.Size.Height);

    // 截取图像
    var outimg = new Bitmap(rect.Width, rect.Height);
    var g = Graphics.FromImage(outimg);
    g.DrawImage(img, new Rectangle(0,0,rect.Width,rect.Height), rect,GraphicsUnit.Pixel);
    g.Flush(); g.Dispose();

    // 保存
    outimg.Save(Path.Combine(OUT_DIR, filename));

    // 释放图片内存
    img.Dispose();
    outimg.Dispose();
}


Console.WriteLine("运行完成");
Console.ReadKey();

// 获取亮度图
bool[,] GetBrightnessMap(Bitmap image, float min)
{
    bool[,] map = new bool[image.Width, image.Height];
    //var bitdata = image.LockBits(new Rectangle(0,0,image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

    //image.UnlockBits(bitdata);

    for (int x = 0; x < image.Width; x++)
    {
        for (int y = 0; y < image.Height; y++)
        {
            map[x, y] = image.GetPixel(x, y).GetBrightness() > min;
        }
    }

    return map;
}

// 递归获取相邻点
void GetClosePoints(Vector2 current, ref bool[,] map, ref int groupId, float dist, ref Dictionary<int, Rectangle> dic)
{
    // X 边界
    int minX = (int)Math.Floor(current.X - dist);
    int maxX = (int)Math.Ceiling(current.X + dist);
    int boundX = map.GetLength(0);
    // Y 边界
    int minY = (int)Math.Floor(current.Y - dist);
    int maxY = (int)Math.Ceiling(current.Y + dist);
    int boundY = map.GetLength(1);
    // 循环
    for (int y = minY; y <= maxY; y++)
    {
        if (y < 0 || y >= boundY) { continue; }
        for (int x = minX; x <= maxX; x++)
        {
            if (x < 0 || x >= boundX) { continue; }
            if (map[x,y])
            {
                var p = new Vector2(x, y);

                // 跳过超距的
                var len = (p - current).Length();
                if (len > dist) { continue; }

                // 跳过已在某个矩形内的
                if (IsPointInResult(p, dic)) { continue; }

                // 扩展 rect 边界
                bool hasRect = dic.TryGetValue(groupId, out var rect);
                if (!hasRect) // 没有点集时创建新点集
                {
                    dic.Add(groupId, new Rectangle((int)p.X, (int)p.Y, 1, 1));
                }
                else // 有点集时扩展点集
                {
                    ExpandRect(ref rect, ref p);
                    // 可能需要更新
                    dic[groupId] = rect;
                }

                GetClosePoints(p, ref map, ref groupId, dist, ref dic);
            }
        }
    }
}

// 判断点是否在矩形内
bool IsPointInResult(Vector2 point, Dictionary<int, Rectangle> result)
{
    foreach(var value in result.Values)
    {
        if (point.X >= value.X && point.Y >= value.Y && point.X < value.X + value.Width && point.Y < value.Y + value.Height)
        {
            return true;
        }
    }
    return false;
}

void ExpandRect(ref Rectangle rect, ref Vector2 p)
{
    // X
    if (p.X < rect.Left)
    {
        var dif = rect.Left - p.X;
        rect.Offset((int)-dif, 0);
        rect.Width+=(int)dif;
    }
    else if(p.X >= rect.Right)
    {
        var dif = p.X - (rect.Right-1);
        rect.Width+=(int)dif;
    }
    // Y
    if (p.Y < rect.Top)
    {
        var dif = rect.Top - p.X;
        rect.Offset(0, (int)-dif);
        rect.Height+=(int)dif;
    }
    else if (p.Y >= rect.Bottom)
    {
        var dif = p.Y - (rect.Bottom-1);
        rect.Height+=(int)dif;
    }
}

void SaveRectMap(Bitmap img, Dictionary<int, Rectangle> result)
{
    var debug = new Bitmap(img.Width, img.Height);
    var g = Graphics.FromImage(debug);
    var rp = new Pen(Color.Red);
    foreach(var rect in result.Values)
    {
        g.DrawRectangle(rp, rect);
    }
    g.Flush();
    g.Dispose();
    debug.Save("debug.png");
}