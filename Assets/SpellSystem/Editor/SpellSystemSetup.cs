using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpellSystem;

/// Tools > Spell System > Create Starter Assets   (elements, statuses, deliveries, modifiers, 8 example spells)
/// Tools > Spell System > Setup Test Scene        (player + enemies in the open scene)
public static class SpellSystemSetup
{
    const string Data = "Assets/SpellSystem/Data";

    static T Make<T>(string sub, string name, System.Action<T> init) where T : ScriptableObject
    {
        string dir = Data + "/" + sub;
        Directory.CreateDirectory(dir);
        string path = dir + "/" + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;     // never overwrite your edits
        var a = ScriptableObject.CreateInstance<T>();
        if (init != null) init(a);
        AssetDatabase.CreateAsset(a, path);
        EditorUtility.SetDirty(a);
        return a;
    }

    static StatModifierSO Mod(string name, string desc, System.Action<StatModifierSO> init)
    {
        return Make<StatModifierSO>("Modifiers", name, m => { m.description = desc; init(m); });
    }

    static SpellSO Spell(string name, ElementSO e, DeliverySO d, params ModifierSO[] mods)
    {
        return Make<SpellSO>("Spells", name.Replace(" ", ""), s =>
        {
            s.displayName = name; s.element = e; s.delivery = d;
            s.modifiers = new List<ModifierSO>(mods);
        });
    }

    [MenuItem("Tools/Spell System/Create Starter Assets")]
    public static void CreateStarterAssets() { Starter(); }

