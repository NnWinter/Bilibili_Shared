using H.Hooks;
using System.Numerics;
using WindowsInput;
using 技能切换器;


var skills = Skills.GetSkillLocs(Screen.PrimaryScreen);
var bar = Skills.GetBarLocs(Screen.PrimaryScreen);
var close = Skills.GetPanelCloseLoc(Screen.PrimaryScreen);

var sim = new InputSimulator();
var keyMapping = new VirtualKeyCode[] { VirtualKeyCode.VK_Q, VirtualKeyCode.VK_W, VirtualKeyCode.VK_E, VirtualKeyCode.VK_R, VirtualKeyCode.LBUTTON, VirtualKeyCode.XBUTTON2 };

var enable = true;

using var keyboardHook = new LowLevelKeyboardHook();
keyboardHook.Down += (_, args) =>
{
    var key = args.CurrentKey;

    if (key.Equals(Key.End))
    {
        enable = false; Console.WriteLine("Disable");
    }
    if (key.Equals(Key.Home))
    {
        enable = true; Console.WriteLine("Enable");
    }

    if (!enable) { return; }

    switch (key)
    {
        case Key.D1:
            SwitchToSkill((2, 2), 1, true);
            break;
        case Key.D2:
            SwitchToSkill((6, 2), 1, true);
            break;
        case Key.D3:
            SwitchToSkill((1, 3), 1, true);
            break;
        case Key.D4:
            SwitchToSkill((2, 3), 1, true);
            break;

        case Key.D5:
            SwitchToSkill((3, 3), 3, true);
            break;

        case Key.T:
            SwitchToSkill((4, 3), 3, true);
            break;
        case Key.G:
            SwitchToSkill((1, 3), 3, true);
            break;
    }
};
keyboardHook.Start();

Console.ReadLine();

keyboardHook.Stop();

void SwitchToSkill((int x, int y) skillIndex, int barIndex, bool autoTrigger = false)
{
    Action<VirtualKeyCode> Press = (key) =>
    {
        sim.Keyboard.KeyDown(key);
        sim.Keyboard.Sleep(15);
        sim.Keyboard.KeyUp(key);
    };

    Action<Point> LClick = (loc) =>
    {
        Cursor.Position = loc;
        Thread.Sleep(5);
        sim.Mouse.LeftButtonDown();
        sim.Mouse.Sleep(10);
        sim.Mouse.LeftButtonUp();
    };

    Action<Point> RClick = (loc) =>
    {
        Cursor.Position = loc;
        Thread.Sleep(5);
        sim.Mouse.RightButtonDown();
        sim.Mouse.Sleep(10);
        sim.Mouse.RightButtonUp();
    };

    var cd0 = 10;

    // 暂存鼠标位置

    var pos = Cursor.Position;

    // 获取用于清除冷却的技能位置

    var altLoc = barIndex == 5 ? bar[4] : bar[5];

    // 获取技能位置

    var skillLoc = skills[skillIndex.x - 1, skillIndex.y - 1];
    var barLoc = bar[barIndex - 1];

    // 打开技能面板

    Press(VirtualKeyCode.VK_S);

    sim.Mouse.Sleep(cd0);

    // 将可能处于冷却中的技能移除

    RClick(barLoc);

    sim.Mouse.Sleep(cd0);

    // 交换技能位置

    LClick(altLoc);

    sim.Mouse.Sleep(cd0);

    LClick(barLoc);

    sim.Mouse.Sleep(cd0);

    // 放置新技能 (在新空槽)

    LClick(skillLoc);

    sim.Mouse.Sleep(cd0);

    LClick(altLoc);

    sim.Mouse.Sleep(cd0);

    // 交换技能位置

    LClick(barLoc);

    sim.Mouse.Sleep(cd0);

    LClick(altLoc);

    sim.Mouse.Sleep(cd0);

    // 关闭技能面板

    LClick(close);

    sim.Mouse.Sleep(cd0);

    // 还原鼠标位置

    Cursor.Position = pos;
    Thread.Sleep(10);

    // 是否自动使用技能(0=不释放, 其它=槽位)
    if (autoTrigger)
    {
        Press(keyMapping[barIndex - 1]);
    }
}