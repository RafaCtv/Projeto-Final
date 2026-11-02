using UnityEngine;

// Cosmetic variation only. Every appearance shares the existing controller/data/collider.
public sealed class ZombieAppearance : MonoBehaviour
{
    [SerializeField] private GameObject[] appearances;
    private static int nextAppearance;
    public int AppearanceIndex { get; private set; }
    public int Count => appearances == null ? 0 : appearances.Length;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSequence() => nextAppearance = 0;
    private void Awake() { if (Count > 0) Select(nextAppearance++ % Count); }
    public void Select(int index)
    {
        if (Count == 0) return;
        AppearanceIndex = Mathf.Clamp(index, 0, Count - 1);
        for (int i = 0; i < Count; i++) if (appearances[i] != null) appearances[i].SetActive(i == AppearanceIndex);
    }
}
