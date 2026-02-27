using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Content
{
    public class Arena
    {
        private static readonly ArenaTier[] TierOrder = new[]
        {
            ArenaTier.BRONZE, ArenaTier.SILVER, ArenaTier.GOLD,
            ArenaTier.PLATINUM, ArenaTier.DIAMOND, ArenaTier.MASTER,
        };

        private struct TierStatRange
        {
            public int MinAtk;
            public int MaxAtk;
            public int MinHp;
            public int MaxHp;
            public int Def;
        }

        private static readonly Dictionary<ArenaTier, TierStatRange> TierStatRanges = new Dictionary<ArenaTier, TierStatRange>
        {
            { ArenaTier.BRONZE, new TierStatRange { MinAtk = 10, MaxAtk = 20, MinHp = 80, MaxHp = 150, Def = 3 } },
            { ArenaTier.SILVER, new TierStatRange { MinAtk = 20, MaxAtk = 40, MinHp = 150, MaxHp = 300, Def = 6 } },
            { ArenaTier.GOLD, new TierStatRange { MinAtk = 40, MaxAtk = 70, MinHp = 300, MaxHp = 500, Def = 10 } },
            { ArenaTier.PLATINUM, new TierStatRange { MinAtk = 70, MaxAtk = 120, MinHp = 500, MaxHp = 800, Def = 15 } },
            { ArenaTier.DIAMOND, new TierStatRange { MinAtk = 120, MaxAtk = 200, MinHp = 800, MaxHp = 1500, Def = 22 } },
            { ArenaTier.MASTER, new TierStatRange { MinAtk = 200, MaxAtk = 350, MinHp = 1500, MaxHp = 3000, Def = 30 } },
        };

        private const int PointsToPromote = 100;
        private const int DailyEntries = 5;

        public ArenaTier Tier;
        public int Points;
        public int TodayEntries;

        public Arena(ArenaTier tier = ArenaTier.BRONZE, int points = 0)
        {
            Tier = tier;
            Points = points;
            TodayEntries = 0;
        }

        public bool IsAvailable()
        {
            return TodayEntries < DailyEntries;
        }

        public int GetRemainingEntries()
        {
            return Math.Max(0, DailyEntries - TodayEntries);
        }

        public BattleUnit[] MatchOpponents(SeededRandom rng)
        {
            var range = TierStatRanges[Tier];
            var opponents = new BattleUnit[4];

            for (int i = 0; i < 4; i++)
            {
                int atk = rng.NextInt(range.MinAtk, range.MaxAtk);
                int hp = rng.NextInt(range.MinHp, range.MaxHp);
                var stats = Stats.Create(hp: hp, maxHp: hp, atk: atk, def: range.Def, crit: 0.05f);
                opponents[i] = new BattleUnit($"Opponent {i + 1}", stats, null, null, false);
            }

            return opponents;
        }

        public Result<ArenaFightResult> Fight(BattleUnit playerUnit, int ticketCount, SeededRandom rng)
        {
            if (ticketCount < 1)
                return Result.Fail<ArenaFightResult>("No arena tickets");

            if (!IsAvailable())
                return Result.Fail<ArenaFightResult>("No entries remaining today");

            TodayEntries += 1;
            var opponents = MatchOpponents(rng);
            var battles = new List<Battle.Battle>();
            var results = new List<BattleState>();

            foreach (var opponent in opponents)
            {
                var playerClone = new BattleUnit(
                    playerUnit.Name,
                    Stats.Create(
                        hp: playerUnit.MaxHp, maxHp: playerUnit.MaxHp,
                        atk: playerUnit.BaseAtk, def: playerUnit.BaseDef, crit: playerUnit.BaseCrit),
                    null,
                    null,
                    true);
                var battle = new Battle.Battle(playerClone, opponent, rng.NextInt(0, 999999));
                battle.RunToCompletion(50);
                battles.Add(battle);
                results.Add(battle.State);
            }

            int wins = results.Count(r => r == BattleState.VICTORY);
            UpdatePoints(wins);

            return Result.Ok(new ArenaFightResult { Battles = battles, Results = results });
        }

        private void UpdatePoints(int wins)
        {
            int pointsGained = wins * 30 - (4 - wins) * 10;
            Points = Math.Max(0, Points + pointsGained);

            if (Points >= PointsToPromote)
            {
                TryPromote();
            }
        }

        private void TryPromote()
        {
            int idx = Array.IndexOf(TierOrder, Tier);
            if (idx < TierOrder.Length - 1)
            {
                Tier = TierOrder[idx + 1];
                Points = 0;
            }
        }

        public Reward GetReward()
        {
            int tierIdx = Array.IndexOf(TierOrder, Tier);
            int gemReward = 20 + tierIdx * 15;
            int goldReward = 100 + tierIdx * 50;

            return Reward.FromResources(
                new ResourceReward(ResourceType.GEMS, gemReward),
                new ResourceReward(ResourceType.GOLD, goldReward));
        }

        public int GetTierIndex()
        {
            return Array.IndexOf(TierOrder, Tier);
        }

        public void DailyReset()
        {
            TodayEntries = 0;
        }
    }

    public class ArenaFightResult
    {
        public List<Battle.Battle> Battles;
        public List<BattleState> Results;
    }
}
