using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Battle
{
    public class BattleLogEntry
    {
        public int Turn;
        public BattleLogType Type;
        public string Source;
        public string Target;
        public int Value;
        public string SkillName;
        public string SkillIcon;
        public string Message;
    }

    public class BattleLog
    {
        public List<BattleLogEntry> Entries = new List<BattleLogEntry>();

        public void Add(BattleLogEntry entry)
        {
            Entries.Add(entry);
        }

        public List<BattleLogEntry> GetEntriesForTurn(int turn)
        {
            return Entries.Where(e => e.Turn == turn).ToList();
        }

        public List<BattleLogEntry> GetLastEntries(int count)
        {
            int skip = Math.Max(0, Entries.Count - count);
            return Entries.Skip(skip).ToList();
        }

        public void Clear()
        {
            Entries = new List<BattleLogEntry>();
        }
    }
}
