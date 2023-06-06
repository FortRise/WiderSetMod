using System;
using System.Collections;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class GifExporterPatch 
    {
        private static IDetour hook_ExportGif;
        public static void Load() 
        {
            hook_ExportGif = new ILHook(
                typeof(GifExporter).GetMethod("ExportGIF", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                GifExporterExportGif_patch
            );
            IL.TowerFall.GifExporter.Render += GifExporterRender_patch;
        }

        public static void Unload() 
        {
            hook_ExportGif.Dispose();
            IL.TowerFall.GifExporter.Render -= GifExporterRender_patch;
        }

        private static void GifExporterRender_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => 
                {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420f;
                    return width;
                });
            }
        }

        private static void GifExporterctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(ModifyWidth);
            }
        }

        private static void GifExporterExportGif_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(ModifyWidth);
            }
        }

        private static int ModifyWidth(int width) 
        {
            if (EightPlayerModule.IsEightPlayer)
                return 420;
            return width;
        }
    }
}