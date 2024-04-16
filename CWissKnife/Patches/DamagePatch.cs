using HarmonyLib;
using CWissKnife;

[HarmonyPatch(typeof(Player), "TakeDamage")]
public class DamagePatch {
    static void Prefix(ref float damage) {
        if (!Plugin.configToggleInfiniteHealth.Value) {
            return;
        }
        damage = 0f;
    }
}

[HarmonyPatch(typeof(Player), "Die")]
public class DamagePatch2 {
    static bool Prefix() {
        if (!Plugin.configToggleInfiniteHealth.Value) {
            return true;
        }
        return false;
    }
}
