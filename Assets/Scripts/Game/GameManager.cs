using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Preparation, Combat, WaveCompleted, GameOver }

[DefaultExecutionOrder(-20)]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private BaseCore baseCore;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private Transform projectileRoot;
    [SerializeField] private MonoBehaviour[] controlsToStop;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private ResourceData coins;
    [SerializeField] private InteractionPromptUI feedback;
    public event Action<int> CoinsAwarded;
    public bool AwardCoins(int amount)
    {
        if (!IsRunning || baseCore == null || !baseCore.Health.IsAlive || amount <= 0 || resources == null || !resources.TryAdd(coins, amount)) return false;
        CoinsAwarded?.Invoke(amount);
        if (feedback != null) feedback.ShowFeedback("+" + amount + " Moedas");
        return true;
    }
    public GameState State { get; private set; } = GameState.Preparation;
    public bool IsRunning => State != GameState.GameOver;
    public BaseCore Base => baseCore;
    public Transform EnemyRoot => enemyRoot;
    public Transform ProjectileRoot => projectileRoot;
    public event Action<GameState> StateChanged;
    private void Awake() => Time.timeScale = 1;
    private void OnEnable() { if (baseCore != null) baseCore.Destroyed += EndGame; }
    private void OnDisable() { if (baseCore != null) baseCore.Destroyed -= EndGame; }
    public bool TransitionTo(GameState state)
    {
        if (!IsRunning || state == State) return false;
        if (state == GameState.GameOver) { EndGame(); return true; }
        State = state; StateChanged?.Invoke(State); return true;
    }
    public void EndGame()
    {
        if (!IsRunning) return;
        State = GameState.GameOver;
        // Stop spawners/reward producers before removing entities.
        StateChanged?.Invoke(State);
        if (controlsToStop != null)
            foreach (var control in controlsToStop) if (control != null) control.enabled = false;
        if (cameraController != null) cameraController.InputEnabled = false;
        if (enemyRoot != null)
            foreach (var enemy in enemyRoot.GetComponentsInChildren<ZombieController>()) enemy.DespawnWithoutReward();
        if (projectileRoot != null)
            foreach (Transform projectile in projectileRoot) { projectile.gameObject.SetActive(false); Destroy(projectile.gameObject); }
        Time.timeScale = 0;
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }
}
