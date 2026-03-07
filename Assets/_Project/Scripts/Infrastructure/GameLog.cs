using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatCatGo.Infrastructure
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
    }

    public struct LogEntry
    {
        public float Time;
        public LogLevel Level;
        public string Tag;
        public string Message;
    }

    public static class GameLog
    {
        public static LogLevel MinLevel { get; set; } = LogLevel.Debug;

        private static readonly List<LogEntry> _entries = new();
        private static int _maxEntries = 200;

        public static event Action<LogEntry> OnLogAdded;

        public static IReadOnlyList<LogEntry> Entries => _entries;

        public static void D(string tag, string message) => Log(LogLevel.Debug, tag, message);
        public static void I(string tag, string message) => Log(LogLevel.Info, tag, message);
        public static void W(string tag, string message) => Log(LogLevel.Warn, tag, message);
        public static void E(string tag, string message) => Log(LogLevel.Error, tag, message);

        private static void Log(LogLevel level, string tag, string message)
        {
            if (level < MinLevel) return;

            var entry = new LogEntry
            {
                Time = Time.realtimeSinceStartup,
                Level = level,
                Tag = tag,
                Message = message,
            };

            _entries.Add(entry);
            if (_entries.Count > _maxEntries)
                _entries.RemoveAt(0);

            OnLogAdded?.Invoke(entry);

            string formatted = $"[{tag}] {message}";
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formatted);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(formatted);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formatted);
                    break;
            }
        }

        public static void Clear()
        {
            _entries.Clear();
        }
    }
}
