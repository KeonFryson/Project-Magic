using UnityEngine;

namespace SpellSystem
{
    /// Test dummy that walks toward the player's SpellCaster. Respects slow, stun and knockback.
    [RequireComponent(typeof(Health))]
    public class SimpleEnemy : MonoBehaviour
    {
        public float speed = 2.5f;
        public float stopDistance = 0.8f;

        Transform target;
        StatusReceiver status;

        void Start()
        {
            status = GetComponent<StatusReceiver>();
            if (status == null) status = gameObject.AddComponent<StatusReceiver>();
#if UNITY_2023_1_OR_NEWER
            var c = FindFirstObjectByType<SpellCaster>();
#else
            var c = FindObjectOfType<SpellCaster>();
#endif
            if (c != null) target = c.transform;
        }

        void Update()
        {
            Vector2 v = status.KnockVelocity;
            if (target != null && !status.Stunned)
            {
                Vector2 to = (Vector2)target.position - (Vector2)transform.position;
                if (to.magnitude > stopDistance) v += to.normalized * speed * status.SpeedMultiplier;
            }
            transform.position += (Vector3)(v * Time.deltaTime);
        }
    }
}
