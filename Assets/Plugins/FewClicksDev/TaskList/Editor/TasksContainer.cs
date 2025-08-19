namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class TasksContainer
    {
        [SerializeField] private List<Task> tasks = new List<Task>();

        public List<Task> Items => tasks;

        public void AddTask(Task _task)
        {
            tasks.Add(_task);
        }

        public void RemoveTask(Task _task)
        {
            if (tasks.Contains(_task))
            {
                tasks.Remove(_task);
            }
        }

        public string ConvertToJson()
        {
            if (tasks.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return JsonUtility.ToJson(this);
        }
    }
}
