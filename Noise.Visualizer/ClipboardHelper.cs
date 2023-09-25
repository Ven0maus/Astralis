using System;
using System.Runtime.InteropServices;

namespace Noise.Visualizer
{
    internal static class ClipboardHelper
    {
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern bool GlobalUnlock(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;

        public static void SetText(string text)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                // Clear the clipboard
                EmptyClipboard();

                // Allocate global memory for the text
                IntPtr hGlobal = GlobalAlloc(0x2000, (UIntPtr)((text.Length + 1) * 2));

                // Lock the memory and copy the text
                IntPtr lpGlobal = GlobalLock(hGlobal);
                Marshal.Copy(text.ToCharArray(), 0, lpGlobal, text.Length);
                GlobalUnlock(hGlobal);

                // Set the data on the clipboard
                SetClipboardData(CF_UNICODETEXT, hGlobal);

                // Close the clipboard
                CloseClipboard();

                Console.WriteLine("Text copied to clipboard successfully.");
            }
            else
            {
                Console.WriteLine("Failed to open the clipboard.");
            }
        }

        public static string GetText()
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                IntPtr clipboardData = GetClipboardData(CF_UNICODETEXT);

                if (clipboardData != IntPtr.Zero)
                {
                    IntPtr lockedMemory = GlobalLock(clipboardData);

                    if (lockedMemory != IntPtr.Zero)
                    {
                        string text = Marshal.PtrToStringUni(lockedMemory);

                        // Release the locked memory
                        GlobalUnlock(lockedMemory);

                        // Close the clipboard
                        CloseClipboard();

                        return text;
                    }
                }

                // Close the clipboard
                CloseClipboard();
            }
            else
            {
                Console.WriteLine("Failed to open the clipboard.");
            }

            return null;
        }
    }
}
