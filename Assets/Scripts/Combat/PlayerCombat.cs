using System.Collections;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(AudioSource))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform muzzle;
    [SerializeField] private SpriteRenderer flash;
    [SerializeField] private AudioClip shotSound;
    [SerializeField, Min(1)] private int damage = 20;
    [SerializeField, Min(0.05f)] private float shotInterval = 0.35f;
    [SerializeField] private GameManager game;
    private InputAction attack;
    private PlayerController player;
    private AudioSource source;
    private float nextShot;
    private void Awake()
    {
        player = GetComponent<PlayerController>(); source = GetComponent<AudioSource>();
        attack = GameInput.Gameplay.FindAction("Fire", true);
    }
    public void Initialize(GameManager session) => game = session;
    private void OnEnable() => attack?.Enable();
    private void Update()
    {
        if (game == null || !game.IsRunning || !attack.IsPressed() || Time.time < nextShot) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        nextShot = Time.time + shotInterval;
        var bullet = Instantiate(projectile, muzzle.position, Quaternion.identity, game.ProjectileRoot);
        bullet.Initialize(Vector2.right * player.FacingDirection, damage, game);
        if (shotSound != null) source.PlayOneShot(shotSound);
        if (flash != null) StartCoroutine(Flash());
    }
    private IEnumerator Flash() { flash.enabled = true; yield return new WaitForSeconds(0.06f); flash.enabled = false; }
    private void OnDisable()
    {
        attack?.Disable(); StopAllCoroutines();
        if (flash != null) flash.enabled = false;
        if (source != null) source.Stop();
    }
}
