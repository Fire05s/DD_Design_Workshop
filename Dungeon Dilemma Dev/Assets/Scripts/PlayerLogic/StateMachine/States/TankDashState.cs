using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the Tank Dash State.
/// </summary>
public class TankDashState : DashState
{
    [Header("Tank Dash")]
    [SerializeField] private float _powerScaling;
    [SerializeField] private float _allyCheckRadius;
    [SerializeField] private string _allyTag = "Challenger";
    [SerializeField] private Collider2D _armorCollider;
    private List<Collider2D> _protectedAllies = new List<Collider2D>();

    public override void Enter()
    {
        base.Enter();
        _protectedAllies.Clear();
    }

    protected override void ProcessDashEffect()
    {
        ApplyArmorBuff();
    }

    /// <summary>
    /// Applies armor to this player and allies hit by the dash
    /// </summary>
    private void ApplyArmorBuff()
    {
        if (stats == null) return;

        float amount = stats.AttackPower.value * _powerScaling;

        if (!_protectedAllies.Contains(_armorCollider))
        {
            stats.AddArmor(amount, stats.MaximumHealth * 0.25f);
            _protectedAllies.Add(_armorCollider);
        }

        List<Collider2D> hits = new List<Collider2D>();
        _armorCollider.Overlap(new ContactFilter2D().NoFilter(), hits);

        foreach (Collider2D col in hits)
        {
            if (col.gameObject == core.gameObject || _protectedAllies.Contains(col)) continue;
            if (!string.IsNullOrEmpty(_allyTag) && !col.CompareTag(_allyTag)) continue;

            _protectedAllies.Add(col);
            SyncedStats otherStats = col.GetComponentInParent<SyncedStats>();
            if (otherStats == null)
            {
                otherStats = col.GetComponent<SyncedStats>();
            }
            if (otherStats != null && otherStats != stats)
            {
                otherStats.AddArmor(amount, otherStats.MaximumHealth * 0.25f);
            }
        }
    }
}
