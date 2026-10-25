using UnityEngine;

/// Drives a beam prefab: stretches the lines between two points, scrolls the texture,
/// pulses the width, and moves the start/end glows.
///
///   var b = Instantiate(beamPrefab).GetComponent<BeamVFX>();
///   b.follow = staffTip;     // beam shoots along follow.right every frame
///   b.duration = 2f;         // auto-fades after 2s (0 = until StopBeam())
///
/// Or call b.Aim(origin, direction) / b.SetBeam(from, to) yourself each frame.
public class BeamVFX : MonoBehaviour
{
    [Header("Look")]
    public float width = 0.7f;
    public float scrollSpeed = 4f;
    public float pulseAmount = 0.15f;
    public float pulseSpeed = 25f;
    public float tilesPerUnit = 0.5f;
    public float fadeTime = 0.12f;

    [Header("Behaviour")]
    public float duration = 0f;          // 0 = until StopBeam()
    public float maxLength = 12f;
    public LayerMask hitMask = ~0;       // exclude the caster's layer or the beam hits the caster!
    public Transform follow;             // optional: aim along follow.right each frame
    public Transform startGlow, endGlow;

    LineRenderer[] lines;
    Material[] mats;
    float amount, age;
    bool stopping, aimed;
    Vector2 a, b;

    void Awake()
    {
        lines = GetComponentsInChildren<LineRenderer>();
        mats = new Material[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            mats[i] = lines[i].material; // per-beam instance so scrolling is independent
            lines[i].positionCount = 2;
            lines[i].useWorldSpace = true;
        }
    }

    void Start()
    {
        if (!aimed && !follow) Aim(transform.position, transform.right);
    }

    public void StopBeam() { stopping = true; }

    public void Aim(Vector2 origin, Vector2 direction)
    {
        direction.Normalize();
        var hit = Physics2D.Raycast(origin, direction, maxLength, hitMask);
        Vector2 end = hit.collider ? hit.point : origin + direction * maxLength;
        SetBeam(origin, end);
    }

    public void SetBeam(Vector2 from, Vector2 to)
    {
        aimed = true;
        a = from; b = to;
        foreach (var lr in lines)
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
        }
        if (startGlow) startGlow.position = from;
        if (endGlow) endGlow.position = to;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;
        if (duration > 0f && age >= duration) stopping = true;

        amount = Mathf.MoveTowards(amount, stopping ? 0f : 1f, dt / Mathf.Max(0.01f, fadeTime));
        if (stopping && amount <= 0f) { Destroy(gameObject); return; }

        if (follow) Aim(follow.position, follow.right);

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float len = Vector2.Distance(a, b);

        for (int i = 0; i < lines.Length; i++)
        {
            bool core = lines[i].name == "Core";
            float w = width * (core ? 0.35f : 1f) * amount * pulse;
            lines[i].startWidth = w;
            lines[i].endWidth = w;
            mats[i].mainTextureScale = new Vector2(len * tilesPerUnit, 1f);
            mats[i].mainTextureOffset = new Vector2(-Time.time * scrollSpeed * (core ? 1.4f : 1f), 0f);
        }

        if (startGlow) startGlow.localScale = Vector3.one * amount;
        if (endGlow) endGlow.localScale = Vector3.one * amount;
    }

    void OnDestroy()
    {
        if (mats == null) return;
        foreach (var m in mats) if (m) Destroy(m);
    }
}
