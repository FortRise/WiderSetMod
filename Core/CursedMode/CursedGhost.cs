using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace CursedMode;

[CustomEnemy(
    "CursedMode/Ghost = Blue",
    "CursedMode/GreenGhost = Green"
)]
public class CursedGhost : Ghost
{
    public static CursedGhost Blue(Vector2 position, Facing facing, Vector2[] nodes) 
        => new CursedGhost(position, facing, nodes, GhostTypes.Blue);

    public static CursedGhost Green(Vector2 position, Facing facing, Vector2[] nodes) 
        => new CursedGhost(position, facing, nodes, GhostTypes.Green);
    public CursedGhost(Vector2 position, Facing facing, Vector2[] nodes, GhostTypes ghostType) 
        : base(position, facing, nodes, ghostType)
    {
        NaiveMove = true;
    }

    public override bool CanSeePlayer(Player player)
    {
        return player != null && player.Detectable(Position + SightCheckOffset);
    }

    protected override void OnCollideH(TowerFall.Platform platform)
    {
    }

    protected override void OnCollideV(TowerFall.Platform platform)
    {
    }
}
