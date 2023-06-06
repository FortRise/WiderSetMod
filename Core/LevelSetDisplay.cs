using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace EightPlayerMod 
{
    public class LevelSetDisplay : MenuItem
    {
        private static Color WideTextColor = Calc.HexToColor("95F94D");
        private StandardSetButton standard;
        private EightPlayerSetButton eightPlayer;
        private bool standardSelected;
        private bool wideSelected;
        private float drawStandard;
        private float drawWide;
        private bool tweeningOut;
        private SineWave alphaSine;

        public LevelSetDisplay(StandardSetButton standardSet, EightPlayerSetButton eightPlayerSet) : base(Vector2.Zero)
        {
            standard = standardSet;
            eightPlayer = eightPlayerSet;
            LayerIndex = -1;
            Add(alphaSine = new SineWave(120));
        }

        public override void Update()
        {
            base.Update();
            if (standard.Selected) 
            {
                standardSelected = true;
                wideSelected = false;
            }
            else if (eightPlayer.Selected) 
            {
                standardSelected = false;
                wideSelected = true;
            }
			if (tweeningOut)
			{
				drawStandard = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
				drawWide = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
			}
			else if (standardSelected)
			{
				drawStandard = Calc.Approach(drawStandard, 1f, 0.05f * Engine.TimeMult);
				drawWide = Calc.Approach(drawWide, 0f, 0.05f * Engine.TimeMult);
			}
			else if (wideSelected)
			{
				drawStandard = Calc.Approach(drawStandard, 0f, 0.05f * Engine.TimeMult);
				drawWide = Calc.Approach(drawWide, 1f, 0.05f * Engine.TimeMult);
			}
        }

        public override void Render()
        {
            base.Render();
            if (drawStandard > 0) 
            {
                Vector2 vector = Vector2.Lerp(Vector2.UnitX * -160f, Vector2.UnitX * 160f, Ease.CubeInOut(drawStandard));
				Draw.TextureBannerV(
                    TFGame.MenuAtlas["questResults/tipBanner"], vector + new Vector2(0f, 185f), 
                    new Vector2(100f, 37f), Vector2.One, 0f, Color.White * drawStandard, 
                    SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
                if (EightPlayerModule.CanCoopLevelSet) 
                {
                    Draw.OutlineTextCentered(TFGame.Font, "1-4 ARCHERS ONLY", vector + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "ORIGINAL AND BALANCED LEVELS!", vector + new Vector2(0f, 168f), Color.White, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "PERFECT SUITE FOR SOLO PLAYERS", vector + new Vector2(0f, 176f), Color.White, Color.Black);
                }
                else 
                {
                    Draw.OutlineTextCentered(TFGame.Font, "2-4 ARCHERS ONLY", vector + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "THE ORIGINAL TOWERFALL STAGES!", vector + new Vector2(0f, 168f), Color.White, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "CLASSIC, FRANTIC FUN.", vector + new Vector2(0f, 176f), Color.White, Color.Black);
                }
            }
            if (drawWide > 0) 
            {
				Vector2 vector2 = Vector2.Lerp(Vector2.UnitX * 480f, Vector2.UnitX * 160f, Ease.CubeInOut(drawWide));
				Draw.TextureBannerV(TFGame.MenuAtlas["questResults/darkWorldBanner"], vector2 + new Vector2(0f, 185f), new Vector2(100f, 37f), Vector2.One, 0f, Color.White * drawWide, SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
                if (EightPlayerModule.CanCoopLevelSet) 
                {
                    Draw.OutlineTextCentered(TFGame.Font, "SUPPORTS UP TO 4-8 ARCHERS", vector2 + new Vector2(0f, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "CHAOTIC AND FUN LEVELS", vector2 + new Vector2(0f, 168f), Color.White, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "WIDER EXPERIENCE!", vector2 + new Vector2(0f, 176f), Color.White, Color.Black);
                }
                else 
                {
                    Draw.OutlineTextCentered(TFGame.Font, "SUPPORTS UP TO 8 ARCHERS", vector2 + new Vector2(0f, 160f), WideTextColor, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "EXPANDED STAGES FOR MORE FIGHTERS", vector2 + new Vector2(0f, 168f), Color.White, Color.Black);
                    Draw.OutlineTextCentered(TFGame.Font, "AND EPIC TEAM BATTLES!", vector2 + new Vector2(0f, 176f), Color.White, Color.Black);
                }
            }
        }

        public override void TweenIn()
        {
        }

        public override void TweenOut()
        {
            tweeningOut = true;
        }

        protected override void OnConfirm()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDeselect()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnSelect()
        {
            throw new System.NotImplementedException();
        }
    }
}