using UnityEngine;

// Gasto e recuperação de estamina para a corrida (testável em EditMode).
// Ao zerar, o jogador fica "exausto" até recuperar o mínimo, evitando correr aos trancos.
public sealed class Stamina
{
    private readonly float maximum, drainPerSecond, regenPerSecond, regenDelay, minimumToSprint;
    private float regenBlockedFor;

    public float Current { get; private set; }
    public float Normalized => maximum > 0 ? Current / maximum : 0;
    public bool IsExhausted { get; private set; }
    public bool IsSprinting { get; private set; }

    public Stamina(float maximum, float drainPerSecond, float regenPerSecond, float regenDelay, float minimumToSprint)
    {
        this.maximum = Mathf.Max(1, maximum);
        this.drainPerSecond = Mathf.Max(0, drainPerSecond);
        this.regenPerSecond = Mathf.Max(0, regenPerSecond);
        this.regenDelay = Mathf.Max(0, regenDelay);
        this.minimumToSprint = Mathf.Clamp(minimumToSprint, 0, this.maximum);
        Current = this.maximum;
    }

    // Avança o tempo; retorna se o jogador está correndo neste passo.
    public bool Tick(float deltaTime, bool wantsToSprint, bool isMoving)
    {
        if (IsExhausted && Current >= minimumToSprint) IsExhausted = false;
        IsSprinting = wantsToSprint && isMoving && !IsExhausted && Current > 0;
        if (IsSprinting)
        {
            Current = Mathf.Max(0, Current - drainPerSecond * deltaTime);
            regenBlockedFor = regenDelay;
            if (Current == 0) { IsExhausted = true; IsSprinting = false; }
        }
        else if (regenBlockedFor > 0) regenBlockedFor = Mathf.Max(0, regenBlockedFor - deltaTime);
        else Current = Mathf.Min(maximum, Current + regenPerSecond * deltaTime);
        return IsSprinting;
    }
}
