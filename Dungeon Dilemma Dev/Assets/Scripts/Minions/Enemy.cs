using ControllerLogic;
using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// Public base enemy class. All minions should apply unique stat and behavior modules
/// IMPORTANT README: All navmesh and raycast detections work by converting a 2d (x,y) to a 3d (x,0,y). 
/// The z value is only used for navmesh and LOS checks
/// Ty value of enemies are manually overriden by the z value produced by 3d navmesh agents.
/// </summary>

public class Enemy : StateMachineCore
{
    [Header("State Machine")]
    [SerializeField] private AIMove _aiMove;
    [SerializeField] private AIAttack _aiAttack;
    [SerializeField] private AIFlee _aiFlee;
    [SerializeField] private AIKnockback _aiIKnockback;

    [Header("Component References")]
    //[SerializeField] protected Transform _targetPlayer;
    [SerializeField] protected UnityEngine.AI.NavMeshAgent _agent;
    //[SerializeField] protected ParticleSystem _bloodParticles;

    [Header("Enemy Stats")]
    [SerializeField] private float _atkRaidScaling;
    [SerializeField] private float _hpChallScaling;
    [SerializeField] private float _hpRaidScaling;

    [SerializeField] private float _aggroDuration; // seconds to stay in combat after losing sight
    [SerializeField] private float _attackRange;
    [SerializeField] protected float _detectionRange = 5f;

    [Header("Fleeing Settings")]
    [SerializeField] private bool _willFlee;
    [SerializeField] private float _fleeProximityThreshold;
    [Header("Death")]
    [SerializeField] private ParticleSystem _deathParticles;
    private float _aggroTimer = 0f;
    private Transform _currentTarget = null; // cached target
    protected float _attackCooldown;
    protected float _knockbackDuration;
    protected float _weight;
    //network and additional combat variables

    protected SyncVar<Vector3> _destination = new();
    protected List<Transform> _players = new List<Transform>();
    private HealthHandler _healthHandler;
    protected Coroutine _applyKnockback;
    protected Color _originalColor;
    protected bool _hasLineOfSight;
    private bool _isReady = false;
    private bool _isDead = false;

    private void Start()
    {

    }

    protected override void OnSpawned()
    {
        SetupInstances();
        machine.Set(_aiMove);
        _agent.updateRotation = false;     // don't rotate the agent in 3D
        _agent.updateUpAxis = false;

        HealthHandler _healthHandler = GetComponent<HealthHandler>();
        _healthHandler.OnAttacked += HandleKnockback;
        _healthHandler.OnDeath += HandleDeath;
        int challNum = Math.Max(1, GlobalController.Singleton.GetAllStats().Count - 1);
        int roundNum = Math.Max(1, GlobalController.Singleton.GetWorldData().Round + 1);
        if (isServer)
        {
            // Handle stat changes on server
            stats.AttackPower.value += _atkRaidScaling * roundNum;

            _healthHandler.UpdateMaxHealth((_hpChallScaling * challNum) + (_hpRaidScaling * roundNum));
        }
        _isReady = true;
    }

    private void OnDisable()
    {
        if (_healthHandler != null) _healthHandler.OnAttacked -= HandleKnockback;
    }

    private void Update()
    {
        if (!IsValidOwner() || !_isReady || _isDead) return;
        FlipSpriteTowardsTarget();
        SelectState();
        machine.state.DoBranch();
        /*
        Vector3 pos = transform.position;
        pos.y = pos.z;
        transform.position = pos;*/
    }

    private void FixedUpdate()
    {
        if (!_isReady || _isDead) return;
        machine.state.FixedDoBranch();
    }

