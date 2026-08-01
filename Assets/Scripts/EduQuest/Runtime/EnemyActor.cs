using UnityEngine;

namespace EduQuest
{
    /// <summary>Tap-to-damage enemy shell around an imported Quaternius mesh.</summary>
    public class EnemyActor : MonoBehaviour
    {
        public bool Alive { get; private set; } = true;

        float m_Hp;
        int m_Level;
        Animation m_Anim;

        public static EnemyActor Spawn(Transform parent, Vector3 worldPos, float hp, int level)
        {
            var prefab = EnemyCatalog.PrefabForLevel(level);
            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, parent);
                go.name = level >= 2 ? $"Enemy_{prefab.name}_Aggressive" : $"Enemy_{prefab.name}_Training";
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = level >= 2 ? "Enemy_Aggressive_Fallback" : "Enemy_Training_Fallback";
                go.transform.SetParent(parent, false);
                var r = go.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial = LabMaterials.Solid(
                        level >= 2 ? new Color(1f, 0.25f, 0.2f) : new Color(0.35f, 0.75f, 1f), 0.4f);
            }

            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            go.transform.localScale = Vector3.one * EnemyCatalog.ScaleFor(go.name, level);
            // Plant feet on the shared living-room ground plane
            go.transform.position = worldPos;
            SitOnGround(go, worldPos.y);

            EnsureClickCollider(go);

            var actor = go.GetComponent<EnemyActor>() ?? go.AddComponent<EnemyActor>();
            actor.m_Hp = hp;
            actor.m_Level = level;
            actor.Alive = true;
            actor.m_Anim = go.GetComponentInChildren<Animation>();
            actor.PlayClip(level >= 2 ? "Attack" : "Idle", fallbackContains: level >= 2 ? "attack" : "idle");
            if (actor.m_Anim == null)
                actor.PlayClip("Walk", fallbackContains: "walk");

            // Soft tint light so they read on a dark table
            if (go.GetComponent<Light>() == null)
            {
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 0.55f;
                light.intensity = 1.1f;
                light.color = level >= 2 ? new Color(1f, 0.35f, 0.25f) : new Color(0.45f, 0.85f, 1f);
            }

            return actor;
        }

        public void TakeHit(float dmg)
        {
            if (!Alive) return;
            m_Hp -= dmg;
            transform.localScale *= 0.94f;
            PlayClip("Jump", fallbackContains: "jump");

            if (m_Hp <= 0f)
            {
                Alive = false;
                PlayClip("Death", fallbackContains: "death");
                // Hide after a short beat so death anim can flash
                Destroy(gameObject, 0.45f);
            }
        }

        void PlayClip(string preferred, string fallbackContains)
        {
            if (m_Anim == null) return;
            if (m_Anim.GetClip(preferred) != null)
            {
                m_Anim.Play(preferred);
                return;
            }

            foreach (AnimationState st in m_Anim)
            {
                if (st == null || st.clip == null) continue;
                if (st.clip.name.ToLowerInvariant().Contains(fallbackContains))
                {
                    m_Anim.Play(st.clip.name);
                    return;
                }
            }

            // Play whatever first clip exists
            foreach (AnimationState st in m_Anim)
            {
                if (st != null && st.clip != null)
                {
                    m_Anim.Play(st.clip.name);
                    return;
                }
            }
        }

        static void EnsureClickCollider(GameObject go)
        {
            // Prefer a single root box so taps are reliable even with many mesh colliders
            foreach (var c in go.GetComponentsInChildren<Collider>())
                Destroy(c);
            var box = go.AddComponent<BoxCollider>();
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                box.center = go.transform.InverseTransformPoint(b.center);
                var size = go.transform.InverseTransformVector(b.size);
                box.size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            }
            else
            {
                box.center = new Vector3(0f, 0.08f, 0f);
                box.size = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }

        static void SitOnGround(GameObject go, float groundY)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return;
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            var delta = groundY - b.min.y;
            go.transform.position += Vector3.up * delta;
        }
    }

    /// <summary>Maps campaign levels to Quaternius CC0 enemy meshes in Resources.</summary>
    public static class EnemyCatalog
    {
        static readonly string[] Level1 = { "EduQuest/Enemies/Frog", "EduQuest/Enemies/Rat" };
        static readonly string[] Level2 = { "EduQuest/Enemies/Spider", "EduQuest/Enemies/Snake_angry", "EduQuest/Enemies/Wasp" };

        public static GameObject PrefabForLevel(int level)
        {
            var paths = level >= 2 ? Level2 : Level1;
            // Pick a random available mesh so waves feel varied
            for (int attempt = 0; attempt < paths.Length; attempt++)
            {
                var path = paths[Random.Range(0, paths.Length)];
                var go = Resources.Load<GameObject>(path);
                if (go != null) return go;
            }

            foreach (var path in paths)
            {
                var go = Resources.Load<GameObject>(path);
                if (go != null) return go;
            }

            Debug.LogWarning("[EduQuest] Enemy FBX missing in Resources/EduQuest/Enemies — using capsule fallback.");
            return null;
        }

        public static float ScaleFor(string name, int level)
        {
            // Quaternius FBX are authored large; shrink to table-top AR size
            var n = (name ?? "").ToLowerInvariant();
            if (n.Contains("spider")) return 0.045f;
            if (n.Contains("wasp")) return 0.05f;
            if (n.Contains("snake")) return 0.04f;
            if (n.Contains("frog")) return 0.055f;
            if (n.Contains("rat")) return 0.05f;
            return level >= 2 ? 0.14f : 0.12f; // capsule fallback
        }
    }
}
