using HarmonyLib;
using CWissKnife;
using Photon.Pun;

[HarmonyPatch(typeof(Player), "TakeDamage")]
public class DamagePatch {
    static void Prefix(ref float damage) {
        if (!PhotonNetwork.IsMasterClient || MainMenuHandler.SteamLobbyHandler.IsPlayingWithRandoms()) {
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
        if (!PhotonNetwork.IsMasterClient || MainMenuHandler.SteamLobbyHandler.IsPlayingWithRandoms()) {
            return true;
        }

        if (!Plugin.configToggleInfiniteHealth.Value) {
            return true;
        }
        return false;
    }
}
