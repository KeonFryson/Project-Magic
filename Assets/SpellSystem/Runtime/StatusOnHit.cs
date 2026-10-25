using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/On-Hit/Apply Status", fileName = "ApplyStatus")]
    public class StatusOnHit : OnHitEffectSO
    {
        public StatusEffectSO status;
        [Range(0f, 1f)] public float chance = 1f;

        public override void Apply(SpellContext ctx, Collider2D target, Vector2 point, Vector2 direction)
        {
            if (status == null || Random.value > chance) return;
            var r = SpellUtil.GetReceiver(target, true);
            if (r != null) r.Apply(status, ctx.caster.gameObject);
        }
    }
}
