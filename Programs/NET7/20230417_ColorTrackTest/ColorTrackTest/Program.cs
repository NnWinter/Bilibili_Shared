using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Numerics;

// 使用 Emgu CV 读取参考图片 (灰度图)
Mat refImg = CvInvoke.Imread("sample.png", ImreadModes.Grayscale);

// 处理图像序列
List<List<Vector2>> pointsList = new List<List<Vector2>>();

var begin = 478;
var end = 508;
var len = end - begin;
for (int i = begin; i <= begin + len; i++)
{
    // 读取图片为 Mat
    Mat img = CvInvoke.Imread("Images/img_" + i.ToString("000") + ".jpg", ImreadModes.Grayscale);

    // 所有匹配位置
    var points = MultipleTemplateMatching(img, refImg, 0.20);
    pointsList.Add(points);

    /** Debug 代码

    // 对匹配位置生成对应的debug矩形
    Image<Bgr, byte> imgBgr = img.ToImage<Bgr, byte>();
    var rects = points.Select(x=>new Rectangle(x, refImg.Size));
    foreach(var rect in rects) { imgBgr.Draw(rect, new Bgr(Color.Red), 3); }
    
    // 保存debug图片
    imgBgr.Save("Debug/img_" + i.ToString("000") + ".jpg");

    */
}

// 根据坐标生成轨迹
var trails = GetTrails(pointsList,20);

// 将轨迹转换为图片
Bitmap nimg = new Bitmap(480, 608);
var g = Graphics.FromImage(nimg);
g.Clear(Color.White);
foreach(var trail in trails) {
    var c = (int)((float)trail.Index / len * 255f);
    var first = trail.Path.First();
    g.DrawPie(new Pen(Color.FromArgb(c,255-c,0)),new Rectangle((int)(first.X - 2), (int)(first.Y-2), 4,4), 0, 360);

    var prev = new Point((int)first.X, (int)first.Y);
    for(int i=1; i<trail.Path.Count; i++)
    {
        var cp = (int)(((float)trail.Index + i) / len * 255f);
        g.DrawLine(new Pen(Color.FromArgb(cp,255-cp, 0)), new Point((int)trail.Path[i].X, (int)trail.Path[i].Y), prev);
        g.DrawPie(new Pen(Color.FromArgb(cp, 255-cp, 0)), new Rectangle((int)trail.Path[i].X - 2, (int)trail.Path[i].Y - 2, 4, 4), 0, 360);
        prev = new Point((int)trail.Path[i].X, (int)trail.Path[i].Y);
    }
    g.Flush();
}
g.Flush();
nimg.Save("result.jpg");

// Console.ReadLine();

static List<Vector2> MultipleTemplateMatching(Mat source, Mat template, double threshold)
{
    Mat result = new Mat();
    CvInvoke.MatchTemplate(source, template, result, TemplateMatchingType.SqdiffNormed);
    Image<Gray, double> imgGray = result.ToImage<Gray, double>();

    List<Vector2> matchLocations = new List<Vector2>();
    double minVal = 0, maxVal = 0;
    Point minLoc = new Point(), maxLoc = new Point();

    while (true)
    {
        CvInvoke.MinMaxLoc(imgGray, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        double matchValue = minVal;
        Point matchLoc = minLoc;

        if (matchValue < threshold)
        {
            matchLocations.Add(new Vector2(matchLoc.X, matchLoc.Y));

            // 为避免重复匹配，将已匹配区域的值设为最小值（对于SqDiff和SqDiffNormed方法）或最大值（对于其他方法）
            int xStart = Math.Max(matchLoc.X - template.Cols, 0);
            int xEnd = Math.Min(matchLoc.X + template.Cols, source.Cols);
            int yStart = Math.Max(matchLoc.Y - template.Rows, 0);
            int yEnd = Math.Min(matchLoc.Y + template.Rows, source.Rows);

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    if (x < 0 || y < 0 || x >= imgGray.Width || y >= imgGray.Height) { continue; }
                    imgGray[y, x] = new Gray(1);
                }
            }
        }
        else
        {
            break;
        }
    }

    return matchLocations;
}
static List<Trail> GetTrails(List<List<Vector2>> pList, float threshold)
{
    List<Trail> trails = new List<Trail>();

    var index = 0;
    foreach (List<Vector2> points in pList)
    {
        // 根据矢量更新轨迹路径
        var actives = trails.Where(x => x.Active);
        var distMap = new float[actives.Count(), points.Count];
        for(int p = 0; p < points.Count; p++)
        {
            for(int a = 0; a < actives.Count(); a++)
            {
                // distMap[a, p] = Vector2.Distance((actives.ElementAt(a).LastLoc + actives.ElementAt(a).LastLoc + actives.ElementAt(a).Motion) / 2, points[p]);
                distMap[a, p] = Vector2.Distance(actives.ElementAt(a).LastLoc + actives.ElementAt(a).Motion, points[p]);
            }
        }
        var minp = ExtractMin(distMap, threshold);
        for(int x=0; x < actives.Count(); x++)
        {
            if (minp[x] >= 0) { actives.ElementAt(x).Add(points[minp[x]]); }
            else { actives.ElementAt(x).Active = false; }
        }

        // 将新的位置节点作为轨迹加入其中
        foreach (var point in points)
        {
            if (!trails
                .Where(x => x.Active)
                .Any(x => x.LastLoc.Equals(point)))
            { 
                trails.Add(new Trail(trails.Count==0?0:trails.Max(x=>x.Id),index, point)); 
            }
        }
        index++;
    }

    return trails;
}
static int[] ExtractMin(float[,] matrix, float threshold)
{
    int[] f = new int[matrix.GetLength(0)];
    for(int i = 0; i<f.Length; i++) { f[i] = -1; }

    for (int fx=0; fx<f.Length; fx++)
    {
        var value = FindMin(matrix, threshold, out int x, out int y);
        f[x] = y;
        for(int my = 0; my < matrix.GetLength(1); my++)
        {
            matrix[x, my] = -1;
        }
        for(int mx = 0; mx < matrix.GetLength(0); mx++)
        {
            matrix[mx, y] = -1;
        }
    }
    return f;
}
static float FindMin(float[,] matrix, float threshold, out int x, out int y)
{
    x = 0; y = 0;
    if(matrix.Length == 0) { return -1; }
    var value = -1f;
    for (int i=0; i<matrix.GetLength(0); i++)
    {
        for(int j = 0; j<matrix.GetLength(1); j++)
        {
            if (matrix[i,j] >= 0 && matrix[i, j] < value && matrix[i, j] < threshold || value == -1) { x = i; y = j; value = matrix[i, j]; }
        }
    }
    return value;
}
class Trail
{
    public int Id;
    public int Index;
    public List<Vector2> Path = new List<Vector2>();
    public Vector2 LastLoc;
    public Vector2 Motion = new Vector2(0, 0);
    public bool Active = true;
    public Trail(int id, int index, Vector2 location)
    {
        Id = id;
        Index = index;
        Path.Add(location);
        LastLoc = location;
    }
    public void Add(Vector2 location)
    {
        Motion = location - LastLoc;
        Path.Add(location);
        LastLoc = location;
    }
}