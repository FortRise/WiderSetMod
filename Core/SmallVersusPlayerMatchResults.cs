using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public class SmallVersusPlayerMatchResults : VersusPlayerMatchResults
    {
        public SmallVersusPlayerMatchResults(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards) 
            :base(session, matchResults, playerIndex, tweenFrom, tweenTo, awards)
        {
            
        }

        public static void Load() 
        {
            On.TowerFall.VersusPlayerMatchResults.DoSequence += DoSequence_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.VersusPlayerMatchResults.DoSequence -= DoSequence_patch;
        }

        private static void DoSequence_patch(On.TowerFall.VersusPlayerMatchResults.orig_DoSequence orig, VersusPlayerMatchResults self)
        {
            if (self is SmallVersusPlayerMatchResults result) 
            {
                result.Add(new Coroutine(result.SmallSequence()));
                return;
            }
            orig(self);
        }

        private IEnumerator SmallSequence() 
        {
            var selfDynamic = DynamicData.For(this);
            var portrait = selfDynamic.Get<Image>("portrait");
            var session = selfDynamic.Get<Session>("session");
            var playerIndex = selfDynamic.Get<int>("playerIndex");
            var awards = selfDynamic.Get<List<AwardInfo>>("awards");
            yield return 30;
			Vector2 end2 = portrait.Position + Vector2.UnitY * (portrait.Height / 2f + 2f);
			Vector2 start2 = end2 + Vector2.UnitX * -20f;
			ulong kills = session.MatchStats[playerIndex].Kills.Kills;
			ulong deaths = session.MatchStats[playerIndex].Deaths.Total - session.MatchStats[playerIndex].Deaths.Environment;

            var killDeathSelf = new OutlineText(TFGame.Font, $"{kills}/{deaths}");
			killDeathSelf.Color = Color.Transparent;
			killDeathSelf.OutlineColor = Color.Transparent;
			Add(killDeathSelf);
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 30, true);
			tween.OnUpdate = delegate(Tween t)
			{
				killDeathSelf.Position = Vector2.Lerp(start2, end2, t.Eased);
				killDeathSelf.Color = Color.White * t.Eased;
				killDeathSelf.OutlineColor = Color.Black * t.Eased * t.Eased;
			};
            Add(tween);

			yield return 30;
			int num5;
			for (int i = 0; i < awards.Count; i = num5 + 1)
			{
				AwardInfo awardInfo = awards[i];
				Vector2 iconEnd = portrait.Position + Vector2.UnitY * (portrait.Height / 2f + 16f + (float)(i * 15));
				Vector2 iconStart = iconEnd + Vector2.UnitX * -20f;
				Sprite<int> icon = awardInfo.GetSprite(false);
				icon.CenterOrigin();
				icon.Position = iconStart;
				icon.Scale = Vector2.One * 1.5f;
				icon.Color = Color.Transparent;
				icon.Play(0, false);
				int anim = 0;
				icon.OnAnimationComplete = delegate(Sprite<int> s)
				{
					if (!icon.Looping)
					{
						Sprite<int> icon2 = icon;
						int num6 = anim + 1;
						anim = num6;
						icon2.Play(num6, false);
					}
				};
				this.Add(icon);
				tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
				tween.OnUpdate = delegate(Tween t)
				{
					icon.Color = Color.White * t.Eased;
					icon.Rotation = MathHelper.Lerp(-0.34906584f, 0f, t.Eased);
					icon.Position = Vector2.Lerp(iconStart, iconEnd, t.Eased);
				};
				tween.OnComplete = delegate(Tween t)
				{
					Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 20, true);
					tween2.OnUpdate = delegate(Tween tt)
					{
						icon.Scale = Vector2.One * MathHelper.Lerp(1.5f, 1f, tt.Eased);
					};
					Add(tween2);
				};
				Add(tween);
				if (i == 0)
				{
					Sounds.sfx_statscreenReward1.Play(this.X, 1f);
				}
				else if (i == 1)
				{
					Sounds.sfx_statscreenReward2.Play(this.X, 1f);
				}
				else
				{
					Sounds.sfx_statscreenReward3.Play(this.X, 1f);
				}
				yield return 40;
				num5 = i;
			}
			this.Finished = true;
			yield break;
        }

        public void AsSmall(bool won) 
        {
            var rect = this.GetFirst<DrawRectangle>();
            Remove(rect);
            var privateData = DynamicData.For(this);
            var portrait = privateData.Get<Image>("portrait");
            var gem = privateData.Get<Sprite<string>>("gem");
            portrait.Scale = Vector2.One * 0.7f;
            var bg = privateData.Get<Image>("bg");
            bg.SwapSubtexture(EightPlayerModule.Instance.EightPlayerAtlas[won ? "portrait/gold" : "portrait/silver"]);
            portrait.Position.Y -= 10f;
            gem.Scale = Vector2.One * 0.7f;
            gem.Position.Y += 5f;
        }
    }
}