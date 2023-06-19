using System;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod
{
    public static class BackdropPatch 
    {
        private static IDetour hook_MoonBreakSequence;

        public static void Load() 
        {
            IL.TowerFall.Background.Backdrop.ctor_Level_XmlElement += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ctor_Level_CustomBackgroundData += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ctor_Level_Texture2D_CustomBackgroundData += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.FlightMoonLayer.ctor += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.GhostShipLayer.ctor += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.SacredGroundMoonLayer.Render += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.SacredGroundMoonLayer.ctor += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ScrollLayer.ctor_Level_XmlElement += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.VortexLayer.VortexRing.ctor += TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.WavyLayer.ctor_Level_XmlElement += TFGameBGAtlasToEightPlayerBGAtlas;

            hook_MoonBreakSequence = new ILHook(
                typeof(Background).GetMethod("MoonBreakSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                TFGameBGAtlasToEightPlayerBGAtlas
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.Background.Backdrop.ctor_Level_XmlElement -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ctor_Level_CustomBackgroundData -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ctor_Level_Texture2D_CustomBackgroundData -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.FlightMoonLayer.ctor -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.GhostShipLayer.ctor -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.SacredGroundMoonLayer.Render -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.SacredGroundMoonLayer.ctor -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.ScrollLayer.ctor_Level_XmlElement -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.VortexLayer.VortexRing.ctor -= TFGameBGAtlasToEightPlayerBGAtlas;
            IL.TowerFall.Background.WavyLayer.ctor_Level_XmlElement -= TFGameBGAtlasToEightPlayerBGAtlas;

            hook_MoonBreakSequence.Dispose();
        }


        private static void TFGameBGAtlasToEightPlayerBGAtlas(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<TFGame>("get_BGAtlas"))) 
            {
                cursor.EmitDelegate<Func<Atlas, Atlas>>(atlas => {
                    if (EightPlayerModule.IsEightPlayer)
                        return EightPlayerModule.Instance.EightPlayerBGAtlas;
                    return atlas;
                });
            }
        }

        private static void SacredGroundMoonLayerctor_patch(On.TowerFall.Background.SacredGroundMoonLayer.orig_ctor orig, Background.SacredGroundMoonLayer self, Level level, XmlElement xml)
        {
            orig(self, level, xml);
            if (EightPlayerModule.IsEightPlayer) 
            {
                self.Position.X += 100;
            }
        }
        
        private static void ScrollLayerctor_patch(On.TowerFall.Background.ScrollLayer.orig_ctor_Level_XmlElement orig, Background.ScrollLayer self, Level level, XmlElement xml)
        {
            orig(self, level, xml);
            if (EightPlayerModule.IsEightPlayer) 
            {
                self.Image.Position.Y -= 25f;
                self.Image.Scale = new Vector2(1.4f);
                self.WrapSize = new Vector2(Math.Max(420f, self.Image.Width * 1.4f), Math.Max(240f, self.Image.Height * 1.4f));
            }
        }

        private static void FlightMoonLayerRender_patch(On.TowerFall.Background.FlightMoonLayer.orig_Render orig, Background.FlightMoonLayer self)
        {
            if (EightPlayerModule.IsEightPlayer)
            {
                Draw.Texture(self.Subtexture, self.Position, Color.White, 1.4f);
                return;
            }
            orig(self);
        }

        private static void Backdropctor_patch(On.TowerFall.Background.Backdrop.orig_ctor_Level_XmlElement orig, Background.Backdrop self, Level level, XmlElement xml)
        {
            orig(self, level, xml);
            if (EightPlayerModule.IsEightPlayer) 
            {
                self.Image.Scale = new Vector2(1.4f);
                self.Image.Position.Y -= 25;
            }
        }
    }
}
