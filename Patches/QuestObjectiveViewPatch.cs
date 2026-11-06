using System.Linq;
using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using JetBrains.Annotations;
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dark.BTRLightkeeperSkipper.Patches
{
    /// <summary>
    /// Adds Skip buttons to BTR Driver / Lightkeeper objectives in the Tasks menu,
    /// where Skipper intentionally does nothing because there is no handover button.
    /// </summary>
    public class QuestObjectiveViewPatch : ModulePatch
    {
        internal static GameObject LastSeenObjectivesBlock;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(QuestObjectiveView), nameof(QuestObjectiveView.Show));
        }

        [PatchPostfix]
        private static void PatchPostfix(
            [CanBeNull] DefaultUIButton ____handoverButton,
            QuestController questController,
            Condition condition,
            Quest quest,
            QuestObjectiveView __instance)
        {
            if (!Plugin.ModEnabled.Value || quest?.Template == null)
            {
                return;
            }

            // Always remember trader-screen handover buttons so Tasks-menu rows can clone them.
            QuestSkipService.CacheButtonTemplate(____handoverButton);

            if (!TraderIds.IsSupported(quest.Template.TraderId))
            {
                return;
            }

            // Skipper already handles trader screens that expose a handover button.
            if (____handoverButton != null)
            {
                return;
            }

            if (__instance.transform.Cast<Transform>().Any(t => t.name == Plugin.SkipButtonName))
            {
                return;
            }

            var template = QuestSkipService.ResolveButtonTemplate(null, __instance.transform);
            if (template == null)
            {
                Plugin.Logger.LogWarning(
                    "Could not find a DefaultUIButton template to clone for Tasks-menu Skip button.");
                return;
            }

            LastSeenObjectivesBlock = __instance.transform.parent != null
                ? __instance.transform.parent.gameObject
                : __instance.gameObject;

            var skipButton = QuestSkipService.CreateButton(
                template,
                __instance.transform,
                Plugin.SkipButtonName,
                "SKIP");

            var alreadyDone = quest.IsConditionDone(condition);
            skipButton.gameObject.SetActive(Plugin.AlwaysDisplay.Value && !alreadyDone);

            skipButton.OnClick.AddListener(() => ItemUiContext.Instance.ShowMessageWindow(
                description: "Are you sure you want to autocomplete this quest objective?",
                acceptAction: () =>
                {
                    if (quest.IsConditionDone(condition))
                    {
                        skipButton.gameObject.SetActive(false);
                        return;
                    }

                    QuestSkipService.SkipCondition(questController, quest, condition);
                    skipButton.gameObject.SetActive(false);
                    NotesTaskDescriptionPatch.TryInjectFinishButton(__instance, quest, questController);
                },
                cancelAction: () => { },
                caption: "Confirmation"));
        }
    }
}
