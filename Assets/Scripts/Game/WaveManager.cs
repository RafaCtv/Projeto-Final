using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaveManager : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private ZombieController[] zombiePrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField, Min(0)] private float preparationDuration = 60;
    [SerializeField, Min(0.05f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(0)] private float completionDuration = 3;
    [SerializeField, Min(1)] private int initialEnemies = 3;
    [SerializeField, Min(0)] private int additionalEnemies = 2;
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
                PreparationRemaining = Mathf.Max(0, PreparationRemaining - Time.deltaTime);
            }
            if (!game.IsRunning) yield break;
            game.TransitionTo(GameState.Combat);
            int count = PlannedEnemies;
            for (int i = 0; i < count && game.IsRunning; i++)
            {
                var prefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
                var enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity, game.EnemyRoot);
                enemy.Initialize(game); enemy.Defeated += OnDefeated; alive.Add(enemy); Spawned++;
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
    private void OnDefeated(ZombieController zombie) { zombie.Defeated -= OnDefeated; alive.Remove(zombie); }
    private void OnStateChanged(GameState state)
    {
        if (state != GameState.GameOver) return;
        StopAllCoroutines(); Clear();
    }
    private void Clear() { foreach (var enemy in alive) if (enemy != null) enemy.Defeated -= OnDefeated; alive.Clear(); }
    private void OnDisable() { if (game != null) game.StateChanged -= OnStateChanged; StopAllCoroutines(); Clear(); }
}
