using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System.Reflection.Emit;

namespace ShowUserspaceSpawnButton
{
    public class ShowUserspaceSpawnButton : ResoniteMod
    {
        internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
    	public override string Name => "ShowUserspaceSpawnButton";
    	public override string Author => "EIA485";
    	public override string Version => VERSION_CONSTANT;
    	public override string Link => "https://github.com/fpgagay/NeosShowUserspaceSpawnButtonRML/";
        public override void OnEngineInit()
        {
            Harmony harmony = new("com.EIA485.ShowUserspaceSpawnButton");
    		harmony.PatchAll();
        }
        
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
                        && codes[i + 5].opcode == OpCodes.Brfalse_S) 
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
                if (!patched) Msg("Transpiler failed on InventoryBrowser.OnItemSelected");
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
