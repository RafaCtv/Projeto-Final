using UnityEngine;

public enum ZombieTargetKind { None, Player, Base, Building }

public static class ZombieTargets
{
    public static ZombieTargetKind Classify(Health target)
    {
        if (target == null) return ZombieTargetKind.None;
        if (target.GetComponent<PlayerController>() != null) return ZombieTargetKind.Player;
        if (target.GetComponent<BaseCore>() != null) return ZombieTargetKind.Base;
        if (target.GetComponent<BuildingInstance>() != null) return ZombieTargetKind.Building;
        return ZombieTargetKind.None;
    }

    public static bool IsAttackable(Health target) => Classify(target) != ZombieTargetKind.None;
}
