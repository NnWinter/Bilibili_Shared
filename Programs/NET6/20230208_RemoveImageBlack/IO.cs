using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveImageBlack
{
    internal class IO
    {
        public static Dictionary<string, Bitmap> GetImages(string[] args)
        {
            var dic = new Dictionary<string, Bitmap>();
#if DEBUG
            var currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var files = currentDir.GetFiles("*.jpg");
            return files.Select(x => KeyValuePair.Create(x.Name, (Bitmap)Image.FromFile(x.Name))).ToDictionary(x => x.Key, x => x.Value);
#else
            return args.Select(x => KeyValuePair.Create(Path.GetFileName(x),(Bitmap)Image.FromFile(x))).ToDictionary(x=>x.Key, x=>x.Value);
#endif
        }
    }
}
