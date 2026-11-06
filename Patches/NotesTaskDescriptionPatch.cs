using System.Linq;
using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dark.BTRLightkeeperSkipper.Patches
{
    /// <summary>
    /// When a BTR/Lightkeeper quest is expanded in the Tasks menu and is ready to turn in,
    /// inject a Finish button onto the shared description panel.
    /// </summary>
    public class NotesTaskDescriptionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NotesTask), nameof(NotesTask.OpenTask));
        }

        [PatchPostfix]
        private static void PatchPostfix(NotesTask __instance)
        {
            var quest = AccessTools.Field(typeof(NotesTask), "_quest")?.GetValue(__instance) as Quest;
            var questController =
                AccessTools.Field(typeof(NotesTask), "_questController")?.GetValue(__instance) as QuestController;
            var description =
                AccessTools.Field(typeof(NotesTask), "_taskDescription")?.GetValue(__instance) as NotesTaskDescriptionShort;

            TryInjectFinishButton(description, quest, questController);
        }

        internal static void TryInjectFinishButton(
            Component searchRoot,
            Quest quest,
            QuestController questController)
        {
            if (!Plugin.ModEnabled.Value || !Plugin.ShowFinishButton.Value)
            {
                return;
            }

            if (quest?.Template == null || questController == null || searchRoot == null)
            {
                return;
            }

            if (!TraderIds.IsSupported(quest.Template.TraderId))
            {
                return;
            }

            var description = searchRoot as NotesTaskDescriptionShort
                              ?? searchRoot.GetComponentInParent<NotesTaskDescriptionShort>();
            if (description == null)
            {
                return;
            }

            ClearFinishButtons(description);

            if (quest.QuestStatus != EQuestStatus.AvailableForFinish)
            {
                return;
            }

            var template = QuestSkipService.ResolveButtonTemplate(null, description.transform);
            if (template == null)
            {
                Plugin.Logger.LogWarning("Could not find a DefaultUIButton template for Tasks-menu Finish button.");
                return;
            }

            var finishButton = QuestSkipService.CreateButton(
                template,
                description.transform,
                Plugin.FinishButtonName,
                "FINISH",
                minWidth: 120f);

            finishButton.gameObject.SetActive(Plugin.AlwaysDisplay.Value);
            QuestObjectiveViewPatch.LastSeenObjectivesBlock = description.gameObject;

            finishButton.OnClick.AddListener(() => ItemUiContext.Instance.ShowMessageWindow(
                description: "Are you sure you want to finish this quest and claim rewards via trader message?",
                acceptAction: () =>
                {
                    finishButton.gameObject.SetActive(false);
                    QuestSkipService.FinishQuest(questController, quest);
                },
                cancelAction: () => { },
                caption: "Confirmation"));
        }

        private static void ClearFinishButtons(NotesTaskDescriptionShort description)
        {
            foreach (var existing in description.GetComponentsInChildren<DefaultUIButton>(true)
                         .Where(b => b.name == Plugin.FinishButtonName)
                         .ToArray())
            {
                Object.Destroy(existing.gameObject);
            }
        }
    }
}
