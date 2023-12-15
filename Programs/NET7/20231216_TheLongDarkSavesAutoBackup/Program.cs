
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var savesDir = new DirectoryInfo(Path.Combine(userDir, @"C:\Users\NnWinter\AppData\Local\Hinterland\TheLongDark"));
var backupDir = new DirectoryInfo(Path.Combine(userDir, @"C:\Users\NnWinter\AppData\Local\Hinterland\TheLongDark_NnBackup"));

var checkBackupInterval = 1 * 60 * 1000; // ms
var backupInterval = new TimeSpan(0, 10, 0);

var backupCountWarning = 50;

while (true)
{
    if (!backupDir.Exists) { backupDir.Create(); }

    var gameSaves = GetGameSaves(savesDir);
    var backups = GetBackups(backupDir);

    if (backups.Count > backupCountWarning) { Console.WriteLine("注意：存档备份文件已超过50个"); }

    var savesToBackup = GetSavesToBackup(gameSaves, backups);
    BackupSaves(savesToBackup);

    Thread.Sleep(checkBackupInterval);
}

List<GameSave> GetGameSaves(DirectoryInfo savesDir)
{
    var saves = new List<GameSave>();

    foreach (var modeFolder in savesDir.GetDirectories())
    {
        foreach (var saveFiles in modeFolder.GetFiles())
        {
            var gameSave = new GameSave(modeFolder.Name, saveFiles);
            saves.Add(gameSave);
        }
    }

    return saves;
}
List<GameBackupFileInfo> GetBackups(DirectoryInfo backupDir)
{
    var backupFiles = new List<GameBackupFileInfo>();
    foreach (var backupFile in backupDir.GetFiles())
    {
        var backupFileInfo = new GameBackupFileInfo(backupFile.Name);
        backupFiles.Add(backupFileInfo);
    }
    return backupFiles;
}
List<GameSave> GetSavesToBackup(List<GameSave> gameSaves, List<GameBackupFileInfo> backups)
{
    var savesToBackup = new List <GameSave>();
    foreach (var gameSave in gameSaves)
    {
        var relatedBackups = backups.Where(x => x.GameMode == gameSave.GameMode && x.SaveName == gameSave.SaveName);

        // 如果备份中没有这个存档就进行备份
        if (!relatedBackups.Any()) { savesToBackup.Add(gameSave); continue; }

        // 如果备份中有这个存档，但是 (备份时间+间隔时间) 比 (存档时间) 早，就进行备份
        var latestBackup = relatedBackups.MaxBy(x => x.LastWriteTime);
        if (latestBackup.LastWriteTime + backupInterval < gameSave.LastWriteTime)
        {
            savesToBackup.Add(gameSave); continue;
        }
    }
    return savesToBackup;
}

void BackupSaves(List<GameSave> saves)
{
    try
    {
        foreach (var save in saves)
        {
            var backupFile = new FileInfo(Path.Combine(backupDir.FullName, save.BackupFileName));
            if (backupFile.Exists) { Errors.Show($"已存在同名备份文件，程序错误:\n{backupFile.FullName}"); }

            File.Copy(save.FullPath, backupFile.FullName);
            Console.WriteLine($"已备份存档：{save.BackupFileName}");
        }
    }
    catch (Exception ex)
    {
        Errors.Show($"备份存档时出现错误:\n{ex.Message}");
    }
}

void CopyDirectory(string sourceDirName, string destDirName)
{
    // 获取源文件夹及其所有子目录
    DirectoryInfo dir = new DirectoryInfo(sourceDirName);

    if (!dir.Exists)
    {
        throw new DirectoryNotFoundException(
            "源目录不存在或无法找到: "
            + sourceDirName);
    }

    // 如果目标文件夹不存在则创建
    if (!Directory.Exists(destDirName))
    {
        Directory.CreateDirectory(destDirName);
    }

    // 复制所有文件
    FileInfo[] files = dir.GetFiles();
    foreach (FileInfo file in files)
    {
        string tempPath = Path.Combine(destDirName, file.Name);
        file.CopyTo(tempPath, false);
    }

    // 复制所有子文件夹
    DirectoryInfo[] dirs = dir.GetDirectories();
    foreach (DirectoryInfo subdir in dirs)
    {
        string tempPath = Path.Combine(destDirName, subdir.Name);
        CopyDirectory(subdir.FullName, tempPath);
    }
}

struct GameSave
{
    public string GameMode { get; init; }
    public string SaveName { get; init; }
    public string FullPath { get; init; }
    public DateTime LastWriteTime { get; init; }
    public string BackupFileName { get; init; }

    public const string TIME_FORMAT = "yyyyMMdd-HHmm";

    public GameSave(string gameMode, FileInfo save)
    {
        GameMode = gameMode;
        SaveName = save.Name;
        FullPath = save.FullName;

        LastWriteTime = save.LastWriteTime;
        BackupFileName = $"{GameMode}@{save.Name}@{LastWriteTime.ToString(TIME_FORMAT)}";
    }
}

struct GameBackupFileInfo
{
    public string GameMode { get; init; }
    public string SaveName { get; init; }
    public DateTime LastWriteTime { get; init; }

    public const string FILE_REGEX_STR = @"(.+?)@(.+?)@(\d{8}-\d{4})";

    public GameBackupFileInfo(string fileName)
    {
        var match = Regex.Match(fileName, FILE_REGEX_STR);
        if (!match.Success) { Errors.Show($"文件名不符合正则规则，程序错误:\n{fileName}"); }
        GameMode = match.Groups[1].Value;
        SaveName = match.Groups[2].Value;

        var isTimeValid = DateTime.TryParseExact(match.Groups[3].Value, GameSave.TIME_FORMAT,null,System.Globalization.DateTimeStyles.None, out DateTime lastWriteTime);
        if (!isTimeValid) { Errors.Show($"文件时间无法识别，程序错误:\n{fileName}"); }
        LastWriteTime = lastWriteTime;
    }
}

class Errors
{
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void Show(string message)
    {
        // 获取当前控制台的句柄
        IntPtr consoleWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

        // 将控制台窗口设置为前景窗口
        SetForegroundWindow(consoleWindow);

        // 输出错误信息
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("按任意键退出");
        Environment.Exit(1);
    }
}