using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Entities/Projectile")]
public class ProjectileData : ScriptableObject
{
    [field: Header("Information")]
    [field: SerializeField]
    public string ProjectileType { get; private set; }
    [field: Header("Base Stats")]
    [field: SerializeField]
    public float FlightSpeed;
    [field: Header("Additional")]
    [field: SerializeField]
    public float Size;
    [field: SerializeField]
    public int ProjectileUses;
    [field: SerializeField]
    public bool Despawns;
    [field: SerializeField]
    public float DespawnTime;
    [field: Header("Explosive Projectiles")]
    [field: SerializeField]
    public bool IsExplosive;
    [field: SerializeField]
    public GameObject ExplosionPrefab;

    void Reset()
    {
        DespawnTime = 0;
        Despawns = false;
        ProjectileUses = 1;
    }
}