    static List<SpellSO> Starter()
    {
        AssetDatabase.Refresh();

        // ---- Elements ----
        var fire = Make<ElementSO>("Elements", "Fire", e => { e.vfxName = "Fire"; e.color = new Color(1f, 0.45f, 0.1f); });
        var ice = Make<ElementSO>("Elements", "Ice", e => { e.vfxName = "Ice"; e.color = new Color(0.4f, 0.85f, 1f); });
        var lightning = Make<ElementSO>("Elements", "Lightning", e => { e.vfxName = "Lightning"; e.color = new Color(0.8f, 0.6f, 1f); });
        var poison = Make<ElementSO>("Elements", "Poison", e => { e.vfxName = "Poison"; e.color = new Color(0.5f, 1f, 0.3f); });

        // ---- Status effects ----
        var burning = Make<StatusEffectSO>("Status", "Burning", s => { s.statusTag = "Burning"; s.kind = StatusKind.DamageOverTime; s.duration = 3f; s.magnitude = 4f; s.tickInterval = 0.5f; s.element = fire; });
        var chilled = Make<StatusEffectSO>("Status", "Chilled", s => { s.statusTag = "Chilled"; s.kind = StatusKind.Slow; s.duration = 3f; s.magnitude = 0.5f; s.element = ice; });
        var shocked = Make<StatusEffectSO>("Status", "Shocked", s => { s.statusTag = "Shocked"; s.kind = StatusKind.Stun; s.duration = 0.35f; s.element = lightning; });
        var poisoned = Make<StatusEffectSO>("Status", "Poisoned", s => { s.statusTag = "Poisoned"; s.kind = StatusKind.DamageOverTime; s.duration = 5f; s.magnitude = 3f; s.tickInterval = 0.5f; s.element = poison; });

        // ---- On-hit effects ----
        var burnFx = Make<StatusOnHit>("OnHit", "Apply_Burning", f => f.status = burning);
        var chillFx = Make<StatusOnHit>("OnHit", "Apply_Chilled", f => f.status = chilled);
        var shockFx = Make<StatusOnHit>("OnHit", "Apply_Shocked", f => { f.status = shocked; f.chance = 0.35f; });
        var poisonFx = Make<StatusOnHit>("OnHit", "Apply_Poisoned", f => f.status = poisoned);
        var knockFx = Make<KnockbackOnHit>("OnHit", "Knockback", f => f.force = 8f);
        var explodeFx = Make<ExplodeOnHit>("OnHit", "Explode", f => { f.radius = 1.6f; f.damageMultiplier = 0.5f; });
        var lingerFx = Make<LingerZoneOnHit>("OnHit", "LingerZone", f => { f.radius = 1.6f; f.duration = 3f; });

        // ---- Hook effects + interactions into elements (only if empty, so your edits survive) ----
        if (fire.defaultOnHit.Count == 0) { fire.defaultOnHit.Add(burnFx); fire.interactions.Add(new ElementInteraction { targetHasTag = "Chilled", damageMultiplier = 1.5f, consumeTag = true }); }
        if (ice.defaultOnHit.Count == 0) { ice.defaultOnHit.Add(chillFx); ice.interactions.Add(new ElementInteraction { targetHasTag = "Burning", damageMultiplier = 1.5f, consumeTag = true }); }
        if (lightning.defaultOnHit.Count == 0) { lightning.defaultOnHit.Add(shockFx); lightning.interactions.Add(new ElementInteraction { targetHasTag = "Chilled", damageMultiplier = 1.8f, consumeTag = true }); }
        if (poison.defaultOnHit.Count == 0) { poison.defaultOnHit.Add(poisonFx); }
        EditorUtility.SetDirty(fire); EditorUtility.SetDirty(ice); EditorUtility.SetDirty(lightning); EditorUtility.SetDirty(poison);

        // ---- Deliveries ----
        var projectile = Make<ProjectileDelivery>("Deliveries", "Projectile", d => d.baseStats = new SpellStats { damage = 12f, speed = 12f, lifetime = 2f, cooldown = 0.45f, manaCost = 6f });
        var beam = Make<BeamDelivery>("Deliveries", "Beam", d => d.baseStats = new SpellStats { damage = 28f, lifetime = 2f, range = 9f, tickInterval = 0.15f, cooldown = 2.6f, manaCost = 20f });
        var slash = Make<SlashDelivery>("Deliveries", "Slash", d => d.baseStats = new SpellStats { damage = 20f, range = 2.5f, cooldown = 0.5f, manaCost = 8f });
        var area = Make<ExplosionDelivery>("Deliveries", "Explosion", d => d.baseStats = new SpellStats { damage = 30f, radius = 2.2f, range = 9f, cooldown = 1.4f, manaCost = 18f });
        var orbit = Make<OrbitDelivery>("Deliveries", "Orbit", d => d.baseStats = new SpellStats { damage = 8f, speed = 200f, range = 2f, lifetime = 5f, projectileCount = 3, size = 0.8f, cooldown = 6f, manaCost = 20f });

        // ---- Modifiers ----
        var homing = Mod("Homing", "Steers toward nearby enemies.", m => { m.addHoming = 240f; m.manaMult = 1.2f; });
        var piercing = Mod("Piercing", "Passes through extra enemies.", m => { m.addPierce = 2; m.damageMult = 0.85f; });
        var multishot = Mod("Multishot", "Fires extra projectiles in a fan.", m => { m.addProjectiles = 2; m.addSpread = 30f; m.damageMult = 0.7f; m.manaMult = 1.5f; });
        var bouncing = Mod("Bouncing", "Ricochets off walls and enemies.", m => m.addBounces = 3);
        var splitting = Mod("Splitting", "Shatters into smaller shards on impact.", m => { m.addSplit = 3; m.damageMult = 0.9f; });
        var chain = Mod("Chain", "Arcs to nearby enemies after a hit.", m => { m.addChain = 2; m.damageMult = 0.85f; });
        var heavy = Mod("Heavy", "Bigger, slower, much stronger.", m => { m.damageMult = 1.8f; m.sizeMult = 1.4f; m.speedMult = 0.7f; m.cooldownMult = 1.4f; m.manaMult = 1.3f; });
        var rapid = Mod("Rapid", "Casts twice as fast but weaker.", m => { m.cooldownMult = 0.5f; m.damageMult = 0.7f; });
        var wide = Mod("Wide", "Larger area and reach.", m => { m.sizeMult = 1.5f; m.radiusMult = 1.3f; m.rangeMult = 1.15f; });
        var knockback = Mod("Knockback", "Shoves enemies back.", m => m.addOnHit.Add(knockFx));
        var explosive = Mod("Explosive", "Small blast on every hit.", m => { m.addOnHit.Add(explodeFx); m.manaMult = 1.3f; });
        var lingering = Mod("Lingering", "Leaves a damaging zone behind.", m => { m.addOnHit.Add(lingerFx); m.manaMult = 1.3f; });

        // ---- Example spells (order = hotkeys 1-8) ----
        var spells = new List<SpellSO>
        {
            Spell("Fireball", fire, projectile, explosive),
            Spell("Frost Shards", ice, projectile, splitting),
            Spell("Chain Lightning", lightning, projectile, chain, homing),
            Spell("Venom Beam", poison, beam, wide),
            Spell("Flame Slash", fire, slash, rapid),
            Spell("Meteor", fire, area, wide, lingering),
            Spell("Frost Orbs", ice, orbit, wide),
            Spell("Storm Blade", lightning, slash, chain, knockback),
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Spell System: starter assets ready in " + Data + ". Try combining modules in a new Spell asset (Create > Spells > Spell).");
        return spells;
    }

    [MenuItem("Tools/Spell System/Setup Test Scene")]
    public static void SetupTestScene()
    {
        // VFX prefabs must exist (from the Spell VFX kit).
        if (Resources.Load<GameObject>("SpellVFX/Projectile_Fire") == null)
        {
            if (!EditorApplication.ExecuteMenuItem("Tools/Spell VFX/Build All Prefabs") ||
                Resources.Load<GameObject>("SpellVFX/Projectile_Fire") == null)
            {
                EditorUtility.DisplayDialog("Spell System",
                    "Spell VFX prefabs not found. Install the Spell VFX kit and run Tools > Spell VFX > Build All Prefabs first.", "OK");
                return;
            }
        }

        var spells = Starter();
        var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            Undo.RecordObject(cam, "Setup Spell Test Scene");
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f);
        }

        // Player
        var player = new GameObject("Player");
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = knob; psr.sortingOrder = -5;
        var caster = player.AddComponent<SpellCaster>();
        caster.team = 0;
        caster.loadout = spells;
        player.AddComponent<SpellCasterInput>();

        // Enemy template (inactive; the spawner clones it)
        var tpl = new GameObject("EnemyTemplate");
        Undo.RegisterCreatedObjectUndo(tpl, "Create Enemy Template");
        var esr = tpl.AddComponent<SpriteRenderer>();
        esr.sprite = knob; esr.color = new Color(0.85f, 0.25f, 0.25f); esr.sortingOrder = -5;
        var col = tpl.AddComponent<CircleCollider2D>();
        col.radius = knob != null ? knob.bounds.extents.x : 0.5f;
        var hp = tpl.AddComponent<Health>();
        hp.maxHealth = 60f; hp.team = 1;
        tpl.AddComponent<StatusReceiver>();
        tpl.AddComponent<SimpleEnemy>();
        tpl.SetActive(false);

        // Spawner
        var sp = new GameObject("EnemySpawner");
        Undo.RegisterCreatedObjectUndo(sp, "Create Spawner");
        var spawner = sp.AddComponent<EnemySpawner>();
        spawner.template = tpl;
        spawner.center = player.transform;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = player;
        Debug.Log("Spell System test scene ready. Press Play: aim with the mouse, hold left click to cast, keys 1-8 switch spells.");
    }
}
