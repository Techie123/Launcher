using System.IO;

namespace Launcher
{
    public static class Helper
    {
        public static void CopyDirectory(string oldpath, string newpath)
        {
            foreach (string dirPath in Directory.GetDirectories(oldpath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(oldpath, newpath));

            foreach (string newPath in Directory.GetFiles(oldpath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(oldpath, newpath), true);
        }
    }
}
