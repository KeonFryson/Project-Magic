using UnityEngine;

namespace SpellSystem
{
    [CreateAssetMenu(menuName = "Spells/Status Effect", fileName = "NewStatus")]
    public class StatusEffectSO : ScriptableObject
    {
        [Tooltip("Name other spells can react to (see Element > Interactions), e.g. Burning, Chilled.")]
        public string statusTag = "Burning";
        public StatusKind kind = StatusKind.DamageOverTime;
        public float duration = 3f;
        [Tooltip("DamageOverTime: damage per tick. Slow: fraction of speed removed (0.5 = half speed).")]
        public float magnitude = 4f;
        public float tickInterval = 0.5f;
        public ElementSO element;
    }
}
