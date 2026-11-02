using ProjetoFinal.Audio;
using UnityEngine;

public sealed class GameMusic : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private MusicPlayer player;
    [SerializeField] private AudioClip preparation;
    [SerializeField] private AudioClip combat;

    private void OnEnable() { game.StateChanged += OnStateChanged; OnStateChanged(game.State); }
    private void OnDisable() { if (game != null) game.StateChanged -= OnStateChanged; }

    private void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Preparation: player.Play(preparation); break;
            case GameState.Combat: player.Play(combat); break;
            case GameState.GameOver: player.Stop(); break;
        }
    }
}
