using UnityEngine;

/// Root component for every spell VFX prefab. SetTint recolors all layers
/// (particles and beam lines). Layers named "Core" get a hot white-ish tint.
public class SpellVFX : MonoBehaviour
{
    public Color tint = Color.white;
    public bool destroyWhenDone;

    ParticleSystem[] systems;
    LineRenderer[] lines;

    void Awake()
    {
        Cache();
        // Lets the whole effect scale when you scale the root (used by the spell system for size).
        foreach (var ps in systems)
        {
            var m = ps.main;
            m.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    void Cache()
    {
        systems = GetComponentsInChildren<ParticleSystem>();
        lines = GetComponentsInChildren<LineRenderer>();
    }

    public void SetTint(Color c)
    {
        tint = c;
        if (systems == null) Cache();

        foreach (var ps in systems)
        {
            var m = ps.main;
            float a = m.startColor.color.a;
            Color col = ps.name == "Core" ? Color.Lerp(c, Color.white, 0.6f) : c;
            col.a = a;
            m.startColor = col;
        }
        foreach (var lr in lines)
        {
            Color col = lr.name == "Core" ? Color.Lerp(c, Color.white, 0.6f) : c;
            lr.startColor = col;
            lr.endColor = col;
        }
    }

    void Update()
    {
        if (!destroyWhenDone) return;
        foreach (var ps in systems) if (ps.IsAlive(false)) return;
        Destroy(gameObject);
    }
}
