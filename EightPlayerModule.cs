using FortRise;
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
            IsEightPlayer = true;
            ScreenPatch.Load();
        }

        public override void Initialize()
        {
            Engine.Instance.Screen.Resize(420, 240, 1f);
            WrapMath.AddWidth = new Vector2(420, 0f);
        }

        public override void Unload()
        {
            ScreenPatch.Unload();
        }
    }
}
