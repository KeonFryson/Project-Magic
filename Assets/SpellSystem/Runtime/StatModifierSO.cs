using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// One flexible modifier. Homing, Piercing, Multishot, Heavy, Chain, Explosive...
    /// are all just different values on this asset. No code needed.
    [CreateAssetMenu(menuName = "Spells/Modifier (Stats)", fileName = "NewModifier")]
    public class StatModifierSO : ModifierSO
    {
        [Header("Multipliers (1 = no change)")]
        public float damageMult = 1f;
        public float speedMult = 1f;
        public float sizeMult = 1f;
        public float cooldownMult = 1f;
        public float manaMult = 1f;
        public float lifetimeMult = 1f;
        public float rangeMult = 1f;
        public float radiusMult = 1f;

        [Header("Additions (0 = no change)")]
        public float addHoming;
        public float addSpread;
        public int addProjectiles;
        public int addPierce;
        public int addBounces;
        public int addSplit;
        public int addChain;

        [Header("Extra on-hit effects")]
        public List<OnHitEffectSO> addOnHit = new List<OnHitEffectSO>();

        public override void Apply(SpellStats s, List<OnHitEffectSO> onHit)
        {
            s.damage *= damageMult;
            s.speed *= speedMult;
            s.size *= sizeMult;
            s.cooldown *= cooldownMult;
            s.manaCost *= manaMult;
            s.lifetime *= lifetimeMult;
            s.range *= rangeMult;
            s.radius *= radiusMult;

            s.homing += addHoming;
            s.spread += addSpread;
            s.projectileCount += addProjectiles;
            s.pierce += addPierce;
            s.bounces += addBounces;
            s.splitCount += addSplit;
            s.chainCount += addChain;

            foreach (var fx in addOnHit)
                if (fx != null && !onHit.Contains(fx)) onHit.Add(fx);
        }
    }
}
