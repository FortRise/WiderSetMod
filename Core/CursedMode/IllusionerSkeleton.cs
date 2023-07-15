using System;
using System.Collections;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using TowerFall;

namespace CursedMode;

[CustomEnemy("CursedMode/LiteralLaserSkeleton = NoShield")]
public class LiteralLaserSkeleton : Skeleton
{
    private KingReaper.ReaperBeam beam;
    private DynamicData data;
    private PlayerShield data_shield 
    {
        get => data.Get<PlayerShield>("shield");
        set => data.Set("shield", value);
    } 
    public static Skeleton NoShield(Vector2 position, Facing facing) 
        => new LiteralLaserSkeleton(position, facing, ArrowTypes.Normal, false, false, false, false, false);

    public LiteralLaserSkeleton(Vector2 position, Facing facing, ArrowTypes arrows, bool hasShield, bool hasWings, bool canMimic, bool jester, bool boss) 
        : base(position, facing, arrows, hasShield, hasWings, canMimic, jester, boss)
    {
        data = DynamicData.For(this);
        DefineState(4, "Laser");
    }

    public static void LoadEntity() 
    {
        IL.TowerFall.Skeleton.ctor += ctor_patch;
        On.TowerFall.Skeleton.ShootCoroutine += ShootCoroutine_patch;
    }

    public static void UnloadEntity() 
    {
        IL.TowerFall.Skeleton.ctor -= ctor_patch;
        On.TowerFall.Skeleton.ShootCoroutine -= ShootCoroutine_patch;
    }

    private static void ctor_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Skeleton, int>>((x, skelly) => {
                if (skelly is LiteralLaserSkeleton) 
                {
                    return 5;
                }
                return x;
            });
        }
    }

    public override void Die(int killerIndex, Arrow arrow = null, Explosion explosion = null, ShockCircle shock = null)
    {
        if (beam != null && beam.Scene != null) 
        {
            beam.QuickFade();
            Sounds.en_tentacleBossBeam.Stop();
        }
        base.Die(killerIndex, arrow, explosion, shock);
    }

    private static IEnumerator ShootCoroutine_patch(On.TowerFall.Skeleton.orig_ShootCoroutine orig, Skeleton self)
    {
        if (self is LiteralLaserSkeleton literalLaserSkeleton) 
        {
            literalLaserSkeleton.internal_ShootCoroutine();
            yield break;
        }
        yield return orig(self);
    }

    private void internal_ShootCoroutine() 
    {
        var target = DynamicData.For(this).Get<Player>("target");
        AimingAngle = 
            Calc.Snap(WrapMath.WrapAngle(Position, 
                target.Position), 0.3926991f) + Calc.Random.Choose(new float[] { -0.3926991f, 0f, 0f, 0.3926991f });
        State = 4;
    }

    private IEnumerator LaserCoroutine() 
    {
        Speed = Vector2.Zero;
        Sounds.en_tentacleBossBeam.Play(this.X, 1f);
        beam = new KingReaper.ReaperBeam(this, AimingAngle, 0, false, 60);
        Level.Add(beam);
        yield return 60f;
        Aiming = true;
        yield return 240f;
        data_shield = new PlayerShield(this);
        Add(data_shield);
        yield return 6f;
        data_shield.Gain();
        while (beam.Scene != null)
            yield return null;
        if (data_shield != null) 
        {
            data_shield.Lose();
            Remove(data_shield);
            data_shield = null;
        }

        State = 1;
    }

    private void LaserLeave() 
    {
        Aiming = false;
    }

    [MonoModLinkTo("TowerFall.Enemy", "System.Void Update()")]
    public void base_Update() 
    {
        base.Update();
    }

    public override void Update()
    {
        if (State == 4) 
        {
            if (beam != null) 
            {
                beam.SetAngle(AimingAngle);
            }
            base_Update();
            return;
        }
        base.Update();
    }
}

[CustomEnemy(
    "CursedMode/IllusionerSkeleton = NoShield",
    "CursedMode/IllusionerSkeletonS = Shield"
)]
public class IllusionerSkeleton : Skeleton
{
    private DynamicData data;
    private FakeSkeleton illusion;
    public Player data_target => data.Get<Player>("target");
    public List<Vector2> data_warpPoints => data.Get<List<Vector2>>("warpPoints");
    public Comparison<Vector2> data_warpSorter => data.Get<Comparison<Vector2>>("warpSorter");

    public static Skeleton NoShield(Vector2 position, Facing facing) 
        => new IllusionerSkeleton(position, facing, ArrowTypes.Normal, false, false, true, false, false);

    public static Skeleton Shield(Vector2 position, Facing facing) 
        => new IllusionerSkeleton(position, facing, ArrowTypes.Normal, true, false, true, false, false);


    public IllusionerSkeleton(Vector2 position, Facing facing, ArrowTypes arrows, bool hasShield, bool hasWings, bool canMimic, bool jester, bool boss) : base(position, facing, arrows, hasShield, hasWings, canMimic, jester, boss)
    {
        data = DynamicData.For(this);

    }

