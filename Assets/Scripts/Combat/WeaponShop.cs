using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class WeaponShop : MonoBehaviour, IInteractable
{
    [Serializable]
    public struct Offer
    {
        public WeaponData weapon;
        [Min(0)] public int price;
        [Min(1)] public int ammoPack;
        [Min(1)] public int ammoPrice;
    }

    [SerializeField] private GameManager game;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private ResourceData coins;
    [SerializeField] private Sprite sign;
    [SerializeField] private Offer[] offers = Array.Empty<Offer>();

    private PlayerCombat customer;
    private WeaponShopPanel panel;
    private float previousTimeScale;

    public string InteractionDescription => "Abrir loja de armas";
    public bool IsOpen => customer != null;
    public int Balance => resources.GetAmount(coins);
    public IReadOnlyList<Offer> Offers => offers;

    private void Start()
    {
        var facade = GetComponent<SpriteRenderer>();
        if (facade == null) return;
        WeaponShopPanel.CreateSign(facade);
        PlaceStock(facade, sign, new Vector2(1.02f, .49f), .60f);
        if (offers.Length > 2)
        {
            PlaceStock(facade, offers[0].weapon.Icon, new Vector2(.28f, .82f), .36f);
            PlaceStock(facade, offers[2].weapon.Icon, new Vector2(.77f, .88f), .66f);
        }
    }

    private static void PlaceStock(SpriteRenderer facade, Sprite sprite, Vector2 position, float width)
    {
        if (sprite == null) return;
        var display = new GameObject(sprite.name).AddComponent<SpriteRenderer>();
        display.transform.SetParent(facade.transform, false);
        display.transform.localPosition = position;
        display.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
        display.sprite = sprite;
        display.sortingLayerID = facade.sortingLayerID;
        display.sortingOrder = facade.sortingOrder + 1;
    }

    public bool CanInteract(GameObject interactor) =>
        !IsOpen && Time.timeScale > 0 && game != null && game.IsRunning &&
        resources != null && resources.IsRegistered(coins) &&
        game.Player != null && interactor == game.Player.gameObject &&
        interactor.TryGetComponent<PlayerCombat>(out _);

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (panel == null) panel = WeaponShopPanel.Create(transform, this, coins.Icon, offers);
        customer = interactor.GetComponent<PlayerCombat>();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0;
        customer.BlockFireUntilReleased();
        panel.Show();
    }

    private void Update()
    {
        if (IsOpen && (!game.IsRunning || Keyboard.current?.escapeKey.wasPressedThisFrame == true))
            Close();
    }

    private void OnDisable() => Close();
    private void OnDestroy()
    {
        if (panel != null) Destroy(panel.gameObject);
    }

    public void Close()
    {
        if (!IsOpen) return;
        customer.BlockFireUntilReleased();
        customer = null;
        if (panel != null) panel.Hide();
        if (game != null && game.IsRunning) Time.timeScale = previousTimeScale;
    }

    public WeaponAmmo OwnedAmmo(WeaponData weapon) => customer != null ? customer.GetOwnedAmmo(weapon) : null;
    public bool IsEquipped(WeaponData weapon) => customer != null && customer.Weapon == weapon;

    public bool BuyOrEquip(Offer offer)
    {
        if (!CanTrade(offer) || IsEquipped(offer.weapon)) return false;
        if (OwnedAmmo(offer.weapon) != null)
        {
            customer.Equip(offer.weapon);
            return true;
        }
        return Pay(offer.price, () =>
        {
            customer.Equip(offer.weapon);
            return true;
        });
    }

    public int AmmoQuantity(Offer offer)
    {
        var ammo = OwnedAmmo(offer.weapon);
        return ammo == null ? 0 : Mathf.Min(offer.ammoPack, ammo.MaxReserve - ammo.Reserve);
    }

    public int AmmoCost(Offer offer)
    {
        int rounds = AmmoQuantity(offer);
        return rounds > 0 && offer.ammoPack > 0
            ? (int)(((long)rounds * offer.ammoPrice + offer.ammoPack - 1) / offer.ammoPack)
            : 0;
    }

    public bool BuyAmmo(Offer offer)
    {
        if (!CanTrade(offer)) return false;
        int rounds = AmmoQuantity(offer);
        int cost = AmmoCost(offer);
        return rounds > 0 && cost > 0 && Pay(cost, () => customer.AddAmmo(offer.weapon, rounds) == rounds);
    }

    private bool CanTrade(Offer offer) =>
        IsOpen && game.IsRunning && offer.weapon != null && Array.Exists(offers, configured =>
            configured.weapon == offer.weapon && configured.price == offer.price &&
            configured.ammoPack == offer.ammoPack && configured.ammoPrice == offer.ammoPrice);

    private bool Pay(int cost, Func<bool> install) =>
        resources.TryPurchase(new[] { new ResourceCost(coins, cost) }, install);
}
