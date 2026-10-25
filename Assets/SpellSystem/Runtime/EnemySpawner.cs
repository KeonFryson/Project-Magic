using UnityEngine;

namespace SpellSystem
{
    /// Keeps a few test enemies alive around a center object. template should be an inactive scene object.
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject template;
        public Transform center;
        public int maxAlive = 8;
        public float radius = 8f;
        public float interval = 0.8f;

        int alive;
        float timer;

        void Update()
        {
            if (template == null || center == null) return;
            timer -= Time.deltaTime;
            if (timer > 0f || alive >= maxAlive) return;
            timer = interval;

            Vector2 offset = Random.insideUnitCircle.normalized * radius;
            var go = Instantiate(template, center.position + (Vector3)offset, Quaternion.identity);
            go.SetActive(true);
            alive++;
            var h = go.GetComponent<Health>();
            if (h != null) h.Died += _ => alive--;
        }
    }
}
