using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class CyclopsEyePatch 
    {
        private static IDetour hook_ShootSequence;
        public static void Load() 
        {
            CyclopsEyeExt.Load();
            On.TowerFall.CyclopsEye.ctor += ctor_normal_patch;
            On.TowerFall.CyclopsEye.BuildFistSequenceList += BuildFistSequenceList_patch;
            IL.TowerFall.CyclopsEye.ctor += ctor_patch;
            IL.TowerFall.CyclopsEye.Update += Update_patch;
            IL.TowerFall.CyclopsEye.FistMirrorTo_Vector2_Easer_int_Impacts += FistMirrorTo_patch;
            IL.TowerFall.CyclopsEye.FistMirrorTo_Vector2_Vector2_Easer_int_Impacts += FistMirrorTo_patch;
            IL.TowerFall.CyclopsShot.Update += CyclopsShotUpdate_patch;
            IL.TowerFall.CyclopsEye.Added += Added_patch;
            IL.TowerFall.CyclopsCeilingBones.ctor += CyclopsCeilingBonesctor_patch;
            IL.TowerFall.CyclopsPlatform.ctor += CyclopsPlatformctor_patch;
            hook_ShootSequence = new ILHook(
                typeof(CyclopsEye).GetMethod("ShootSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                ShootSequence_patch
            );
        }

        public static void Unload() 
        {
            On.TowerFall.CyclopsEye.ctor -= ctor_normal_patch;
            On.TowerFall.CyclopsEye.BuildFistSequenceList -= BuildFistSequenceList_patch;
            IL.TowerFall.CyclopsEye.ctor -= ctor_patch;
            IL.TowerFall.CyclopsEye.Update -= Update_patch;
            IL.TowerFall.CyclopsEye.FistMirrorTo_Vector2_Easer_int_Impacts -= FistMirrorTo_patch;
            IL.TowerFall.CyclopsEye.FistMirrorTo_Vector2_Vector2_Easer_int_Impacts -= FistMirrorTo_patch;
            IL.TowerFall.CyclopsShot.Update -= CyclopsShotUpdate_patch;
            IL.TowerFall.CyclopsEye.Added -= Added_patch;
            IL.TowerFall.CyclopsCeilingBones.ctor -= CyclopsCeilingBonesctor_patch;
            IL.TowerFall.CyclopsPlatform.ctor -= CyclopsPlatformctor_patch;
            hook_ShootSequence.Dispose();
        }

        private static void BuildFistSequenceList_patch(On.TowerFall.CyclopsEye.orig_BuildFistSequenceList orig, CyclopsEye self, int difficulty)
        {
            if (!EightPlayerModule.IsEightPlayer)
                orig(self, difficulty);
            
            var selfDynamic = DynamicData.For(self);
            var fistSequenceList = selfDynamic.Get<List<Func<IEnumerator>>>("fistSequenceList");
            if (fistSequenceList == null) 
            {
                fistSequenceList = new List<Func<IEnumerator>>();
                selfDynamic.Set("fistSequenceList", fistSequenceList);
            }
            // Instead of creating another one, we just clear it to avoid allocating more memory
            fistSequenceList.Clear();

            switch (difficulty)
            {
            case 0:
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistStompA")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCeilingA(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistSideA")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCornerCheeseA(self)));
                return;
            case 1:
                fistSequenceList.Add(new Func<IEnumerator>(() => FistStompB(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistCeilingA")));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistSideA")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCornerCheeseA(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistMiddleA")));
                return;
            case 2:
                fistSequenceList.Add(new Func<IEnumerator>(() => FistStompB(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistSideB(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistMiddleB")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCornerCheeseA(self)));
                return;
            case 3:
                fistSequenceList.Add(new Func<IEnumerator>(() => FistStompC(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistCeilingC")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistSideB(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistMiddleC")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCornerCheeseA(self)));
                return;
            case 4:
                fistSequenceList.Add(new Func<IEnumerator>(() => FistStompD(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCeilingD(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistSideB(self)));
                fistSequenceList.Add(new Func<IEnumerator>(() => Invoke("FistMiddleD")));
                fistSequenceList.Add(new Func<IEnumerator>(() => FistCornerCheeseA(self)));
                return;
            default:
                return;
            }

            IEnumerator Invoke(string name) 
            {
                return selfDynamic.Invoke<IEnumerator>(name);
            }
        }

#region Sequences
        private static IEnumerator FistStompB(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(data.LFistHome.X, data.LFistFloor.Y), Ease.BackIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 25f, data.LFistFloor.Y), new Vector2(data.LFistHome.X + 25f, data.LFistFloor.Y - 90f), Ease.CubeIn, eye.FR(6), CyclopsFist.Impacts.Arena);
            yield return eye.FR(6);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y), new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y - 90f), Ease.CubeIn, eye.FR(6), CyclopsFist.Impacts.Arena);
            yield return eye.FR(6);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0f, 50f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }

        private static IEnumerator FistStompC(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(data.LFistHome.X, data.LFistFloor.Y), Ease.BackIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y), new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y - 90f), Ease.CubeIn, eye.FR(8), CyclopsFist.Impacts.Arena);
            yield return eye.FR(8);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0f, 50f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }

        private static IEnumerator FistStompD(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(data.LFistHome.X, data.LFistFloor.Y), Ease.BackIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y), new Vector2(data.LFistHome.X + 60f, data.LFistFloor.Y - 90f), Ease.CubeIn, eye.FR(6), CyclopsFist.Impacts.Arena);
            yield return eye.FR(6);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 25f, data.LFistFloor.Y), new Vector2(data.LFistHome.X + 25f, data.LFistFloor.Y - 90f), Ease.CubeIn, eye.FR(4), CyclopsFist.Impacts.Arena);
            yield return eye.FR(4);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0f, 50f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }


        private static IEnumerator FistSideB(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(40f, 110f), Ease.CubeIn, eye.FR(2), CyclopsFist.Impacts.Arena);
            yield return eye.FR(2);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(data.LFistTop, Ease.CubeIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            eye.FistMirrorTo(new Vector2(40f, 150f), Ease.CubeIn, eye.FR(1), CyclopsFist.Impacts.Arena);
            yield return eye.FR(1);
            yield return eye.FR(3);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0f, 30f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }

        private static IEnumerator FistCornerCheeseA(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(40f, 60f), Ease.CubeIn, eye.FR(2), CyclopsFist.Impacts.Arena);
            yield return eye.FR(1);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(190f, 60f), Ease.CubeIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(3);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0, 30f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }

        private static IEnumerator FistCeilingA(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            data.moveOffset = Vector2.UnitY * 20f;
            eye.UploadData(data);

            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 10f, 60f), new Vector2(data.LFistHome.X + 10f, data.LFistHome.Y), Ease.CubeIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(2);
            eye.FistMirrorTo(data.LFist.Position, data.LFist.Position + new Vector2(-16f, 0f), Ease.BackOut, eye.FR(3), CyclopsFist.Impacts.None);
            yield return eye.FR(5);
            data.moveOffset = Vector2.Zero;
            eye.UploadData(data);

            eye.FistMirrorTo(data.LFistHome, data.LFistFloor + new Vector2(0f, -80f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();

            yield break;
        }

        private static IEnumerator FistCeilingB(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            data.moveOffset = Vector2.UnitY * 20f;
            eye.UploadData(data);
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 10f, 60f), new Vector2(data.LFistHome.X + 10f, data.LFistHome.Y), Ease.CubeIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(190f, 60f), Ease.CubeIn, eye.FR(2), CyclopsFist.Impacts.Fists);
            yield return eye.FR(2);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(2);
            data.moveOffset = Vector2.Zero;
            eye.UploadData(data);
            eye.FistMirrorTo(data.LFistHome, data.LFistHome + new Vector2(0f, -60f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        } 

        private static IEnumerator FistCeilingD(CyclopsEye eye)
        {
            var data = eye.DownloadData();
            eye.FistMirrorTo(new Vector2(data.LFistHome.X + 10f, 60f), new Vector2(data.LFistHome.X + 10f, data.LFistHome.Y), Ease.CubeIn, eye.FR(3), CyclopsFist.Impacts.Arena);
            yield return eye.FR(3);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(new Vector2(190f, 190f), new Vector2(data.LFistHome.X - 20f, 190f), Ease.CubeIn, eye.FR(4), CyclopsFist.Impacts.Fists);
            yield return eye.FR(4);
            DarkWorldBoss.ShakeLevel();
            yield return eye.FR(1);
            eye.FistMirrorTo(data.LFistHome, data.LFistFloor + new Vector2(0f, -80f), Ease.CubeInOut, eye.FR(9), CyclopsFist.Impacts.None);
            yield return eye.FR(9);
            eye.SineFists();
            yield break;
        }
#endregion


        private static void CyclopsPlatformctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 190;
                    }
                    return x;
                });
            }
        }

        private static void CyclopsCeilingBonesctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
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
        private static void Added_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(220))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 320;
                    }
                    return x;
                });
            }
        }
        private static void FistMirrorTo_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                if (cursor.Next.MatchLdcR4(1))
                    continue;
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return x;
                });
            }
        }


        private static void CyclopsShotUpdate_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(360))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420;
                    }
                    return x;
                });
            }
        }

        private static void Update_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<TowerFall.CyclopsEye>("LevelCenter"))) 
            {
                cursor.EmitDelegate<Func<Vector2, Vector2>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return new Vector2(420 / 2f, x.Y);
                    }
                    return x;
                });
            }
        }

        private static void ShootSequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
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

        private static void ctor_normal_patch(On.TowerFall.CyclopsEye.orig_ctor orig, TowerFall.CyclopsEye self, int difficulty)
        {
            orig(self, difficulty);
            if (EightPlayerModule.IsEightPlayer) 
            {
                var selfDynamic = DynamicData.For(self);
                selfDynamic.Set("LFistHome", new Vector2(90 + 40f, 120f));
                selfDynamic.Set("RFistHome", new Vector2(230 + 60f, 120f));
                selfDynamic.Set("LFistFloor", new Vector2(70 + 20f, 190f));
                selfDynamic.Set("RFistFloor", new Vector2(250 + 40f, 190f));
                selfDynamic.Set("LFistTop", new Vector2(40 + 20f, 150f));
                selfDynamic.Set("RFistTop", new Vector2(280 + 40f, 150f));
                selfDynamic.Set("LFistCenter", new Vector2(140 + 50f, 190f));
                selfDynamic.Set("RFistCenter", new Vector2(180 + 40f, 190f));
                selfDynamic.Set("fistRate", 12 - self.Difficulty);
            }
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<TowerFall.CyclopsEye>("LevelCenter"))) 
            {
                cursor.EmitDelegate<Func<Vector2, Vector2>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return new Vector2(420 / 2f, x.Y);
                    }
                    return x;
                });
            }
        }
    }

    public struct DataBank 
    {
        public Vector2 moveOffset;
        public Vector2 LFistHome;
        public CyclopsFist LFist;
        public Vector2 LFistFloor;
        public Vector2 LFistTop;
    }

    public static class CyclopsEyeExt 
    {
        public static DataBank DownloadData(this CyclopsEye eye) 
        {
            var dynData = DynamicData.For(eye);
            var dataBank = new DataBank() 
            {
                moveOffset = dynData.Get<Vector2>("moveOffset"),
                LFist = dynData.Get<CyclopsFist>("LFist"),
                LFistFloor = dynData.Get<Vector2>("LFistFloor"),
                LFistHome = dynData.Get<Vector2>("LFistHome"),
                LFistTop = dynData.Get<Vector2>("LFistTop")
            };
            return dataBank;
        }

        public static void UploadData(this CyclopsEye eye, DataBank dataBank) 
        {
            var dynData = DynamicData.For(eye);
            dynData.Set("moveOffset", dataBank.moveOffset);
        }

        private static FastReflectionDelegate FistMirrorTo_Vector2_Easer_int_Impacts;
        private static FastReflectionDelegate FistMirrorTo_Vector2_Vector2_Easer_int_Impacts;

        public static void Load() 
        {
            var FistMirrorTo = typeof(CyclopsEye).GetMethod(
                "FistMirrorTo", BindingFlags.Instance | BindingFlags.NonPublic, null, 
                new Type[4] { typeof(Vector2), typeof(Ease.Easer), typeof(int), typeof(CyclopsFist.Impacts) }, null);
            FistMirrorTo_Vector2_Easer_int_Impacts = FistMirrorTo.GetFastDelegate();

            var FistMirrorTo2 = typeof(CyclopsEye).GetMethod(
                "FistMirrorTo", BindingFlags.Instance | BindingFlags.NonPublic, null, 
                new Type[5] { typeof(Vector2), typeof(Vector2), typeof(Ease.Easer), typeof(int), typeof(CyclopsFist.Impacts) }, null);
            FistMirrorTo_Vector2_Vector2_Easer_int_Impacts = FistMirrorTo2.GetFastDelegate();
        }

        public static void FistMirrorTo(this CyclopsEye eye, Vector2 to, Ease.Easer ease, int time, CyclopsFist.Impacts impact) 
        {
            var dynData = DynamicData.For(eye);
            FistMirrorTo_Vector2_Easer_int_Impacts(eye, to, ease, time, impact);
        }

        public static void FistMirrorTo(this CyclopsEye eye, Vector2 to, Vector2 control, Ease.Easer ease, int time, CyclopsFist.Impacts impact) 
        {
            var dynData = DynamicData.For(eye);
            FistMirrorTo_Vector2_Vector2_Easer_int_Impacts(eye, to, control, ease, time, impact);
        }

        public static void SineFists(this CyclopsEye eye) 
        {
            var dynData = DynamicData.For(eye);
            dynData.Invoke("SineFists");
        }

        public static int FR(this CyclopsEye eye, int mult) 
        {
            var dynData = DynamicData.For(eye);
            return dynData.Invoke<int>("FR", mult);
        }
    }
}