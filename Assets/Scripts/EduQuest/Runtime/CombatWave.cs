using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest
{
    /// <summary>
    /// Living-room-floor combat using Quaternius enemy meshes.
    /// Level 1: Frog/Rat (no attack). Level 2: Spider/Snake/Wasp (attack player).
    /// </summary>
    public class CombatWave : MonoBehaviour
    {
        public event Action WaveCleared;
        public event Action PlayerDefeated;

        readonly List<EnemyActor> m_Enemies = new();
        Camera m_Cam;
        int m_Level = 1;
        float m_PlayerHp = 100f;
        float m_AttackTimer;
        bool m_Active;
        bool m_ClearedFired;

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
            m_ClearedFired = false;

            int count = m_Level == 1 ? 2 : 3;
            float hp = m_Level == 1 ? 2f : 4f;

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f + 0.4f;
                var pos = groundCenter + new Vector3(Mathf.Cos(angle) * 0.38f, 0f, Mathf.Sin(angle) * 0.28f);
                m_Enemies.Add(EnemyActor.Spawn(transform, pos, hp, m_Level));
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

            if (!m_ClearedFired && m_Enemies.Count > 0 && EnemiesAlive == 0)
            {
                m_ClearedFired = true;
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

            var enemy = hit.collider.GetComponentInParent<EnemyActor>();
            if (enemy == null || !enemy.Alive) return;
            enemy.TakeHit(1f);
        }
    }
}
