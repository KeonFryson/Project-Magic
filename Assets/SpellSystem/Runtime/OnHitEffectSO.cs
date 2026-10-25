using UnityEngine;

namespace SpellSystem
{
    /// Something that happens when a spell hits a target. Subclass this to make your own.
    public abstract class OnHitEffectSO : ScriptableObject
    {
        public abstract void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 direction);
    }
}
