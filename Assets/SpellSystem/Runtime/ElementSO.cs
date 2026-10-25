using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/Element", fileName = "NewElement")]
    public class ElementSO : ScriptableObject
    {
        [Tooltip("Which generated VFX set to use: Fire, Ice, Lightning or Poison (prefabs in Resources/SpellVFX).")]
        public string vfxName = "Fire";
        [Tooltip("Recolors the VFX. Use any color to make a brand-new element from an existing VFX set.")]
        public Color color = Color.white;
        public float damageMultiplier = 1f;

        [Tooltip("Effects every spell of this element applies on hit (burn, slow...).")]
        public List<OnHitEffectSO> defaultOnHit = new List<OnHitEffectSO>();
        public List<ElementInteraction> interactions = new List<ElementInteraction>();

        [Header("Optional VFX overrides (leave empty to use Resources/SpellVFX)")]
        public GameObject projectileVfx;
        public GameObject impactVfx;
        public GameObject beamVfx;
        public GameObject slashVfx;
        public GameObject explosionVfx;

        public GameObject GetVFX(VFXKind kind)
        {
            GameObject o = null;
            switch (kind)
            {
                case VFXKind.Projectile: o = projectileVfx; break;
                case VFXKind.Impact: o = impactVfx; break;
                case VFXKind.Beam: o = beamVfx; break;
                case VFXKind.Slash: o = slashVfx; break;
                case VFXKind.Explosion: o = explosionVfx; break;
            }
            if (o != null) return o;
            return Resources.Load<GameObject>("SpellVFX/" + kind + "_" + vfxName);
        }
    }
}
