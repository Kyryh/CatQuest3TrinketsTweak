using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ProjectStar.Data;
using System.Collections.Generic;
using System.Linq;

namespace CatQuest3TrinketsTweak;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    static HashSet<string> tweakedTrinkets = new(); 

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    static EquipmentItemData GetTrinket(string name) {
        return AddressableSingletonScriptableObject<EquipmentDatabase>.Instance.contentTable.Values.FirstOrDefault(eq => eq.name == $"Equipment_Trinket_{name}");
    }

    public static void TweakTrinket(string trinketToTweak, params string[] trinketsToUse) {
        if (IsTweaked(trinketToTweak))
            return;
        var trinket = GetTrinket(trinketToTweak);
        var trinkets = trinketsToUse.Select(GetTrinket);
        trinket.passiveAbilityDescription = trinkets.Join(t => t.passiveAbilityDescription, "\n");
        var abilities = trinkets.SelectMany(t => t.abilities.abilityList).ToList();
        trinket.abilities.abilityList = abilities;
        tweakedTrinkets.Add($"Equipment_Trinket_{trinketToTweak}");
    }

    public static bool IsTweaked(string trinkedName) {
        return tweakedTrinkets.Contains(trinkedName);
    }
}


[HarmonyPatch(typeof(Prologue))]
class ProloguePatch {
    [HarmonyPatch(nameof(Prologue.Start))]
    [HarmonyPostfix]
    static void StartPostfix() {
        Plugin.Logger.LogInfo("Tweaking Lousy Boot");
        Plugin.TweakTrinket("LousyBoot", "Warrior'sBraid", "OinkerNecklace", "BoarTusk", "MilkPawer");
    }
}

[HarmonyPatch(typeof(EquipmentItemData))]
class EquipmentItemDataPatch {
    [HarmonyPatch(nameof(EquipmentItemData.GetDescription))]
    [HarmonyPrefix]
    static bool GetDescriptionPrefix(EquipmentItemData __instance, ref (string, string) __result) {
        if (Plugin.IsTweaked(__instance.name)) {
            __result = ("", __instance.passiveAbilityDescription);
            return false;
        }
        return true;
    }
}