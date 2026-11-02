using UnityEngine;

[CreateAssetMenu(menuName = "O Ultimo Bastiao/Zombie", fileName = "BasicZombie")]
public sealed class ZombieData : ScriptableObject
{
    [SerializeField, Min(1)] private int maxHealth = 60;
    [SerializeField, Min(0)] private float speed = 1.4f;
    [SerializeField, Min(1)] private int damage = 8;
    [SerializeField, Min(0.05f)] private float attackInterval = 1;
    [SerializeField, Min(0)] private int coinReward = 2;
    public int MaxHealth => maxHealth;
    public float Speed => speed;
    public int Damage => damage;
    public float AttackInterval => attackInterval;
    public int CoinReward => coinReward;
}
