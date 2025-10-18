using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AShortHike.MoreSaveSlots
{
    internal class PatchObjectsData
    {
        public static int SaveSlotPages = 0;
        public static int SelectedIndex = 0;
        public static LinearMenu CurrentSaveSlotMenu = null;
        public static GameObject SaveSlotMenuItemObj = null;
        public static OptionsMenu CurrentOptionsMenu = null;
        public static LinearMenu CurrentParentMenu = null;

        public static LinearMenu GetSaveSlotMenuItem(LinearMenu submenu)
        {
            submenu.selectedIndex = SelectedIndex;
            submenu.onKill += onSaveSlotMenuKill;
            CurrentSaveSlotMenu = submenu;
            return submenu;
        }
        public static LinearMenu GetSaveMenuItem(LinearMenu menu)
        {
            CurrentParentMenu = menu;
            SaveSlotMenuItemObj = CurrentParentMenu.GetMenuObjects()[0];
            return menu;
        }

        public static void onSaveSlotMenuKill()
        {
            SaveSlotPages = GameSettings.saveSlot / 10;
            SelectedIndex = GameSettings.saveSlot.Mod(10);
        }
    }
}
