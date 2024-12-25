using BepInEx;
using UnityEngine;

namespace AShortHike.MoreSaveSlots
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("AShortHike.ModdingAPI", "1.0.1")]
    public class Main : BaseUnityPlugin
    {
        public static MoreSaveSlots MoreSaveSlots { get; private set; }

        private void Awake()
        {
            MoreSaveSlots = new MoreSaveSlots();
        }
    }
}
