using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace WinCry.Models
{
    #region user32.dll's SetWindowCompositionAttribute Members

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    #endregion

    /// <summary>
    /// Applies blur effects on System.Windows.Window
    /// </summary>
    public static class WindowBlur
    {
        private static Effect effectBuffer;
        private static Brush brushBuffer;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        /// <summary>
        /// Enables Blur Effect behind transparent window
        /// </summary>
        /// <param name="window">Window to apply blur behind to</param>
        public static void ApplyBlurBehind(Window window, bool enable = true)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy();

            if (enable)
            {
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            }
            else
            {
                accent.AccentState = AccentState.ACCENT_DISABLED;

                var _backgroundColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF1B1B1B");
                window.Dispatcher.Invoke(new Action(delegate { window.Background = _backgroundColor; }), DispatcherPriority.Normal);
            }

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        /// <summary>
        /// Removes Blur Effect behind transparent window
        /// </summary>
        /// <param name="window">Window to remove blur behind from</param>
        public static void RemoveBlurBehind(Window window)
        {
            ApplyBlurBehind(window, false);
        }

        /// <summary>
        /// Applies Blur Effects
        /// </summary>
        /// <param name="window">Window to apply effect onto</param>
        public static void ApplyBlur(this Window window)
        {
            // Create new effective objects
            var _blurEffect = new BlurEffect { Radius = 3, RenderingBias = RenderingBias.Quality };
            var _opacityMask = new SolidColorBrush(Colors.Black) { Opacity = .8 };

            // Buffering ...
            effectBuffer = window.Effect;
            brushBuffer = window.OpacityMask;

            // Change this.win effective objects
            window.Dispatcher.Invoke(new Action(delegate { window.Effect = _blurEffect; }), DispatcherPriority.Normal);
            window.Dispatcher.Invoke(new Action(delegate { window.OpacityMask = _opacityMask; }), DispatcherPriority.Normal);
        }

        /// <summary>
        /// Removes Blur Effects
        /// </summary>
        /// <param name="window">Window to remove effect from</param>
        public static void RemoveBlur(this Window window)
        {
            window.Dispatcher.Invoke(new Action(delegate { window.Effect = effectBuffer; }), DispatcherPriority.Normal);
            window.Dispatcher.Invoke(new Action(delegate { window.OpacityMask = brushBuffer; }), DispatcherPriority.Normal);
            window.Dispatcher.Invoke(new Action(delegate { window.Focus(); }), DispatcherPriority.Normal);
        }
    }
}