using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IWshRuntimeLibrary;

namespace Azazel.FileSystem {
    public enum IconSize {
        Small,
        Large
    }

    public class IconExtractor {
        public static readonly IconExtractor Instance = new IconExtractor();
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private IconExtractor() {}

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private static Icon ExtractIcon(FileSystemElement<FileInfo, File> file, IconSize size) {
            var shinfo = new SHFILEINFO();

            if (size == IconSize.Large)
                SHGetFileInfo(file.FullName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            else
                SHGetFileInfo(file.FullName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
            return Icon.FromHandle(shinfo.hIcon);
        }

        public ImageSource Extract(File file, IconSize iconSize) {
            try {
                var icon = ExtractIcon(ActualFile(file), iconSize);
                var bmp = icon.ToBitmap();
                var strm = new MemoryStream();
                bmp.Save(strm, ImageFormat.Png);
                return Extract(strm);
            }
            catch (ArgumentException) {
                return new BitmapImage();
            }
        }

        private static ImageSource Extract(Stream strm) {
            try {
                var bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                strm.Seek(0, SeekOrigin.Begin);
                bmpImage.StreamSource = strm;
                bmpImage.EndInit();
                return bmpImage;
            }
            catch {
                return new BitmapImage();
            }
        }

        private static File ActualFile(File file) {
            if (!file.FullName.EndsWith(".lnk")) return file;
            var linkPathName = file.FullName;
            var link = (IWshShortcut) new WshShell().CreateShortcut(linkPathName);
            if (string.IsNullOrEmpty(link.TargetPath) || !File.Exists(link.TargetPath)) return file;
            return new File(link.TargetPath);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        } ;

        public ImageSource Extract(File file) {
            return Extract(file, IconSize.Large);
        }
    }
}