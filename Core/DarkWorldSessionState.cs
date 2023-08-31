namespace EightPlayerMod;

public static class DarkWorldSessionStatePatch 
{
    public static void Load() 
    {
        IL.TowerFall.DarkWorldSessionState.ctor += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.DistributeArrow += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.OnContinue += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.OnLevelComplete += PlayerAmountUtils.Player4ToPlayer8;
    }

    public static void Unload() 
    {
        IL.TowerFall.DarkWorldSessionState.ctor -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.DistributeArrow -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.OnContinue -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldSessionState.OnLevelComplete -= PlayerAmountUtils.Player4ToPlayer8;
    }
}