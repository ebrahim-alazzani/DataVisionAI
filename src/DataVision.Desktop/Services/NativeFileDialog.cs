using System;
using System.Runtime.InteropServices;

namespace DataVision.Desktop.Services;

public static class NativeFileDialog
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string? filter = null;
        public string? customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public string? file = null;
        public int maxFile = 0;
        public string? fileTitle = null;
        public int maxFileTitle = 0;
        public string? initialDir = null;
        public string? title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public string? defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string? templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_EXPLORER = 0x00080000;

    public static string? ShowOpenFileDialog(IntPtr ownerHwnd, string title = "اختر ملف إكسل أو CSV")
    {
        var ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.dlgOwner = ownerHwnd;
        ofn.filter = "Excel & CSV Files (*.xlsx;*.csv;*.xls)\0*.xlsx;*.csv;*.xls\0All Files (*.*)\0*.*\0\0";
        ofn.file = new string(new char[1024]);
        ofn.maxFile = ofn.file.Length;
        ofn.title = title;
        ofn.flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_EXPLORER;

        if (GetOpenFileName(ofn))
        {
            return ofn.file.Split('\0')[0];
        }

        return null;
    }
}
