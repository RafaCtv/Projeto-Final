using UnityEngine;

// Um asset por arma. Novas armas = novo asset; o PlayerCombat só lê estes valores.
[CreateAssetMenu(menuName = "O Ultimo Bastiao/Weapon", fileName = "Weapon")]
public sealed class WeaponData : ScriptableObject
{
    [SerializeField] private string displayName = "Rifle";
    [SerializeField] private Sprite icon;
    [SerializeField] private Projectile projectile;
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip reloadSound;

    [Header("Visual na mão")]
    [SerializeField, Min(0.1f)] private float heldScale = 1;
    [SerializeField] private float heldAngle;
    [Tooltip("Posições na sprite entre 0 e 1, a partir do canto inferior esquerdo.")]
    [SerializeField] private Vector2 grip = new Vector2(.75f, .3f);
    [SerializeField] private Vector2 barrel = new Vector2(0, .55f);

    [Header("Disparo")]
    [SerializeField, Min(1)] private int damage = 20;
    [Tooltip("Segundos entre tiros (menor = cadência maior).")]
    [SerializeField, Min(0.02f)] private float shotInterval = 0.18f;
    [SerializeField, Min(1)] private float range = 24;

    [Header("Munição")]
    [SerializeField] private string ammoName = "5,56 mm";
    [SerializeField, Min(1)] private int magazineSize = 12;
    [SerializeField, Min(0)] private int startingReserve = 36;
    [SerializeField, Min(0)] private int maxReserve = 120;
    [SerializeField, Min(0)] private float reloadDuration = 1.4f;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public Projectile Projectile => projectile;
    public AudioClip ShotSound => shotSound;
    public AudioClip ReloadSound => reloadSound;
    public float HeldScale => heldScale;
    public float HeldAngle => heldAngle;
    public Vector2 Grip => grip;
    public Vector2 Barrel => barrel;
    public int Damage => damage;
    public float ShotInterval => shotInterval;
    public float Range => range;
    public string AmmoName => ammoName;
    public int MagazineSize => magazineSize;
    public int StartingReserve => Mathf.Min(startingReserve, maxReserve);
    public int MaxReserve => maxReserve;
    public float ReloadDuration => reloadDuration;
}
