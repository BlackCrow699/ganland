using System.Collections.Generic;

public static class BattleTurnController
{
    public static void SortBySpeed(List<CombatUnit> units)
    {
        if (units == null) return;
        units.Sort((a, b) =>
        {
            int speed = b.speed.CompareTo(a.speed);
            return speed != 0 ? speed : a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
    }
}
