using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using static UnityEngine.ScriptingUtility;

namespace AShortHike.MoreSaveSlots
{
    [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.maxSaveSlots), MethodType.Getter)]
    class MaxSaveSlotsPatch
    {
        public static void Postfix(ref int __result)
        {
            __result = 10;
        }
    }
}
