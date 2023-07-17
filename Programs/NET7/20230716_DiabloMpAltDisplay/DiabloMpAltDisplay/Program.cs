using DiabloMpAltDisplay;

//Debug.Run();

var barState = new BarStatus(Screen.PrimaryScreen.WorkingArea.Size);

while (true)
{
    Console.SetCursorPosition(0, 0);
    Console.Write($"HP: {barState.Hp}, MP: {barState.Mp}");
    Thread.Sleep(100);
}