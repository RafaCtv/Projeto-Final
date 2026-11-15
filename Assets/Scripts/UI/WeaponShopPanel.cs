using System;
using ProjetoFinal.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class WeaponShopPanel : MonoBehaviour
{
    private static readonly Color Panel = new Color32(25, 18, 37, 255);
    private static readonly Color Card = new Color32(44, 34, 57, 255);
    private static readonly Color IconPlate = new Color32(144, 161, 156, 255);
    private static readonly Color Teal = new Color32(61, 205, 179, 255);
    private static readonly Color Muted = new Color32(205, 191, 216, 255);
    private static readonly Color Ink = new Color32(26, 16, 37, 255);

    private WeaponShop shop;
    private TMP_Text balance;
    private Button close;
    private Row[] rows;

    private sealed class Row
    {
        public WeaponShop.Offer offer;
        public Button weaponButton, ammoButton;
        public TMP_Text weaponLabel, ammoLabel, ammoStatus, ownership;
    }

    public static void CreateSign(SpriteRenderer storefront)
    {
        var board = new GameObject("Placa da loja de armas", typeof(RectTransform), typeof(Canvas));
        var rect = (RectTransform)board.transform;
        rect.SetParent(storefront.transform, false);
        rect.localPosition = new Vector3(.08f, 2.56f, 0);
        rect.localScale = Vector3.one * .009f;
        rect.sizeDelta = new Vector2(318, 68);
        var canvas = board.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingLayerID = storefront.sortingLayerID;
        canvas.sortingOrder = storefront.sortingOrder + 3;
        Box(rect, "Moldura", 0, 0, 1, 1, Ink);
        Box(rect, "Placa", .025f, .11f, .975f, .89f, Teal);
        Label(rect, "LOJA DE ARMAS", .07f, .13f, .93f, .87f, 40, TextAlignmentOptions.Center).color = Ink;
    }

    public static WeaponShopPanel Create(Transform parent, WeaponShop shop, Sprite coin,
        WeaponShop.Offer[] offers)
    {
        var root = new GameObject("Interface do arsenal", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(WeaponShopPanel));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = .5f;
        // An overlay canvas must be a scene root.
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, parent.gameObject.scene);
        var panel = root.GetComponent<WeaponShopPanel>();
        panel.shop = shop;
        panel.Build((RectTransform)root.transform, coin, offers);
        root.SetActive(false);
        return panel;
    }

    private void Build(RectTransform root, Sprite coin, WeaponShop.Offer[] offers)
    {
        Box(root, "Fundo", 0, 0, 1, 1, new Color(0.04f, 0.02f, 0.08f, .73f)).raycastTarget = true;
        var frame = Box(root, "Moldura", .084f, .065f, .916f, .935f, new Color32(79, 61, 91, 255)).rectTransform;
        var body = Box(frame, "Arsenal", .004f, .008f, .996f, .992f, Panel).rectTransform;
        Box(body, "Faixa", 0, .984f, 1, 1, Teal);
        Box(body, "Cabeçalho", 0, .823f, 1, .984f, Card);
        Label(body, "LOJA DE ARMAS", .032f, .892f, .58f, .963f, 50, TextAlignmentOptions.MidlineLeft).color = Teal;
        Label(body, "Armas e munição", .034f, .848f, .53f, .898f, 26,
            TextAlignmentOptions.MidlineLeft).color = Muted;
        Box(body, "Saldo", .667f, .861f, .812f, .948f, Ink);
        Art(body, "Moeda", coin, .681f, .881f, .711f, .929f);
        balance = Label(body, "", .719f, .869f, .803f, .939f, 28, TextAlignmentOptions.MidlineLeft);
        balance.color = Accent;
        close = ActionButton(body, "Fechar [Esc]", .833f, .861f, .963f, .948f, shop.Close, Muted, out _);
        rows = new Row[offers.Length];
        const int columns = 2;
        float height = .696f / Mathf.Max(1, (offers.Length + columns - 1) / columns);
        for (int i = 0; i < offers.Length; i++)
        {
            var offer = offers[i];
            float left = .029f + (i % columns) * .481f;
            float top = .795f - height * (i / columns);
            var card = Box(body, offer.weapon.DisplayName, left, top - height + .026f, left + .461f, top, Card).rectTransform;
            Box(card, "Destaque", 0, .983f, 1, 1, i == 0 ? Teal : new Color32(102, 81, 119, 255));
            Box(card, "Vitrine", .036f, .402f, .348f, .906f, IconPlate);
            Art(card, offer.weapon.DisplayName, offer.weapon.Icon, .058f, .464f, .326f, .844f);
            var ownership = Label(card, "", .385f, .776f, .959f, .934f, 21, TextAlignmentOptions.MidlineLeft);
            Label(card, offer.weapon.DisplayName, .383f, .576f, .964f, .803f, 34, TextAlignmentOptions.MidlineLeft);
            Label(card, $"{offer.weapon.Damage} dano  ·  {1 / offer.weapon.ShotInterval:0.#} tiros/s  ·  {offer.weapon.Range:0} alcance",
                .385f, .41f, .965f, .57f, 22, TextAlignmentOptions.MidlineLeft).color = Muted;
            var ammoStatus = Label(card, "", .039f, .257f, .961f, .389f, 22, TextAlignmentOptions.MidlineLeft);
            var weaponButton = ActionButton(card, "", .037f, .056f, .457f, .247f, () =>
            {
                shop.BuyOrEquip(offer);
                Refresh();
            }, Teal, out var weaponLabel);
            var ammoButton = ActionButton(card, "", .473f, .056f, .963f, .247f, () =>
            {
                shop.BuyAmmo(offer);
                Refresh();
            }, Accent, out var ammoLabel);
            rows[i] = new Row { offer = offer, weaponButton = weaponButton, weaponLabel = weaponLabel,
                ammoButton = ammoButton, ammoLabel = ammoLabel, ammoStatus = ammoStatus, ownership = ownership };
        }
        Label(body, "Jogo pausado", .031f, .021f, .4f, .095f, 20, TextAlignmentOptions.MidlineLeft).color = Muted;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Refresh();
        EventSystem.current?.SetSelectedGameObject(close.gameObject);
    }

    public void Hide()
    {
        var events = EventSystem.current;
        if (events != null && events.currentSelectedGameObject != null &&
            events.currentSelectedGameObject.transform.IsChildOf(transform))
            events.SetSelectedGameObject(null);
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        int coins = shop.Balance;
        balance.text = coins + " moedas";
        foreach (var row in rows)
        {
            var weapon = row.offer.weapon;
            var ammo = shop.OwnedAmmo(weapon);
            bool equipped = shop.IsEquipped(weapon);
            row.ownership.text = equipped ? "EM USO" : ammo != null ? "NO INVENTÁRIO" :
                coins >= row.offer.price ? "À VENDA" : "SALDO INSUFICIENTE";
            row.ownership.color = ammo != null ? Teal : Accent;
            row.weaponLabel.text = equipped ? "Equipada" : ammo != null ? "Equipar" :
                row.offer.price == 0 ? "Pegar grátis" : $"Comprar arma\n{row.offer.price} moedas";
            row.weaponButton.interactable = !equipped && (ammo != null || coins >= row.offer.price);
            row.ammoStatus.text = ammo == null ? weapon.AmmoName :
                $"{weapon.AmmoName}  ·  Pente {ammo.Magazine}/{ammo.MagazineSize}  ·  Reserva {ammo.Reserve}/{ammo.MaxReserve}";
            int quantity = shop.AmmoQuantity(row.offer);
            int cost = shop.AmmoCost(row.offer);
            row.ammoLabel.text = ammo == null ? "Munição bloqueada\nAdquira a arma" :
                quantity == 0 ? "Reserva cheia" : $"Comprar {quantity} munições\n{cost} moedas";
            row.ammoButton.interactable = quantity > 0 && cost > 0 && coins >= cost;
            row.weaponLabel.color = row.weaponButton.interactable ? Ink : Muted;
            row.ammoLabel.color = row.ammoButton.interactable ? Ink : Muted;
        }
    }

    private static Color Accent => UITheme.Default != null
        ? UITheme.Default.Get(ThemeColor.Highlight) : new Color32(255, 201, 60, 255);

    private static RectTransform Region(Transform parent, string name, float left, float bottom, float right, float top)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(left, bottom);
        rect.anchorMax = new Vector2(right, top);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static Image Box(Transform parent, string name, float left, float bottom, float right, float top, Color color)
    {
        var image = Region(parent, name, left, bottom, right, top).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void Art(Transform parent, string name, Sprite sprite, float left, float bottom, float right, float top)
    {
        var image = Box(parent, name, left, bottom, right, top, Color.white);
        image.sprite = sprite;
        image.enabled = sprite != null;
        image.preserveAspect = true;
    }

    private static TMP_Text Label(Transform parent, string value, float left, float bottom, float right, float top,
        float size, TextAlignmentOptions alignment)
    {
        var text = Region(parent, "Texto", left, bottom, right, top).gameObject.AddComponent<TextMeshProUGUI>();
        var theme = UITheme.Default;
        if (theme != null && theme.Font != null) text.font = theme.Font;
        text.text = value;
        text.color = new Color32(255, 246, 224, 255);
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8;
        text.fontSizeMax = size;
        text.raycastTarget = false;
        return text;
    }

    private static Button ActionButton(Transform parent, string value, float left, float bottom, float right, float top,
        Action action, Color fill, out TMP_Text label)
    {
        var image = Box(parent, "Botão do arsenal", left, bottom, right, top, Color.white);
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = fill;
        colors.highlightedColor = colors.selectedColor = Color.Lerp(fill, Color.white, .38f);
        colors.pressedColor = Color.Lerp(fill, Ink, .22f);
        colors.disabledColor = new Color32(66, 54, 79, 255);
        button.colors = colors;
        label = Label(image.transform, value, .06f, .04f, .94f, .96f, 22, TextAlignmentOptions.Center);
        label.color = Ink;
        button.onClick.AddListener(() => action());
        return button;
    }
}
