MODULAR SPELL SYSTEM (2D, Unity) - ScriptableObject based
Works with the Spell VFX kit (Assets/SpellVFX). Replace SpellVFX/SpellVFX.cs with the one in this zip.

QUICK START (2 clicks)
1. Make sure the Spell VFX kit is imported. (This zip's SpellVFX.cs replaces the old one.)
2. Open a 2D scene (with a Main Camera) and run:  Tools > Spell System > Setup Test Scene
   It builds the VFX prefabs if needed, creates all starter assets, and adds a Player + enemy spawner.
3. Press Play. Mouse = aim, hold Left Click = cast, keys 1-8 = switch spell, WASD = move.
   (Add Bloom in a Global Volume for the glow.)

THE IDEA
  Spell = Element + Delivery + Modifiers (up to 4)
  - Element  (Assets > Create > Spells > Element)       color, VFX set, damage multiplier, default effects, interactions
  - Delivery (Create > Spells > Delivery > ...)         Projectile, Beam, Slash, Explosion (Area), Orbit  (+ base stats)
  - Modifier (Create > Spells > Modifier (Stats))       Homing, Piercing, Multishot, Bouncing, Splitting, Chain,
                                                        Heavy, Rapid, Wide, Knockback, Explosive, Lingering ...
  - Spell    (Create > Spells > Spell)                  drag an Element, a Delivery and Modifiers into it. Done.
  Right-click a Spell > "Log Compiled Stats" to see the final numbers.

MAKE THINGS WITHOUT CODE
- New modifier: Create > Spells > Modifier (Stats), change the numbers / add on-hit effects.
- New element: Create > Spells > Element. Pick a vfxName (Fire/Ice/Lightning/Poison) and ANY color -> instant new look.
- New status: Create > Spells > Status Effect (DamageOverTime / Slow / Stun / Mark), then
  Create > Spells > On-Hit > Apply Status, and add it to an Element's "Default On Hit" or a modifier's "Add On Hit".
- Elemental combos: Element > Interactions: "if target has tag X, damage x1.5 (and consume it)".
  Starter combos: Fire melts Chilled, Ice shatters Burning, Lightning shocks Chilled for x1.8.

USE IT IN YOUR GAME
  SpellCaster: add to player (or enemies). Fill Loadout. Set Team, Target Mask (enemy layers), Obstacle Mask (walls).
  Drive it yourself:  caster.AimAt(worldPoint);  caster.TryCast();  caster.Select(i);
  (SpellCasterInput is just sample mouse/keyboard controls; delete it when you write your own.)
  Enemies: implement IDamageable (or use the included Health) so spells can hurt them.
  Add StatusReceiver for burn/slow/stun/knockback, and in your movement code:
     speed * receiver.SpeedMultiplier,  skip movement if receiver.Stunned,  add receiver.KnockVelocity.

NOTES ON STATS
- Beam "damage" is damage PER SECOND. Everything else is per hit.
- Chain works on projectile, slash and orbit hits (not beams/explosions).
- Orbit: speed = degrees/sec, range = orbit radius, lifetime = duration.
- Slash: range = reach. Explosion: radius, range = max cast distance from caster.

EXTENDING WITH CODE (each ScriptableObject class MUST live in a file with the same name)
- New delivery:   class MyDelivery : DeliverySO  { override IEnumerator Cast(SpellContext ctx) { ... } }
- New on-hit:     class MyEffect : OnHitEffectSO { override void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 dir) { ... } }
- New modifier:   class MyMod : ModifierSO { override void Apply(SpellStats s, List<OnHitEffectSO> onHit) { ... } }
  Add [CreateAssetMenu(menuName = "Spells/...")] to get a right-click entry.
  Helpers: SpellUtil.Hit / DamageArea / FindNearest / SpawnVFX.

TROUBLESHOOTING
- Nothing visible: run Tools > Spell VFX > Build All Prefabs (effects load from Resources/SpellVFX).
- Spells hit nothing: enemies need a Collider2D and a Health (or IDamageable) with a different Team than the caster,
  on a layer included in the caster's Target Mask.
- Beam/projectiles ignore walls: set Obstacle Mask on the SpellCaster.
