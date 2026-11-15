using UnityEngine;

// Contagem de balas de uma arma: pente + reserva. Sem tempo nem input (testável em EditMode).
public sealed class WeaponAmmo
{
    public int MagazineSize { get; }
    public int MaxReserve { get; }
    public int Magazine { get; private set; }
    public int Reserve { get; private set; }

    public WeaponAmmo(int magazineSize, int startingReserve, int maxReserve)
    {
        MagazineSize = Mathf.Max(1, magazineSize);
        MaxReserve = Mathf.Max(0, maxReserve);
        Magazine = MagazineSize;
        Reserve = Mathf.Clamp(startingReserve, 0, MaxReserve);
    }

    public bool IsEmpty => Magazine == 0;
    public bool IsOutOfAmmo => Magazine == 0 && Reserve == 0;
    public bool CanReload => Magazine < MagazineSize && Reserve > 0;
    public bool ReserveIsFull => Reserve >= MaxReserve;

    public bool TryConsume()
    {
        if (Magazine == 0) return false;
        Magazine--;
        return true;
    }

    // Move balas da reserva para o pente; retorna quantas foram carregadas.
    public int Reload()
    {
        int loaded = Mathf.Min(MagazineSize - Magazine, Reserve);
        Magazine += loaded;
        Reserve -= loaded;
        return loaded;
    }

    // Retorna quantas balas couberam na reserva.
    public int AddReserve(int amount)
    {
        int added = Mathf.Clamp(amount, 0, MaxReserve - Reserve);
        Reserve += added;
        return added;
    }
}
