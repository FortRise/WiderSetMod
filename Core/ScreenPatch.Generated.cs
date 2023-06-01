using System;
using Monocle;
using Moments.Encoder;
using TowerFall;
using Microsoft.Xna.Framework;
using System.Xml;

namespace EightPlayerMod 
{
    public static partial class EightPlayerMod 
    {
        public static AbstractILType[] ILTypes = {
            new MethodILType(typeof(TFGame), nameof(TFGame.orig_ctor)),
            new MethodILType(typeof(LevelTiles), nameof(LevelTiles.Added)),
            new MethodILType(typeof(Screen), nameof(Screen.Render)),
            new MethodILType(typeof(Level), nameof(Level.HandleGraphicsDispose)),
  
            new MethodILType(typeof(WrapHitbox), "BuildHitList", true),
            new MethodILType(typeof(LevelEntity), nameof(LevelEntity.Render)),
  
            new MethodILType(typeof(LevelEntity), "EnforceScreenWrap", true),
            new ConstructorILType<Session, XmlElement>(typeof(Level )),
            new MethodILType(typeof(GifEncoder), nameof(GifEncoder.AddFrame)),
  
            new MethodILType(typeof(GifEncoder), "GetImagePixels", true),
            new MethodILType(typeof(SFX), nameof(SFX.CalculatePan)),
            new MethodILType(typeof(SFX), nameof(SFX.CalculateX)),
            new MethodILType(typeof(Actor), nameof(Actor.MoveTowardsWrap)),
            new ConstructorILType<Level, XmlElement>(typeof(Background )),
            new ConstructorILType<Level, Subtexture, float, Vector2, Vector2, bool, bool>(typeof(Background )),
            new MethodILType(typeof(BottomMiasma), nameof(BottomMiasma.DrawLight)),
  
            new MethodILType(typeof(CataclysmEye), "PatternLegendaryD", true),
  
            new MethodILType(typeof(CataclysmEye), "DeadCoroutine", true),
            new ConstructorILType<Vector2>(typeof(CrackedWall )),
            new ConstructorILType<Vector2, int, int>(typeof(CrumbleBlock )),
            new ConstructorILType<Vector2>(typeof(CrumbleWall)),
            new MethodILType(typeof(Enemy), nameof(Enemy.GetHorizontalPlayer)),
            new MethodILType(typeof(FakeWall), nameof(FakeWall.SceneBegin)),
            new MethodILType(typeof(GameplayLayer), nameof(GameplayLayer.BatchedRender)),
            new ConstructorILType<ReplayData, Action<bool>>(typeof(GifExporter )),
            new ConstructorILType<Vector2, int>(typeof(HotCoals )),
            new MethodILType(typeof(HUDFade), nameof(HUDFade.Render)),
        };
    }
}