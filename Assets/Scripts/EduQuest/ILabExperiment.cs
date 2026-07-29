using UnityEngine;

namespace EduQuest
{
    public interface ILabExperiment
    {
        string Title { get; }
        string Prompt { get; }
        string Status { get; }
        GameObject Root { get; }
        void Enter();
        void Exit();
        void ResetExperiment();
    }
}
