using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace AShortHike.MoreSaveSlots
{
    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.ShowSaveSlotsMenu))]
    class ShowSaveSlotsPatch
    {
        private static int getFirstSlot() => PatchObjectsData.SaveSlotPages * 10;
        private static int getBound() => (PatchObjectsData.SaveSlotPages + 1) * 10;

        // Modify for loop bounds and intercept submenu local variable.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var maxSlotsMethod = AccessTools.PropertyGetter(typeof(GameSettings), nameof(GameSettings.maxSaveSlots));

            int currentPage = PatchObjectsData.SaveSlotPages;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsConstant(0))
                {
                    yield return CodeInstruction.Call(typeof(ShowSaveSlotsPatch), nameof(getFirstSlot));
                }
                else if (codes[i].Calls(maxSlotsMethod))
                {
                    yield return CodeInstruction.Call(typeof(ShowSaveSlotsPatch), nameof(getBound));
                }
                else if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Ret)
                {
                    // Intercept LinearMenu output from BuildSimpleMenu
                    yield return CodeInstruction.Call(typeof(PatchObjectsData), nameof(PatchObjectsData.GetSaveSlotMenuItem));
                    yield return codes[i];
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.ShowSaveMenu))]
    class ShowSaveMenuPatch
    {
        // Intercept menu local variable.
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var maxSlotsMethod = AccessTools.PropertyGetter(typeof(GameSettings), nameof(GameSettings.maxSaveSlots));

            int currentPage = PatchObjectsData.SaveSlotPages;
            for (int i = 0; i < codes.Count; i++)
            {
                if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Ret)
                {
                    // Intercept LinearMenu output from BuildSimpleMenu
                    yield return CodeInstruction.Call(typeof(PatchObjectsData), nameof(PatchObjectsData.GetSaveMenuItem));
                    yield return codes[i];
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static void Prefix(OptionsMenu __instance)
        {
            PatchObjectsData.CurrentOptionsMenu = __instance;
        }
    }

    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.GetSaveSlotText))]
    class GetSaveSlotTextPatch
    {
        // Fetch the page and index for cursor when loading up save slot menu.
        public static void Prefix()
        {
            PatchObjectsData.SaveSlotPages = GameSettings.saveSlot / 10;
            PatchObjectsData.SelectedIndex = GameSettings.saveSlot.Mod(10);
        }
    }

    [HarmonyPatch(typeof(LinearMenu), nameof(LinearMenu.Update))]
    class LinearMenuUpdatePatch
    {
        private static int prevHorizonalScrollDirection = 0;
        private static float horizontalHeldTime = 0.0f;

        public static void Postfix(LinearMenu __instance)
        {
            if (__instance.userInput.hasFocus && __instance.GetMenuObjects().Count == 10 && PatchObjectsData.CurrentSaveSlotMenu != null)
            {
                float fRight = Vector2.Dot(Vector2.right, __instance.userInput.leftStick.vector);
                int currentScrollDirection = Mathf.Abs(fRight) > 0.86f ? (int)Mathf.Sign(fRight) : 0;
                if (currentScrollDirection != 0 && currentScrollDirection == prevHorizonalScrollDirection)
                {
                    horizontalHeldTime += Time.deltaTime;
                }
                else
                {
                    horizontalHeldTime = 0f;
                }
                prevHorizonalScrollDirection = currentScrollDirection;

                int prevHorizonalScrollDirection2 = 0;
                if (horizontalHeldTime > __instance.fastScrollActivateTime)
                {
                    prevHorizonalScrollDirection2 = prevHorizonalScrollDirection;
                    horizontalHeldTime = __instance.fastScrollActivateTime - __instance.fastScrollStepTime;
                }
                if (__instance.userInput.leftStick.WasDirectionTapped(Vector2.right) || prevHorizonalScrollDirection2 == 1)
                {
                    PatchObjectsData.SaveSlotPages = (PatchObjectsData.SaveSlotPages + 1).Mod(10);
                    int selectedIndex = PatchObjectsData.CurrentSaveSlotMenu.selectedIndex;
                    PatchObjectsData.SelectedIndex = selectedIndex;
                    PatchObjectsData.CurrentSaveSlotMenu.onKill -= PatchObjectsData.onSaveSlotMenuKill;
                    PatchObjectsData.CurrentSaveSlotMenu.Kill();
                    BasicMenuItem basicMenuItem = PatchObjectsData.SaveSlotMenuItemObj.GetComponentInChildren<BasicMenuItem>();
                    MethodInfo showSaveSlotsMenu = PatchObjectsData.CurrentOptionsMenu.GetType().GetMethod("ShowSaveSlotsMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                    showSaveSlotsMenu.Invoke(PatchObjectsData.CurrentOptionsMenu, [PatchObjectsData.CurrentParentMenu, basicMenuItem]);
                    __instance.moveSound.Play();
                }
                else if (__instance.userInput.leftStick.WasDirectionTapped(Vector2.left) || prevHorizonalScrollDirection2 == -1)
                {
                    PatchObjectsData.SaveSlotPages = (PatchObjectsData.SaveSlotPages - 1).Mod(10);
                    int selectedIndex = PatchObjectsData.CurrentSaveSlotMenu.selectedIndex;
                    PatchObjectsData.SelectedIndex = selectedIndex;
                    PatchObjectsData.CurrentSaveSlotMenu.onKill -= PatchObjectsData.onSaveSlotMenuKill;
                    PatchObjectsData.CurrentSaveSlotMenu.Kill();
                    BasicMenuItem basicMenuItem = PatchObjectsData.SaveSlotMenuItemObj.GetComponentInChildren<BasicMenuItem>();
                    MethodInfo showSaveSlotsMenu = PatchObjectsData.CurrentOptionsMenu.GetType().GetMethod("ShowSaveSlotsMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                    showSaveSlotsMenu.Invoke(PatchObjectsData.CurrentOptionsMenu, [PatchObjectsData.CurrentParentMenu, basicMenuItem]);
                    __instance.moveSound.Play();
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                // The joystick valid hold angle is a bit bigger than its valid tap angle.
                // This is going to be a slight problem if I want to implement left and right inputs for page change.
                // Make this angle to be more in line with tap angle.
                if (codes[i].LoadsConstant(0.5f))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.86f);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }
}
