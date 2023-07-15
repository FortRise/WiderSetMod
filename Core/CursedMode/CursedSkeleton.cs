using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace CursedMode;

[CustomEnemy(
    "CursedMode/CursedSkeleton = NoShield",
    "CursedMode/CursedSkeletonS = Shield"
)]
public class CursedSkeleton : Skeleton
{
    public static long GlobalCursedID = 0;
    private long cursedID;
    public static Skeleton NoShield(Vector2 position, Facing facing) 
        => new CursedSkeleton(position, facing, ArrowTypes.Prism, false, false, false, false, false);

    public static Skeleton Shield(Vector2 position, Facing facing) 
        => new CursedSkeleton(position, facing, ArrowTypes.Prism, true, false, false, false, false);

    public CursedSkeleton(Vector2 position, Facing facing, ArrowTypes arrows, bool hasShield, bool hasWings, bool canMimic, bool jester, bool boss) : base(position, facing, arrows, hasShield, hasWings, canMimic, jester, boss)
    {
        cursedID = GlobalCursedID++;
    }

    public static void LoadEntity() 
    {

    }

    public static void UnloadEntity() 
    {

    }
}