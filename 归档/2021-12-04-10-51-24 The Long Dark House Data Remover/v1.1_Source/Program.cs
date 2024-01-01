using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using The_Long_Dark_Save_Editor_2.Helpers;
using The_Long_Dark_Save_Editor_2.Serialization;

// check if there's a file path
CheckArgCount();
// Get path of save while ensure it exists, print both path
GetSaveFileInfos(out FileInfo save, out FileInfo save_back);
// Check if its save file or json raw file
var filetype = GetFileType(save);

switch (filetype)
{
    case SaveFileType.SaveFile: EditSave(); break;
    case SaveFileType.JsonRaw: PackJson(); break;
    case SaveFileType.Undefined: ErrorExit("Unknown save file type"); break;
}

// Short codes even with such a bad coding (Get me .Net8 !)
Console.WriteLine("\n----- Done, press any key to exit -----");
Console.ReadKey();
return 0;

void ErrorExit(string message) { Console.WriteLine($"{message}\npress any key to exit."); Console.ReadKey(); Environment.Exit(0); }
void CheckArgCount() { if (args == null || args.Length == 0) { ErrorExit("Drag save file into exe first."); } }
void GetSaveFileInfos(out FileInfo save, out FileInfo save_back)
{
    save = new FileInfo($"{args[0]}");
    save_back = new FileInfo($"{save.FullName}_backup-{DateTime.Now:yyMMdd-HHmmss}");
    if (!save.Exists) { ErrorExit($"Save file \"{save.FullName}\"not exist."); }
    if (save_back.Exists) { ErrorExit($"Duplicated backup file name \"{save_back.FullName}\"."); }

    Console.WriteLine("Save path is : " + save.FullName);
    Console.WriteLine($"Backup path is : {save_back.FullName}");
}
SaveFileType GetFileType(FileInfo save)
{
    switch (save.Extension.ToLower())
    {
        case "": return SaveFileType.SaveFile;
        case "json": return SaveFileType.JsonRaw;
        default: return SaveFileType.Undefined;
    }
}
bool Backup(string savePath, string backupPath)
{
    Console.WriteLine("\nMaking backup...");
    try
    {
        File.Copy(savePath, backupPath);
        Console.WriteLine("Backup created");
        return true;
    }
    catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine("Backup faild, press any key to exit"); Console.ReadKey(); return false; }
}
void Save(JObject slotJson, string path)
{
    Console.WriteLine("\nSaving file...");
    string slotJson_str = JsonConvert.SerializeObject(slotJson);
    var data = EncryptString.Compress(slotJson_str);
    File.WriteAllBytes(path, data);
    Console.WriteLine("\nSaved");
}
void PackJson()
{
    var data = EncryptString.Compress(File.ReadAllText(save.FullName));
    var newpath = Path.ChangeExtension(save.FullName, null) + "_json";
    File.WriteAllBytes(newpath, data);
    Console.WriteLine($"raw json packed to \"{newpath}\"");
}
void EditSave()
{
    // Load save file and bla bla bla... (Didnt border to write comment for these back then, not doing it :p)
    string json = EncryptString.Decompress(File.ReadAllBytes(save.FullName));
    // Create a editable decompressed json for custom edits
    var rawPath = save.FullName + ".json";
    File.WriteAllText(rawPath, json);
    Console.WriteLine($"Decompressed json saved at \"{rawPath}\"");

    var slotJson = (JObject)JsonConvert.DeserializeObject(json);
    var dict = slotJson["m_Dict"];

    // Load places I think? (I dont understand how this shit works nowadays)
    Console.WriteLine();
    while (true)
    {
        Console.WriteLine("===== Loading all m_Dict =====");
        Console.WriteLine();

        var index_width = dict.Children().Count().ToString().Length;

        for (int i = 2; i < dict.Children().Count(); i++)
        {
            var key = ((JProperty)dict.Children().ElementAt(i)).Name;
            var format = " {0," + index_width + "}. {1" + "}";
            Console.WriteLine(format, i - 1, key);
        }

        Console.WriteLine("\nChoose to delete using integer (write '0' to save)");

        var choice = Console.ReadLine();

        int choice_int = -1;
        var flag = int.TryParse(choice, out choice_int);

        choice_int++;

        if (choice_int == 1) { break; }

        if (!flag || choice_int < 1 || choice_int >= dict.Children().Count())
        {
            Console.WriteLine("Invalid Choice\n"); continue;
        }

        var children = (JProperty)dict.Children().ElementAt(choice_int);
        var name = children.Name;
        children.Remove();

        Console.WriteLine("\n'" + name + "' Removed! Press any key to unpause");
        Console.ReadKey();
        Console.WriteLine();
    }

    // Backup and Save save file (save save file?)
    if (Backup(save.FullName, save_back.FullName))
    {
        Save(slotJson, save.FullName);
    }
}
class SlotData
{
    public string m_Name { get; set; }
    public string m_BaseName { get; set; }
    public string m_DisplayName { get; set; }
    public string m_Timestamp { get; set; }
    public EnumWrapper<SaveSlotType> m_GameMode { get; set; }
    public uint m_GameId { get; set; }
    public Dictionary<string, byte[]> m_Dict { get; set; }
}
public enum SaveSlotType
{
    UNKNOWN,
    CHALLENGE,
    CHECKPOINT,
    SANDBOX,
    STORY,
    AUTOSAVE,
}

public enum SaveFileType
{
    SaveFile,
    JsonRaw,
    Undefined
}