using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class DreadwoodBossControlPatch 
    {
        private static IDetour hook_SpawnEnemiesSequence;
        private static IDetour hook_DeathSequence;

        public static void Load() 
        {
            hook_SpawnEnemiesSequence = new ILHook(
                typeof(DreadwoodBossControl)
                    .GetMethod("SpawnEnemiesSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                SpawnEnemiesSequence_patch                
            );
            hook_DeathSequence = new ILHook(
                typeof(DreadwoodBossControl)
                    .GetMethod("DeathSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                DeathSequence_patch
            );

            IL.TowerFall.DreadwoodBossControl.ctor += ctor_patch;
            IL.TowerFall.DreadwoodBossControl.Added += Added_patch;
            IL.TowerFall.DreadwoodBossControl.GetMaxHealth += GetMaxHealth_patch;
        }

        public static void Unload() 
        {
            hook_SpawnEnemiesSequence.Dispose();
            hook_DeathSequence.Dispose();

            IL.TowerFall.DreadwoodBossControl.ctor -= ctor_patch;
            IL.TowerFall.DreadwoodBossControl.Added -= Added_patch;
            IL.TowerFall.DreadwoodBossControl.GetMaxHealth -= GetMaxHealth_patch;
        }

        private static void GetMaxHealth_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 8;
                    }
                    return x;
                });
            }
        }

        private static void Added_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(200f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 210;
                    }
                    return x;
                });
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(50f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 40;
                    }
                    return x;
                });
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(260f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 300;
                    }
                    return x;
                });
            }
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 50;
                    }
                    return x;
                });
            }
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Monocle.Scene>("Add"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<DreadwoodBossControl>>(control => {
                if (!EightPlayerModule.IsEightPlayer)
                    return;
                var controlDynamic = DynamicData.For(control);
                var bgTentacles = controlDynamic.Get<List<DreadBGTentacle>>("bgTentacles");
                var dreadBGTentacle = new DreadBGTentacle(360, 60);
                bgTentacles.Add(dreadBGTentacle);
                control.Level.Add(dreadBGTentacle);
            });
        }

        private static void ctor_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return x;
                });
            }
        }

        private static void DeathSequence_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            var deathSequenceSM = typeof(DreadwoodBossControl).GetNestedType("<DeathSequence>d__29", BindingFlags.NonPublic);
            var _this = deathSequenceSM.GetField("<>4__this");
            var eyes = typeof(DreadwoodBossControl).GetField("eyes", BindingFlags.Instance | BindingFlags.NonPublic);


            cursor.GotoNext(MoveType.After, instr => instr.MatchLdcI4(120));

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(0))) 
            {
                cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<DreadEye>("Burst"));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, _this);
                cursor.Emit(OpCodes.Ldfld, eyes);
                cursor.EmitDelegate<Action<List<DreadEye>>>(x => {
                    if (!EightPlayerModule.IsEightPlayer)
                        return;
                    EyeBurst(x, 4);
                });
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(2))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 1;
                    }
                    return x;
                });
                cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<DreadEye>("Burst"));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, _this);
                cursor.Emit(OpCodes.Ldfld, eyes);
                cursor.EmitDelegate<Action<List<DreadEye>>>(x => {
                    if (!EightPlayerModule.IsEightPlayer)
                        return;
                    EyeBurst(x, 3);
                });
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(1)) && 
                cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(1)))
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 2;
                    }
                    return x;
                });
            }
        }

        private static void EyeBurst(List<DreadEye> eye, int index) 
        {
            eye[index].Burst();
        }

        private static void SpawnEnemiesSequence_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 5;
                    }
                    return x;
                });
            }
        }
    }
}