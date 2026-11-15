using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Canto inferior direito: ícone da arma, balas no pente / reserva e progresso da recarga.
public sealed class AmmoHUD : MonoBehaviour
{
    private static readonly Color Normal = Color.white;
    private static readonly Color Low = new Color(1, .77f, .27f);
    private static readonly Color Empty = new Color(1, .32f, .29f);

    [SerializeField] private PlayerCombat combat;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text magazine;
    [SerializeField] private TMP_Text reserve;
    [SerializeField] private TMP_Text status;
    [SerializeField] private Image reloadFill;

    private void OnEnable()
    {
        if (combat == null) return;
        combat.AmmoChanged += Refresh;
        Refresh();
    }

    private void OnDisable() { if (combat != null) combat.AmmoChanged -= Refresh; }

    private void Refresh()
    {
        var ammo = combat.Ammo;
        if (ammo == null) return;
        if (weaponIcon != null && combat.Weapon != null && weaponIcon.sprite != combat.Weapon.Icon)
        {
            weaponIcon.sprite = combat.Weapon.Icon;
            weaponIcon.preserveAspect = true;
        }
        magazine.text = ammo.Magazine.ToString();
        magazine.color = ammo.Magazine == 0 ? Empty : ammo.Magazine <= ammo.MagazineSize / 4 ? Low : Normal;
        reserve.text = "/ " + ammo.Reserve;
        reserve.color = ammo.Reserve == 0 ? Empty : new Color(1, 1, 1, .7f);
    }

    private void Update()
    {
        if (combat == null || combat.Ammo == null) return;
        string text = combat.IsReloading ? "RECARREGANDO" : combat.Ammo.IsOutOfAmmo ? "SEM MUNIÇÃO" : combat.Ammo.IsEmpty ? "R PARA RECARREGAR" : "";
        if (status != null && status.text != text) status.text = text;
        if (reloadFill != null)
        {
            reloadFill.enabled = combat.IsReloading;
            reloadFill.rectTransform.localScale = new Vector3(combat.ReloadProgress, 1, 1);
        }
    }
}
