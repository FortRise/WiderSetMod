using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace EightPlayerMod 
{
    public class MapButtonPatch 
    {
        public static void Load() 
        {
            On.TowerFall.MapScene.StartSession += MapSceneStartSession_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.MapScene.StartSession -= MapSceneStartSession_patch;
        }


        private static void MapSceneStartSession_patch(On.TowerFall.MapScene.orig_StartSession orig, MapScene self)
        {
            orig(self);
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                EightPlayerModule.IsEightPlayer = true;
                Engine.Instance.Screen.Resize(420, 240, 3f);
                WrapMath.AddWidth = new Vector2(420, 0f);
            }
        }
    }
}