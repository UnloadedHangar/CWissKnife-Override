using HarmonyLib;
using CWissKnife;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using BepInEx.Configuration;
using Photon.Pun;

[HarmonyPatch(typeof(PlayerController), "Movement")]
public class SpeedPatch {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
        var foundGlobal = false;
        var lastAddInstructionFound = false;
        CodeInstruction lastAddInstructionOnStack = null;

        foreach (CodeInstruction instruction in instructions) {
            // If current instruction is adding a float on top of stack
            if (!foundGlobal && instruction.opcode == OpCodes.Ldc_R4) {
                lastAddInstructionOnStack = new(instruction);
                lastAddInstructionFound = true;
            // If last instruction was float on top of stack and very next instruction is popping this value into current local variable
            // This means we have a hit ! We need to push both the new float value and pop instruction to stack
            } else if (lastAddInstructionFound && instruction.opcode == OpCodes.Stloc_S) {
                foundGlobal = true;
                lastAddInstructionFound = false;
                // Add custom condition, to either apply our own speedModifier or keep the original
                // if (!this.player.ai && Plugin.configToggleSpeedModifier)
                //  num = 1f * speedModifier;
                // else
                //  num = 1f;
                // -------------------
                // push instance of "this" to stack
                // -> ldarg.0
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                // push "player" field from class "PlayerController"
                // -> ldfld     class Player PlayerController::player
                yield return new CodeInstruction(OpCodes.Ldfld, typeof(PlayerController).GetField("player", BindingFlags.Instance | BindingFlags.NonPublic));
                // push "ai" field from class "Player"
                // -> ldfld     bool Player::ai
                yield return new CodeInstruction(OpCodes.Ldfld, typeof(Player).GetField("ai", BindingFlags.Instance | BindingFlags.Public));
                // create label to jump to
                var toOriginalSpeedjumpLabel = il.DefineLabel();
                // this.player.ai == true: we keep original speed, so we jump
                // -> brtrue.s  <IL_XXXXX>
                yield return new CodeInstruction(OpCodes.Brtrue_S, toOriginalSpeedjumpLabel);
                // load plugin's speed modifier **toggle** value
                // -> ldsfld    BepInEx.Configuration.ConfigEntry<bool> CWissKnife.Plugin::configToggleSpeedMultiplier
                yield return new CodeInstruction(OpCodes.Ldsfld, typeof(Plugin).GetField("configToggleSpeedMultiplier", BindingFlags.Static | BindingFlags.Public));
                // -> callvirt  bool BepInEx.Configuration.ConfigEntry<bool>::get_Value()
                yield return new CodeInstruction(OpCodes.Callvirt, typeof(ConfigEntry<bool>).GetProperty("Value").GetGetMethod());
                // -> brfalse.s <IL_XXXXX>
                yield return new CodeInstruction(OpCodes.Brfalse_S, toOriginalSpeedjumpLabel);
                // Player is host check
                // -> call      static bool Photon.Pun.PhotonNetwork::get_IsMasterClient()
                yield return new CodeInstruction(OpCodes.Call, typeof(PhotonNetwork).GetProperty("IsMasterClient", BindingFlags.Static | BindingFlags.Public).GetGetMethod());
                // -> brfalse.s <IL_XXXXX>
                yield return new CodeInstruction(OpCodes.Brfalse_S, toOriginalSpeedjumpLabel);
                // Player is in random lobby check
                // -> call      static SteamLobbyHandler MainMenuHandler::get_SteamLobbyHandler()
                yield return new CodeInstruction(OpCodes.Call, typeof(MainMenuHandler).GetProperty("SteamLobbyHandler", BindingFlags.Static | BindingFlags.Public).GetGetMethod());
                // -> callvirt  bool SteamLobbyHandler::IsPlayingWithRandoms()
                yield return new CodeInstruction(OpCodes.Callvirt, typeof(SteamLobbyHandler).GetMethod("IsPlayingWithRandoms", BindingFlags.Public | BindingFlags.Instance));
                // -> brtrue.s  <IL_XXXXX>
                yield return new CodeInstruction(OpCodes.Brtrue_S, toOriginalSpeedjumpLabel);
                // --------------
                // DEBUG
                // --------------
                // yield return new CodeInstruction(OpCodes.Ldstr, "HELLO !!!");
                // yield return new CodeInstruction(OpCodes.Callvirt, typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null));
                // --------------
                // here we load the original float value on the stack, that we will multiply
                // -> ldc.r4    <original>
                yield return lastAddInstructionOnStack;
                // load plugin's speed modifier value
                // -> ldsfld    BepInEx.Configuration.ConfigEntry<float> CWissKnife.Plugin::configSpeedMultiplier
                yield return new CodeInstruction(OpCodes.Ldsfld, typeof(Plugin).GetField("configSpeedMultiplier", BindingFlags.Static | BindingFlags.Public));
                // -> callvirt  float BepInEx.Configuration.ConfigEntry<float>::get_Value()
                yield return new CodeInstruction(OpCodes.Callvirt, typeof(ConfigEntry<float>).GetProperty("Value").GetGetMethod());
                // now we multiply
                // -> mul
                yield return new CodeInstruction(OpCodes.Mul);
                // apply this float to our local variable (num)
                // -> stloc.s   <variable>
                yield return new CodeInstruction(instruction);
                // jump over next 2 instructions (or we would be back to original)
                var skipOriginalSpeedJumpLabel = il.DefineLabel();
                yield return new CodeInstruction(OpCodes.Br_S, skipOriginalSpeedJumpLabel);
                // add jump to original float push instruction
                // we make a new object otherwise labels get duplicated in memory (i.e. lasAddInstructionOnStack would have the same label)
                // -> ldc.r4    <original>
                CodeInstruction cleanLoadFloat = new(lastAddInstructionOnStack.opcode, lastAddInstructionOnStack.operand);
                cleanLoadFloat.labels.Add(toOriginalSpeedjumpLabel);
                // original speed
                // -> ldc.r4    <original>
                yield return cleanLoadFloat;
                // apply this float to our local variable (num)
                // -> stloc.s
                yield return instruction;
                // we still need to add a label for the next instruction (when this.player.ai == false) so that we can jump to it
                // instead of waiting for it, we will create our own nop (no instruction)
                CodeInstruction nopInstr = new(OpCodes.Nop);
                nopInstr.labels.Add(skipOriginalSpeedJumpLabel);
                yield return nopInstr;
            // Else, add guarded instruction to stack and push current instruction as well
            } else {
                if (lastAddInstructionFound) {
                    yield return lastAddInstructionOnStack;
                }
                lastAddInstructionFound = false;
                yield return instruction;
            }
        }
        if (foundGlobal is false) {
            Plugin.Log.LogError("Cannot find local var <float num> = [something] in PlayerController.Movement");
        }
    }
}