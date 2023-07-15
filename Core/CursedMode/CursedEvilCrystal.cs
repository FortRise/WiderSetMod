using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace CursedMode;

[CustomEnemy(
    "CursedMode/EvilCrystal = Red",
    "CursedMode/BlueCrystal = Blue"
)]
public class CursedEvilCrystal : EvilCrystal
{
    public static EvilCrystal Red(Vector2 position, Facing facing, Vector2[] nodes) 
        => new CursedEvilCrystal(position, facing, CrystalColors.Red, nodes);

    public static EvilCrystal Blue(Vector2 position, Facing facing, Vector2[] nodes) 
        => new CursedEvilCrystal(position, facing, CrystalColors.Blue, nodes);
    
    public CursedEvilCrystal(Vector2 position, Facing facing, CrystalColors color, Vector2[] nodes) : base(position, facing, color, nodes)
    {
        LaserBounceStart = 0;
    }
}
