using Dark.BTRLightkeeperSkipper.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace Dark.BTRLightkeeperSkipper
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.terkoiz.skipper", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.SPT.core", "4.1.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dark.btrlightkeeperskipper";
        public const string PluginName = "Dark-BTRLightkeeperSkipper";
        public const string PluginVersion = "1.1.0";

        internal const string SkipButtonName = "BTRLightkeeperSkipper_SkipButton";
        internal const string FinishButtonName = "BTRLightkeeperSkipper_FinishButton";

        internal static Plugin Instance { get; private set; }
        internal new static ManualLogSource Logger { get; private set; }

        private const string MainSectionName = "Main";
        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<bool> AlwaysDisplay;
        internal static ConfigEntry<bool> ShowFinishButton;
        internal static ConfigEntry<KeyboardShortcut> DisplayHotkey;

        [UsedImplicitly]
        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            InitConfiguration();

            new QuestObjectiveViewPatch().Enable();
            new NotesTaskDescriptionPatch().Enable();
        }

        private void InitConfiguration()
        {
            ModEnabled = Config.Bind(
                MainSectionName,
                "1. Enabled",
                true,
                "Enable Skip/Finish buttons for BTR Driver and Lightkeeper quests in the Tasks menu.");

            AlwaysDisplay = Config.Bind(
                MainSectionName,
                "2. Always display buttons",
                false,
                "If enabled, Skip/Finish buttons are always visible. Otherwise hold the hotkey.");

            DisplayHotkey = Config.Bind(
                MainSectionName,
                "3. Display hotkey",
                new KeyboardShortcut(KeyCode.LeftControl),
                "Holding this key reveals Skip/Finish buttons (same idea as Skipper).");

            ShowFinishButton = Config.Bind(
                MainSectionName,
                "4. Show Finish button",
                true,
                "When a BTR/Lightkeeper quest is ready to turn in, show a Finish button in the Tasks menu description.");
        }

        [UsedImplicitly]
        private void Update()
        {
            if (!ModEnabled.Value || AlwaysDisplay.Value)
            {
                return;
            }

            if (QuestObjectiveViewPatch.LastSeenObjectivesBlock == null ||
                !QuestObjectiveViewPatch.LastSeenObjectivesBlock.activeSelf)
            {
                return;
            }

            if (DisplayHotkey.Value.IsDown())
            {
                ChangeButtonVisibility(true);
            }

            if (DisplayHotkey.Value.IsUp())
            {
                ChangeButtonVisibility(false);
            }
        }

        private static void ChangeButtonVisibility(bool setVisibilityTo)
        {
            foreach (var button in QuestObjectiveViewPatch.LastSeenObjectivesBlock
                         .GetComponentsInChildren<DefaultUIButton>(includeInactive: true))
            {
                if (button.name != SkipButtonName && button.name != FinishButtonName)
                {
                    continue;
                }

                button.gameObject.SetActive(setVisibilityTo);
            }
        }
    }
}
