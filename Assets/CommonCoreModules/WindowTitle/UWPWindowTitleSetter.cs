#if UNITY_WSA && ENABLE_WINMD_SUPPORT
using Windows.UI.ViewManagement;

public static class UWPWindowTitleSetter
{
    /// <summary>
    /// If set, ignore calls to SetWindowTitle on this platform
    /// </summary>
    private static readonly bool IgnoreOnPlatform = false;

    internal static void SetWindowTitle(string windowTitle)
    {
        if (IgnoreOnPlatform)
            return;

        ApplicationView appView = ApplicationView.GetForCurrentView();
        appView.Title = windowTitle;
    }
}
#endif