SPELL VFX KIT (2D, URP) - beams, slashes, flipbook explosions, projectiles, impacts

SETUP
1. Copy the SpellVFX folder into your project's Assets folder.
2. Menu: Tools > Spell VFX > Build All Prefabs
   -> 20 prefabs appear in Assets/SpellVFX/Resources/SpellVFX
      (Projectile / Impact / Beam / Slash / Explosion  x  Fire / Ice / Lightning / Poison)
3. PREVIEW: make an empty GameObject in a 2D scene, add the SpellVFXDemo component, press Play.
   It cycles through every effect. Turn on Bloom in a Global Volume (Threshold ~0.6) for the glow.

USING THEM IN CODE
  // Projectile: parent it to your projectile object (or spawn it and move it)
  var p = Instantiate(Resources.Load<GameObject>("SpellVFX/Projectile_Fire"), projectile.transform);

  // Impact / Explosion: spawn and forget (they destroy themselves)
  Instantiate(Resources.Load<GameObject>("SpellVFX/Impact_Ice"), hitPoint, Quaternion.identity);
  Instantiate(Resources.Load<GameObject>("SpellVFX/Explosion_Fire"), pos, Quaternion.identity);

  // Slash: the prefab's +X axis is the swing direction, so rotate it toward the target
  float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
  Instantiate(Resources.Load<GameObject>("SpellVFX/Slash_Lightning"), pos, Quaternion.Euler(0, 0, angle));

  // Beam: follows a transform's +X (right) axis, e.g. a staff tip
  var beam = Instantiate(Resources.Load<GameObject>("SpellVFX/Beam_Poison")).GetComponent<BeamVFX>();
  beam.follow = staffTip;
  beam.duration = 2f;                 // or call beam.StopBeam()
  beam.hitMask = enemyAndWallLayers;  // IMPORTANT: leave out the caster's layer
  // Or aim manually each frame: beam.Aim(origin, direction);  /  beam.SetBeam(from, to);

  // Any effect: recolor on the fly (for new elements, no new prefab needed)
  vfx.GetComponent<SpellVFX>().SetTint(new Color(1f, 0.2f, 0.6f));

ADDING ELEMENTS
Edit the Elements array at the top of Editor/SpellVFXBuilder.cs, then run the menu again.

TWEAKING LOOK
- Beam width, scroll speed, pulse: fields on the BeamVFX component.
- Everything else: open any prefab and tweak the child Particle Systems (each layer is separate).
- Textures are procedural. Edit and run Tools~/GenerateTextures.py (needs numpy + pillow) to change them,
  or replace the PNGs in Textures/ with your own art (same filenames) and rebuild.

TROUBLESHOOTING
- Pink particles: your pipeline lacks the legacy additive shader. In Assets/SpellVFX/Materials, switch each
  material's shader to a URP Particles/Unlit shader with Additive blending (or "Sprites/Default" for a quick test).
- Beam hits the caster: set hitMask so it excludes the caster's layer.
- Effects look dim: add Bloom (URP Global Volume) and use a Point Light 2D at impacts for extra punch.
