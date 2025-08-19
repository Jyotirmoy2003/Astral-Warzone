namespace FewClicksDev.TaskList
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class TaskListGenericMenu
    {
        private static readonly GUIContent DELETE_CONTENT = new GUIContent("Delete", "Delete selected tasks from the list.");
        private static readonly GUIContent MOVE_TO_CURRENT = new GUIContent("Move to Current", "Move tasks to the current task list.");
        private static readonly GUIContent MOVE_TO_BACKLOG = new GUIContent("Move to Backlog", "Move tasks to the backlog task list.");
        private static readonly GUIContent SET_AS_CLOSED = new GUIContent("Set as Closed/True", "Set the selected tasks as closed.");
        private static readonly GUIContent SET_AS_OPEN = new GUIContent("Set as Closed/False", "Set the selected tasks as open.");
        private static readonly GUIContent CHANGE_PRIORITY_IMMEDIATE = new GUIContent("Change Priority/Immediate", "Change the priority of the selected tasks to 'Immediate'.");
        private static readonly GUIContent CHANGE_PRIORITY_HIGH = new GUIContent("Change Priority/High", "Change the priority of the selected tasks to 'High'.");
        private static readonly GUIContent CHANGE_PRIORITY_NORMAL = new GUIContent("Change Priority/Normal", "Change the priority of the selected tasks to 'Normal'.");
        private static readonly GUIContent CHANGE_PRIORITY_LOW = new GUIContent("Change Priority/Low", "Change the priority of the selected tasks to 'Low'.");

        public static void ShowForTasks(TaskListWindow _window, Event _currentEvent, List<Task> _selectedTasks, TaskListType _listType)
        {
            GenericMenu _menu = new GenericMenu();

            string _label = _selectedTasks.Count == 1 ? _selectedTasks[0].Label : $"Selected Tasks ({_selectedTasks.Count})";

            _menu.AddDisabledItem(new GUIContent(_label));
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(DELETE_CONTENT, false, _delete);

            switch (_listType)
            {
                case TaskListType.Current:
                    _menu.AddItem(MOVE_TO_BACKLOG, false, _moveToBacklog);
                    break;

                case TaskListType.Backlog:
                    _menu.AddItem(MOVE_TO_CURRENT, false, _moveToCurrent);
                    break;
            }

            _menu.AddSeparator(string.Empty);
            _menu.AddItem(SET_AS_CLOSED, false, () => _setAsClosed(true));
            _menu.AddItem(SET_AS_OPEN, false, () => _setAsClosed(false));
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(CHANGE_PRIORITY_IMMEDIATE, false, () => _changePriority(TaskPriority.Immediate));
            _menu.AddItem(CHANGE_PRIORITY_HIGH, false, () => _changePriority(TaskPriority.High));
            _menu.AddItem(CHANGE_PRIORITY_NORMAL, false, () => _changePriority(TaskPriority.Normal));
            _menu.AddItem(CHANGE_PRIORITY_LOW, false, () => _changePriority(TaskPriority.Low));
            _menu.ShowAsContext();

            _currentEvent.Use();

            void _delete()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.RemoveTask(_task, _listType);
                }

                TaskListUserPreferences.SavePreferences();
                _window.RefreshTaskLists();
            }

            void _moveToCurrent()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.MoveBacklogTaskToCurrent(_task);
                }

                TaskListUserPreferences.SavePreferences();
                _window.RefreshTaskLists();
            }

            void _moveToBacklog()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.MoveTaskToBacklog(_task);
                }

                TaskListUserPreferences.SavePreferences();
                _window.RefreshTaskLists();
            }

            void _setAsClosed(bool _closed)
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetAsClosed(_closed);
                }
            }

            void _changePriority(TaskPriority _newPriority)
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }
                    _task.SetPriority(_newPriority);
                }
            }
        }
    }
}