    private int WarpSorter(Vector2 a, Vector2 b)
    {
        if (this is null || data_target is null)
            return 1;
        if (Vector2.DistanceSquared(a, this.Position) <= 400f)
        {
            return 1;
        }
        if (Vector2.DistanceSquared(b, this.Position) <= 400f)
        {
            return -1;
        }
        return (int)(WrapMath.WrapDistanceSquared(a, data_target.Position) - WrapMath.WrapDistanceSquared(b, data_target.Position));
    }

    public static void LoadEntity() 
    {
        On.TowerFall.Skeleton.MimicPlayer += MimicPlayer_patch;
        On.TowerFall.Skeleton.Added += Added_patch;
        On.TowerFall.Skeleton.Die += Die_patch;
    }

    public static void UnloadEntity() 
    {
        On.TowerFall.Skeleton.MimicPlayer -= MimicPlayer_patch;
        On.TowerFall.Skeleton.Added -= Added_patch;
        On.TowerFall.Skeleton.Die -= Die_patch;
    }

    private static void Die_patch(On.TowerFall.Skeleton.orig_Die orig, Skeleton self, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock)
    {
        if (self is IllusionerSkeleton skeleton) 
        {
            if (skeleton.illusion?.Scene != null)
                skeleton.illusion.Die(killerIndex, arrow, explosion, shock);
        }
        orig(self, killerIndex, arrow, explosion, shock);
    }

    private void AddedSetup() 
    {
        var warpPoints = data.Get<List<Vector2>>("warpPoints");
        var warpSorter = data.Get<Comparison<Vector2>>("warpSorter");
        warpPoints = Level.GetXMLPositions("Spawner");
        warpSorter = new Comparison<Vector2>(WarpSorter);
        data.Set("warpSorter", warpSorter);
        data.Set("warpPoints", warpPoints);
    }

    private static void Added_patch(On.TowerFall.Skeleton.orig_Added orig, Skeleton self)
    {
        orig(self);
        if (self is IllusionerSkeleton skeleton) 
        {
            skeleton.AddedSetup();
        }
    }

    private static void MimicPlayer_patch(On.TowerFall.Skeleton.orig_MimicPlayer orig, Skeleton self, Player player)
    {
        orig(self, player);
        if (self is IllusionerSkeleton skeleton) 
        {
            var shield = skeleton.data.Get<PlayerShield>("shield");

            if (skeleton.data_warpPoints.Count <= 1)
                return;
            skeleton.data_warpPoints.Sort(skeleton.data_warpSorter);
            var desiredPosition = skeleton.data_warpPoints[1];

            if (shield == null)
                skeleton.Level.Add(skeleton.illusion = FakeSkeleton.Summon(desiredPosition, skeleton.Facing, false, player, skeleton.Arrows));
            else
                skeleton.Level.Add(skeleton.illusion = FakeSkeleton.Summon(desiredPosition, skeleton.Facing, true, player, skeleton.Arrows));
            skeleton.Level.Particles.Emit(Particles.TeamDust[1], 12, desiredPosition, new Vector2(5f, 8f));
        }
    }
}

[CustomEnemy(
    "CursedMode/FakeSkeleton = NoShield",
    "CursedMode/FakeSkeletonS = Shield"
)]
public class FakeSkeleton : Skeleton
{
    private Player toCopy;
    public static FakeSkeleton NoShield(Vector2 position, Facing facing) 
        => new FakeSkeleton(position, facing, ArrowTypes.Normal, false, false, true, false, false);

    public static FakeSkeleton Shield(Vector2 position, Facing facing) 
        => new FakeSkeleton(position, facing, ArrowTypes.Normal, true, false, true, false, false);

    public static FakeSkeleton Summon(Vector2 position, Facing facing, bool shield, Player player, EnemyArrowList arrowList)  
    {
        var skelly = new FakeSkeleton(position, facing, ArrowTypes.Normal, shield, false, true, false, false);
        skelly.toCopy = player;
        skelly.Arrows.Arrows.Clear();
        for (int i = 0; i < arrowList.Arrows.Count; i++) 
        {
            skelly.Arrows.AddArrow(arrowList.Arrows[i]);
        }
        return skelly;
    }

    public FakeSkeleton(Vector2 position, Facing facing, ArrowTypes arrows, bool hasShield, bool hasWings, bool canMimic, bool jester, bool boss) : base(position, facing, arrows, hasShield, hasWings, canMimic, jester, boss)
    {
    }

    public override void Load()
    {
        BouncesOnPlayer = false;
        base.Untag(Monocle.GameTags.PlayerCollider);
    }

    public override void Added()
    {
        base.Added();
        DynamicData.For(this).Invoke("MimicPlayer", toCopy);
    }

    private static Action<Skeleton, int, Arrow, Explosion, ShockCircle> base_Die;

