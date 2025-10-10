using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    [InitializeOnLoad]
    public static class Doxygen
    {
        private const string MenuName = "Tools/Doxygen/Auto-Refresh";
        private const string SettingName = "Tools_Doxygen_Auto-Refresh";
        public static bool AutoRefreshEnabled
        {
            get => EditorPrefs.GetBool(SettingName, true);
            set => EditorPrefs.SetBool(SettingName, value);
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnCompilationFinished()
        {
            if (AutoRefreshEnabled)
                DoxygenRefresh();
        }

        [MenuItem("Tools/Doxygen/Open")]
        public static void DoxygenOpen()
        {
            var workingDirectory = Directory.GetParent($"{Application.dataPath}/../Docs/html/");
            if (workingDirectory == null)
                return;
            
            Debug.Log(workingDirectory.FullName);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "index.html",
                WorkingDirectory = workingDirectory.FullName
            };

            Process.Start(processInfo);
        }
        
        [MenuItem("Tools/Doxygen/Refresh")]
        public static void DoxygenRefresh()
        {
            var workingDirectory = Directory.GetParent(Application.dataPath);
            if (workingDirectory == null)
                return;
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-command \"doxygen Docs.doxy\"",
                WorkingDirectory = workingDirectory.FullName,
                //UseShellExecute = false,
                //RedirectStandardError = true,
                //RedirectStandardInput = true,
                //RedirectStandardOutput = true,
                //ErrorDialog = false,
            };
            
            Process.Start(processInfo);
        }

        [MenuItem(MenuName)]
        public static void ToggleAutoRefresh()
        {
            AutoRefreshEnabled ^= true;
        }
        
        [MenuItem(MenuName, true)]
        public static bool ToggleAutoRefreshValidate()
        {
            Menu.SetChecked(MenuName, AutoRefreshEnabled);
            return true;
        }
    }
}