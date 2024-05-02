using HarmonyLib;
using CWissKnife;
using Photon.Pun;

[HarmonyPatch(typeof(VideoCamera), "Update")]
public class CameraPatch {
    static void Prefix(VideoInfoEntry ___m_recorderInfoEntry) {
    //    if (!PhotonNetwork.IsMasterClient || MainMenuHandler.SteamLobbyHandler.IsPlayingWithRandoms()) {
    //        return;
    //    }
        
        if (!Plugin.configToggleInfiniteCamera.Value) {
            return;
        }
        ___m_recorderInfoEntry.timeLeft = ___m_recorderInfoEntry.maxTime;
    }
}