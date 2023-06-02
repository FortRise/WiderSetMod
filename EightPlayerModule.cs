using System;
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
        public static EightPlayerModule Instance;
        public static bool IsEightPlayer;

        public EightPlayerModule() 
        {
            Instance = this;
        }

        public override void LoadContent()
        {
            FakeVersusTowerData.Load(0, Content.GetContentPath("Levels/Versus/00 - Sacred Ground"));
            FakeVersusTowerData.Load(1, Content.GetContentPath("Levels/Versus/01 - Twilight Spire"));
            FakeVersusTowerData.Load(2, Content.GetContentPath("Levels/Versus/02 - Backfire"));
            FakeVersusTowerData.Load(3, Content.GetContentPath("Levels/Versus/03 - Flight"));
            FakeVersusTowerData.Load(4, Content.GetContentPath("Levels/Versus/04 - Mirage"));
            FakeVersusTowerData.Load(5, Content.GetContentPath("Levels/Versus/05 - Thornwood"));
            FakeVersusTowerData.Load(6, Content.GetContentPath("Levels/Versus/06 - Frostfang Keep"));
            FakeVersusTowerData.Load(7, Content.GetContentPath("Levels/Versus/07 - Kings Court"));
            FakeVersusTowerData.Load(8, Content.GetContentPath("Levels/Versus/08 - Sunken City"));
            FakeVersusTowerData.Load(9, Content.GetContentPath("Levels/Versus/09 - Moonstone"));
            FakeVersusTowerData.Load(10, Content.GetContentPath("Levels/Versus/10 - TowerForge"));
            FakeVersusTowerData.Load(11, Content.GetContentPath("Levels/Versus/11 - Ascension"));
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
