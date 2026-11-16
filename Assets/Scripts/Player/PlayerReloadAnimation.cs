using UnityEngine;

// Recarga caricata e procedural: a arma levanta, chacoalha, a mão de apoio "puxa o pente" e tudo volta.
// Só lê o estado do PlayerCombat, então funciona com qualquer arma. A pose de repouso é relida
// sempre que outra coisa mexe nas peças (ex.: trocar de arma reposiciona o HeldWeapon).
[DefaultExecutionOrder(100)]
public sealed class PlayerReloadAnimation : MonoBehaviour
{
    [SerializeField] private PlayerCombat combat;
    [Tooltip("Arma e braços que giram juntos durante a recarga.")]
    [SerializeField] private Transform[] gunParts;
    [Tooltip("Mão que desce e volta (tirando/colocando o pente).")]
    [SerializeField] private Transform supportHand;
    [Tooltip("Ponto (local, no mesmo pai das peças) em torno do qual a arma gira.")]
    [SerializeField] private Vector2 pivot = new Vector2(0.1f, -0.05f);
    [SerializeField] private float tiltAngle = 38;
    [SerializeField, Min(0)] private float shakeAngle = 7;
    [SerializeField, Min(0)] private float shakeFrequency = 22;
    [SerializeField, Min(0)] private float handDrop = 0.22f;

    private Vector3[] basePositions;
    private Quaternion[] baseRotations;
    private Vector3[] writtenPositions;
    private Quaternion[] writtenRotations;
    private Vector3 handBase;
    private Vector3 handWritten;
    private bool posed;

    private void Awake()
    {
        if (combat == null) combat = GetComponentInParent<PlayerCombat>();
        basePositions = new Vector3[gunParts.Length];
        baseRotations = new Quaternion[gunParts.Length];
        writtenPositions = new Vector3[gunParts.Length];
        writtenRotations = new Quaternion[gunParts.Length];
        CaptureRest();
    }

    private void LateUpdate()
    {
        CaptureRest();
        if (combat == null || !combat.IsReloading) { if (posed) ApplyPose(0, 0); return; }
        float t = combat.ReloadProgress;
        // Sobe rápido (0–20%), chacoalha no meio, desce no fim (80–100%).
        float lift = t < .2f ? Ease(t / .2f) : t > .8f ? Ease((1 - t) / .2f) : 1;
        float shake = t > .25f && t < .75f ? Mathf.Sin(Time.time * shakeFrequency) * shakeAngle : 0;
        float hand = t > .3f && t < .7f ? Mathf.Sin((t - .3f) / .4f * Mathf.PI) : 0;
        ApplyPose(tiltAngle * lift + shake, hand);
    }

    // Peça que não está posada, ou que alguém moveu depois da nossa última escrita, vira a nova pose de repouso.
    private void CaptureRest()
    {
        for (int i = 0; i < gunParts.Length; i++)
        {
            var part = gunParts[i];
            if (part == null) continue;
            if (!posed || part.localPosition != writtenPositions[i] || part.localRotation != writtenRotations[i])
            {
                basePositions[i] = part.localPosition;
                baseRotations[i] = part.localRotation;
            }
        }
        if (supportHand != null && System.Array.IndexOf(gunParts, supportHand) < 0
            && (!posed || supportHand.localPosition != handWritten))
            handBase = supportHand.localPosition;
    }

    private void ApplyPose(float angle, float hand)
    {
        var rotation = Quaternion.Euler(0, 0, angle);
        var center = (Vector3)pivot;
        for (int i = 0; i < gunParts.Length; i++)
        {
            var part = gunParts[i];
            if (part == null) continue;
            part.localPosition = center + rotation * (basePositions[i] - center);
            part.localRotation = rotation * baseRotations[i];
        }
        if (supportHand != null)
        {
            // Se a mão não gira com a arma, parte da pose original em vez de acumular deslocamento.
            if (System.Array.IndexOf(gunParts, supportHand) < 0) supportHand.localPosition = handBase;
            supportHand.localPosition += Vector3.down * (handDrop * hand);
            handWritten = supportHand.localPosition;
        }
        for (int i = 0; i < gunParts.Length; i++)
        {
            if (gunParts[i] == null) continue;
            writtenPositions[i] = gunParts[i].localPosition;
            writtenRotations[i] = gunParts[i].localRotation;
        }
        posed = angle != 0 || hand != 0;
    }

    private static float Ease(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x); }

    private void OnDisable() { if (posed) ApplyPose(0, 0); }
}
