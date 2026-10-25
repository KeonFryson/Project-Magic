using UnityEditor;
using UnityEngine;
using System.IO;

/// One-click generator for all spell VFX prefabs.
/// Menu: Tools > Spell VFX > Build All Prefabs
/// Output: Assets/SpellVFX/Resources/SpellVFX/<Type>_<Element>.prefab
/// Types: Projectile, Impact, Beam, Slash, Explosion.
public static class SpellVFXBuilder
{
    const string Root = "Assets/SpellVFX";
    const string PrefabDir = Root + "/Resources/SpellVFX";

    // ===================== EDIT THESE to add elements =====================
    static readonly (string name, Color color)[] Elements = {
        ("Fire",      new Color(1.00f, 0.45f, 0.10f)),
        ("Ice",       new Color(0.40f, 0.85f, 1.00f)),
        ("Lightning", new Color(0.80f, 0.60f, 1.00f)),
        ("Poison",    new Color(0.50f, 1.00f, 0.30f)),
    };
    // ======================================================================

    [MenuItem("Tools/Spell VFX/Build All Prefabs")]
    public static void Build()
    {
        Directory.CreateDirectory(PrefabDir);
        Directory.CreateDirectory(Root + "/Materials");
        AssetDatabase.Refresh();

        foreach (var t in new[] { "soft_circle", "ring", "spark", "slash", "explosion_sheet" })
            Import(t, TextureWrapMode.Clamp);
        Import("beam", TextureWrapMode.Repeat);

        var glow = Mat("glow", "soft_circle");
        var ring = Mat("ring", "ring");
        var spark = Mat("spark", "spark");
        var slash = Mat("slash", "slash");
        var beam = Mat("beam", "beam");
        var flip = Mat("explosion", "explosion_sheet");

        foreach (var e in Elements)
        {
            Save(Projectile(e.color, glow, spark), "Projectile_" + e.name);
            Save(Impact(e.color, glow, ring, spark), "Impact_" + e.name);
            Save(Beam(e.color, glow, spark, beam), "Beam_" + e.name);
            Save(Slash(e.color, glow, spark, slash), "Slash_" + e.name);
            Save(Explosion(e.color, glow, ring, spark, flip), "Explosion_" + e.name);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Spell VFX: built " + (Elements.Length * 5) + " prefabs in " + PrefabDir);
    }

    // ===================== EFFECT RECIPES =====================

    static GameObject Projectile(Color c, Material glow, Material spark)
    {
        var root = new GameObject("Projectile");
        var vfx = root.AddComponent<SpellVFX>();

        Alpha(Layer(root.transform, "Aura", glow, 1, 0.25f, 1.8f, 25, false), 0.35f);
        Layer(root.transform, "Core", glow, 3, 0.2f, 0.6f, 30, false);
        Alpha(Layer(root.transform, "Trail", glow, 0, 0.45f, 0.55f, 70, true), 0.6f);

        var sparks = Layer(root.transform, "Sparks", spark, 2, 0.5f, 0.18f, 30, true);
        Circle(sparks, 0.1f);
        Speed(sparks, 1f, 3f);
        Stretch(sparks);

        vfx.SetTint(c);
        return root;
    }

    static GameObject Impact(Color c, Material glow, Material ring, Material spark)
    {
        var root = new GameObject("Impact");
        var vfx = root.AddComponent<SpellVFX>();
        vfx.destroyWhenDone = true;

        OneShot(Layer(root.transform, "Core", glow, 3, 0.18f, 3f, 0, false), 1);

        var rg = Layer(root.transform, "Ring", ring, 2, 0.35f, 4f, 0, false);
        OneShot(rg, 1);
        Grow(rg, 0.15f, 1f);

        var sparks = Layer(root.transform, "Sparks", spark, 2, 0.5f, 0.22f, 0, true);
        OneShot(sparks, 24);
        Circle(sparks, 0.1f);
        Speed(sparks, 3f, 7f);
        Lifetime(sparks, 0.3f, 0.6f);
        Stretch(sparks);

        vfx.SetTint(c);
        return root;
    }

    static GameObject Beam(Color c, Material glow, Material spark, Material beamMat)
    {
        var root = new GameObject("Beam");
        var vfx = root.AddComponent<SpellVFX>();
        root.AddComponent<BeamVFX>();
        var beam = root.GetComponent<BeamVFX>();

        MakeLine(root.transform, "Outer", beamMat, 1);
        MakeLine(root.transform, "Core", beamMat, 2);

        var start = new GameObject("Start"); start.transform.SetParent(root.transform, false);
        Alpha(Layer(start.transform, "Aura", glow, 0, 0.25f, 2.6f, 20, false), 0.4f);
        Layer(start.transform, "Core", glow, 3, 0.2f, 1.4f, 25, false);

        var end = new GameObject("End"); end.transform.SetParent(root.transform, false);
        Alpha(Layer(end.transform, "Aura", glow, 0, 0.25f, 3f, 20, false), 0.4f);
        Layer(end.transform, "Core", glow, 3, 0.2f, 1.7f, 25, false);
        var sparks = Layer(end.transform, "Sparks", spark, 2, 0.5f, 0.18f, 40, true);
        Circle(sparks, 0.15f);
        Speed(sparks, 2f, 5f);
        Stretch(sparks);

        beam.startGlow = start.transform;
        beam.endGlow = end.transform;
        vfx.SetTint(c);
        return root;
    }

    static GameObject Slash(Color c, Material glow, Material spark, Material slashMat)
    {
        var root = new GameObject("Slash");
        var vfx = root.AddComponent<SpellVFX>();
        vfx.destroyWhenDone = true;

        // The root's +X axis is the swing direction. Rotate the root to aim it.
        Alpha(SlashArc(root.transform, "Echo", slashMat, 2, 0.30f, 4.4f, 0.05f), 0.4f);
        SlashArc(root.transform, "Arc", slashMat, 3, 0.22f, 3.4f, 0f);
        SlashArc(root.transform, "Core", slashMat, 4, 0.18f, 2.6f, 0f);

        OneShot(Layer(root.transform, "Flash", glow, 1, 0.12f, 1.8f, 0, false), 1);

        var sparks = Layer(root.transform, "Sparks", spark, 2, 0.5f, 0.2f, 0, true);
        OneShot(sparks, 18);
        var sh = sparks.shape;
        sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 30f; sh.radius = 0.2f; sh.rotation = new Vector3(0, 90, 0);
        Speed(sparks, 4f, 9f);
        Lifetime(sparks, 0.25f, 0.5f);
        Stretch(sparks);

        vfx.SetTint(c);
        return root;
    }

    static GameObject Explosion(Color c, Material glow, Material ring, Material spark, Material flip)
    {
        var root = new GameObject("Explosion");
        var vfx = root.AddComponent<SpellVFX>();
        vfx.destroyWhenDone = true;

        // Flipbook fireballs (4x4 sheet), randomly rotated so they never look identical.
        var fire = Flipbook(root.transform, "Fireball", flip, 2, 0.75f, 6f);
        var core = Flipbook(root.transform, "Core", flip, 3, 0.55f, 3.4f);
        Alpha(core, 0.9f);

        var flash = Layer(root.transform, "Flash", glow, 1, 0.3f, 10f, 0, false);
        Alpha(flash, 0.3f);
        OneShot(flash, 1);

        var rg = Layer(root.transform, "Ring", ring, 4, 0.4f, 9f, 0, false);
        OneShot(rg, 1);
        Grow(rg, 0.1f, 1f);

        var sparks = Layer(root.transform, "Sparks", spark, 3, 0.8f, 0.25f, 0, true);
        OneShot(sparks, 34);
        Circle(sparks, 0.2f);
        Speed(sparks, 4f, 10f);
        Lifetime(sparks, 0.4f, 0.9f);
        Stretch(sparks);

        vfx.SetTint(c);
        return root;
    }

    // ===================== BUILDING BLOCKS =====================

    static ParticleSystem Layer(Transform parent, string name, Material mat, int order,
        float life, float size, float rate, bool world)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.sortingOrder = order;

        var m = ps.main;
        m.startLifetime = life;
        m.startSize = size;
        m.startSpeed = 0f;
        m.maxParticles = 300;
        m.simulationSpace = world ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;

        var em = ps.emission; em.rateOverTime = rate;
        var sh = ps.shape; sh.enabled = false;

        FadeInOut(ps);

        var so = ps.sizeOverLifetime; so.enabled = true;
        so.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1f, 1, 0.1f));
        return ps;
    }

    static void FadeInOut(ParticleSystem ps) { SetFade(ps, 0.1f, 0.1f); }

    // alpha: 0 -> 1 by fadeIn, holds until fadeOutStart, then -> 0 at the end.
    // (fadeOutStart <= fadeIn means a simple triangle fade.)
    static void SetFade(ParticleSystem ps, float fadeIn, float fadeOutStart)
    {
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        if (fadeOutStart <= fadeIn)
        {
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeIn), new GradientAlphaKey(0f, 1f) });
        }
        else
        {
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeIn),
                        new GradientAlphaKey(1f, fadeOutStart), new GradientAlphaKey(0f, 1f) });
        }
        col.color = g;
    }

    static ParticleSystem SlashArc(Transform parent, string name, Material mat, int order,
        float life, float size, float delay)
    {
        var ps = Layer(parent, name, mat, order, life, size, 0, false);
        OneShot(ps, 1);

        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.alignment = ParticleSystemRenderSpace.Local;   // follows the root's rotation

        var m = ps.main; m.startDelay = delay;

        var v = ps.velocityOverLifetime; v.enabled = true;
        v.space = ParticleSystemSimulationSpace.Local;
        v.x = new ParticleSystem.MinMaxCurve(5f);
        v.y = new ParticleSystem.MinMaxCurve(0f);
        v.z = new ParticleSystem.MinMaxCurve(0f);

        var so = ps.sizeOverLifetime; so.enabled = true;
        so.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.55f), new Keyframe(0.35f, 1f), new Keyframe(1f, 1.1f)));

        SetFade(ps, 0.05f, 0.35f);
        return ps;
    }

    static ParticleSystem Flipbook(Transform parent, string name, Material mat, int order, float life, float size)
    {
        var ps = Layer(parent, name, mat, order, life, size, 0, false);
        OneShot(ps, 1);

        var m = ps.main;
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var so = ps.sizeOverLifetime; so.enabled = false;
        SetFade(ps, 0.04f, 0.8f);

        var t = ps.textureSheetAnimation; t.enabled = true;
        t.mode = ParticleSystemAnimationMode.Grid;
        t.numTilesX = 4; t.numTilesY = 4;
        t.animation = ParticleSystemAnimationType.WholeSheet;
        t.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0, 1, 1));
        t.cycleCount = 1;
        return ps;
    }

    static LineRenderer MakeLine(Transform parent, string name, Material mat, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = mat;
        lr.sortingOrder = order;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.numCapVertices = 0;
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.SetPositions(new[] { Vector3.zero, Vector3.right * 5f });
        lr.startWidth = lr.endWidth = 0.5f;
        return lr;
    }

    static void OneShot(ParticleSystem ps, int count)
    {
        var m = ps.main; m.loop = false; m.duration = 0.6f;
        var em = ps.emission; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
    }

    static void Grow(ParticleSystem ps, float from, float to)
    {
        var so = ps.sizeOverLifetime; so.enabled = true;
        so.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, from, 1, to));
    }

    static void Circle(ParticleSystem ps, float radius)
    {
        var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = radius;
    }

    static ParticleSystem Alpha(ParticleSystem ps, float a)
    {
        var m = ps.main; m.startColor = new Color(1, 1, 1, a);
        return ps;
    }

    static void Speed(ParticleSystem ps, float min, float max)
    {
        var m = ps.main; m.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
    }

    static void Lifetime(ParticleSystem ps, float min, float max)
    {
        var m = ps.main; m.startLifetime = new ParticleSystem.MinMaxCurve(min, max);
    }

    static void Stretch(ParticleSystem ps)
    {
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Stretch;
        r.lengthScale = 2f;
        r.velocityScale = 0.1f;
    }

    static void Import(string tex, TextureWrapMode wrap)
    {
        string path = $"{Root}/Textures/{tex}.png";
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.textureType = TextureImporterType.Default;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.wrapMode = wrap;
        imp.SaveAndReimport();
    }

    static Material Mat(string name, string tex)
    {
        string path = $"{Root}/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing) return existing;
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/Textures/{tex}.png");
        var m = new Material(Shader.Find("Legacy Shaders/Particles/Additive")) { mainTexture = t };
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static void Save(GameObject go, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
    }
}
