using ProjetoFinal.Audio;
using TMPro;
using UnityEngine;

// Visual da obra: troca de sprite por etapa (estrutura → tábuas → paredes), barra de progresso e tempo restante.
public sealed class ConstructionSite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer stageRenderer;
    [Tooltip("Sprites em ordem; cada um ocupa uma fatia igual do tempo de obra.")]
    [SerializeField] private Sprite[] stages;
    [SerializeField] private Transform progressFill;
    [SerializeField] private TMP_Text timeLabel;
    [SerializeField] private AudioSource workLoop;
    [SerializeField] private AudioClip completedSound;
    private int currentStage = -1;

    public int Stage => currentStage;

    public void SetProgress(float progress, float secondsRemaining)
    {
        progress = Mathf.Clamp01(progress);
        if (stages != null && stages.Length > 0 && stageRenderer != null)
        {
            int stage = Mathf.Min(stages.Length - 1, Mathf.FloorToInt(progress * stages.Length));
            if (stage != currentStage) { currentStage = stage; stageRenderer.sprite = stages[stage]; }
        }
        if (progressFill != null) progressFill.localScale = new Vector3(progress, 1, 1);
        if (timeLabel != null)
        {
            string text = Mathf.CeilToInt(secondsRemaining) + "s";
            if (timeLabel.text != text) timeLabel.text = text;
        }
    }

    public void PlayCompleted()
    {
        if (workLoop != null) workLoop.Stop();
        SoundEffects.PlayAt(completedSound, transform.position, 0.8f, 8, 30);
    }
}
