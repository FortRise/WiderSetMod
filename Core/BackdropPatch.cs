using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace EightPlayerMod
{
    public static class BackdropPatch 
    {
        public static void Load() 
        {
            On.TowerFall.Background.Backdrop.ctor_Level_XmlElement += Backdropctor_patch;
            On.TowerFall.Background.FlightMoonLayer.Render += FlightMoonLayerRender_patch;
            On.TowerFall.Background.SacredGroundMoonLayer.ctor += SacredGroundMoonLayerctor_patch;
            On.TowerFall.Background.ScrollLayer.ctor_Level_XmlElement += ScrollLayerctor_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.Background.Backdrop.ctor_Level_XmlElement -= Backdropctor_patch;
            On.TowerFall.Background.FlightMoonLayer.Render -= FlightMoonLayerRender_patch;
            On.TowerFall.Background.SacredGroundMoonLayer.ctor -= SacredGroundMoonLayerctor_patch;
            On.TowerFall.Background.ScrollLayer.ctor_Level_XmlElement -= ScrollLayerctor_patch;
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
