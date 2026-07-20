using BepInEx;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using FrooxEngine;
using HarmonyLib;
using System.Reflection.Emit;

namespace ShowUserspaceSpawnButton
{
    [ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
    [BepInDependency(BepInExResoniteShim.PluginMetadata.GUID)]
    public class ShowUserspaceSpawnButton : BasePlugin
    {
        static ManualLogSource log;
        public override void Load()
        {
            log = Log;
            HarmonyInstance.PatchAll();
        }
        public override bool Unload() { HarmonyInstance.UnpatchSelf(); return true; }

        [HarmonyPatch(typeof(InventoryBrowser), "OnItemSelected")]
        class ShowUserspaceSpawnButtonPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var patched = false;
                var codes = instructions.ToList();
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_1 //currentType
                        && codes[i + 1].opcode == OpCodes.Ldc_I4_5 //SpecialItemType.Facet
                        && codes[i + 5].opcode == OpCodes.Brfalse) 
                    {
                        //checking if item is null
                        codes[i] = new CodeInstruction(OpCodes.Ldloc_0);//load item ui
                        codes[i + 1] = codes[i + 5]; //jump if false(null ptr)
                        //removing unnecessary instructions
                        codes.RemoveRange(i + 2, 4);
                        codes[i - 2].opcode = OpCodes.Ldc_I4_1; //set isSafeSpawn it be always true
                        patched = true;
                        break; //only patch first occurrence
                    }
                }
                if (!patched) log.LogError("Transpiler failed on InventoryBrowser.OnItemSelected");
                return codes.AsEnumerable();
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnAttach")]
            static void OnAttachPostFixSync(Sync<InventoryBrowser.SpecialItemType> ____lastSpecialItemType)
            {
                ____lastSpecialItemType.Value = (InventoryBrowser.SpecialItemType)(-1);
            }
        }
    }
}