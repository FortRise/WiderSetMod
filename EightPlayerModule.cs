using System;
using System.Reflection;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using TowerFall;

namespace EightPlayerMod 
{
    [Fort("com.fortrise.eightplayer", "EightPlayer")]
    public sealed class EightPlayerModule : FortModule
    {
        public static EightPlayerModule Instance;
        public static bool IsEightPlayer;
        public static bool LaunchedEightPlayer;
        public static bool CanVersusLevelSet;
        public static bool CanCoopLevelSet;
        public Atlas EightPlayerAtlas;
        public SpriteData EightPlayerSpriteData;

        public EightPlayerModule() 
        {
            Instance = this;
        }

        public override void LoadContent()
        {
            EightPlayerAtlas = Content.LoadAtlas("Atlas/atlas.xml", "Atlas/atlas.png");
            EightPlayerSpriteData = Content.LoadSpriteData("Atlas/spriteData.xml", EightPlayerAtlas);
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
            TFGame.Players = new bool[8];
            TFGame.Characters = new int[8];
            TFGame.AltSelect = new ArcherData.ArcherTypes[8];
            TFGame.CoOpCrowns = new bool[8];
            for (int i = 4; i < 8; i++) 
            {
                TFGame.Characters[i] = i;
            }
            typeof(Player).GetField("wasColliders", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, new Collider[8]);
            ScreenPatch.Load();
            BackdropPatch.Load();
            RollcallPatch.Load();
            MainMenuPatch.Load();
            MiasmaPatch.Load();
            MapButtonPatch.Load();
            KingReaperPatch.Load();
            PlayerInputPatch.Load();
            TFGamePatch.Load();
            TreasureSpawnerPatch.Load();
            RoundLogicPatch.Load();
            ArcherPortraitPatch.Load();
            GifExporterPatch.Load();
            VersusStartPatch.Load();

            typeof(ModExports).ModInterop();
        }

        public override void Initialize()
        {
        }

        public override void Unload()
        {
            ScreenPatch.Unload();
            BackdropPatch.Unload();
            RollcallPatch.Unload();
            MainMenuPatch.Unload();
            MiasmaPatch.Unload();
            MapButtonPatch.Unload();
            KingReaperPatch.Unload();
            PlayerInputPatch.Unload();
            TFGamePatch.Unload();
            TreasureSpawnerPatch.Unload();
            RoundLogicPatch.Unload();
            ArcherPortraitPatch.Unload();
            GifExporterPatch.Unload();
            VersusStartPatch.Unload();
        }
    }

    [ModExportName("com.fortrise.EightPlayerMod")]
    public static class ModExports 
    {
        public static bool IsEightPlayer() => EightPlayerModule.IsEightPlayer;
        public static bool LaunchedEightPlayer() => EightPlayerModule.LaunchedEightPlayer;
    }

    public static class Commands 
    {
        [Command("totalInputs")]
        public static void ShowTotalInput(string[] args) 
        {
            Logger.Log(TFGame.PlayerInputs.Length);
            Logger.Log(ArcherData.Archers.Length);
        }

        [Command("widescreen")]
        public static void TurnOnWidescreen(string[] args) 
        {
            EightPlayerModule.IsEightPlayer = !EightPlayerModule.IsEightPlayer;
            if (EightPlayerModule.IsEightPlayer) 
            {
                Engine.Instance.Screen.Resize(420, 240, 3f);
                WrapMath.AddWidth = new Vector2(420, 0f);
            }
            else 
            {
                Engine.Instance.Screen.Resize(320, 240, 3f);
                WrapMath.AddWidth = new Vector2(320, 0f);
            }
        }
    }
}
