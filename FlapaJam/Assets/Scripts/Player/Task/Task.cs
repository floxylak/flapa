using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class Task
    {
        [SerializeField] private string name;
        [SerializeField] private string description;
        [SerializeField] private int initialCount;
        [SerializeField] private int currentCount;
        [SerializeField] private bool isCompleted;
        [SerializeField] private bool isMandatory;
        [SerializeField] private string targetObjectTag;

        public Task(string name, string description, int count = 1, bool isMandatory = true, string targetObjectTag = "")
        {
            this.name = name;
            this.description = description;
            this.initialCount = count;
            this.currentCount = count;
            isCompleted = false;
            this.isMandatory = isMandatory;
            this.targetObjectTag = targetObjectTag;
        }

        public string Name => name;
        public string Description => description;
        public bool IsCompleted => currentCount <= 0 || isCompleted;
        public bool IsMandatory => isMandatory;
        public string TargetObjectTag => targetObjectTag;

        public void CompleteTask()
        {
            isCompleted = true;
            currentCount = 0;
            Debug.Log($"Task '{name}' completed!");
        }
        
        public void UpdateCount()
        {
            if (currentCount > 0)
            {
                currentCount--;
                Debug.Log($"Task '{name}' progressed! Remaining: {currentCount}/{initialCount}");
                if (currentCount <= 0)
                {
                    CompleteTask();
                }
            }
        }
        
        public int GetCount() => initialCount;

        public bool MatchesTarget(GameObject target)
        {
            return !string.IsNullOrEmpty(targetObjectTag) && target.CompareTag(targetObjectTag);
        }
    }
}