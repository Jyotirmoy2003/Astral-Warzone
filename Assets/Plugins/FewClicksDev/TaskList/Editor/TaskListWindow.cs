namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;
    using Preferences = TaskListUserPreferences;

    public class TaskListWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            CurrentTasks = 0,
            Backlog = 1,
            Settings = 2
        }

        private const float TOOLBAR_WIDTH = 0.8f;
        private const float LABEL_WIDTH = 140f;
        private const float SETTINGS_LABEL_WIDTH = 220f;
        private const float DATE_WIDTH = 160f;
        private const float INDEX_WIDTH = 30f;
        private const float COLOR_INDICATOR_WIDTH = 8f;

        public const int MAX_VISIBLE_TASKS = 20;
        private static readonly Color SELECT_COLOR = new Color(0.65f, 0.65f, 0.65f, 1f);
        private static readonly GUIContent REMOVE_CONTEXT_CONTENT = new GUIContent(" X ", "Remove the context from the task.");

        private const string VISIBILITY = "Visibility";
        private const string SORT_BY = "Sort by";
        private const string FILTER_BY_LABEL = "Filter by label";
        private const string FILTER_BY_DESCRIPTION = "Filter by description";
        private const string RESET_TO_DEFAULTS = "Reset to defaults";
        private const string CLOSED_STRING = "  <b><i>(Closed)</i></b>";
        private const string SELECT_ALL = "Select all";
        private const string COLLAPSE_ALL = "Collapse all";
        private const string MOVE_ALL_TO_BACKLOG = "Move all to backlog";
        private const string MOVE_ALL_TO_CURRENT = "Move all to current";
        private const string CREATE_NEW_TASK = "Create a new Task";
        private const string COLORS = "Colors";
        private const string OTHERS = "Others";
        private const string OBJECT = "Object";
        private const string SCRIPT = "Script";
        private const string PING = "Ping";
        private const string OPEN = "Open";
        private const string CAMERA_POSITION = "Camera position";
        private const string CAMERA_ROTATION = "Camera rotation";
        private const string MAIN_SCENE = "Main scene";
        private const string SET = "Set";
        private const string UPDATE_SCENES_AND_LOCATION = "Update scenes and location";
        private const string LOAD_CONTEXT_SCENES = "Load context scenes";
        private const string ADD_CONTEXT = "Add context";
        private const string TYPE = "Type";
        private const string CONTEXTS = "Contexts";
        private const string LABEL = "Label";
        private const string DESCRIPTION = "Description";
        private const string PRIORITY = "Priority";
        private const string NO_TASKS_INFO = "No tasks to display. Create a new task using the button above or change the visibility options!";

        protected override string windowName => "Task List";
        protected override string version => TaskList.VERSION;
        protected override Vector2 minWindowSize => new Vector2(600f, 740f);
        protected override Color mainColor => TaskList.MAIN_COLOR;

        protected override bool askForReview => true;
        protected override string reviewURL => "https://assetstore.unity.com/packages/slug/281097";
        protected override bool hasDocumentation => true;
        protected override string documentationURL => "https://docs.google.com/document/d/1npJsJKl5jSMF94ZkjN-bsgJVx6uJN1ErUE8MI-fmixI/edit?usp=sharing";

        public GUIStyle SingleLineLabelStyle
        {
            get
            {
                if (singleLineLabelStyle == null)
                {
                    singleLineLabelStyle = Styles.CustomizedButton(DEFAULT_LINE_HEIGHT, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));
                    singleLineLabelStyle.richText = true;
                }

                return singleLineLabelStyle;
            }
        }

        public GUIStyle SingleLineButtonStyle
        {
            get
            {
                if (singleLineButtonStyle == null)
                {
                    singleLineButtonStyle = Styles.CustomizedButton(DEFAULT_LINE_HEIGHT, TextAnchor.MiddleLeft, new RectOffset(5, 5, 0, 0));
                    singleLineButtonStyle.richText = true;
                }

                return singleLineButtonStyle;
            }
        }

        private GUIStyle singleLineLabelStyle = null;
        private GUIStyle singleLineButtonStyle = null;

        private WindowMode windowMode = WindowMode.CurrentTasks;

        private string tasksLabelFilter = string.Empty;
        private string tasksDescriptionFilter = string.Empty;
        private TaskSelectableList sortedAndFilteredTasks = null;

        private string backlogTasksLabelFilter = string.Empty;
        private string backlogTasksDescriptionFilter = string.Empty;
        private TaskSelectableList sortedAndFilteredBacklogTasks = null;

        private Rect collidersRect = Rect.zero;

        protected override void OnEnable()
        {
            base.OnEnable();

            Preferences.OnTasksUpdated -= generateTasksLists;
            Preferences.OnTasksUpdated += generateTasksLists;
            Preferences.LoadPreferences();

            CompilationPipeline.compilationStarted -= onCompilationStarted;
            CompilationPipeline.compilationStarted += onCompilationStarted;

            singleLineLabelStyle = null;
            singleLineButtonStyle = null;
        }

        private void OnDestroy()
        {
            Preferences.SavePreferences();
            Preferences.OnTasksUpdated -= generateTasksLists;
            CompilationPipeline.compilationStarted -= onCompilationStarted;
        }

        protected override void drawWindowGUI()
        {
            NormalSpace();
            windowMode = this.DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);
            SmallSpace();
            DrawLine();
            SmallSpace();

            switch (windowMode)
            {
                case WindowMode.CurrentTasks:
                    drawCurrentTasksList();
                    break;

                case WindowMode.Backlog:
                    drawBacklogTasks();
                    break;

                case WindowMode.Settings:
                    drawSettings();
                    break;
            }
        }

        public void RefreshTaskLists()
        {
            generateTasksLists();
        }

        private void generateTasksLists()
        {
            if (sortedAndFilteredTasks == null)
            {
                sortedAndFilteredTasks = new TaskSelectableList();
            }

            if (sortedAndFilteredBacklogTasks == null)
            {
                sortedAndFilteredBacklogTasks = new TaskSelectableList();
            }

            sortAndFilterTasks(TaskListType.Current);
            sortAndFilterTasks(TaskListType.Backlog);
        }

        private void drawCurrentTasksList()
        {
            using (new LabelWidthScope(LABEL_WIDTH))
            {
                using (var _changeCheck = new ChangeCheckScope())
                {
                    Preferences.VisibilityMode = (TasksVisibilityMode) EditorGUILayout.EnumFlagsField(VISIBILITY, Preferences.VisibilityMode);
                    Preferences.SortMode = DrawEnumWithOrder(Preferences.SortMode, SORT_BY, ref Preferences.SortOrder, sumOfPaddings);

                    SmallSpace();
                    tasksLabelFilter = EditorGUILayout.TextField(FILTER_BY_LABEL, tasksLabelFilter);
                    tasksDescriptionFilter = EditorGUILayout.TextField(FILTER_BY_DESCRIPTION, tasksDescriptionFilter);

                    if (_changeCheck.changed)
                    {
                        sortAndFilterTasks(TaskListType.Current);
                    }
                }
            }

            SmallSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(SELECT_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredTasks.SelectAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(COLLAPSE_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredTasks.CollapseAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(MOVE_ALL_TO_BACKLOG, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    foreach (var _task in sortedAndFilteredTasks.Items)
                    {
                        Preferences.MoveTaskToBacklog(_task);
                    }

                    sortAndFilterTasks(TaskListType.Current);
                    sortAndFilterTasks(TaskListType.Backlog);
                    Preferences.SavePreferences();
                    return;
                }
            }

            SmallSpace();
            DrawLine();
            SmallSpace();
            drawCreateNewTask(TaskListType.Current);
            drawTasksList(sortedAndFilteredTasks);
        }

        private void drawBacklogTasks()
        {
            using (new LabelWidthScope(LABEL_WIDTH))
            {
                using (var _changeCheck = new ChangeCheckScope())
                {
                    Preferences.BacklogVisibilityMode = (TasksVisibilityMode) EditorGUILayout.EnumFlagsField(VISIBILITY, Preferences.BacklogVisibilityMode);
                    Preferences.BacklogSortMode = DrawEnumWithOrder(Preferences.BacklogSortMode, SORT_BY, ref Preferences.BacklogSortOrder, sumOfPaddings);

                    SmallSpace();
                    backlogTasksLabelFilter = EditorGUILayout.TextField(FILTER_BY_LABEL, backlogTasksLabelFilter);
                    backlogTasksDescriptionFilter = EditorGUILayout.TextField(FILTER_BY_DESCRIPTION, backlogTasksDescriptionFilter);

                    if (_changeCheck.changed)
                    {
                        sortAndFilterTasks(TaskListType.Backlog);
                    }
                }
            }

            SmallSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(SELECT_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredBacklogTasks.SelectAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(COLLAPSE_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredBacklogTasks.CollapseAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(MOVE_ALL_TO_CURRENT, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    foreach (var _task in sortedAndFilteredBacklogTasks.Items)
                    {
                        Preferences.MoveBacklogTaskToCurrent(_task);
                    }

                    sortAndFilterTasks(TaskListType.Backlog);
                    sortAndFilterTasks(TaskListType.Current);
                    Preferences.SavePreferences();
                    return;
                }
            }

            SmallSpace();
            DrawLine();
            SmallSpace();
            drawCreateNewTask(TaskListType.Backlog);
            drawTasksList(sortedAndFilteredBacklogTasks);
        }

        private void drawCreateNewTask(TaskListType _taskList)
        {
            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * 0.75f;

                if (DrawClearBoxButton(CREATE_NEW_TASK, TaskList.MAIN_COLOR, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    createNewTask(_taskList);
                }

                FlexibleSpace();
            }
        }

        private void createNewTask(TaskListType _list)
        {
            int _totalTasks = sortedAndFilteredTasks.Count + sortedAndFilteredBacklogTasks.Count;

            Task _task = new Task(_list, _totalTasks);
            Preferences.AddTask(_task, _list);

            switch (_list)
            {
                case TaskListType.Current:
                    sortAndFilterTasks(TaskListType.Current);
                    break;

                case TaskListType.Backlog:
                    sortAndFilterTasks(TaskListType.Backlog);
                    break;
            }
        }

        private void sortAndFilterTasks(TaskListType _list)
        {
            switch (_list)
            {
                case TaskListType.Current:
                    sortAndFilterTasks(sortedAndFilteredTasks, Preferences.Tasks.Items, TaskListType.Current);
                    break;

                case TaskListType.Backlog:
                    sortAndFilterTasks(sortedAndFilteredBacklogTasks, Preferences.BacklogTasks.Items, TaskListType.Backlog);
                    break;
            }
        }

        private void sortAndFilterTasks(TaskSelectableList _taskList, List<Task> _tasks, TaskListType _list)
        {
            if (_taskList == null)
            {
                return;
            }

            _taskList.Destroy();
            string _labelSearchFilter = _getLabelSearchFilter();
            string _descriptionSearchFilter = _getDescriptionSearchFilter();

            var _newTasks = new List<Task>();

            //First we filter by name to decrease the number of groups to check
            foreach (var _task in _tasks)
            {
                if (_labelSearchFilter.IsNullEmptyOrWhitespace() && _descriptionSearchFilter.IsNullEmptyOrWhitespace())
                {
                    _newTasks.Add(_task);
                    continue;
                }

                string _taskNameToLower = _task.Label.ToLower().Trim();

                if (_labelSearchFilter.IsNullEmptyOrWhitespace() == false && _taskNameToLower.Contains(_labelSearchFilter))
                {
                    _newTasks.Add(_task);
                    continue;
                }

                string _taskDescriptionToLower = _task.Description.ToLower().Trim();

                if (_descriptionSearchFilter.IsNullEmptyOrWhitespace() == false && _taskDescriptionToLower.Contains(_descriptionSearchFilter))
                {
                    _newTasks.Add(_task);
                    continue;
                }
            }

            //Then we filter by visibility
            for (int i = _newTasks.Count - 1; i >= 0; i--)
            {
                if (_newTasks[i].ShouldBeVisible(_getVisibilityMode()) == false)
                {
                    _newTasks.RemoveAt(i);
                    continue;
                }
            }

            //Then we sort
            switch (_getSortMode())
            {
                case TasksSortMode.CreationDate:
                    _newTasks.Sort((x, y) => x.CreationTimeInTicks.CompareTo(y.CreationTimeInTicks));
                    break;

                case TasksSortMode.Priority:
                    _newTasks.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                    break;

                case TasksSortMode.Label:
                    _newTasks.Sort((x, y) => string.Compare(x.Label, y.Label, System.StringComparison.OrdinalIgnoreCase));
                    break;
            }

            if (_getSortOrder() == false)
            {
                _newTasks.Reverse();
            }

            _taskList.Init(_newTasks, DEFAULT_LINE_HEIGHT);

            string _getLabelSearchFilter()
            {
                return _list switch
                {
                    TaskListType.Current => tasksLabelFilter.ToLower().Trim(),
                    TaskListType.Backlog => backlogTasksLabelFilter.ToLower().Trim(),
                    _ => string.Empty
                };
            }

            string _getDescriptionSearchFilter()
            {
                return _list switch
                {
                    TaskListType.Current => tasksDescriptionFilter.ToLower().Trim(),
                    TaskListType.Backlog => backlogTasksDescriptionFilter.ToLower().Trim(),
                    _ => string.Empty
                };
            }

            TasksVisibilityMode _getVisibilityMode()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.VisibilityMode,
                    TaskListType.Backlog => Preferences.BacklogVisibilityMode,
                    _ => TasksVisibilityMode.All
                };
            }

            TasksSortMode _getSortMode()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.SortMode,
                    TaskListType.Backlog => Preferences.BacklogSortMode,
                    _ => TasksSortMode.CreationDate
                };
            }

            bool _getSortOrder()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.SortOrder,
                    TaskListType.Backlog => Preferences.BacklogSortOrder,
                    _ => true
                };
            }
        }

        private void drawTasksList(TaskSelectableList _list)
        {
            if (_list == null)
            {
                return;
            }

            NormalSpace();

            if (_list.Items.IsNullOrEmpty())
            {
                EditorGUILayout.HelpBox(NO_TASKS_INFO, MessageType.Info);
                return;
            }

            float _width = windowWidthWithPaddings - 3f;
            int _visibleEntries = MAX_VISIBLE_TASKS + getExtraLines(DEFAULT_LINE_HEIGHT);
            int _numberOfEntries = _list.Count > _visibleEntries ? _visibleEntries : _list.Count;
            float _height = DEFAULT_LINE_HEIGHT * _visibleEntries;
            bool _visibleScroll = _list.TotalElementsHeight > _height;

            Event _currentEvent = Event.current;

            using (var _scrollScope = new ScrollViewScope(_list.ScrollPosition, false, _visibleScroll, FixedWidth(_width)))
            {
                _list.ScrollPosition = _scrollScope.scrollPosition;
                int _index = 0;

                foreach (var _task in _list.Items)
                {
                    if (drawSingleTask(_currentEvent, _list, _task, _index, _height, _visibleScroll))
                    {
                        return;
                    }

                    _index++;
                }
            }

            Rect _lastRect = GetLastRect();
            Rect _finalRect = new Rect(leftPadding, _lastRect.y, _lastRect.width, _numberOfEntries * DEFAULT_LINE_HEIGHT);

            if (_finalRect.width != 0)
            {
                collidersRect = _finalRect;
            }

            bool _mouseInAnyRect = collidersRect.Contains(_currentEvent.mousePosition);

            if (_currentEvent.type is EventType.MouseDown && _currentEvent.button == 0 && _mouseInAnyRect == false)
            {
                _list.UnselectAll();
                _list.ClearFirstSelected();
                Repaint();
            }
        }

        private int getExtraLines(float _singleLineHeight)
        {
            float _heightDifference = windowHeight - minWindowSize.y;
            return Mathf.FloorToInt(_heightDifference / _singleLineHeight);
        }

        private bool drawSingleTask(Event _event, TaskSelectableList _list, Task _task, int _index, float _visibleAreaHeight, bool _visibleSlider)
        {
            if (_task == null)
            {
                return false;
            }

            bool _visible = _list.IsVisible(_task, _visibleAreaHeight);

            if (_visible == false)
            {
                EditorGUILayout.LabelField(string.Empty, FixedHeight(_task.StartPositionAndHeight.y));
                return false;
            }

            using (ColorScope.Background(_task.IsSelected ? SELECT_COLOR : Color.white))
            {
                using (new HorizontalScope())
                {
                    GUILayout.Label($"{_index + 1}", SingleLineLabelStyle, FixedWidth(INDEX_WIDTH));

                    using (ColorScope.BackgroundAndContent(_task.IndicatorColor))
                    {
                        GUILayout.Box(string.Empty, Styles.ClearBox, FixedWidthAndHeight(COLOR_INDICATOR_WIDTH, DEFAULT_LINE_HEIGHT));
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedZoom(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        _task.ToggleExpandState();
                    }

                    float _maxWidth = windowWidthWithPaddings - (_visibleSlider ? VERTICAL_SLIDER_WIDTH : 0f) - INDEX_WIDTH - DATE_WIDTH - (4f * DEFAULT_LINE_HEIGHT) - COLOR_INDICATOR_WIDTH - 6f;

                    if (GUILayout.Button(_task.Label + _getClosedString(_task.IsClosed), SingleLineButtonStyle, FixedWidth(_maxWidth)))
                    {
                        _list.HandleSelection(_task, _event,
                                () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _getListFromWindow()));

                        return false;
                    }

                    if (GUILayout.Button(_task.DateTimeString, SingleLineLabelStyle, FixedWidthAndHeight(DATE_WIDTH, DEFAULT_LINE_HEIGHT)))
                    {
                        _list.HandleSelection(_task, _event,
                                () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _getListFromWindow()));

                        return false;
                    }

                    bool _isClosed = GUILayout.Toggle(_task.IsClosed, string.Empty, Styles.FixedToggle(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT));

                    if (_isClosed != _task.IsClosed)
                    {
                        _task.SetAsClosed(_isClosed);
                        sortAndFilterTasks(_task.ListType);

                        return true;
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedSettings(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        TaskListGenericMenu.ShowForTasks(this, _event, new List<Task>() { _task }, _task.ListType);
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedClose(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        Preferences.RemoveTask(_task, _task.ListType);
                        sortAndFilterTasks(_task.ListType);

                        return true;
                    }

                    VerySmallSpace();
                }
            }

            if (_task.IsExpanded)
            {
                bool _changed = drawExpandedTask(_task);

                if (_changed)
                {
                    return true;
                }
            }

            string _getClosedString(bool _condition)
            {
                if (_condition == false)
                {
                    return string.Empty;
                }

                return CLOSED_STRING;
            }

            TaskListType _getListFromWindow()
            {
                return windowMode switch
                {
                    WindowMode.CurrentTasks => TaskListType.Current,
                    WindowMode.Backlog => TaskListType.Backlog,
                    _ => TaskListType.Current
                };
            }

            return false;
        }

        private bool drawExpandedTask(Task _task)
        {
            using (new ScopeGroup(new LabelWidthScope(LABEL_WIDTH), new HorizontalScope()))
            {
                using (new VerticalScope(Styles.LightButton))
                {
                    using (new HorizontalScope())
                    {
                        LargeSpace();

                        using (new VerticalScope())
                        {
                            bool _collectionChanged = drawExpandedTasksProperties(_task);

                            if (_collectionChanged)
                            {
                                return true;
                            }
                        }

                        NormalSpace();
                    }
                }

                VerySmallSpace();
            }

            return false;
        }

        private bool drawExpandedTasksProperties(Task _task)
        {
            SmallSpace();
            string _taskLabel = EditorGUILayout.TextField(LABEL, _task.Label);

            if (_taskLabel != _task.Label)
            {
                _task.SetLabel(_taskLabel);
            }

            GUIStyle _areaWithWrap = new GUIStyle(EditorStyles.textArea);
            _areaWithWrap.wordWrap = true;

            string _taskDescription = EditorGUILayout.TextField(DESCRIPTION, _task.Description, _areaWithWrap, FixedHeight(SingleLineHeight * 3f));

            if (_taskDescription != _task.Description)
            {
                _task.SetDescription(_taskDescription);
            }

            TaskPriority _priority = (TaskPriority) EditorGUILayout.EnumPopup(PRIORITY, _task.Priority);

            if (_priority != _task.Priority)
            {
                _task.SetPriority(_priority);
                sortAndFilterTasks(_task.ListType);

                return true;
            }

            if (_task.NumberOfContexts == 0)
            {
                _drawAddContextButton();
                return false;
            }

            SmallSpace();
            GUILayout.Label(CONTEXTS, EditorStyles.boldLabel.WithColor(Color.white));
            VerySmallSpace();

            for (int i = 0; i < _task.NumberOfContexts; i++)
            {
                var _context = _task.GetContextAtIndex(i);

                if (_context == null)
                {
                    continue;
                }

                using (new VerticalScope(EditorStyles.helpBox))
                {
                    VerySmallSpace();
                    var _typeOfContext = _context.ContextType;

                    using (new HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(TYPE);
                        _typeOfContext = (TaskContextType) EditorGUILayout.EnumPopup(_typeOfContext);

                        VerySmallSpace();

                        if (GUILayout.Button(REMOVE_CONTEXT_CONTENT, FixedWidth(20f)))
                        {
                            _task.RemoveContextAtIndex(i);
                            return false;
                        }
                    }

                    if (_typeOfContext != _context.ContextType)
                    {
                        _context.SetContextType(_typeOfContext);

                        switch (windowMode)
                        {
                            case WindowMode.CurrentTasks:
                                sortedAndFilteredTasks.RefreshHeights(_task);
                                break;

                            case WindowMode.Backlog:
                                sortedAndFilteredBacklogTasks.RefreshHeights(_task);
                                break;
                        }
                    }

                    switch (_context.ContextType)
                    {
                        case TaskContextType.Object:
                            _drawObjectContext(_context);
                            break;

                        case TaskContextType.Script:
                            _drawScriptContext(_context);
                            break;

                        case TaskContextType.Scenes:
                            _drawSceneContext(_context);
                            break;
                    }

                    VerySmallSpace();
                }
            }

            _drawAddContextButton();
            return false;

            void _drawObjectContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    var _object = EditorGUILayout.ObjectField(OBJECT, _context.ObjectReference, typeof(Object), false);

                    if (_object != _context.ObjectReference)
                    {
                        _context.SetObjectReference(_object);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.ObjectReference == null))
                    {
                        if (GUILayout.Button(PING, FixedWidth(60f)))
                        {
                            AssetsUtilities.Ping(_context.ObjectReference);
                        }
                    }
                }
            }

            void _drawScriptContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    var _script = EditorGUILayout.ObjectField(SCRIPT, _context.ScriptReference, typeof(MonoScript), false);

                    if (_script != _context.ScriptReference)
                    {
                        _context.SetScriptReference(_script as MonoScript);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.ScriptReference == null))
                    {
                        if (GUILayout.Button(OPEN, FixedWidth(60f)))
                        {
                            AssetDatabase.OpenAsset(_context.ScriptReference);
                        }
                    }
                }
            }

            void _drawSceneContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(CAMERA_POSITION);

                    using (new DisabledScope())
                    {
                        EditorGUILayout.Vector3Field(string.Empty, _context.SceneReference.CameraPosition);
                    }

                    VerySmallSpace();

                    if (GUILayout.Button(SET, FixedWidth(60f)))
                    {
                        _context.UpdateCameraPosition();
                    }
                }

                using (new HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(CAMERA_ROTATION);

                    using (new DisabledScope())
                    {
                        EditorGUILayout.Vector3Field(string.Empty, _context.SceneReference.CameraRotation);
                    }

                    VerySmallSpace();

                    if (GUILayout.Button(SET, FixedWidth(60f)))
                    {
                        _context.UpdateCameraRotation();
                    }
                }

                SmallSpace();
                EditorGUILayout.TextField(MAIN_SCENE, _context.SceneReference.MainSceneName);

                if (_context.NumberOfAdditionalScenes > 0)
                {
                    VerySmallSpace();
                    int _index = 1;

                    foreach (var _sceneName in _context.SceneReference.AdditionalScenesNames)
                    {
                        EditorGUILayout.TextField($"Additional scene #{_index}", _sceneName);
                        _index++;
                    }
                }

                SmallSpace();
                float _buttonWidth = windowWidthWithPaddings / 2.5f;

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (GUILayout.Button(UPDATE_SCENES_AND_LOCATION, FixedWidth(_buttonWidth)))
                    {
                        _context.UpdateLoadedScenes();
                    }

                    FlexibleSpace();

                    using (new DisabledScope(_context.SceneReference.MainSceneName.IsNullEmptyOrWhitespace()))
                    {
                        if (GUILayout.Button(LOAD_CONTEXT_SCENES, FixedWidth(_buttonWidth)))
                        {
                            _context.LoadContextScenes();
                        }
                    }

                    FlexibleSpace();
                }
            }

            void _drawAddContextButton()
            {
                SmallSpace();

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (DrawBoxButton(ADD_CONTEXT, FixedWidthAndHeight(windowWidthScaled(0.5f), DEFAULT_LINE_HEIGHT)))
                    {
                        TaskContext _context = new TaskContext();
                        _task.AddContext(_context);
                    }

                    FlexibleSpace();
                }

                SmallSpace();
            }
        }

        private void drawSettings()
        {
            using (new LabelWidthScope(SETTINGS_LABEL_WIDTH))
            {
                using (var _changeCheck = new ChangeCheckScope())
                {
                    DrawHeader(COLORS);
                    Preferences.ImmediatePriorityColor = EditorGUILayout.ColorField(Preferences.IMMEDIATE_COLOR, Preferences.ImmediatePriorityColor);
                    Preferences.HighPriorityColor = EditorGUILayout.ColorField(Preferences.HIGH_COLOR, Preferences.HighPriorityColor);
                    Preferences.NormalPriorityColor = EditorGUILayout.ColorField(Preferences.NORMAL_COLOR, Preferences.NormalPriorityColor);
                    Preferences.LowPriorityColor = EditorGUILayout.ColorField(Preferences.LOW_COLOR, Preferences.LowPriorityColor);

                    SmallSpace();
                    Preferences.ClosedColor = EditorGUILayout.ColorField(Preferences.CLOSED_COLOR, Preferences.ClosedColor);

                    DrawHeader(LOGS);
                    Preferences.PrintLogs = EditorGUILayout.Toggle(Preferences.PRINT_LOGS, Preferences.PrintLogs);

                    DrawHeader(OTHERS);
                    Preferences.DefaultListType = (TaskListType) EditorGUILayout.EnumPopup(Preferences.DEFAULT_LIST, Preferences.DefaultListType);
                    Preferences.ApplyPositionAndRotationOnLoad = EditorGUILayout.Toggle(Preferences.APPLY_POSITION_AND_ROTATION_ON_LOAD, Preferences.ApplyPositionAndRotationOnLoad);

                    if (_changeCheck.changed)
                    {
                        Preferences.SavePreferences();
                    }
                }
            }

            NormalSpace();

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton(RESET_TO_DEFAULTS, FixedWidthAndHeight(windowWidthWithPaddings / 2f, DEFAULT_LINE_HEIGHT)))
                {
                    Preferences.ResetToDefaults();
                }

                FlexibleSpace();
            }
        }

        private void onCompilationStarted(object _obj)
        {
            Preferences.SavePreferences();
        }

        [MenuItem("Window/FewClicks Dev/Task List", priority = 108)]
        private static void ShowWindow()
        {
            var _window = GetWindow<TaskListWindow>();

            switch (Preferences.DefaultListType)
            {
                case TaskListType.Backlog:
                    _window.windowMode = WindowMode.Backlog;
                    break;

                default:
                    _window.windowMode = WindowMode.CurrentTasks;
                    break;
            }

            _window.Show();
        }
    }
}