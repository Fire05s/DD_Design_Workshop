using System.Collections;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// All traps should inherit from this class. Start() is already implemented and should not have to be modified
/// or overriden. A cooldown coroutine is a part of this base class, for ease of application to all traps. For
/// the few traps where cooldowns don't apply or are more niche, a cooldown timer of 0 or an alternative solution
/// can be used. Any other trap specific information should be added as necessary to the class.
/// 
/// ActivateTrap() is abstract, and MUST be overriden by each child class.
/// </summary>
public abstract class Trap : NetworkBehaviour
{
    // [SerializeField] private TrapData _trapData;
    [SerializeField] protected float _damage;
    [SerializeField] protected string _trapName;
    [SerializeField] protected Sprite _sprite;
    [SerializeField] protected float _cooldown;
    private bool _canActivate = true;

    /// <summary>
    /// Initialize all the Trap Data values.
    /// </summary>
    void Start()
    {
        /*
        _damage = _trapData.BaseDamage;
        _trapName = _trapData.Name;
        _sprite = _trapData.FloorSprite;
        _cooldown = _trapData.Cooldown; */
    }

    /// <summary>
    /// On a Challenger colliding with this Trap, activate this Trap - readying it, dealing damage, or applying its effect.
    /// </summary>
    /// <param name="collision">The other object that collided with this trap.</param>
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Challenger") && _canActivate)
        {
            ActivateTrap(collision);
            StartCoroutine(CooldownTimer(_cooldown));
        }
    }

    /// <summary>
    /// Timer for any internal Trap cooldowns. At base, there will at least be one cooldown for the trap as a whole
    /// that could be set to 0 if applicable. If this needs to be overwritten, it is possible.
    /// </summary>
    /// <param name="time">The time that this cooldown is on.</param>
    /// <returns></returns>
    protected virtual IEnumerator CooldownTimer(float time)
    {
        _canActivate = false;
        yield return new WaitForSeconds(time);
        _canActivate = true;
    }

    /// <summary>
    /// Called upon a collision with this Trap or its associated hitbox - enacting whatever this trap is meant to do.
    /// This MUST be overriden by each Trap.
    /// </summary>
    /// <param name="challenger">The Challenger that collided with this trap.</param>
    protected abstract void ActivateTrap(Collider2D challenger);
}
