using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Economy
{
    public class ChapterTreasure
    {
        public bool CanClaim(ChapterMilestone milestone, Player player)
        {
            if (player.ClaimedMilestones.Contains(milestone.Id)) return false;
            int bestDay = player.BestSurvivalDays.TryGetValue(milestone.ChapterId, out var val) ? val : 0;

            if (milestone.Type == "CLEAR")
                return bestDay >= ChapterTreasureTable.GetClearSentinelDay(milestone.ChapterId);

            return bestDay >= milestone.RequiredDay;
        }

        public Result Claim(ChapterMilestone milestone, Player player)
        {
            if (player.ClaimedMilestones.Contains(milestone.Id))
                return Result.Fail("\uc774\ubbf8 \uc218\ub839\ud55c \ubcf4\uc0c1\uc785\ub2c8\ub2e4");

            if (!CanClaim(milestone, player))
                return Result.Fail("\uc218\ub839 \uc870\uac74\uc744 \ub9cc\uc871\ud558\uc9c0 \uc54a\uc2b5\ub2c8\ub2e4");

            player.ClaimedMilestones.Add(milestone.Id);

            foreach (var resource in milestone.MilestoneReward.Resources)
            {
                player.Resources.Add(resource.Type, resource.Amount);
            }

            return Result.Ok();
        }

        public string GetMilestoneStatus(ChapterMilestone milestone, Player player)
        {
            if (player.ClaimedMilestones.Contains(milestone.Id)) return "claimed";
            if (CanClaim(milestone, player)) return "claimable";
            return "locked";
        }
    }
}
