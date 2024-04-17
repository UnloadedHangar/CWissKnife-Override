using HarmonyLib;
using CWissKnife;
using Photon.Pun;

[HarmonyPatch(typeof(PlayerController), "Update")]
public class StaminaPatch {
    static void Prefix(PlayerController __instance, Player ___player) {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        
        if (!Plugin.configToggleInfiniteStamina.Value) {
            return;
        }
        ___player.data.currentStamina = __instance.maxStamina;
    }
}