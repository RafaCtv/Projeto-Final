using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaveManager : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private ZombieController[] zombiePrefabs;
    [Tooltip("Pontos de surgimento; os zumbis se alternam entre eles (ex.: esquerda e direita da base).")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField, Min(0)] private float preparationDuration = 60;
    [SerializeField, Min(0.05f)] private float spawnInterval = 1.2f;
    [SerializeField, Min(0)] private float completionDuration = 3;
    [SerializeField, Min(1)] private int initialEnemies = 3;
    [SerializeField, Min(0)] private int additionalEnemies = 3;
    [SerializeField, Min(0)] private int completionReward = 10;
    private readonly HashSet<ZombieController> alive = new HashSet<ZombieController>();
    public int WaveNumber { get; private set; } = 1;
    public int PlannedEnemies => initialEnemies + (WaveNumber - 1) * additionalEnemies;
    public int Spawned { get; private set; }
    public int AliveCount => alive.Count;
    public float PreparationRemaining { get; private set; }
    public int CompletionReward => completionReward;
    private void OnEnable() { game.StateChanged += OnStateChanged; }
    private void Start() => StartCoroutine(Run());
    private IEnumerator Run()
    {
        while (game.IsRunning)
        {
            PreparationRemaining = preparationDuration; Spawned = 0;
            game.TransitionTo(GameState.Preparation);
            while (PreparationRemaining > 0 && game.IsRunning)
            {
                yield return null;
                if (Time.timeScale > 0)
                    PreparationRemaining = Mathf.Max(0, PreparationRemaining - Time.deltaTime);
            }
            if (!game.IsRunning) yield break;
            game.TransitionTo(GameState.Combat);
            int count = PlannedEnemies;
            int firstSpawn = Random.Range(0, spawnPoints != null ? Mathf.Max(1, spawnPoints.Length) : 1);
            for (int i = 0; i < count && game.IsRunning; i++)
            {
                var prefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
                var enemy = Instantiate(prefab, SpawnPosition(firstSpawn + i), Quaternion.identity, game.EnemyRoot);
                enemy.Initialize(game, WaveNumber); enemy.Defeated += OnDefeated; alive.Add(enemy); Spawned++;
                if (i + 1 < count) yield return new WaitForSeconds(spawnInterval);
            }
            while (game.IsRunning && alive.Count > 0) yield return null;
            if (!game.IsRunning || !game.Base.Health.IsAlive) yield break;
            game.TransitionTo(GameState.WaveCompleted);
            game.AwardCoins(completionReward);
            yield return new WaitForSeconds(completionDuration);
            if (!game.IsRunning) yield break;
            WaveNumber++;
        }
    }
    // Alterna os pontos a cada zumbi, começando por um ponto sorteado a cada onda.
    private Vector3 SpawnPosition(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return transform.position;
        var point = spawnPoints[index % spawnPoints.Length];
        return point != null ? point.position : transform.position;
    }
    private void OnDefeated(ZombieController zombie) { zombie.Defeated -= OnDefeated; alive.Remove(zombie); }
    private void OnStateChanged(GameState state)
    {
        if (state != GameState.GameOver) return;
        StopAllCoroutines(); Clear();
    }
    private void Clear() { foreach (var enemy in alive) if (enemy != null) enemy.Defeated -= OnDefeated; alive.Clear(); }
    private void OnDisable() { if (game != null) game.StateChanged -= OnStateChanged; StopAllCoroutines(); Clear(); }
}
