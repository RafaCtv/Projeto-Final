using System;
using System.Collections;
using System.Collections.Generic;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController), typeof(AudioSource))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponData weapon;
    [SerializeField] private SpriteRenderer heldWeapon;
    [SerializeField] private Vector2 gripAnchor = new Vector2(-.60f, .27f);
    [SerializeField] private Transform muzzle;
    [SerializeField] private SpriteRenderer flash;
    [SerializeField] private GameManager game;
    private InputAction attack, reload;
    private PlayerController player;
    private AudioSource source;
    private float nextShot;
    private Coroutine reloading;
    private Coroutine firingFlash;
    private float reloadStartedAt;
    private bool waitForFireRelease;
    private readonly Dictionary<WeaponData, WeaponAmmo> inventory = new();

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
        if (data == weapon && Ammo != null) return;
        CancelReload();
        ClearFlash();
        weapon = data;
        ApplyHeldWeapon();
        Ammo = null;
        if (data != null)
        {
            if (!inventory.TryGetValue(data, out var ammo))
            {
                ammo = new WeaponAmmo(data.MagazineSize, data.StartingReserve, data.MaxReserve);
                inventory.Add(data, ammo);
            }
            Ammo = ammo;
        }
        AmmoChanged?.Invoke();
    }

    private void ApplyHeldWeapon()
    {
        if (heldWeapon == null) return;
        var sprite = weapon != null ? weapon.Icon : null;
        heldWeapon.sprite = sprite;
        heldWeapon.enabled = sprite != null;
        if (sprite == null) return;

        var mount = heldWeapon.transform;
        var rotation = Quaternion.Euler(0, 0, weapon.HeldAngle);
        mount.localRotation = rotation;
        mount.localScale = new Vector3(weapon.HeldScale, weapon.HeldScale, 1);
        mount.localPosition = (Vector3)gripAnchor - rotation * SpritePoint(sprite, weapon.Grip) * weapon.HeldScale;
        if (muzzle != null) muzzle.localPosition = SpritePoint(sprite, weapon.Barrel);
    }

    private static Vector2 SpritePoint(Sprite sprite, Vector2 position) =>
        (Vector2.Scale(sprite.rect.size, position) - sprite.pivot) / sprite.pixelsPerUnit;

    public WeaponAmmo GetOwnedAmmo(WeaponData data) =>
        data != null && inventory.TryGetValue(data, out var ammo) ? ammo : null;

    public void BlockFireUntilReleased() => waitForFireRelease = true;

    private void OnEnable() { attack?.Enable(); reload?.Enable(); }
    private void Update()
    {
        if (game == null || !game.IsRunning || Time.timeScale <= 0 || weapon == null) return;
        if (reload.WasPressedThisFrame()) StartReload();
        if (waitForFireRelease)
        {
            if (attack.IsPressed()) return;
            waitForFireRelease = false;
        }
        if (!attack.IsPressed() || Time.time < nextShot || IsReloading) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (!Ammo.TryConsume()) { StartReload(); return; }
        nextShot = Time.time + weapon.ShotInterval;
        var bullet = Instantiate(weapon.Projectile, muzzle.position, Quaternion.identity, game.ProjectileRoot);
        bullet.Initialize(Vector2.right * player.FacingDirection, weapon.Damage, game, weapon.Range);
        if (weapon.ShotSound != null) source.PlayOneShot(weapon.ShotSound);
        if (flash != null)
        {
            ClearFlash();
            firingFlash = StartCoroutine(Flash());
        }
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
    public int AddAmmo(int amount) => AddAmmo(weapon, amount);

    public int AddAmmo(WeaponData data, int amount)
    {
        var ammo = GetOwnedAmmo(data);
        if (ammo == null) return 0;
        if (ammo != Ammo) return ammo.AddReserve(amount);
        int added = Ammo.AddReserve(amount);
        if (added > 0)
        {
            AmmoChanged?.Invoke();
            if (Ammo.IsEmpty && Time.timeScale > 0) StartReload();
        }
        return added;
    }
    private IEnumerator Flash()
    {
        flash.enabled = true;
        yield return new WaitForSeconds(0.06f);
        flash.enabled = false;
        firingFlash = null;
    }

    private void ClearFlash()
    {
        if (firingFlash != null) StopCoroutine(firingFlash);
        firingFlash = null;
        if (flash != null) flash.enabled = false;
    }
    private void OnDisable()
    {
        attack?.Disable(); reload?.Disable(); StopAllCoroutines();
        reloading = null;
        ClearFlash();
        if (source != null) source.Stop();
    }
}
