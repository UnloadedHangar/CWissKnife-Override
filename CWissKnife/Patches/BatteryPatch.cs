using HarmonyLib;
using CWissKnife;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;

[HarmonyPatch]
public class BatteryPatch {
    // Find all children of ItemInstanceBehaviour containing "Update"
    static IEnumerable<MethodBase> TargetMethods() {
        Type baseType = typeof(ItemInstanceBehaviour);
        // Find classes derived from ItemInstanceBehaviour containing an "Update" method (public/private and instance/static)
        // https://stackoverflow.com/questions/135443/how-do-i-use-reflection-to-invoke-a-private-method
        var derived = baseType.Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(baseType) &&
                t.GetMethod("Update",
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.Static) != null
            );
        // Now we use AccessTools (provided by Harmony) to get this method as MethodBase
        // https://harmony.pardeike.net/articles/annotations.html
        return derived.Select(t => AccessTools.Method(t, "Update"));
    }

    [HarmonyPrefix]
    static void Prefix(object __instance) {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        if (!Plugin.configToggleInfiniteBattery.Value) {
            return;
        }
        var compatibleFields = __instance
            .GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(t2 => t2.FieldType.Equals(typeof(BatteryEntry)));
        if (compatibleFields != null && compatibleFields.Any()) {
            foreach (var field in compatibleFields) {
                BatteryEntry batteryEntry = (BatteryEntry)field.GetValue(__instance);
                batteryEntry.m_charge = batteryEntry.m_maxCharge;
            }
        }
    }
}
