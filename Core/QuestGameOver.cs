using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class QuestGameOverPatch 
    {
        private static IDetour hook_QuestGameOverSequence;
        private static IDetour hook_QuestGameOverSequence6_0b__2;
        private static IDetour hook_QuestGameOverSequence6_2b__5;
        private static IDetour hook_QuestGameOverSequence6_3b__6;
        public static void Load() 
        {
            IL.TowerFall.QuestGameOver.Render += Render_patch;
            hook_QuestGameOverSequence = new ILHook(
                typeof(QuestGameOver).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                Sequence_patch
            );
            hook_QuestGameOverSequence6_0b__2 = new ILHook(
                typeof(QuestGameOver).GetNestedType(
                    "<>c__DisplayClass6_0", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__2", BindingFlags.Instance | BindingFlags.NonPublic),
                Sequence_patch
            );
            hook_QuestGameOverSequence6_2b__5 = new ILHook(
                typeof(QuestGameOver).GetNestedType(
                    "<>c__DisplayClass6_2", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__5", BindingFlags.Instance | BindingFlags.NonPublic),
                Sequence_patch
            );
            hook_QuestGameOverSequence6_3b__6 = new ILHook(
                typeof(QuestGameOver).GetNestedType(
                    "<>c__DisplayClass6_3", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__6", BindingFlags.Instance | BindingFlags.NonPublic),
                Sequence_patch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.QuestGameOver.Render -= Render_patch;
            hook_QuestGameOverSequence.Dispose();
            hook_QuestGameOverSequence6_0b__2.Dispose();
            hook_QuestGameOverSequence6_2b__5.Dispose();
            hook_QuestGameOverSequence6_3b__6.Dispose();
        }


        private static void Sequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                if (cursor.Next.MatchLdcR4(1))
                    continue;
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420 / 2;
                    return width;
                });
            }
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420 / 2;
                    return width;
                });
            }

            var screencursor = new ILCursor(ctx);
            while (screencursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                screencursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }
        }
    }
}