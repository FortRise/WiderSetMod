using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class ReplayViewerPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.ReplayViewer.ctor += ctor_patch;
            IL.TowerFall.ReplayViewer.Render += Render_patch;
            IL.TowerFall.ReplayViewer.PostScreenRender += PostScreenRender_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.ReplayViewer.ctor -= ctor_patch;
            IL.TowerFall.ReplayViewer.Render -= Render_patch;
            IL.TowerFall.ReplayViewer.PostScreenRender -= PostScreenRender_patch;
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return size;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(180))) 
            {
                cursor.EmitDelegate<Func<float, float>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 230;
                    return size;
                });
            }
        }

        private static void PostScreenRender_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return size;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(310))) 
            {
                cursor.EmitDelegate<Func<float, float>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 410;
                    return size;
                });
            }
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return size;
                });
            }
        }
    }

    public static class GifExporterPatch 
    {
        private static ILHook hook_ExportGif;
        public static void Load() 
        {
            hook_ExportGif = new ILHook(
                typeof(GifExporter).GetMethod("ExportGIF", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                GifExporterExportGif_patch
            );
            IL.TowerFall.GifExporter.Render += GifExporterRender_patch;
            IL.Moments.Encoder.GifEncoder.GetImagePixels += GifEncoderGetImagePixels_patch;
            IL.Moments.Encoder.GifEncoder.AddFrame += GifEncoderAddFrame_patch;
        }

        public static void Unload() 
        {
            hook_ExportGif.Dispose();
            IL.TowerFall.GifExporter.Render -= GifExporterRender_patch;
            IL.Moments.Encoder.GifEncoder.GetImagePixels -= GifEncoderGetImagePixels_patch;
            IL.Moments.Encoder.GifEncoder.AddFrame -= GifEncoderAddFrame_patch;
        }

        private static void GifEncoderAddFrame_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return size;
                });
            }
        }

        private static void GifEncoderGetImagePixels_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(230400))) 
            {
                cursor.EmitDelegate<Func<int, int>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 302400;
                    return size;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(76800))) 
            {
                cursor.EmitDelegate<Func<int, int>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 100800;
                    return size;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(size => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return size;
                });
            }
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