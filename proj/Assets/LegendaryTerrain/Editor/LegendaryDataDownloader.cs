using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace LegendaryTerrain.Editor
{
    public static class LegendaryDataDownloader
    {
        private const string DataPath = "StreamingAssets/LegendaryData";
        private const string CrystalDbUrl = "https://github.com/Suprcode/Crystal.Database.git";

        [MenuItem("Tools/Legendary/Download Crystal Data")]
        public static void Download()
        {
            string basePath = Path.Combine(Application.dataPath, DataPath);
            string dbPath = Path.Combine(basePath, "Crystal.Database");

            Directory.CreateDirectory(basePath);

            if (Directory.Exists(Path.Combine(dbPath, ".git")))
            {
                UnityEngine.Debug.Log("Crystal.Database exists, pulling...");
                RunGit(dbPath, "pull");
            }
            else
            {
                UnityEngine.Debug.Log("Cloning Crystal.Database...");
                RunGit(basePath, $"clone --depth 1 {CrystalDbUrl} Crystal.Database");
            }

            CopyJevData(basePath, dbPath);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("Legendary data ready at " + basePath);
        }

        private static void CopyJevData(string basePath, string dbPath)
        {
            string jev = Path.Combine(dbPath, "Jev");
            string destMaps = Path.Combine(basePath, "Maps");
            string destEnvir = Path.Combine(basePath, "Envir");

            if (Directory.Exists(Path.Combine(jev, "Maps")))
            {
                CopyDir(Path.Combine(jev, "Maps"), destMaps);
            }
            if (Directory.Exists(Path.Combine(jev, "Envir")))
            {
                CopyDir(Path.Combine(jev, "Envir"), destEnvir);
            }

            string mirDb = Path.Combine(jev, "Server.MirDB");
            if (File.Exists(mirDb))
            {
                File.Copy(mirDb, Path.Combine(basePath, "Server.MirDB"), true);
            }
        }

        private static void CopyDir(string src, string dest)
        {
            if (Directory.Exists(dest)) Directory.Delete(dest, true);
            Directory.CreateDirectory(dest);
            foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                string rel = f.Substring(src.Length + 1);
                string d = Path.Combine(dest, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(d));
                File.Copy(f, d, true);
            }
        }

        private static void RunGit(string cwd, string args)
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = cwd,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var p = Process.Start(psi);
            p.WaitForExit(60000);
            string err = p.StandardError.ReadToEnd();
            if (p.ExitCode != 0) UnityEngine.Debug.LogError("git: " + err);
        }
    }
}
