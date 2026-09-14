using System;
using System.IO;
using UnityEngine;

namespace _001_Scripts.Core
{
    public sealed class DebugHandler : MonoBehaviour
    {
        [SerializeField] private string Path = "Logs/DebugLog.txt";

        private string _fullPath;

        private void Awake()
        {
            InitializePath();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += Debug2Txt;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= Debug2Txt;
        }

        private void InitializePath()
        {
            if (!System.IO.Path.IsPathRooted(Path))
            {
                _fullPath = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    Path
                );
            }
            else
            {
                _fullPath = Path;
            }

            string directory = System.IO.Path.GetDirectoryName(_fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void Debug2Txt(
            string condition,
            string stackTrace,
            LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    WriteLog(condition, stackTrace, type);
                    break;
            }
        }

        private void WriteLog(
            string condition,
            string stackTrace,
            LogType type)
        {
            string log =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n" +
                $"Type: {type}\n" +
                $"Message: {condition}\n" +
                $"StackTrace:\n{stackTrace}\n" +
                $"{new string('-', 80)}\n";

            try
            {
                File.AppendAllText(_fullPath, log);
            }
            catch (Exception)
            {
                // never call Debug.Log();
            }
        }
    }
}