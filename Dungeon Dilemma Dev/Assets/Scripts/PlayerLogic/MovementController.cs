using PurrNet;
using UnityEngine;

/// <summary>
/// Universal helper script for handling rigidbody movement.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : NetworkBehaviour
{
    // Movement
    private Rigidbody2D _rb;
    private Vector2 _velocity;
    private bool _canMove = true;

    [Header("Audio")]
    [SerializeField] private AudioClip _walkAudio;

    private bool _playAudio = true;

    // Public read-only properties
    public Vector2 CurrentVelocity => _rb.linearVelocity;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Moves the player's rigidbody based on the provided input and speed.
    /// </summary>
    /// <param name="input"> The player's x and y inputs. </param>
    /// <param name="speed"> The player's current speed. </param>
    public void SetMovement(Vector2 direction, float speed, bool withAudio = true)
    {
        if (!_canMove)
        {
            _velocity = Vector2.zero;
            return;
        }
        _velocity = direction * speed;
        _playAudio = withAudio;
    }

    /// <summary>
    /// Controls the player's ability to move.
    /// </summary>
    /// <param name="status"> Setting this to false restricts the player's movement. </param>
    public void SetCanMove(bool status)
    {
        _canMove = status;
        if (!status) _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Applies a force on the player's rigidbody.
    /// </summary>
    /// <param name="force"> The amount of force to apply on the player's rigidbody. </param>
    public void ApplyImpulse(Vector2 force)
    {
        _rb.AddForce(force, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Plays alternating footstep sounds.
    /// </summary>
    private void PlayFootstep()
    {
        SoundFXManager.Instance.SetLoopPitch(transform, Random.Range(0.95f, 1.05f));
        SoundFXManager.Instance.PlayLoop(_walkAudio, transform);
    }

    private void Update()
    {
        if (!isOwner) return;

        SoundCheck();
    }

    private void FixedUpdate()
    {
        if (!isOwner) return;

        MovementCheck();
    }

    /// <summary>
    /// Updates the player's velocity if they can move.
    /// </summary>
    private void MovementCheck()
    {
        if (_canMove)
        {
            _rb.linearVelocity = _velocity;
        }
    }

    /// <summary>
    /// Plays footstep sounds if player is moving.
    /// </summary>
    private void SoundCheck()
    {
        if (_rb.linearVelocity.magnitude > 0.1f && _playAudio) PlayFootstep();
        else SoundFXManager.Instance.StopLoop(transform);
    }
}
