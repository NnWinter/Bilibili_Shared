//读取文件
var path = @"D:\Win11\Administrator\Desktop\2022-06-25-05-02-28.txt";
var text = File.ReadAllText(path);
//判断是否为空格或回车的函数
Func<char, bool> isSpaceOrReturn = (c) => { return c == ' ' || c == '\r' || c == '\n' || c == '\t'; };
//列表
var li = new List<CName>();
//循环 解析
for (int i = 0; i < text.Length; i++)
{
    //读排名序号
    string index = "";
    while (i < text.Length && !isSpaceOrReturn(text[i])) { index += text[i]; i++; }
    //跳过空格或换行符
    while (i < text.Length && isSpaceOrReturn(text[i])) { i++; }
    //读取名字
    string name = "";
    while (i < text.Length && !isSpaceOrReturn(text[i])) { name += text[i]; i++; }
    //跳过空格或换行符
    while (i < text.Length && isSpaceOrReturn(text[i])) { i++; }
    //读取人数（含千分位符）
    string count = "";
    while (i < text.Length && (!isSpaceOrReturn(text[i]) || text[i] == ',')) { count += text[i]; i++; }
    //添加到列表
    li.Add(new CName(index, name, count));
}
//格式化为csv
string out_str = "";
foreach (var l in li)
{
    out_str += string.Format("{0},{1},\"{2}\"\n", l.Index, l.Name, l.Count);
}
//输出为文件
File.WriteAllText(@"D:\Win11\Administrator\Desktop\2022-06-25-05-02-28.csv", out_str);
//格式化为原格式
out_str = "";
foreach (var l in li)
{
    out_str += string.Format("{0} {1} {2}\n\n", l.Index, l.Name, l.Count);
}
File.WriteAllText(@"D:\Win11\Administrator\Desktop\2022-06-25-05-02-28_new.txt", out_str);
//类
class CName
{
    public string Index { get; }
    public string Name { get; }
    public string Count { get; }
    public CName(string index, string name, string count)
    {
        Index = index;
        Name = name;
        Count = count;
    }
}