    public static void LoadEntity() 
    {
        base_Die = CallHelper.CallBaseGen<Enemy, Skeleton, int, Arrow, Explosion, ShockCircle>("Die");
        On.TowerFall.Skeleton.CanCatchArrow += CanCatchArrow_patch;
        On.TowerFall.Skeleton.CanShoot_float += CanShoot_float_patch;
        On.TowerFall.Skeleton.CanShoot_Player += CanShoot_Player_patch;
        On.TowerFall.Skeleton.Die += Die_patch;
        On.TowerFall.Skeleton.Hurt += Hurt_patch;
        
        On.TowerFall.Enemy.OnPlayerCollide += OnPlayerCollide_patch;
        On.TowerFall.Enemy.OnBounceOnPlayer += OnBounceOnPlayer_patch;
        On.TowerFall.Enemy.OnPlayerTouch += OnPlayerTouch_patch;
        On.TowerFall.Enemy.CollectArrow += CollectArrow_patch;
    }

    public static void UnloadEntity() 
    {
        On.TowerFall.Skeleton.CanCatchArrow -= CanCatchArrow_patch;
        On.TowerFall.Skeleton.CanShoot_float -= CanShoot_float_patch;
        On.TowerFall.Skeleton.CanShoot_Player -= CanShoot_Player_patch;
        On.TowerFall.Skeleton.Die -= Die_patch;
        On.TowerFall.Skeleton.Hurt -= Hurt_patch;

        On.TowerFall.Enemy.OnPlayerCollide -= OnPlayerCollide_patch;
        On.TowerFall.Enemy.OnBounceOnPlayer -= OnBounceOnPlayer_patch;
        On.TowerFall.Enemy.OnPlayerTouch -= OnPlayerTouch_patch;
        On.TowerFall.Enemy.CollectArrow -= CollectArrow_patch;
    }

    private static void OnPlayerCollide_patch(On.TowerFall.Enemy.orig_OnPlayerCollide orig, Enemy self, Player player)
    {
        if (self is FakeSkeleton) 
        {
            return;
        }
        orig(self, player);
    }

    private static void CollectArrow_patch(On.TowerFall.Enemy.orig_CollectArrow orig, Enemy self, Arrow arrow)
    {
        if (self is FakeSkeleton) 
        {
            return;
        }
        orig(self, arrow);
    }

    private static void OnPlayerTouch_patch(On.TowerFall.Enemy.orig_OnPlayerTouch orig, Enemy self, Player player)
    {
        if (self is FakeSkeleton) 
        {
            return;
        }
        orig(self, player);
    }

    private static void OnBounceOnPlayer_patch(On.TowerFall.Enemy.orig_OnBounceOnPlayer orig, Enemy self, Player player)
    {
        if (self is FakeSkeleton) 
        {
            return;
        }
        orig(self, player);
    }

    [MonoModLinkTo("TowerFall.Enemy", "System.Void Hurt(Microsoft.Xna.Framework.Vector2,System.Int32,System.Int32,TowerFall.Arrow,TowerFall.Explosion,TowerFall.ShockCircle)")]
    private void base_Hurt(Vector2 force, int damage, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock) 
    {
        base.Hurt(force, damage, killerIndex, arrow, explosion, shock);
    }

    private void internal_Hurt(Vector2 force, int damage, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock) 
    {
        base_Hurt(force, damage, killerIndex, arrow, explosion, shock);
    }

    private static void Hurt_patch(On.TowerFall.Skeleton.orig_Hurt orig, Skeleton self, Vector2 force, int damage, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock)
    {
        if (self is FakeSkeleton skeleton) 
        {
            skeleton.internal_Hurt(force, damage, killerIndex, arrow, explosion, shock);
            return;
        }
        orig(self, force, damage, killerIndex, arrow, explosion, shock);
    }

    private static void Die_patch(On.TowerFall.Skeleton.orig_Die orig, Skeleton self, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock)
    {
        if (self is FakeSkeleton skeleton) 
        {
            skeleton.Level.Particles.Emit(Particles.TeamDust[1], 12, skeleton.Position, new Vector2(5f, 8f));
            base_Die(self, killerIndex, arrow, explosion, shock);
            skeleton.RemoveSelf();
            return;
        }
        orig(self, killerIndex, arrow, explosion, shock);
    }

    private static bool CanShoot_Player_patch(On.TowerFall.Skeleton.orig_CanShoot_Player orig, Skeleton self, Player target)
    {
        if (self is FakeSkeleton)
            return false;
        return orig(self, target);
    }

    private static bool CanShoot_float_patch(On.TowerFall.Skeleton.orig_CanShoot_float orig, Skeleton self, float angle)
    {
        if (self is FakeSkeleton)
            return false;
        return orig(self, angle);
    }

    private static bool CanCatchArrow_patch(On.TowerFall.Skeleton.orig_CanCatchArrow orig, Skeleton self, Arrow arrow)
    {
        if (self is FakeSkeleton)
            return false;
        return orig(self, arrow);
    }
}