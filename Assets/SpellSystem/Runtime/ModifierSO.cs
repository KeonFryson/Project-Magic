using System.Collections.Generic;
using UnityEngine;

namespace SpellSystem
{
    /// A piece you slot into a spell. Subclass for fully custom behaviour,
    /// or just use StatModifierSO for anything that is "change some numbers / add effects".
    public abstract class ModifierSO : ScriptableObject
    {
        [TextArea] public string description;
        public abstract void Apply(SpellStats stats, List<OnHitEffectSO> onHit);
    }
}
