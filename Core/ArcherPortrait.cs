using System;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class ArcherPortraitPatch 
    {
        private static Action<ArcherPortrait> base_Render;
        private static ConstructorInfo base_ArcherPortrait;
        private static FastReflectionDelegate InitGem;
        private static FastReflectionDelegate ShouldFlip;
        public static void Load() 
        {
            base_Render = CallHelper.CallBaseGen<CompositeComponent, ArcherPortrait>("Render");
            base_ArcherPortrait = typeof(CompositeComponent).GetConstructor(new Type[2] { typeof(bool), typeof(bool) });
            InitGem = FastReflectionHelper.GetFastDelegate(typeof(ArcherPortrait).GetMethod("InitGem", BindingFlags.Instance | BindingFlags.NonPublic));
            ShouldFlip = FastReflectionHelper.GetFastDelegate(typeof(ArcherPortrait).GetMethod("ShouldFlip", BindingFlags.Instance | BindingFlags.NonPublic));
            On.TowerFall.ArcherPortrait.ctor += ArcherPortraitctor_patch;
            On.TowerFall.ArcherPortrait.SetCharacter += SetCharacter_patch;
            On.TowerFall.ArcherPortrait.Render += Render_patch;
            On.TowerFall.ArcherPortrait.Leave += Leave_patch;
            IL.TowerFall.ArcherPortrait.StartJoined += ShrinkPortraitArcherDataJoined;
        }

        public static void Unload() 
        {
            On.TowerFall.ArcherPortrait.ctor -= ArcherPortraitctor_patch;
            On.TowerFall.ArcherPortrait.SetCharacter -= SetCharacter_patch;
            On.TowerFall.ArcherPortrait.Render -= Render_patch;
            On.TowerFall.ArcherPortrait.Leave -= Leave_patch;
            IL.TowerFall.ArcherPortrait.StartJoined -= ShrinkPortraitArcherDataJoined;
        }

        private static void Leave_patch(On.TowerFall.ArcherPortrait.orig_Leave orig, ArcherPortrait self)
        {
            if (!EightPlayerModule.LaunchedEightPlayer) 
            {
                orig(self);
                return;
            }
            var selfDynamic = DynamicData.For(self);
            if (selfDynamic.Get<bool>("joined"))
            {
                var portrait = selfDynamic.Get<Image>("portrait");
                var gem = selfDynamic.Get<Sprite<string>>("gem");
                var gemWiggler = selfDynamic.Get<Wiggler>("gemWiggler");
                var wiggler = selfDynamic.Get<Wiggler>("wiggler");
                var shakeCounter = selfDynamic.Get<Counter>("shakeCounter");
                selfDynamic.Set("joined", false);
                portrait.Color = new Color(0.55f, 0.55f, 0.55f);
                var rect = self.ArcherData.Portraits.NotJoined.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60));
                portrait.SwapSubtexture(self.ArcherData.Portraits.NotJoined, rect);
                gem.Play("off", false);
                gemWiggler.Start();
                wiggler.Start();
                shakeCounter.Set(20);
                self.ArcherData.SFX.Deselect.Play(160f, 1f);
            }           
        }

        private static void Render_patch(On.TowerFall.ArcherPortrait.orig_Render orig, ArcherPortrait self)
        {
            if (!EightPlayerModule.LaunchedEightPlayer)
            {
                orig(self);
                return;
            }
            var selfDynamic = DynamicData.For(self);
            var offset = selfDynamic.Get<Vector2>("offset");
            var portrait = selfDynamic.Get<Image>("portrait");
            var portraitAlt = selfDynamic.Get<Image>("portraitAlt");
            var gem = selfDynamic.Get<Sprite<string>>("gem");
            var flash = selfDynamic.Get<Sprite<int>>("flash");
            var wiggler = selfDynamic.Get<Wiggler>("wiggler");
            var gemWiggler = selfDynamic.Get<Wiggler>("gemWiggler");
            var lastShake = selfDynamic.Get<Vector2>("lastShake");
            var lastMove = selfDynamic.Get<int>("lastMove");
            var joined = selfDynamic.Get<bool>("joined");
            var flipEase = selfDynamic.Get<float>("flipEase");

            Vector2 vector = self.Entity.Position + offset;

            portrait.Scale = (portraitAlt.Scale = (flash.Scale = new Vector2(1f + wiggler.Value * 0.05f, 1f - wiggler.Value * 0.05f)));
            portrait.Position = (portraitAlt.Position = (flash.Position = offset + lastShake));
            if (joined)
            {
                gem.Scale = Vector2.One * (1.5f + 0.2f * gemWiggler.Value);
                gem.Rotation = (float)(45 * lastMove) * 0.017453292f * gemWiggler.Value;
            }
            else
            {
                gem.Scale = Vector2.One * (1f + 0.2f * gemWiggler.Value);
                gem.Rotation = (float)(20 * lastMove) * 0.017453292f * gemWiggler.Value;
            }
            if (!joined)
            {
                if (flipEase < 0.5f)
                {
                    portraitAlt.Visible = true;
                    portrait.Visible = false;
                    Image image = portraitAlt;
                    image.Scale.X = image.Scale.X * MathHelper.Lerp(1f, 0f, flipEase * 2f);
                }
                else
                {
                    portraitAlt.Visible = false;
                    portrait.Visible = true;
                    Image image2 = portrait;
                    image2.Scale.X = image2.Scale.X * MathHelper.Lerp(0f, 1f, (flipEase - 0.5f) * 2f);
                }
                if (portrait.Visible)
                {
                    portrait.DrawOutline(2);
                }
                else if (portraitAlt.Visible)
                {
                    portraitAlt.DrawOutline(2);
                }
            }
            if (joined)
            {
                var vector2 = Calc.Round(new Vector2(vector.X - portrait.Width / 2f * portrait.Scale.X - 2f, vector.Y - portrait.Height * portrait.Scale.Y / 2f - 2f));
                var vector3 = Calc.Round(new Vector2(portrait.Width * portrait.Scale.X + 4f, portrait.Height * portrait.Scale.Y + 4f));
                Draw.Rect(vector2.X, vector2.Y, vector3.X, vector3.Y, self.ArcherData.ColorA);
                portrait.DrawOutline(1);
            }
            base_Render(self);
            gem.DrawOutline(1);
            gem.Render();
            // TODO fading title
        }

        private static void SetCharacter_patch(On.TowerFall.ArcherPortrait.orig_SetCharacter orig, ArcherPortrait self, int characterIndex, ArcherData.ArcherTypes altSelect, int moveDir)
        {
            if (!EightPlayerModule.LaunchedEightPlayer)
            {
                orig(self, characterIndex, altSelect, moveDir);
                return;
            }
            var selfDynamic = DynamicData.For(self);
            if (!selfDynamic.Get<bool>("joined"))
            {
                selfDynamic.Set("lastMove", moveDir);
                if ((bool)ShouldFlip(self, altSelect))
                {
                    selfDynamic.Set("flipEase", 1f - selfDynamic.Get<float>("flipEase"));
                }
                else
                {
                    selfDynamic.Get<Wiggler>("gemWiggler").Start();
                }
                selfDynamic.Set("CharacterIndex", characterIndex);
                selfDynamic.Set("AltSelect", altSelect);
                selfDynamic.Set("ArcherData", ArcherData.Get(
                    selfDynamic.Get<int>("CharacterIndex"), selfDynamic.Get<ArcherData.ArcherTypes>("AltSelect")));

                var portrait = selfDynamic.Get<Image>("portrait");
                var portraitAlt = selfDynamic.Get<Image>("portraitAlt");
                var flipSide = selfDynamic.Get<ArcherData>("FlipSide");

                portrait.SwapSubtexture(
                    self.ArcherData.Portraits.NotJoined,
                    self.ArcherData.Portraits.NotJoined.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60)));
                portraitAlt.SwapSubtexture(
                    flipSide.Portraits.NotJoined,
                    flipSide.Portraits.NotJoined.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60)));
                var gem = selfDynamic.Get<Sprite<string>>("gem");
                if (gem != null)
                {
                    self.Remove(gem);
                }
                gem = TFGame.MenuSpriteData.GetSpriteString(self.ArcherData.Gems.Menu);
                gem.Position = selfDynamic.Get<Vector2>("offset") + new Vector2(0f, 30f);
                gem.Visible = false;
                self.Add(gem);
                selfDynamic.Set("gem", gem);
                selfDynamic.Get<Wiggler>("wiggler").Start();
            }
        }

        private static void InitGem_patch(On.TowerFall.ArcherPortrait.orig_InitGem orig, ArcherPortrait self)
        {
            if (!EightPlayerModule.LaunchedEightPlayer)
            {
                orig(self);
                return;
            }
            var selfDynamic = DynamicData.For(self);
            var gem = selfDynamic.Get<Sprite<string>>("gem");
            if (gem != null)
            {
                self.Remove(gem);
            }
            gem = TFGame.MenuSpriteData.GetSpriteString(self.ArcherData.Gems.Menu);
            gem.Position = selfDynamic.Get<Vector2>("offset") + new Vector2(0f, 60f);
            gem.Visible = false;
            self.Add(gem);
            selfDynamic.Set("gem", gem);
        }

        private static void ShrinkPortraitArcherDataJoined(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            var get_ArcherData = typeof(ArcherPortrait).GetProperty("ArcherData").GetGetMethod();
            var Portraits = typeof(ArcherData).GetField("Portraits");
            var Joined = typeof(ArcherData.PortraitInfo).GetField("Joined");

            var token0 = ctx.Body.Variables[0];
            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdfld<ArcherData.PortraitInfo>("Joined"),
                instr => instr.MatchLdloca(0),
                instr => instr.MatchInitobj<Rectangle?>(),
                instr => instr.MatchLdloc(0))) 
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Callvirt, get_ArcherData);
                cursor.Emit(OpCodes.Ldflda, Portraits);
                cursor.Emit(OpCodes.Ldfld, Joined);
                cursor.EmitDelegate<Func<Rectangle?, Subtexture, Rectangle?>>((n, texture) => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return texture.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60));
                    }
                    return n;
                });
            }
        }

        private static void ShrinkPortraitArcherDataNotJoined(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            var get_ArcherData = typeof(ArcherPortrait).GetProperty("ArcherData").GetGetMethod();
            var Portraits = typeof(ArcherData).GetField("Portraits");
            var NotJoined = typeof(ArcherData.PortraitInfo).GetField("NotJoined");

            var token0 = ctx.Body.Variables[0];
            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdfld<ArcherData.PortraitInfo>("NotJoined"),
                instr => instr.MatchLdloca(0),
                instr => instr.MatchInitobj<Rectangle?>(),
                instr => instr.MatchLdloc(0))) 
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Callvirt, get_ArcherData);
                cursor.Emit(OpCodes.Ldflda, Portraits);
                cursor.Emit(OpCodes.Ldfld, NotJoined);
                cursor.EmitDelegate<Func<Rectangle?, Subtexture, Rectangle?>>((n, texture) => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return texture.GetAbsoluteClipRect(new Rectangle(0, 10, 60, 60));
                    }
                    return n;
                });
            }
        }

        private static void ArcherPortraitctor_patch(On.TowerFall.ArcherPortrait.orig_ctor orig, TowerFall.ArcherPortrait self, Vector2 offset, int characterIndex, TowerFall.ArcherData.ArcherTypes altSelect, bool showTitle)
        {
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                var selfDynamic = DynamicData.For(self);
                selfDynamic.Set("flipEase", 1f);
                base_ArcherPortrait.Invoke(self, new object[2] { true, true });
                selfDynamic.Set("offset", offset);
                self.ShowTitle = showTitle;
                selfDynamic.Set("CharacterIndex", characterIndex);
                selfDynamic.Set("AltSelect", altSelect);
                selfDynamic.Set("ArcherData", ArcherData.Get(self.CharacterIndex, self.AltSelect));
                var portrait = new Image(self.ArcherData.Portraits.NotJoined, new Rectangle(0, 10, 60, 60));
                portrait.CenterOrigin();
                portrait.Color = new Color(0.55f, 0.55f, 0.55f);
                self.Add(portrait);
                selfDynamic.Set("portrait", portrait);
                var flipSide = selfDynamic.Get<ArcherData>("FlipSide");
                var portraitAlt = new Image(flipSide.Portraits.NotJoined, new Rectangle(0, 10, 60, 60));
                portraitAlt.CenterOrigin();
                portraitAlt.Color = new Color(0.55f, 0.55f, 0.55f);
                portraitAlt.Visible = false;
                self.Add(portraitAlt);
                selfDynamic.Set("portraitAlt", portraitAlt);

                var flash = new Sprite<int>(TFGame.MenuAtlas["portraits/flash"], 60, 60, 0);
                flash.Add(0, 0.06f, false, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                flash.Visible = false;
                flash.CenterOrigin();
                flash.OnAnimationComplete = delegate(Sprite<int> s)
                {
                    s.Visible = false;
                };
                self.Add(flash);
                selfDynamic.Set("flash", flash);
                var gem = selfDynamic.Get<Sprite<string>>("gem");
                if (gem != null)
                {
                    self.Remove(gem);
                }
                gem = TFGame.MenuSpriteData.GetSpriteString(self.ArcherData.Gems.Menu);
                gem.Position = offset + new Vector2(0f, 30f);
                gem.Visible = false;
                self.Add(gem);
                selfDynamic.Set("gem", gem);

                var wiggler = Wiggler.Create(30, 4f);
                self.Add(wiggler);

                var gemWiggler = Wiggler.Create(30, 4f);
                self.Add(gemWiggler);
                var shakeCounter = new Counter();

                selfDynamic.Set("wiggler", wiggler);
                selfDynamic.Set("gemWiggler", gemWiggler);
                selfDynamic.Set("shakeCounter", shakeCounter);
                return;
            }
            orig(self, offset, characterIndex, altSelect, showTitle);
        }
    }
}