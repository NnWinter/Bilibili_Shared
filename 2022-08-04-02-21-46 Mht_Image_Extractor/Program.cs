using System.Drawing.Imaging;
using System.Text.RegularExpressions;

#if DEBUG
var dataPath = "WPX讨论组.mht";
#else
var dataPath = args[0];
#endif

var outDir = new DirectoryInfo($"{dataPath} - Images");
var imageDataList = new List<ImageData>();

try
{
    string mht = File.ReadAllText(dataPath).Replace("\r\n", "\n").Replace("\t", "");

    var regex = new Regex(@"Content-Type:(.*)\nContent-Transfer-Encoding:(.*)\nContent-Location:(.*)\n\n([\s\S]+?)\n\n");

    foreach (Match m in regex.Matches(mht))
    {
        imageDataList.Add(new ImageData(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value));
    }
}
catch (Exception ex) { Console.WriteLine($"读取文件 {dataPath} 失败：" + ex.Message); }

if (!outDir.Exists) { outDir.Create(); }
foreach (var imageData in imageDataList)
{
    var path = Path.Combine(outDir.FullName, imageData.GetName());
    var isSaved = imageData.SaveImage(path);
    if (isSaved) { Console.WriteLine($"图片已保存： {imageData.Location}  ->  {path}"); }
}

Console.WriteLine("程序结束，按任意键退出");
Console.ReadKey();

class ImageData
{
    public enum ImageExt
    {
        JPG,
        PNG,
        GIF,
        WEBP,
        UNDEF
    }
    public string Type { get; init; }
    public string ImgEncoding { get; init; }
    public string Location { get; init; }
    public string Data { get; init; }
    public ImageExt Extension { get; init; }
    public string ExtensionStr { get; init; }

    public ImageData(string type, string encoding, string location, string data)
    {
        Type = type.Replace("\r", "").Replace("\n", "");
        ImgEncoding = encoding.Replace("\r", "").Replace("\n", "");
        Location = location.Replace("\r", "").Replace("\n", "");
        Data = data.Replace("\r", "").Replace("\n", "");

        Extension =
            Data[0] == '/' ? ImageExt.JPG :
            Data[0] == 'i' ? ImageExt.PNG :
            Data[0] == 'R' ? ImageExt.GIF :
            Data[0] == 'U' ? ImageExt.WEBP :
            ImageExt.UNDEF;

        ExtensionStr =
            Extension == ImageExt.JPG ? ".jpg" :
            Extension == ImageExt.PNG ? ".png" :
            Extension == ImageExt.GIF ? ".gif" :
            Extension == ImageExt.WEBP ? ".webp" :
            ".dat";
    }

    public string GetUrl()
    {
        return $"data:{Type};{ImgEncoding},{Data}";
    }
    public string GetName()
    {
        return Location + ExtensionStr;
    }
    public bool SaveImage(string path)
    {
        try
        {

            var bytes = Convert.FromBase64String(Data);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                Image img;
                img = Image.FromStream(ms);
                if (img == null) { return false; }

                if (Extension == ImageExt.UNDEF) { Console.WriteLine($"不支持的文件格式位于 {Location}"); return false; }

                ImageFormat imgFormat =
                    Extension == ImageExt.JPG ? ImageFormat.Jpeg :
                    Extension == ImageExt.PNG ? ImageFormat.Png :
                    ImageFormat.Gif; //Missing Webp
                try
                {
                    img.Save(path, imgFormat);
                    img.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存文件 {GetName()} 失败: {ex.Message}");
                    return false;
                }
            }
        }
        catch
        {
            Console.WriteLine($"解码文件 {Location} 为图片失败");
            return false;
        }
    }
}