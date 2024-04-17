using HarmonyLib;
using CWissKnife;
using Photon.Pun;

[HarmonyPatch(typeof(Player), "TakeDamage")]
public class DamagePatch {
    static void Prefix(ref float damage) {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        
        if (!Plugin.configToggleInfiniteHealth.Value) {
            return;
        }
        damage = 0f;
    }
}

[HarmonyPatch(typeof(Player), "Die")]
public class DamagePatch2 {
    static bool Prefix() {
        if (!PhotonNetwork.IsMasterClient) {
            return true;
        }

        if (!Plugin.configToggleInfiniteHealth.Value) {
            return true;
        }
        return false;
    }
}
