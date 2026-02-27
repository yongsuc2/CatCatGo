using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Meta
{
    public class RoutineStep
    {
        public RoutineAction Action;
        public int Priority;
        public string Description;
        public ResourceType? RequiredResource;
    }

    public class RoutineStatus
    {
        public RoutineAction Action;
        public bool Available;
        public string Reason;
    }

    public class RoutineContext
    {
        public int DungeonDragonRemaining;
        public int DungeonCelestialRemaining;
        public int DungeonSkyRemaining;
        public int ChallengeTokens;
        public int ArenaTickets;
        public int Stamina;
        public int Pickaxes;
    }

    public class DailyRoutineScheduler
    {
        private static readonly RoutineStep[] DailyRoutine =
        {
            new RoutineStep { Action = RoutineAction.DAILY_DUNGEON_DRAGON, Priority = 1, Description = "Dragon Nest dungeon", RequiredResource = null },
            new RoutineStep { Action = RoutineAction.DAILY_DUNGEON_CELESTIAL, Priority = 2, Description = "Celestial Tree dungeon", RequiredResource = null },
            new RoutineStep { Action = RoutineAction.DAILY_DUNGEON_SKY, Priority = 3, Description = "Sky Island dungeon", RequiredResource = null },
            new RoutineStep { Action = RoutineAction.TOWER_CHALLENGE, Priority = 4, Description = "Tower challenge", RequiredResource = ResourceType.CHALLENGE_TOKEN },
            new RoutineStep { Action = RoutineAction.CATACOMB_RUN, Priority = 5, Description = "Catacomb dungeon run", RequiredResource = null },
            new RoutineStep { Action = RoutineAction.ARENA_FIGHT, Priority = 6, Description = "Arena PvP", RequiredResource = ResourceType.ARENA_TICKET },
            new RoutineStep { Action = RoutineAction.CHAPTER_PROGRESS, Priority = 7, Description = "Chapter progress", RequiredResource = ResourceType.STAMINA },
            new RoutineStep { Action = RoutineAction.TRAVEL, Priority = 8, Description = "Travel farming", RequiredResource = ResourceType.STAMINA },
            new RoutineStep { Action = RoutineAction.GOBLIN_MINE, Priority = 9, Description = "Goblin mining", RequiredResource = ResourceType.PICKAXE },
        };

        public List<RoutineStep> GetFullRoutine()
        {
            return new List<RoutineStep>(DailyRoutine);
        }

        public List<RoutineStatus> GetAvailableActions(RoutineContext context)
        {
            return DailyRoutine.Select(step =>
            {
                bool available = true;
                string reason = "Ready";

                switch (step.Action)
                {
                    case RoutineAction.DAILY_DUNGEON_DRAGON:
                        available = context.DungeonDragonRemaining > 0;
                        reason = available ? "Ready" : "Daily limit reached";
                        break;
                    case RoutineAction.DAILY_DUNGEON_CELESTIAL:
                        available = context.DungeonCelestialRemaining > 0;
                        reason = available ? "Ready" : "Daily limit reached";
                        break;
                    case RoutineAction.DAILY_DUNGEON_SKY:
                        available = context.DungeonSkyRemaining > 0;
                        reason = available ? "Ready" : "Daily limit reached";
                        break;
                    case RoutineAction.TOWER_CHALLENGE:
                        available = context.ChallengeTokens > 0;
                        reason = available ? "Ready" : "No challenge tokens";
                        break;
                    case RoutineAction.ARENA_FIGHT:
                        available = context.ArenaTickets > 0;
                        reason = available ? "Ready" : "No arena tickets";
                        break;
                    case RoutineAction.CHAPTER_PROGRESS:
                        available = context.Stamina >= 5;
                        reason = available ? "Ready" : "Not enough stamina";
                        break;
                    case RoutineAction.TRAVEL:
                        available = context.Stamina > 0;
                        reason = available ? "Ready" : "No stamina";
                        break;
                    case RoutineAction.GOBLIN_MINE:
                        available = context.Pickaxes > 0;
                        reason = available ? "Ready" : "No pickaxes";
                        break;
                    case RoutineAction.CATACOMB_RUN:
                        available = true;
                        break;
                }

                return new RoutineStatus { Action = step.Action, Available = available, Reason = reason };
            }).ToList();
        }

        public RoutineAction? GetNextAction(RoutineContext context)
        {
            var statuses = GetAvailableActions(context);
            var available = statuses.FirstOrDefault(s => s.Available);
            return available?.Action;
        }
    }
}
