using System.Collections;
using UnityEngine;

namespace SpellSystem
{
    /// HOW a spell reaches its target (projectile, beam, slash...). Subclass to add new shapes.
    public abstract class DeliverySO : ScriptableObject
    {
        public SpellStats baseStats = new SpellStats();

        /// How long the caster is "busy" with this spell (beams and orbits last a while).
        public virtual float Duration(SpellStats s) { return 0f; }

        public abstract IEnumerator Cast(SpellContext ctx);
    }
}
