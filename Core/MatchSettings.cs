using System;
using FortRise;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public class MatchSettingsPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.MatchSettings.ctor += ctor_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.MatchSettings.ctor -= ctor_patch;
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<MatchSettings>())) 
            {
                cursor.EmitDelegate<Func<MatchTeams, MatchTeams>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return new EightPlayerMatchTeams(Allegiance.Neutral);
                    return x;
                });
            }
        }
    }


    public class EightPlayerMatchTeams : MatchTeams
    {
        private static IDetour hook_get_ProperlyAssigned;
        private static IDetour hook_get_HasEvenTeams;
        private static IDetour hook_get_Item;
        private static IDetour hook_set_Item;
        public Allegiance player5Team;
		public Allegiance player6Team;
		public Allegiance player7Team;
		public Allegiance player8Team;


        public EightPlayerMatchTeams(Allegiance start) : base(start)
        {
            player5Team = start;
            player6Team = start;
            player7Team = start;
            player8Team = start;
        }

        public static void Load() 
        {
            On.TowerFall.MatchTeams.GetAmountOfPlayersOfAllegiance += GetAmountOfPlayersOfAllegiance_patch;
            hook_get_ProperlyAssigned = new ILHook(
                typeof(MatchTeams).GetProperty("ProperlyAssigned").GetGetMethod(),
                ProperlyAssigned_patch
            );
            hook_get_HasEvenTeams = new Hook(
                typeof(MatchTeams).GetProperty("HasEvenTeams").GetGetMethod(),
                typeof(EightPlayerMatchTeams).GetMethod(nameof(HasEvenTeams_patch))
            );
            hook_get_Item = new Hook(
                typeof(MatchTeams).GetMethod("get_Item"),
                typeof(EightPlayerMatchTeams).GetMethod(nameof(get_Item_patch))
            );
            hook_set_Item = new Hook(
                typeof(MatchTeams).GetMethod("set_Item"),
                typeof(EightPlayerMatchTeams).GetMethod(nameof(set_Item_patch))
            );
        }

        public static void Unload() 
        {
            On.TowerFall.MatchTeams.GetAmountOfPlayersOfAllegiance -= GetAmountOfPlayersOfAllegiance_patch;
            hook_get_ProperlyAssigned.Dispose();
            hook_get_HasEvenTeams.Dispose();
            hook_get_Item.Dispose();
            hook_set_Item.Dispose();
        }

        public delegate void orig_set_Item(MatchTeams self, int index, Allegiance value);

        public static void set_Item_patch(orig_set_Item orig, MatchTeams self, int index, Allegiance value) 
        {
            if ((EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer) && self is EightPlayerMatchTeams eightSelf) 
            {
                switch (index)
                {
                case 0:
                    eightSelf.player1Team = value;
                    return;
                case 1:
                    eightSelf.player2Team = value;
                    return;
                case 2:
                    eightSelf.player3Team = value;
                    return;
                case 3:
                    eightSelf.player4Team = value;
                    return;
                case 4:
                    eightSelf.player5Team = value;
                    return;
                case 5:
                    eightSelf.player6Team = value;
                    return;
                case 6:
                    eightSelf.player7Team = value;
                    return;
                case 7:
                    eightSelf.player8Team = value;
                    return;
                default:
                    throw new Exception("Index out of bounds!");
                }
            }

            orig(self, index, value);
        }

        public delegate Allegiance orig_get_Item(MatchTeams self, int index);

        public static Allegiance get_Item_patch(orig_get_Item orig, MatchTeams self, int index) 
        {
            if ((EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer) && self is EightPlayerMatchTeams eightSelf) 
            {
                return index switch 
                {
                    -1 => Allegiance.Red,
                    0 => eightSelf.player1Team,
                    1 => eightSelf.player2Team,
                    2 => eightSelf.player3Team,
                    3 => eightSelf.player4Team,
                    4 => eightSelf.player5Team,
                    5 => eightSelf.player6Team,
                    6 => eightSelf.player7Team,
                    7 => eightSelf.player8Team,
                    _ => throw new Exception("Index out of bounds!")
                };
            }

            return orig(self, index);
        }

        public delegate bool orig_HasEvenTeams(MatchTeams self);

        public static bool HasEvenTeams_patch(orig_HasEvenTeams orig, MatchTeams self) 
        {
            if ((EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer) && self is EightPlayerMatchTeams eightSelf) 
            {
                var selfDynamic = DynamicData.For(self);
                int num = Calc.Count<Allegiance>(Allegiance.Blue, 
                    selfDynamic.Get<Allegiance>("player1Team"),
                    selfDynamic.Get<Allegiance>("player2Team"),
                    selfDynamic.Get<Allegiance>("player3Team"),
                    selfDynamic.Get<Allegiance>("player4Team"),
                    eightSelf.player5Team,
                    eightSelf.player6Team,
                    eightSelf.player7Team,
                    eightSelf.player8Team
                );
                int num2 = Calc.Count<Allegiance>(Allegiance.Blue, 
                    selfDynamic.Get<Allegiance>("player1Team"),
                    selfDynamic.Get<Allegiance>("player2Team"),
                    selfDynamic.Get<Allegiance>("player3Team"),
                    selfDynamic.Get<Allegiance>("player4Team"),
                    eightSelf.player5Team,
                    eightSelf.player6Team,
                    eightSelf.player7Team,
                    eightSelf.player8Team
                );
                return num == num2;
            }
            return orig(self);
        }

        private static void ProperlyAssigned_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer)
                        return 8;
                    return x;
                });
            }
        }

        private static int GetAmountOfPlayersOfAllegiance_patch(On.TowerFall.MatchTeams.orig_GetAmountOfPlayersOfAllegiance orig, MatchTeams self, Allegiance allegiance)
        {
            int num = orig(self, allegiance);
            if (self is not EightPlayerMatchTeams eightSelf)
                return num;
			if (eightSelf.player5Team == allegiance && TFGame.Players[4])
			{
				num++;
			}
			if (eightSelf.player6Team == allegiance && TFGame.Players[5])
			{
				num++;
			}
			if (eightSelf.player7Team == allegiance && TFGame.Players[6])
			{
				num++;
			}
			if (eightSelf.player8Team == allegiance && TFGame.Players[7])
			{
				num++;
			}
			return num;
        }
    }
}