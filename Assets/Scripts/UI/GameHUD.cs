using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameHUD : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private WaveManager waves;
    [SerializeField] private TMP_Text baseHealth;
    [SerializeField] private TMP_Text playerHealth;
    [SerializeField] private TMP_Text status;
    [SerializeField] private TMP_Text waveNumber;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private TMP_Text pending;
    [SerializeField] private TMP_Text message;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TMP_Text defeatSubtitle;
    [SerializeField] private Button restart;
    private float messageUntil;
    private void OnEnable()
    {
        game.StateChanged += OnStateChanged; game.Base.Health.Changed += OnHealth;
        if (game.Player != null) { game.Player.Changed += OnPlayerHealth; OnPlayerHealth(game.Player); }
        restart.onClick.AddListener(game.Restart);
        OnHealth(game.Base.Health); OnStateChanged(game.State);
    }
    private void OnHealth(Health health) => baseHealth.text = health.Current + " / " + health.Maximum;
    private void OnPlayerHealth(Health health) { if (playerHealth != null) playerHealth.text = health.Current + " / " + health.Maximum; }
    private void OnStateChanged(GameState state)
    {
        defeatPanel.SetActive(state == GameState.GameOver);
        if (state == GameState.GameOver)
        {
            message.text = "";
            if (defeatSubtitle != null && game.PlayerDefeated) defeatSubtitle.text = "O sobrevivente foi derrubado.";
            return;
        }
        message.text = state == GameState.Preparation ? "Prepare suas defesas" : state == GameState.Combat ? "Onda " + waves.WaveNumber + " iniciada!" : "Onda concluída! +" + waves.CompletionReward + " Moedas";
        messageUntil = Time.time + 3;
    }
    private void Update()
    {
        Set(waveNumber, "ONDA " + waves.WaveNumber.ToString("00"));
        Set(status, game.State == GameState.Preparation ? "PREPARAÇÃO" : game.State == GameState.Combat ? "COMBATE" : game.State == GameState.WaveCompleted ? "CONCLUÍDA" : "DERROTA");
        if (game.State == GameState.Preparation)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(waves.PreparationRemaining));
            Set(timer, (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00"));
            timer.color = seconds <= 5 ? HealthBarUI.ColorFor(0) : seconds <= 15 ? HealthBarUI.ColorFor(.5f) : Color.white;
            Set(pending, "");
        }
        else if (game.State == GameState.Combat)
        {
            Set(timer, waves.AliveCount + (waves.AliveCount == 1 ? " VIVO" : " VIVOS")); timer.color = Color.white;
            int incoming = Mathf.Max(0, waves.PlannedEnemies - waves.Spawned);
            Set(pending, incoming > 0 ? "+" + incoming + " chegando" : "");
        }
        else { Set(timer, game.State == GameState.WaveCompleted ? "OK" : "-"); Set(pending, ""); }
        if (message.text.Length > 0 && Time.time >= messageUntil) message.text = "";
    }
    private static void Set(TMP_Text label, string value) { if (label != null && label.text != value) label.text = value; }
    private void OnDisable()
    {
        if (game != null)
        {
            game.StateChanged -= OnStateChanged;
            if (game.Base != null) game.Base.Health.Changed -= OnHealth;
            if (game.Player != null) game.Player.Changed -= OnPlayerHealth;
            restart.onClick.RemoveListener(game.Restart);
        }
    }
}
