#if UNITY_STANDALONE_WIN
using CommonCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Window title setter for Windows, using win32 API
/// </summary>
/// <remarks>
/// <para>mostly based on examples from pinvoke.net</para>
/// </remarks>
internal static class WindowsWindowTitleSetter
{
    /// <summary>
    /// If set, ignore calls to SetWindowTitle on this platform
    /// </summary>
    private static readonly bool IgnoreOnPlatform = false;

    private const string MainWindowClassName = "UnityWndClass";

    private delegate bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsCallback callPtr, IntPtr lPar);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowText(IntPtr hwnd, String lpString);

    internal static void SetWindowTitle(string windowTitle)
    {
        if (IgnoreOnPlatform)
            return;

        uint processID = GetCurrentProcessId();

        //this does seem to work despite warnings to the contrary
        //var id = Process.GetCurrentProcess().Id;
        //UnityEngine.Debug.Log($"win32 process ID: {processID} | Mono process ID: {id}");

        //get the window handles
        List<IntPtr> windowHandles = new List<IntPtr>();
        EnumWindowsCallback callback = delegate (IntPtr hwnd, IntPtr lParam) { windowHandles.Add(hwnd); return true; }; //surprisingly, this works
        bool didEnumWindows = EnumWindows(callback, IntPtr.Zero);
        if (!didEnumWindows)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        //UnityEngine.Debug.Log(windowHandles.ToNiceString());

        //check the window handles against our process id
        IntPtr mainWindowHandle = IntPtr.Zero;
        foreach(var windowHandle in windowHandles)
        {
            uint windowProcessId = 0;
            if (GetWindowThreadProcessId(windowHandle, out windowProcessId) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            if(windowProcessId == processID)
            {
                string windowClassName = GetClassName(windowHandle);
                if (windowClassName == MainWindowClassName)
                {
                    mainWindowHandle = windowHandle;
                    break;
                    //UnityEngine.Debug.Log($"Found our window! PID={processID}, hwnd={windowHandle}, text={GetText(windowHandle)}, class={GetClassName(windowHandle)}");
                }
            }            
        }

        //on the editor, we stop short of actually setting the window title
        if(!CoreParams.IsEditor)
        {
            if (mainWindowHandle == IntPtr.Zero)
            {
                throw new KeyNotFoundException("Could not find main window of application");
            }

            bool result = SetWindowText(mainWindowHandle, windowTitle);
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private static string GetText(IntPtr hWnd)
    {
        // Allocate correct string length first
        int length = GetWindowTextLength(hWnd);
        if (length == 0)
            return null;
        StringBuilder sb = new StringBuilder(length + 1);
        int result = GetWindowText(hWnd, sb, sb.Capacity);
        if (result == 0)
            return null;
        return sb.ToString();
    }

    private static string GetClassName(IntPtr hWnd)
    {
        StringBuilder sb = new StringBuilder(256);
        int result = GetClassName(hWnd, sb, sb.Capacity);
        if (result == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return sb.ToString();
    }
}
#endif