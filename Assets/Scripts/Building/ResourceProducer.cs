using ProjetoFinal.Audio;
using TMPro;
using UnityEngine;

// Construção produtora (serraria, sucataria...): a barra enche, o lote vai para o depósito da construção
// e o jogador coleta com E. Com o depósito cheio, a produção para.
// Os valores ficam expostos para upgrades futuros (velocidade, quantidade, capacidade).
public sealed class ResourceProducer : MonoBehaviour, IInteractable, IInteractionPromptSource
{
    private static readonly Color Producing = new Color(1, .77f, .27f);
    private static readonly Color Full = new Color(.45f, .9f, .55f);

    [SerializeField] private ResourceData resource;
    [SerializeField, Min(1)] private int amountPerCycle = 5;
    [SerializeField, Min(0.5f)] private float interval = 10;
    [SerializeField, Min(1)] private int capacity = 50;
    [SerializeField] private AudioClip produceSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0, 1)] private float volume = 0.5f;
    [Header("Visual")]
    [Tooltip("Raiz da barra de produção (escondida durante a obra).")]
    [SerializeField] private GameObject barRoot;
    [Tooltip("Pivô da barra; a escala X vai de 0 a 1.")]
    [SerializeField] private Transform cycleFill;
    [SerializeField] private SpriteRenderer cycleFillRenderer;
    [SerializeField] private TMP_Text storedLabel;
    [SerializeField] private Vector2 popupOffset = new Vector2(0, 3.2f);
    private ResourceManager resources;
    private GameManager game;
    private float elapsed;
    private readonly ResourceCost[] promptItems = new ResourceCost[1];

    public ResourceData Resource => resource;
    public int AmountPerCycle => amountPerCycle;
    public float Interval => interval;
    public int Capacity => capacity;
    public int Stored { get; private set; }
    public bool IsFull => Stored >= capacity;
    public float CycleProgress => IsFull ? 1 : Mathf.Clamp01(elapsed / interval);

    public void Initialize(ResourceManager manager, GameManager session)
    {
        resources = manager;
        game = session;
    }

    // Ponto único para upgrades: ajusta produção sem mexer no que já está guardado.
    public void ApplyStats(int newAmountPerCycle, float newInterval, int newCapacity)
    {
        amountPerCycle = Mathf.Max(1, newAmountPerCycle);
        interval = Mathf.Max(0.5f, newInterval);
        capacity = Mathf.Max(1, newCapacity);
        Stored = Mathf.Min(Stored, capacity);
        RefreshVisual();
    }

    private void Awake()
    {
        if (resources == null) resources = FindAnyObjectByType<ResourceManager>();
        // Awake roda mesmo com o componente desligado (obra): a barra só aparece com a construção pronta.
        if (barRoot != null) barRoot.SetActive(enabled);
    }

    private void OnEnable() { elapsed = 0; if (barRoot != null) barRoot.SetActive(true); RefreshVisual(); }
    private void OnDisable() { if (barRoot != null) barRoot.SetActive(false); }

    private void Update()
    {
        if (game != null && !game.IsRunning) return;
        if (!IsFull)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= interval)
            {
                elapsed -= interval;
                int produced = Mathf.Min(amountPerCycle, capacity - Stored);
                Stored += produced;
                SoundEffects.PlayAt(produceSound, transform.position, volume * 0.6f, 3, 10);
                Popup("+" + produced, new Color(1, .9f, .55f));
            }
        }
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (cycleFill != null) cycleFill.localScale = new Vector3(CycleProgress, 1, 1);
        if (cycleFillRenderer != null) cycleFillRenderer.color = IsFull ? Full : Producing;
        if (storedLabel != null)
        {
            string text = IsFull ? "CHEIO " + Stored : Stored + " / " + capacity;
            if (storedLabel.text != text) storedLabel.text = text;
        }
    }

    public string InteractionDescription => "Coletar " + Stored + " " + (resource != null ? resource.DisplayName : "");
    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && Stored > 0 && resources != null;

    public InteractionPrompt Prompt
    {
        get
        {
            promptItems[0] = new ResourceCost(resource, Stored);
            return new InteractionPrompt("Coletar", promptItems, InteractionItemsKind.Reward);
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        int amount = Stored;
        if (!resources.TryAdd(resource, amount)) return;
        Stored = 0;
        SoundEffects.Play(collectSound, volume);
        Popup("+" + amount + " " + (resource != null ? resource.DisplayName : ""), new Color(.6f, .95f, .65f));
        RefreshVisual();
    }

    private void Popup(string message, Color color)
    {
        var popup = new GameObject("ProducerPopup");
        popup.transform.position = transform.position + (Vector3)popupOffset;
        var text = popup.AddComponent<TextMeshPro>();
        text.text = message;
        text.fontSize = 4;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(5, 1);
        text.color = color;
        text.renderer.sortingLayerName = "Foreground";
        text.renderer.sortingOrder = 110;
        popup.AddComponent<FloatingDamageNumber>();
    }
}
