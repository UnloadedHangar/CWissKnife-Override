using HarmonyLib;
using CWissKnife;

[HarmonyPatch(typeof(Player.PlayerData), "UpdateValues")]
public class O2Patch {
    static void Prefix(ref float ___remainingOxygen, float ___maxOxygen) {
        if (!Plugin.configToggleInfiniteOxygen.Value) {
            return;
        }
        ___remainingOxygen = ___maxOxygen;
    }
}