using UnityEngine;

// Um asset por tipo de zumbi. Novos tipos = novo asset (e prefab, se mudar o visual).
[CreateAssetMenu(menuName = "O Ultimo Bastiao/Zombie", fileName = "BasicZombie")]
public sealed class ZombieData : ScriptableObject
{
    [Header("Atributos")]
    [SerializeField, Min(1)] private int maxHealth = 60;
    [SerializeField, Min(0)] private float speed = 1.4f;
    [SerializeField, Min(0.05f)] private float attackInterval = 1;
    [SerializeField, Min(0)] private int coinReward = 2;

    [Header("Dano")]
    [Tooltip("Dano base por ataque. Os multiplicadores abaixo ajustam por tipo de alvo.")]
    [SerializeField, Min(1)] private int damage = 8;
    [SerializeField, Min(0)] private float playerDamageMultiplier = 1;
    [SerializeField, Min(0)] private float structureDamageMultiplier = 1;

    [Header("Comportamento")]
    [Tooltip("Distância horizontal em que o zumbi passa a perseguir o jogador.")]
    [SerializeField, Min(0)] private float playerDetectionRange = 7;
    [Tooltip("Distância horizontal em que o zumbi desiste do jogador e volta para a base.")]
    [SerializeField, Min(0)] private float playerLoseRange = 10;
    [Tooltip("Diferença de altura máxima para perceber o jogador.")]
    [SerializeField, Min(0)] private float verticalDetectionRange = 3;

    [Header("Drop de munição")]
    [SerializeField, Range(0, 1)] private float ammoDropChance = 0.35f;
    [SerializeField] private Vector2Int ammoDropAmount = new Vector2Int(6, 12);
    [SerializeField] private AmmoPickup ammoPickup;

    public int MaxHealth => maxHealth;
    public float Speed => speed;
    public int Damage => damage;
    public float AttackInterval => attackInterval;
    public int CoinReward => coinReward;
    public float PlayerDetectionRange => playerDetectionRange;
    public float PlayerLoseRange => Mathf.Max(playerDetectionRange, playerLoseRange);
    public float VerticalDetectionRange => verticalDetectionRange;

    public AmmoPickup AmmoPickup => ammoPickup;

    // Sorteia o drop a partir de dois valores em [0, 1): retorna 0 (nada) ou a quantidade de balas.
    public int RollAmmoDrop(float chanceRoll, float amountRoll)
    {
        if (ammoPickup == null || chanceRoll >= ammoDropChance) return 0;
        int min = Mathf.Max(1, Mathf.Min(ammoDropAmount.x, ammoDropAmount.y));
        int max = Mathf.Max(min, Mathf.Max(ammoDropAmount.x, ammoDropAmount.y));
        return Mathf.Min(max, min + Mathf.FloorToInt(Mathf.Clamp01(amountRoll) * (max - min + 1)));
    }

    public float DamageMultiplier(ZombieTargetKind kind) =>
        kind == ZombieTargetKind.Player ? playerDamageMultiplier : structureDamageMultiplier;
}
