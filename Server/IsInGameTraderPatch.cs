using System.Reflection;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Quest;

namespace Dark.BTRLightkeeperSkipper.Server;

public class IsInGameTraderPatch : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(QuestRewardHelper).GetMethod(
            "IsInGameTrader",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException("QuestRewardHelper.IsInGameTrader not found");
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        // Treat BTR/Lightkeeper like normal traders so CompleteQuest mails rewards.
        __result = false;
        return false;
    }
}
