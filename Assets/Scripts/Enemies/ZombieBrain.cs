using UnityEngine;

public enum ZombieGoal { Base, Player }

// Decisão de movimento do zumbi, sem dependência de física ou cena (testável em EditMode).
// Persegue o jogador quando ele está perto; caso contrário, segue para a base.
// A histerese (raio para perder o jogador maior que o de detecção) evita indecisão na borda do raio.
public sealed class ZombieBrain
{
    private const float ArrivalTolerance = 0.15f;
    private readonly float detectionRange;
    private readonly float loseRange;
    private readonly float verticalRange;

    public ZombieGoal Goal { get; private set; } = ZombieGoal.Base;
    public float Facing { get; private set; } = -1;

    public ZombieBrain(float detectionRange, float loseRange, float verticalRange)
    {
        this.detectionRange = Mathf.Max(0, detectionRange);
        this.loseRange = Mathf.Max(this.detectionRange, loseRange);
        this.verticalRange = Mathf.Max(0, verticalRange);
    }

    public ZombieBrain(ZombieData data)
        : this(data != null ? data.PlayerDetectionRange : 0, data != null ? data.PlayerLoseRange : 0, data != null ? data.VerticalDetectionRange : 0) { }

    // Retorna a direção horizontal desejada: -1 (esquerda), 0 (parado) ou 1 (direita).
    public float Decide(Vector2 position, bool playerAvailable, Vector2 playerPosition, float baseMinX, float baseMaxX)
    {
        Goal = ShouldChase(position, playerAvailable, playerPosition) ? ZombieGoal.Player : ZombieGoal.Base;
        float direction = Goal == ZombieGoal.Player
            ? DirectionTo(position.x, playerPosition.x, playerPosition.x)
            : DirectionTo(position.x, baseMinX, baseMaxX);
        if (direction != 0) Facing = direction;
        return direction;
    }

    public void FaceTowards(float fromX, float toX)
    {
        if (!Mathf.Approximately(fromX, toX)) Facing = Mathf.Sign(toX - fromX);
    }

    private bool ShouldChase(Vector2 position, bool playerAvailable, Vector2 playerPosition)
    {
        if (!playerAvailable) return false;
        float range = Goal == ZombieGoal.Player ? loseRange : detectionRange;
        return Mathf.Abs(playerPosition.x - position.x) <= range && Mathf.Abs(playerPosition.y - position.y) <= verticalRange;
    }

    // Anda até alcançar o intervalo [minX, maxX]; dentro dele, para.
    private static float DirectionTo(float x, float minX, float maxX)
    {
        if (x < minX - ArrivalTolerance) return 1;
        if (x > maxX + ArrivalTolerance) return -1;
        return 0;
    }
}
