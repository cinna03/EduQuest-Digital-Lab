using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest
{
    /// <summary>
    /// Simple living-room-floor combat: tap enemies to defeat them.
    /// Higher levels = more HP; from level 2 they damage the player.
    /// Arena stays empty during hunt phases (caller despawns).
    /// </summary>
    public class CombatWave : MonoBehaviour
    {
        public event Action WaveCleared;
        public event Action PlayerDefeated;

        readonly List<EnemyDummy> m_Enemies = new();
        Camera m_Cam;
        int m_Level = 1;
        float m_PlayerHp = 100f;
        float m_AttackTimer;
        bool m_Active;

        public bool IsActive => m_Active;
        public float PlayerHp => m_PlayerHp;
        public int EnemiesAlive
        {
            get
            {
                var n = 0;
                foreach (var e in m_Enemies)
                    if (e != null && e.Alive) n++;
                return n;
            }
        }

        public void Configure(Camera cam) => m_Cam = cam != null ? cam : Camera.main;

        public void BeginWave(int level, Vector3 groundCenter)
        {
            ClearWave();
            m_Level = Mathf.Clamp(level, 1, 2);
            m_PlayerHp = 100f;
            m_AttackTimer = 0f;
            m_Active = true;

            int count = m_Level == 1 ? 2 : 3;
            float hp = m_Level == 1 ? 2f : 4f;

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                var pos = groundCenter + new Vector3(Mathf.Cos(angle) * 0.35f, 0.12f, Mathf.Sin(angle) * 0.25f);
                m_Enemies.Add(EnemyDummy.Spawn(transform, pos, hp, m_Level));
            }
        }

        public void ClearWave()
        {
            m_Active = false;
            foreach (var e in m_Enemies)
            {
                if (e != null) Destroy(e.gameObject);
            }
            m_Enemies.Clear();
        }

        void Update()
        {
            if (!m_Active) return;

            HandleTap();

            // Level 2+: enemies attack the player on a timer
            if (m_Level >= 2 && EnemiesAlive > 0)
            {
                m_AttackTimer += Time.deltaTime;
                if (m_AttackTimer >= 1.6f)
                {
                    m_AttackTimer = 0f;
                    m_PlayerHp -= 8f * EnemiesAlive;
                    if (m_PlayerHp <= 0f)
                    {
                        m_PlayerHp = 0f;
                        m_Active = false;
                        PlayerDefeated?.Invoke();
                    }
                }
            }

            if (m_Active && EnemiesAlive == 0 && m_Enemies.Count > 0)
            {
                m_Active = false;
                WaveCleared?.Invoke();
            }
        }

        void HandleTap()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (m_Cam == null) m_Cam = Camera.main;
            if (m_Cam == null) return;

            var ray = m_Cam.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 40f)) return;

            var enemy = hit.collider.GetComponentInParent<EnemyDummy>();
            if (enemy == null || !enemy.Alive) return;

            enemy.TakeHit(1f);
        }

        class EnemyDummy : MonoBehaviour
        {
            public bool Alive = true;
            float m_Hp;
            int m_Level;
            Renderer m_Renderer;

            public static EnemyDummy Spawn(Transform parent, Vector3 worldPos, float hp, int level)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = level >= 2 ? "Enemy_Aggressive" : "Enemy_Training";
                go.transform.SetParent(parent, true);
                go.transform.position = worldPos;
                go.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);

                var e = go.AddComponent<EnemyDummy>();
                e.m_Hp = hp;
                e.m_Level = level;
                e.m_Renderer = go.GetComponent<Renderer>();
                e.ApplyColor();

                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 0.5f;
                light.intensity = 1.2f;
                light.color = level >= 2 ? new Color(1f, 0.35f, 0.25f) : new Color(0.4f, 0.8f, 1f);
                return e;
            }

            public void TakeHit(float dmg)
            {
                if (!Alive) return;
                m_Hp -= dmg;
                transform.localScale *= 0.92f;
                if (m_Hp <= 0f)
                {
                    Alive = false;
                    gameObject.SetActive(false);
                }
                else ApplyColor();
            }

            void ApplyColor()
            {
                if (m_Renderer == null) return;
                var c = m_Level >= 2
                    ? Color.Lerp(new Color(1f, 0.2f, 0.15f), new Color(1f, 0.7f, 0.2f), m_Hp / 4f)
                    : Color.Lerp(new Color(0.2f, 0.4f, 0.9f), new Color(0.5f, 0.9f, 1f), m_Hp / 2f);
                m_Renderer.sharedMaterial = LabMaterials.Solid(c, 0.4f);
            }
        }
    }
}
