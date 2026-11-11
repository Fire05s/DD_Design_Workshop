using System.Collections.Generic;
using UnityEngine;

public class MageDashState : DashState
{
    [SerializeField] private GameObject cursedGroundPrefab;
    [SerializeField] private List<string> _enemyTags;
    [SerializeField] private float _damageMultiplier;

    public override void Enter()
    {
        base.Enter();
        GameObject cursedGround = Instantiate(cursedGroundPrefab, transform.position, Quaternion.identity);
        cursedGround.GetComponent<CursedGround>().InitGround(combat, combat.GetComponentInChildren<HealthHandler>(), _enemyTags, _damageMultiplier);
    }
}
