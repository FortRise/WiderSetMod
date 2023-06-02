using System;
using Monocle;
using Moments.Encoder;
using TowerFall;
using Microsoft.Xna.Framework;
using System.Xml;

namespace EightPlayerMod 
{
    public static partial class ScreenPatch
    {
        public static AbstractILType[] ILTypes = {
            new MethodILType(typeof(TFGame), nameof(TFGame.orig_ctor)),
            new MethodILType(typeof(Screen), nameof(Screen.Render)),
            new MethodILType(typeof(Level), nameof(Level.HandleGraphicsDispose)),
  
            new MethodILType(typeof(WrapHitbox), "BuildHitList", true),
            new ConstructorILType<Session, XmlElement>(typeof(Level)),
            new MethodILType(typeof(GifEncoder), nameof(GifEncoder.AddFrame)),
  
            new MethodILType(typeof(GifEncoder), "GetImagePixels", true),
            new MethodILType(typeof(SFX), nameof(SFX.CalculatePan)),
            new MethodILType(typeof(SFX), nameof(SFX.CalculateX)),
            new MethodILType(typeof(Actor), nameof(Actor.MoveTowardsWrap)),
            new ConstructorILType<Level, XmlElement>(typeof(Background.ScrollLayer)),
            new ConstructorILType<Level, Subtexture, float, Vector2, Vector2, bool, bool>(typeof(Background.ScrollLayer)),
            new MethodILType(typeof(Background.LightningFlashLayer), nameof(Background.LightningFlashLayer.Render)),
            new MethodILType(typeof(BottomMiasma), nameof(BottomMiasma.DrawLight)),
            new ConstructorILType<Vector2>(typeof(CrackedWall)),
            new ConstructorILType<Vector2, int, int>(typeof(CrumbleBlock)),
            new ConstructorILType<Vector2>(typeof(CrumbleWall)),
            new MethodILType<Vector2, Facing>(typeof(Enemy), nameof(Enemy.GetHorizontalPlayer)),
            new MethodILType(typeof(FakeWall), nameof(FakeWall.SceneBegin)),
            new MethodILType(typeof(GameplayLayer), nameof(GameplayLayer.BatchedRender)),
            new ConstructorILType<ReplayData, Action<bool>>(typeof(GifExporter)),
            new ConstructorILType<Vector2, int>(typeof(HotCoals)),
            new MethodILType(typeof(HUDFade), nameof(HUDFade.Render)),
  
            new MethodILType(typeof(Ice), "BuildTiles", true),
            new ConstructorILType<LavaControl, Lava.LavaSide>(typeof(Lava)),
            new ConstructorILType<Session, XmlElement>(typeof(Level)),
            new MethodILType(typeof(LevelBGTiles), nameof(LevelBGTiles.Added)),
            new MethodILType(typeof(LevelEntity), nameof(LevelEntity.Render)),
  
            new MethodILType(typeof(LevelEntity), "EnforceScreenWrap", true),
            new MethodILType(typeof(LevelLoaderXML), nameof(LevelLoaderXML.Render)),
            new MethodILType(typeof(LevelTiles), nameof(LevelTiles.Added)),
            new MethodILType(typeof(LevelTiles), nameof(LevelTiles.HandleGraphicsDispose)),
            new MethodILType(typeof(LevelRandomTreasure), nameof(LevelRandomTreasure.AddRandomTreasure)),
            new ConstructorILType<Color>(typeof(LightingLayer)),
            new ConstructorILType<Vector2, int, LoopPlatform.MoveDirs, bool>(typeof(LoopPlatform)),
            new ConstructorILType<Vector2, LoopPlatform>(typeof(LoopPlatform)),
            new MethodILType(typeof(LoopPlatform), nameof(LoopPlatform.Added)),
            new MethodILType(typeof(LoopPlatform), nameof(LoopPlatform.Update)),
            new MethodILType(typeof(LoopPlatform), nameof(LoopPlatform.DrawLight)),
            new MethodILType(typeof(MenuBackground), nameof(MenuBackground.Render)),
            new ConstructorILType<Miasma.Modes>(typeof(Miasma)),
            new MethodILType(typeof(Miasma), nameof(Miasma.Update)),
            new MethodILType(typeof(OrbLogic), nameof(OrbLogic.DoSpaceOrb)),
            new MethodILType(typeof(PauseMenu), nameof(PauseMenu.Render)),
  
            new MethodILType(typeof(Player), "SideBouncePlayer", true),
            new MethodILType(typeof(QuestControl), nameof(QuestControl.Render)),
            new MethodILType(typeof(QuestComplete), nameof(QuestComplete.Render)),
            new MethodILType(typeof(QuestLevelSelectOverlay), nameof(QuestLevelSelectOverlay.Render)),
            new MethodILType(typeof(ReplayFrame), nameof(ReplayFrame.Record)),
            new ConstructorILType(typeof(ReplayViewer)),
            new MethodILType(typeof(ReplayViewer), nameof(ReplayViewer.Render)),
            new MethodILType(typeof(RollcallElement), nameof(RollcallElement.GetTweenSource)),
            new MethodILType(typeof(SavingInfoScene), nameof(SavingInfoScene.Render)),
            new MethodILType(typeof(ScreenFlash), nameof(ScreenFlash.Render)),
            new MethodILType(typeof(Spikeball), nameof(Spikeball.Render)),
            new ConstructorILType<Vector2, int, int, SwitchBlock.SwitchColor>(typeof(SwitchBlock)),
  
            new MethodILType(typeof(SwitchBlock), "DrawSolid", true),
            new MethodILType(typeof(TurnToDustImage), nameof(TurnToDustImage.Update)),
            new MethodILType(typeof(VariantItem), nameof(VariantItem.TweenIn)),
            new ConstructorILType<VariantToggle, Vector2>(typeof(VariantPerPlayer)),
            new ConstructorILType<Session, VersusRoundResults>(typeof(VersusMatchResults)),
            new MethodILType(typeof(VersusStart), nameof(VersusStart.Render)),
            new MethodILType<Rectangle>(typeof(WrapMath), nameof(WrapMath.ApplyWrap )),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.ApplyWrapX)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.ShortestOpen)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.DiffX)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.WrapHorizDistanceSquared)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.WrapHorizDistance)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.WrapHorizLineHit)),
            new MethodILType(typeof(WrapMath), nameof(WrapMath.WrapHorizAngle)),
        };
    }
}