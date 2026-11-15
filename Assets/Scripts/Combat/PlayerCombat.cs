using System;
using System.Collections;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(AudioSource))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponData weapon;
    [SerializeField] private Transform muzzle;
    [SerializeField] private SpriteRenderer flash;
    [SerializeField] private GameManager game;
    private InputAction attack, reload;
    private PlayerController player;
    private AudioSource source;
    private float nextShot;
    private Coroutine reloading;
    private float reloadStartedAt;

    public WeaponData Weapon => weapon;
    public WeaponAmmo Ammo { get; private set; }
    public bool IsReloading => reloading != null;
    public float ReloadProgress => IsReloading && weapon.ReloadDuration > 0 ? Mathf.Clamp01((Time.time - reloadStartedAt) / weapon.ReloadDuration) : 0;
    public event Action AmmoChanged;

    private void Awake()
    {
        player = GetComponent<PlayerController>(); source = GetComponent<AudioSource>();
        attack = GameInput.Gameplay.FindAction("Fire", true);
        reload = GameInput.Gameplay.FindAction("Reload", true);
        Equip(weapon);
    }
    public void Initialize(GameManager session) => game = session;
    public void Equip(WeaponData data)
    {
        weapon = data;
        Ammo = data != null ? new WeaponAmmo(data.MagazineSize, data.StartingReserve, data.MaxReserve) : null;
        CancelReload();
        AmmoChanged?.Invoke();
    }
    private void OnEnable() { attack?.Enable(); reload?.Enable(); }
    private void Update()
    {
        if (game == null || !game.IsRunning || weapon == null) return;
        if (reload.WasPressedThisFrame()) StartReload();
        if (!attack.IsPressed() || Time.time < nextShot || IsReloading) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!Ammo.TryConsume()) { StartReload(); return; }
        nextShot = Time.time + weapon.ShotInterval;
        var bullet = Instantiate(weapon.Projectile, muzzle.position, Quaternion.identity, game.ProjectileRoot);
        bullet.Initialize(Vector2.right * player.FacingDirection, weapon.Damage, game);
        if (weapon.ShotSound != null) source.PlayOneShot(weapon.ShotSound);
        if (flash != null) StartCoroutine(Flash());
        AmmoChanged?.Invoke();
        // Recarrega sozinho ao esvaziar o pente.
        if (Ammo.IsEmpty) StartReload();
    }
    public bool StartReload()
    {
        if (IsReloading || Ammo == null || !Ammo.CanReload) return false;
        reloading = StartCoroutine(Reload());
        return true;
    }
    private IEnumerator Reload()
    {
        reloadStartedAt = Time.time;
        if (weapon.ReloadSound != null) source.PlayOneShot(weapon.ReloadSound);
        AmmoChanged?.Invoke();
        yield return new WaitForSeconds(weapon.ReloadDuration);
        Ammo.Reload();
        reloading = null;
        AmmoChanged?.Invoke();
    }
    private void CancelReload()
    {
        if (reloading != null) StopCoroutine(reloading);
        reloading = null;
    }
    // Retorna quantas balas foram aceitas (0 se a reserva estiver cheia).
    public int AddAmmo(int amount)
    {
        if (Ammo == null) return 0;
        int added = Ammo.AddReserve(amount);
        if (added > 0)
        {
            AmmoChanged?.Invoke();
            if (Ammo.IsEmpty) StartReload();
        }
        return added;
    }
    private IEnumerator Flash() { flash.enabled = true; yield return new WaitForSeconds(0.06f); flash.enabled = false; }
    private void OnDisable()
    {
        attack?.Disable(); reload?.Disable(); StopAllCoroutines();
        reloading = null;
        if (flash != null) flash.enabled = false;
        if (source != null) source.Stop();
    }
}
