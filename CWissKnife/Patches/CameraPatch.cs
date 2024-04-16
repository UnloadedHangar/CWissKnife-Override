using HarmonyLib;
using CWissKnife;

[HarmonyPatch(typeof(VideoCamera), "Update")]
public class CameraPatch {
    static void Prefix(VideoInfoEntry ___m_recorderInfoEntry) {
        if (!Plugin.configToggleInfiniteCamera.Value) {
            return;
        }
        ___m_recorderInfoEntry.timeLeft = ___m_recorderInfoEntry.maxTime;
    }
}