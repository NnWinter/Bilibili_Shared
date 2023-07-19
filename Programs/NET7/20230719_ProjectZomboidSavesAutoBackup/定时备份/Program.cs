
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var savesDir = new DirectoryInfo(Path.Combine(userDir, "Zomboid\\Saves"));
var backupDir = new DirectoryInfo(Path.Combine(userDir, "Zomboid\\NnBackups"));

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

List<GameSaveFolder> GetGameSaves(DirectoryInfo savesDir)
{


    var saveFolders = new List<GameSaveFolder>();

    foreach (var modeFolder in savesDir.GetDirectories())
    {
        foreach (var saveFolder in modeFolder.GetDirectories())
        {
            var gameSaveFolder = new GameSaveFolder(modeFolder.Name, saveFolder);
            saveFolders.Add(gameSaveFolder);
        }
    }

    return saveFolders;
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
List<GameSaveFolder> GetSavesToBackup(List<GameSaveFolder> gameSaves, List<GameBackupFileInfo> backups)
{
    var savesToBackup = new List <GameSaveFolder>();
    foreach (var gameSave in gameSaves)
    {
        var relatedBackups = backups.Where(x => x.GameMode == gameSave.GameMode && x.FolderName == gameSave.FolderName);

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

void BackupSaves(List<GameSaveFolder> saves)
{
    try
    {
        foreach (var save in saves)
        {
            var backupFile = new FileInfo(Path.Combine(backupDir.FullName, save.BackupFileName));
            if (backupFile.Exists) { Errors.Show($"已存在同名备份文件，程序错误:\n{backupFile.FullName}"); }
            ZipFile.CreateFromDirectory(save.FullPath, backupFile.FullName);
            Console.WriteLine($"已备份存档：{save.BackupFileName}");
        }
    }
    catch (Exception ex)
    {
        Errors.Show($"备份存档时出现错误:\n{ex.Message}");
    }
}


struct GameSaveFolder
{
    public string GameMode { get; init; }
    public string FolderName { get; init; }
    public string FullPath { get; init; }
    public DateTime LastWriteTime { get; init; }
    public string BackupFileForeName { get; init; }
    public string BackupFileName { get; init; }

    public const string TIME_FORMAT = "yyyyMMdd-HHmm";

    public GameSaveFolder(string gameMode, DirectoryInfo saveFolder)
    {
        GameMode = gameMode;
        FolderName = saveFolder.Name;
        FullPath = saveFolder.FullName;

        var fileSysInfos = saveFolder.GetFileSystemInfos();

        LastWriteTime = fileSysInfos.Max(x => x.LastWriteTime);
        BackupFileForeName = $"{GameMode}@{FolderName}";
        BackupFileName = $"{BackupFileForeName}@{LastWriteTime.ToString(TIME_FORMAT)}.zip";
    }
}

struct GameBackupFileInfo
{
    public string GameMode { get; init; }
    public string FolderName { get; init; }
    public DateTime LastWriteTime { get; init; }

    public const string FILE_REGEX_STR = @"(.+?)@(.+?)@(\d{8}-\d{4})\.zip";

    public GameBackupFileInfo(string fileName)
    {
        var match = Regex.Match(fileName, FILE_REGEX_STR);
        if (!match.Success) { Errors.Show($"文件名不符合正则规则，程序错误:\n{fileName}"); }
        GameMode = match.Groups[1].Value;
        FolderName = match.Groups[2].Value;

        var isTimeValid = DateTime.TryParseExact(match.Groups[3].Value, GameSaveFolder.TIME_FORMAT,null,System.Globalization.DateTimeStyles.None, out DateTime lastWriteTime);
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