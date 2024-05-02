using HarmonyLib;
using CWissKnife;
using Photon.Pun;

[HarmonyPatch(typeof(Player.PlayerData), "UpdateValues")]
public class O2Patch {
    static void Prefix(ref float ___remainingOxygen, float ___maxOxygen) {
   //     if (!PhotonNetwork.IsMasterClient || MainMenuHandler.SteamLobbyHandler.IsPlayingWithRandoms()) {
   //         return;
   //     }
        
        if (!Plugin.configToggleInfiniteOxygen.Value) {
            return;
        }
        ___remainingOxygen = ___maxOxygen;
    }
}