    /// <summary>
    /// Method to determine the correct state to switch to
    /// </summary>
    protected void SelectState()
    {
        if (_currentTarget == null || !CheckLOS(_currentTarget))
        {
            _aggroTimer -= Time.deltaTime;
            if (_aggroTimer <= 0f)
            {
                _currentTarget = GetNearestPlayer();
                if (_currentTarget != null) _aggroTimer = _aggroDuration;
            }
        }
        else
        {
            _aggroTimer = _aggroDuration;
        }

        if (_currentTarget == null)
        {
            _aiMove.SetTarget(_currentTarget);
            machine.Set(_aiMove);
            return;
        }

        if (IsAttacking() || IsFleeing() || IsKnockback()) return;

        float dist = Vector2.Distance(transform.position, _currentTarget.position);

        if (_willFlee && dist <= _fleeProximityThreshold)
        {
            _aiFlee.SetTarget(_currentTarget);
            machine.Set(_aiFlee, true);
        }
        else if (dist <= _attackRange)
        {
            _aiAttack.SetTarget(_currentTarget.position);
            machine.Set(_aiAttack, true);
        }
        else
        {
            _aiMove.SetTarget(_currentTarget);
            machine.Set(_aiMove);
        }
    }

    /// <summary>
    /// Sets and enters knock back state upon taking damage
    /// </summary>
    /// <param name="source"> Source of knockback. </param>
    /// <param name="isStatusEffect"> Whether damage taken was from a status effect or not. </param>
    private void HandleKnockback(Vector2 source, bool isStatusEffect)
    {
        if (!isStatusEffect)
        {
            _aiIKnockback.SetKnockback((Vector2)transform.position - source);
            machine.Set(_aiIKnockback, true);
        }
    }

    [ObserversRpc(runLocally:true)]
    private void HandleDeath()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks if there is a clear line of sight to the target within detection range.
    /// </summary>
    /// <returns>True if there is an unobstructed line of sight, false otherwise.</returns>
    protected bool CheckLOS(Transform target)
    {
        if (target == null) return false;

        Vector3 direction = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, _detectionRange))
        {
            if (hit.transform == target)
                return true;
        }

        return false;
    }


    /// <summary>
    /// Determines if enemy is currently attacking.
    /// </summary>
    /// <returns> True if is attacking, false otherwise. </returns>
    private bool IsAttacking()
    {
        return (machine.state == _aiAttack && !machine.state.isComplete);
    }

    private bool IsFleeing()
    {
        return (machine.state == _aiFlee && !machine.state.isComplete);
    }

    private bool IsKnockback()
    {
        return (machine.state == _aiIKnockback && !machine.state.isComplete);
    }

    /// <summary>
    /// Determines if enemy is currently in range and LOS of any players.
    /// </summary>
    /// <returns> True if is attacking, false otherwise. </returns>
    private bool PlayersInRange()
    {
        return GetNearestPlayer() != null;
    }

    protected Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float closestDist = float.MaxValue;
        if (_players == null || _players.Count == 0)
        {
            _players = new List<Transform>();
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Challenger");
            foreach (GameObject player in playerObjects)
            {
                _players.Add(player.transform);
            }
        }

        foreach (Transform player in _players)
        {
            if (player == null || player.CompareTag("Dead")) continue;

            Collider2D playerCol = player.GetComponent<Collider2D>();
            if (playerCol == null) continue; // Not valid challenger component

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > _detectionRange) continue;

            Vector2 origin = GetComponent<Collider2D>().bounds.center;
            Vector2 target = playerCol.bounds.center;
            Vector2 dir = (target - origin).normalized;

            //bitmask to ignore any objects ignoring raycasts
            int ignoreLayer = LayerMask.GetMask("Enemy", "Ignore Raycast");
            int mask = ~ignoreLayer;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, _detectionRange, mask);

            //DEBUG: Draw the LOS check and what target it hits
            //if(hit.collider != null) Debug.Log(hit.collider.transform);
            Color rayColor = hit.collider != null ? Color.green : Color.red;
            Debug.DrawRay(origin, dir * _detectionRange, rayColor);

            if (hit.collider == null || hit.collider.transform != player) continue;

            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Flips the sprite based on the relative x position of the current target.
    /// Required for enemies since they have no rigidbody velocity
    /// </summary>
    private void FlipSpriteTowardsTarget()
    {
        if (_currentTarget == null) return;

        Vector3 localScale = transform.localScale;

        if (_currentTarget.position.x > transform.position.x)
            localScale.x = Mathf.Abs(localScale.x);   // face right
        else
            localScale.x = -Mathf.Abs(localScale.x);  // face left

        transform.localScale = localScale;
    }

    private bool IsValidOwner()
    {
        if (owner == null && isServer) return true;
        return isOwner;
    }
}