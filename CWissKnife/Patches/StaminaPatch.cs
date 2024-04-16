using HarmonyLib;
using CWissKnife;

[HarmonyPatch(typeof(PlayerController), "Update")]
public class StaminaPatch {
    static void Prefix(PlayerController __instance, Player ___player) {
        if (!Plugin.configToggleInfiniteStamina.Value) {
            return;
        }
        ___player.data.currentStamina = __instance.maxStamina;
    }
}