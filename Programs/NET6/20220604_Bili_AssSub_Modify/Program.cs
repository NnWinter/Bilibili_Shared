using System.Text.RegularExpressions;

try
{
    //作者：NnWinter
#if DEBUG
    var fileArg = @"D:\Win11\Administrator\Downloads\孙吧老哥暴杀牛头人..730639206.ass";
#else
    if (Environment.GetCommandLineArgs().Length<2) { Console.WriteLine("应将字幕文件拖入运行。按任意键退出。"); Console.ReadKey(); return; }
    var fileArg = Environment.GetCommandLineArgs()[1];
#endif

    if (fileArg == null) { Console.WriteLine("应将字幕文件拖入运行。按任意键退出。"); Console.ReadKey(); return; }

    //获取输出文件的全名
    var parentDir = Path.GetDirectoryName(fileArg);
    if (parentDir == null) { Console.WriteLine("未找到文件的文件夹路径。按任意键退出。"); Console.ReadKey(); return; }
    var fileName = Path.GetFileNameWithoutExtension(fileArg);
    var fileExt = Path.GetExtension(fileArg);
    var outPath = Path.Combine(parentDir, fileName + "_new" + fileExt);
    if (File.Exists(outPath)) { Console.WriteLine("输出文件 \"{0}\" 已存在，本程序不会替换已有文件。按任意键退出。", outPath); Console.ReadKey(); return; }

    //读取ass内所有行
    string[] lines = File.ReadAllLines(fileArg);
    //输入字号缩放比例，垂直间隔比例，文字透明度比例。
    Console.Write("输入字号缩放比例<浮点数> (二倍=2，一半=0.5): ");
    var z = float.Parse(Console.ReadLine());
    Console.Write("输入垂直间距比例<浮点数> (通常与字号相同): ");
    var v = float.Parse(Console.ReadLine());
    Console.Write("输入文字填充透明比例<浮点数> (-1=强制不透明, 0=透明，1=与默认相同，其它=相乘倍数): ");
    var aF = float.Parse(Console.ReadLine());
    Console.Write("输入文字描边透明比例<浮点数> (同上): ");
    var aO = float.Parse(Console.ReadLine());
    //修改内容
    for(int i=0; i<lines.Length; i++)
    {
        var x = lines[i];
        if (x.StartsWith("Style:")) //修改 style
        {
            var match = Regex.Match(x, @".+?,.+?,(.+?),&H(........),.+?,&H(........).+");
            if (match.Success)
            {
                var fontSize = int.Parse(match.Groups[1].Value);
                var foreColor = Convert.ToInt32(match.Groups[2].Value, 16);
                var outlColor = Convert.ToInt32(match.Groups[3].Value, 16);

                var new_fontSize = ((int)(fontSize * z)).ToString();
                var new_foreColor = Convert.ToString(AlphaMul_Inv(foreColor, aF), 16).PadLeft(8, '0'); //以0填充，避免出现 0x00RRGGBB -> 0xRRGGBB 的情况
                var new_outlColor = Convert.ToString(AlphaMul_Inv(outlColor, aO), 16).PadLeft(8, '0');

                Console.WriteLine($"字号 {fontSize} -> {new_fontSize}");
                Console.WriteLine($"填充 {Convert.ToString(foreColor, 16)} -> {new_foreColor}");
                Console.WriteLine($"边框 {Convert.ToString(outlColor, 16)} -> {new_outlColor}");

                lines[i] = RegexReplaceWith(match, new string[] { new_fontSize, new_foreColor, new_outlColor });
                continue;
            }
        }
        if (x.StartsWith("Dialogue:")) //修改 dialogue
        {
            var match = Regex.Match(x, @"Dialogue:.+?{\\move\(.+?,(.+?),.+?,(.+?)\)(?:.+?)?}.+");
            if (match.Success)
            {
                var y1 = ((int)(int.Parse(match.Groups[1].Value) * v)).ToString();
                var y2 = ((int)(int.Parse(match.Groups[2].Value) * v)).ToString();

                lines[i] = RegexReplaceWith(match, new string[] { y1, y2 });
                continue;
            }
            match = Regex.Match(x, @"Dialogue:.+?{\\pos\(.+?,(.+?)\)(?:.+?)?}.+");
            if (match.Success)
            {
                var y = ((int)(int.Parse(match.Groups[1].Value) * v)).ToString();

                lines[i] = RegexReplaceWith(match, new string[] { y });
                continue;
            }
        }
    };
    //保存为新文件
    File.WriteAllLines(outPath, lines);
    Console.WriteLine("完成，按任意键退出"); Console.ReadKey(); return;
}
catch (Exception e) { Console.WriteLine("运行中出现了错误: {0}", e.Message); Console.ReadKey(); return;}

//替换 Match 中指定 group 的方法
string RegexReplaceWith(Match match, string[] replacements)
{
    try
    {
        string str = match.Value;
        for (int i = replacements.Length - 1; i >= 0; i--)
        {
            var index = match.Groups[i + 1].Index;
            var len = match.Groups[i + 1].Value.Length;
            str = str.Remove(index, len);
            str = str.Insert(index, replacements[i]);
        }
        return str;
    }
    catch (Exception e) { throw e.GetBaseException(); }
}
//修改 Alpha 的方法（反转）
int AlphaMul_Inv(int color, float mul)
{
    try
    {
        if (mul < 0) { return color & 0x00FFFFFF; } //设定为不透明 => 用 AND 运算强制把透明度位清零

        var alpha = color >> (0x18); //提取 alpha 的值 (1.5个字节 => 0x10+0x08)
        var new_alpha = (int)(0xFF - ((0xFF - alpha) * mul)); //获取计算后的 alpha 值
        var new_alpha_fix = new_alpha < 0x00 ? 0x00 : new_alpha > 0xFF ? 0xFF : new_alpha; //限定范围
        return (color & 0x00FFFFFF) + (new_alpha_fix << (0x18)); //将原本的不透明度去除 并使用 新 alpha
    }
    catch (Exception e) { throw e.GetBaseException(); }
}