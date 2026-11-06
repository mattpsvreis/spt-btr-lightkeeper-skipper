using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace Dark.BTRLightkeeperSkipper.Server;

/// <summary>
/// SPT skips item/standing rewards for BTR Driver and Lightkeeper on quest complete,
/// assuming those traders hand rewards out in-raid. Finishing from the Tasks menu never
/// hits that path, so this patch forces the normal mail reward flow instead.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.Preload)]
public class MailRewardsLoader(ISptLogger<MailRewardsLoader> logger) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        new IsInGameTraderPatch().Enable();
        logger.Success("BTR/Lightkeeper quest rewards will be delivered by trader message.");
        return Task.CompletedTask;
    }
}
