using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace EightPlayerMod 
{
    [Fort("com.fortrise.eightplayer", "EightPlayer")]
    public sealed class EightPlayerModule : FortModule
    {
        private Harmony harmony;
        public static bool IsEightPlayer;

        public override void LoadContent()
        {
        }

        public override void Load()
        {
            harmony = new Harmony("EightPlayerMod");
            IsEightPlayer = true;
            ScreenPatch.Load();
            harmony.PatchAll();
        }

        public override void Initialize()
        {
            Engine.Instance.Screen.Resize(420, 240, 1f);
            WrapMath.AddWidth = new Vector2(420, 0f);
        }

        public override void Unload()
        {
            ScreenPatch.Unload();
            harmony.UnpatchAll();
        }
    }
}
