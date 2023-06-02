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
        public static bool IsEightPlayer;

        public override void LoadContent()
        {
        }

        public override void Load()
        {
            ScreenPatch.Load();
            BackdropPatch.Load();
            RollcallPatch.Load();
        }

        public override void Initialize()
        {
        }

        public override void Unload()
        {
            ScreenPatch.Unload();
            BackdropPatch.Unload();
            RollcallPatch.Unload();
        }
    }
}
