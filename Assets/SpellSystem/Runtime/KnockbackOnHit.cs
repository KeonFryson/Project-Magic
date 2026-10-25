using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/On-Hit/Knockback", fileName = "Knockback")]
    public class KnockbackOnHit : OnHitEffectSO
    {
        public float force = 8f;

        public override void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            var impulse = direction.normalized * force;
            var r = SpellUtil.GetReceiver(target, true);
            if (r != null) r.Knockback(impulse);
            var rb = target.attachedRigidbody;
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) rb.AddForce(impulse * rb.mass, ForceMode2D.Impulse);
        }
    }
}
