using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Chapter
{
    public class EncounterGenerator
    {
        private SeededRandom _rng;

        public EncounterGenerator(int seed)
        {
            _rng = new SeededRandom(seed);
        }

        public Encounter Generate(ChapterType chapterType, int day, List<SessionSkillWrapper> ownedSkills, int chapterId = 1)
        {
            var weights = EncounterDataTable.GetWeights(chapterType);
            var entries = weights.Select(w => (w.Type, (float)w.Weight)).ToList();
            var type = _rng.WeightedPick(entries);

            switch (type)
            {
                case EncounterType.DEMON: return CreateDemonEncounter(ownedSkills);
                case EncounterType.COMBAT: return CreateCombatEncounter();
                case EncounterType.CHANCE: return CreateChanceEncounter(ownedSkills, chapterId);
                default: return CreateCombatEncounter();
            }
        }

        public Encounter Regenerate(EncounterType type, List<SessionSkillWrapper> ownedSkills, int chapterId)
        {
            switch (type)
            {
                case EncounterType.DEMON: return CreateDemonEncounter(ownedSkills);
                case EncounterType.CHANCE: return CreateChanceEncounter(ownedSkills, chapterId);
                default: return CreateCombatEncounter();
            }
        }

        public Encounter GenerateJungbakRoulette(List<SessionSkillWrapper> ownedSkills)
        {
            return CreateJungbakRouletteEncounter(ownedSkills);
        }

        public Encounter GenerateDaebakRoulette(List<SessionSkillWrapper> ownedSkills)
        {
            return CreateDaebakRouletteEncounter(ownedSkills);
        }

        private List<SessionSkillWrapper> GetRandomSkills(int count, List<SessionSkillWrapper> ownedSkills, int? maxTier = null)
        {
            var pool = BuildSkillPool(ownedSkills);
            if (maxTier.HasValue)
            {
                pool = pool.Where(s => s.Tier <= maxTier.Value).ToList();
            }

            var result = new List<SessionSkillWrapper>();
            var candidates = new List<SessionSkillWrapper>(pool);
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int idx = _rng.NextInt(0, candidates.Count - 1);
                result.Add(candidates[idx]);
                candidates.RemoveAt(idx);
            }
            return result;
        }

        private Encounter CreateDemonEncounter(List<SessionSkillWrapper> ownedSkills)
        {
            var skills = GetRandomSkills(1, ownedSkills);
            var skill = skills.Count > 0 ? skills[0] : null;
            var d = EncounterDataTable.Demon;

            var options = new List<EncounterOption>();

            if (skill != null)
            {
                options.Add(new EncounterOption
                {
                    Label = d.SkillLabel(skill.Name),
                    Description = d.SkillDescription(skill.Description),
                    HpCostPercent = d.HpCostPercent,
                    GoldCost = 0,
                    SuccessRate = 1.0f,
                    Reward = SkillReward(new List<SessionSkillWrapper> { skill }),
                    SkillId = skill.Id,
                });
            }

            options.Add(new EncounterOption
            {
                Label = d.RejectLabel,
                Description = d.RejectDescription,
                HpCostPercent = 0,
                GoldCost = 0,
                SuccessRate = 1.0f,
                Reward = EmptyReward(),
            });

            return new Encounter(EncounterType.DEMON, options);
        }

        private Encounter CreateCombatEncounter()
        {
            return new Encounter(EncounterType.COMBAT, new List<EncounterOption>());
        }

        private Encounter CreateChanceEncounter(List<SessionSkillWrapper> ownedSkills, int chapterId)
        {
            var tier1Owned = ownedSkills.Where(s => s.Tier == 1 && !IsSpecialSkill(s.Id)).ToList();
            bool canSwap = tier1Owned.Count > 0;
            var d = EncounterDataTable.Chance;
            var w = d.SubWeights;
            var entries = new List<(int item, float weight)>
            {
                (0, w.ContainsKey("skillBox") ? w["skillBox"] : 0),
                (1, w.ContainsKey("spring") ? w["spring"] : 0),
                (2, w.ContainsKey("blessing") ? w["blessing"] : 0),
            };
            if (canSwap && w.ContainsKey("skillSwap"))
                entries.Add((3, w["skillSwap"]));

            int roll = _rng.WeightedPick(entries);
            var options = new List<EncounterOption>();

            switch (roll)
            {
                case 0:
                {
                    var skills = GetRandomSkills(3, ownedSkills);
                    foreach (var skill in skills)
                    {
                        options.Add(new EncounterOption
                        {
                            Label = skill.Name,
                            Description = skill.Description,
                            HpCostPercent = 0,
                            GoldCost = 0,
                            SuccessRate = 1.0f,
                            Reward = SkillReward(new List<SessionSkillWrapper> { skill }),
                            SkillId = skill.Id,
                        });
                    }
                    break;
                }
                case 1:
                    options.Add(new EncounterOption
                    {
                        Label = d.SpringLabel,
                        Description = d.SpringDescription,
                        HpCostPercent = 0,
                        GoldCost = 0,
                        SuccessRate = 1.0f,
                        Reward = HealReward(d.SpringHealPercent),
                    });
                    break;
                case 2:
                {
                    int goldAmount = d.BlessingGoldBase + _rng.NextInt(0, d.BlessingGoldPerChapter * chapterId);
                    options.Add(new EncounterOption
                    {
                        Label = d.BlessingLabel,
                        Description = $"\uace8\ub4dc {goldAmount} \ud68d\ub4dd",
                        HpCostPercent = 0,
                        GoldCost = 0,
                        SuccessRate = 1.0f,
                        Reward = ResourceRewardCreate(ResourceType.GOLD, goldAmount),
                    });
                    break;
                }
                case 3:
                {
                    var newSkills = GetRandomSkills(3, ownedSkills, 1);
                    if (newSkills.Count == 0)
                    {
                        options.Add(new EncounterOption
                        {
                            Label = d.SpringLabel,
                            Description = d.SpringDescription,
                            HpCostPercent = 0,
                            GoldCost = 0,
                            SuccessRate = 1.0f,
                            Reward = HealReward(d.SpringHealPercent),
                        });
                        break;
                    }
                    var oldSkill = _rng.Pick(tier1Owned);
                    var sw = EncounterDataTable.SkillSwap;
                    foreach (var newSkill in newSkills)
                    {
                        options.Add(new EncounterOption
                        {
                            Label = $"{oldSkill.Name} -> {newSkill.Name}",
                            Description = newSkill.Description,
                            HpCostPercent = 0,
                            GoldCost = 0,
                            SuccessRate = 1.0f,
                            Reward = SwapReward(newSkill, oldSkill.Id),
                            SkillId = newSkill.Id,
                        });
                    }
                    options.Add(new EncounterOption
                    {
                        Label = sw.SkipLabel,
                        Description = sw.SkipDescription,
                        HpCostPercent = 0,
                        GoldCost = 0,
                        SuccessRate = 1.0f,
                        Reward = EmptyReward(),
                    });
                    break;
                }
            }

            return new Encounter(EncounterType.CHANCE, options);
        }

        private Encounter CreateJungbakRouletteEncounter(List<SessionSkillWrapper> ownedSkills)
        {
            var d = EncounterDataTable.JungbakRoulette;
            var skills = GetRandomSkills(1, ownedSkills);
            var skill = skills.Count > 0 ? skills[0] : null;

            var options = new List<EncounterOption>
            {
                new EncounterOption
                {
                    Label = d.HealLabel,
                    Description = d.HealDescription,
                    HpCostPercent = 0,
                    GoldCost = 0,
                    SuccessRate = 1.0f,
                    Reward = HealReward(d.HealPercent),
                },
            };

            if (skill != null)
            {
                options.Add(new EncounterOption
                {
                    Label = d.SkillLabel(skill.Name),
                    Description = d.SkillDescription(skill.Description),
                    HpCostPercent = 0,
                    GoldCost = 0,
                    SuccessRate = 1.0f,
                    Reward = SkillReward(new List<SessionSkillWrapper> { skill }),
                    SkillId = skill.Id,
                });
            }

            options.Add(new EncounterOption
            {
                Label = d.GoldLabel,
                Description = d.GoldDescription,
                HpCostPercent = 0,
                GoldCost = 0,
                SuccessRate = 1.0f,
                Reward = ResourceRewardCreate(ResourceType.GOLD, d.GoldAmount),
            });

            return new Encounter(EncounterType.JUNGBAK_ROULETTE, options);
        }

        private Encounter CreateDaebakRouletteEncounter(List<SessionSkillWrapper> ownedSkills)
        {
            var ownedMap = new Dictionary<string, int>();
            foreach (var s in ownedSkills)
                ownedMap[s.Id] = s.Tier;

            var pool = BuildSkillPool(ownedSkills);
            var mythicPool = pool.Where(s => s.Tier == 3).ToList();
            var d = EncounterDataTable.DaebakRoulette;

            SessionSkillWrapper mythicSkill = mythicPool.Count > 0 ? _rng.Pick(mythicPool) : null;

            SessionSkillWrapper angelPower = null;
            if (!ownedMap.ContainsKey("angel_power"))
            {
                var passive = PassiveSkillRegistry.GetById("angel_power", 4);
                if (passive != null) angelPower = new SessionSkillWrapper(passive);
            }

            SessionSkillWrapper demonPower = null;
            if (!ownedMap.ContainsKey("demon_power"))
            {
                var active = ActiveSkillRegistry.GetById("demon_power", 4);
                if (active != null) demonPower = new SessionSkillWrapper(active);
            }

            var options = new List<EncounterOption>
            {
                new EncounterOption
                {
                    Label = d.NormalLabel,
                    Description = mythicSkill != null
                        ? $"{mythicSkill.Name}: {mythicSkill.Description}"
                        : d.NormalDescription,
                    HpCostPercent = 0,
                    GoldCost = 0,
                    SuccessRate = d.NormalRate,
                    Reward = SkillReward(mythicSkill != null ? new List<SessionSkillWrapper> { mythicSkill } : new List<SessionSkillWrapper>()),
                    SkillId = mythicSkill?.Id,
                },
            };

            if (angelPower != null)
            {
                options.Add(new EncounterOption
                {
                    Label = d.AngelLabel,
                    Description = $"{angelPower.Name}: {angelPower.Description}",
                    HpCostPercent = 0,
                    GoldCost = 0,
                    SuccessRate = d.AngelRate,
                    Reward = SkillReward(new List<SessionSkillWrapper> { angelPower }),
                    SkillId = angelPower.Id,
                });
            }

            if (demonPower != null)
            {
                options.Add(new EncounterOption
                {
                    Label = d.DemonLabel,
                    Description = $"{demonPower.Name}: {demonPower.Description}",
                    HpCostPercent = 0,
                    GoldCost = 0,
                    SuccessRate = d.DemonRate,
                    Reward = SkillReward(new List<SessionSkillWrapper> { demonPower }),
                    SkillId = demonPower.Id,
                });
            }

            options.Add(new EncounterOption
            {
                Label = d.SkipLabel,
                Description = d.SkipDescription,
                HpCostPercent = 0,
                GoldCost = 0,
                SuccessRate = 1.0f,
                Reward = EmptyReward(),
            });

            return new Encounter(EncounterType.DAEBAK_ROULETTE, options);
        }

        public static List<SessionSkillWrapper> BuildSkillPool(List<SessionSkillWrapper> ownedSkills)
        {
            var ownedMap = new Dictionary<string, int>();
            foreach (var s in ownedSkills)
                ownedMap[s.Id] = s.Tier;

            var pool = new List<SessionSkillWrapper>();

            foreach (var tier1 in ActiveSkillRegistry.GetUpperTier1Skills())
            {
                if (!ownedMap.ContainsKey(tier1.Id))
                    pool.Add(new SessionSkillWrapper(tier1));
            }

            foreach (var tier1 in PassiveSkillRegistry.GetTier1Skills())
            {
                if (!ownedMap.ContainsKey(tier1.Id))
                    pool.Add(new SessionSkillWrapper(tier1));
            }

            foreach (var kv in ownedMap)
            {
                string familyId = kv.Key;
                int currentTier = kv.Value;

                if (ActiveSkillRegistry.IsSpecialSkill(familyId) || PassiveSkillRegistry.IsSpecialSkill(familyId)) continue;
                if (ActiveSkillRegistry.IsBuiltinSkill(familyId)) continue;

                var nextActive = ActiveSkillRegistry.GetNextTier(familyId, currentTier);
                if (nextActive != null) { pool.Add(new SessionSkillWrapper(nextActive)); continue; }

                var nextPassive = PassiveSkillRegistry.GetNextTier(familyId, currentTier);
                if (nextPassive != null) { pool.Add(new SessionSkillWrapper(nextPassive)); }
            }

            return pool;
        }

        private static bool IsSpecialSkill(string id)
        {
            return ActiveSkillRegistry.IsSpecialSkill(id) || PassiveSkillRegistry.IsSpecialSkill(id);
        }

        private static EncounterReward EmptyReward()
        {
            return new EncounterReward();
        }

        private static EncounterReward SkillReward(List<SessionSkillWrapper> skills)
        {
            return new EncounterReward { Skills = skills };
        }

        private static EncounterReward HealReward(float percent)
        {
            return new EncounterReward { HealPercent = percent };
        }

        private static EncounterReward ResourceRewardCreate(ResourceType type, int amount)
        {
            return new EncounterReward
            {
                Reward = Reward.FromResources(new ResourceReward(type, amount)),
            };
        }

        private static EncounterReward SwapReward(SessionSkillWrapper newSkill, string oldSkillId)
        {
            return new EncounterReward
            {
                Skills = new List<SessionSkillWrapper> { newSkill },
                SkillIdsToRemove = new List<string> { oldSkillId },
            };
        }

        public static SessionSkillWrapper FindSkillById(string skillId)
        {
            foreach (var skill in ActiveSkillRegistry.GetAll())
            {
                if (skill.Id == skillId)
                    return new SessionSkillWrapper(skill);
            }
            foreach (var skill in PassiveSkillRegistry.GetAll())
            {
                if (skill.Id == skillId)
                    return new SessionSkillWrapper(skill);
            }
            return null;
        }
    }
}
