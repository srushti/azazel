using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel.Logging;
using IWshRuntimeLibrary;

namespace Azazel.FileSystem {
    public class IconExtractor {
        public static readonly IconExtractor Instance = new IconExtractor();
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private IconExtractor() {}

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static Icon ExtractIcon(File file) {
            var shinfo = new SHFILEINFO();

            SHGetFileInfo(file.FullName, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            return Icon.FromHandle(shinfo.hIcon);
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
            string linkPathName = file.FullName;
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
            try {
                if (new List<string> {".ico", ".png", ".jpg", ".gif"}.Contains(file.Extension.ToLower())) {
                    if (!file.Exists()) return new BitmapImage();
                    return new BitmapImage(new Uri(file.FullName));
                }
                Icon icon = ExtractIcon(ActualFile(file));
                Bitmap bmp = icon.ToBitmap();
                DestroyIcon(icon.Handle);
                var strm = new MemoryStream();
                bmp.Save(strm, ImageFormat.Png);
                return Extract(strm);
            }
            catch (ExternalException exception) {
                LogManager.WriteLog(exception);
                return new BitmapImage();
            }
            catch (DirectoryNotFoundException exception) {
                LogManager.WriteLog("Requested file: {0}", file);
                LogManager.WriteLog(exception);
                return new BitmapImage();
            }
            catch (ArgumentException exception) {
                LogManager.WriteLog("Requested file: {0}", file);
                LogManager.WriteLog(exception);
                return new BitmapImage();
            }
        }
    }
}