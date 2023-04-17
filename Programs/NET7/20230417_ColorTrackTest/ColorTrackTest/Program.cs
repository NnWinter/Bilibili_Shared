using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

// 使用 Emgu CV 读取参考图片 (灰度图)
Mat refImg = CvInvoke.Imread("sample.png", ImreadModes.Grayscale);

// 处理图像序列
for (int i = 0; i <= 700; i++)
{
    // 读取图片为 Mat
    Mat img = CvInvoke.Imread("Images/img_" + i.ToString("000") + ".jpg", ImreadModes.Grayscale);
    
    // 所有匹配位置
    var points = MultipleTemplateMatching(img, refImg, 0.20);

    // 对匹配位置生成对应的debug矩形
    Image<Bgr, byte> imgBgr = img.ToImage<Bgr, byte>();
    var rects = points.Select(x=>new Rectangle(x, refImg.Size));
    foreach(var rect in rects) { imgBgr.Draw(rect, new Bgr(Color.Red), 3); }
    
    // 保存debug图片
    imgBgr.Save("Debug/img_" + i.ToString("000") + ".jpg");
}

//Console.ReadLine();

static List<Point> MultipleTemplateMatching(Mat source, Mat template, double threshold)
{
    Mat result = new Mat();
    CvInvoke.MatchTemplate(source, template, result, TemplateMatchingType.SqdiffNormed);
    Image<Gray, double> imgGray = result.ToImage<Gray, double>();

    List<Point> matchLocations = new List<Point>();
    double minVal = 0, maxVal = 0;
    Point minLoc = new Point(), maxLoc = new Point();

    while (true)
    {
        CvInvoke.MinMaxLoc(imgGray, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        double matchValue = minVal;
        Point matchLoc = minLoc;

        if (matchValue < threshold)
        {
            matchLocations.Add(matchLoc);

            // 为避免重复匹配，将已匹配区域的值设为最小值（对于SqDiff和SqDiffNormed方法）或最大值（对于其他方法）
            int xStart = Math.Max(matchLoc.X - template.Cols, 0);
            int xEnd = Math.Min(matchLoc.X + template.Cols, source.Cols);
            int yStart = Math.Max(matchLoc.Y - template.Rows, 0);
            int yEnd = Math.Min(matchLoc.Y + template.Rows, source.Rows);

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    if(x < 0 || y < 0 || x >= imgGray.Width || y >= imgGray.Height) { continue; }
                    imgGray[y,x] = new Gray(1);
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
