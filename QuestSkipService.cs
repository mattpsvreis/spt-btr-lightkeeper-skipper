using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Dark.BTRLightkeeperSkipper.Patches;
using EFT;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dark.BTRLightkeeperSkipper
{
    internal static class QuestSkipService
    {
        private static Type _underlyingQuestControllerType;
        private static DefaultUIButton _buttonTemplate;

        internal static void CacheButtonTemplate(DefaultUIButton button)
        {
            if (button == null || _buttonTemplate != null)
            {
                return;
            }

            _buttonTemplate = button;
        }

        internal static DefaultUIButton ResolveButtonTemplate(DefaultUIButton preferred, Transform searchRoot)
        {
            if (preferred != null)
            {
                CacheButtonTemplate(preferred);
                return preferred;
            }

            if (_buttonTemplate != null)
            {
                return _buttonTemplate;
            }

            if (searchRoot != null)
            {
                var fromParents = searchRoot.GetComponentsInParent<DefaultUIButton>(true)
                    .FirstOrDefault(b => b != null && b.name != Plugin.SkipButtonName && b.name != Plugin.FinishButtonName);

                if (fromParents != null)
                {
                    CacheButtonTemplate(fromParents);
                    return fromParents;
                }
            }

            var any = Object.FindObjectsOfType<DefaultUIButton>()
                .FirstOrDefault(b => b != null && b.name != Plugin.SkipButtonName && b.name != Plugin.FinishButtonName);

            if (any != null)
            {
                CacheButtonTemplate(any);
            }

            return any;
        }

        internal static DefaultUIButton CreateButton(
            DefaultUIButton template,
            Transform parent,
            string objectName,
            string label,
            float minWidth = 100f)
        {
            var button = Object.Instantiate(template, parent);
            button.gameObject.name = objectName;
            button.SetRawText(label, 22);

            var layout = button.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layout != null)
            {
                layout.minWidth = minWidth;
            }

            button.OnClick.RemoveAllListeners();
            return button;
        }

        internal static void SkipCondition(QuestController questController, Quest quest, Condition condition)
        {
            if (quest == null || condition == null || questController == null)
            {
                return;
            }

            if (quest.IsConditionDone(condition))
            {
                return;
            }

            Plugin.Logger.LogDebug($"Skipping condition {condition.id} on quest {quest.Id} to value {condition.value}");

            // Force condition checkers to report the goal value as current progress.
            quest.ProgressCheckers[condition].SetCurrentValueGetter(_ => condition.value);
            SetConditionCurrentValue(questController, quest, condition);
        }

        internal static void FinishQuest(QuestController questController, Quest quest)
        {
            if (questController == null || quest == null)
            {
                return;
            }

            if (quest.QuestStatus != EQuestStatus.AvailableForFinish)
            {
                Plugin.Logger.LogWarning(
                    $"Tried to finish quest {quest.Id} but status is {quest.QuestStatus}, expected AvailableForFinish");
                return;
            }

            if (Plugin.Instance == null)
            {
                Plugin.Logger.LogError("Plugin instance missing; cannot finish quest on main thread.");
                return;
            }

            Plugin.Logger.LogInfo($"Finishing quest {quest.Id} ({quest.Template?.Name}) from Tasks menu");
            Plugin.Instance.StartCoroutine(FinishQuestAndRefreshTasks(questController, quest));
        }

        private static IEnumerator FinishQuestAndRefreshTasks(QuestController questController, Quest quest)
        {
            var finishTask = questController.FinishQuest(quest, runNetworkTransaction: true);
            while (finishTask != null && !finishTask.IsCompleted)
            {
                yield return null;
            }

            if (finishTask == null)
            {
                Plugin.Logger.LogError($"FinishQuest returned null for quest {quest.Id}");
                yield break;
            }

            if (finishTask.IsFaulted)
            {
                Plugin.Logger.LogError($"FinishQuest failed for quest {quest.Id}: {finishTask.Exception?.Flatten().Message}");
                yield break;
            }

            // Let profile/UI listeners apply the Success status before rebuilding the list.
            yield return null;
            RefreshTasksPanel();
        }

        internal static void RefreshTasksPanel()
        {
            var panel = Object.FindObjectOfType<TasksPanel>();
            if (panel == null)
            {
                Plugin.Logger.LogDebug("TasksPanel not found; skipping Tasks menu refresh.");
                return;
            }

            var showQuests = AccessTools.Method(typeof(TasksPanel), "ShowQuests", [typeof(Func<Quest, bool>)]);
            if (showQuests == null)
            {
                Plugin.Logger.LogWarning("Could not find TasksPanel.ShowQuests for UI refresh.");
                return;
            }

            Plugin.Logger.LogDebug("Refreshing TasksPanel after quest finish");
            showQuests.Invoke(panel, [new Func<Quest, bool>(_ => true)]);
            QuestObjectiveViewPatch.LastSeenObjectivesBlock = null;
        }

        private static void ResolveQuestControllerClass()
        {
            if (_underlyingQuestControllerType != null)
            {
                return;
            }

            _underlyingQuestControllerType = AccessTools.GetTypesFromAssembly(typeof(AbstractGame).Assembly)
                .SingleOrDefault(t =>
                    t.GetEvent(
                        "OnConditionQuestTimeExpired",
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public |
                        BindingFlags.Instance
                    ) != null
                );

            if (_underlyingQuestControllerType == null)
            {
                Plugin.Logger.LogError("Failed to locate ConditionsConnectorsManagerClient");
                return;
            }

            Plugin.Logger.LogDebug($"Resolved underlying quest controller type to {_underlyingQuestControllerType.FullName}");
        }

        private static void SetConditionCurrentValue(QuestController questController, Quest quest, Condition condition)
        {
            ResolveQuestControllerClass();
            if (_underlyingQuestControllerType == null)
            {
                return;
            }

            var conditionControllerField = questController.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(field =>
                    field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == _underlyingQuestControllerType
                );

            if (conditionControllerField == null)
            {
                Plugin.Logger.LogError(
                    $"Failed to locate {_underlyingQuestControllerType.Name} field on {questController.GetType().Name}");
                return;
            }

            var conditionController = conditionControllerField.GetValue(questController);
            AccessTools.Method(conditionController.GetType(), "SetConditionCurrentValue")?.Invoke(
                conditionController,
                [
                    quest,
                    EQuestStatus.AvailableForFinish,
                    condition,
                    condition.value,
                    true
                ]);
        }
    }
}
