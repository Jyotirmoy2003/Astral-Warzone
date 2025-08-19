namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    using static FewClicksDev.Core.EditorDrawer;

    [System.Serializable]
    public class TaskContext
    {
        [System.Serializable]
        public struct SceneContext
        {
            public Vector3 CameraPosition;
            public Vector3 CameraRotation;
            public string MainSceneName;
            public string[] AdditionalScenesNames;

            public float GetHeight()
            {
                float _height = SingleLineHeightWithSpacing * 2f; //Position and rotation
                _height += SMALL_SPACE;
                _height += SingleLineHeightWithSpacing; //Main scene
                _height += SMALL_SPACE;
                _height += SingleLineHeightWithSpacing; //Buttons

                if (AdditionalScenesNames.IsNullOrEmpty())
                {
                    return _height;
                }

                return _height + VERY_SMALL_SPACE + (AdditionalScenesNames.Length * SingleLineHeightWithSpacing); //Add additional scenes names
            }
        }

        private const string NO_SCENE_VIEW_ERROR = "There is no active scene view! Camera position and rotation can't be updated.";
        private const string NO_SCENE_VIEW_FOR_POSITION_ERROR = "There is no active scene view! Position can't be updated.";
        private const string NO_SCENE_VIEW_FOR_ROTATION_ERROR = "There is no active scene view! Rotation can't be updated.";

        [SerializeField] private TaskContextType contextType = TaskContextType.Object;
        [SerializeField] private Object objectReference = null;
        [SerializeField] private MonoScript scriptReference = null;
        [SerializeField] private SceneContext sceneReference = default;

        public TaskContextType ContextType => contextType;
        public Object ObjectReference => objectReference;
        public MonoScript ScriptReference => scriptReference;
        public SceneContext SceneReference => sceneReference;
        public int NumberOfAdditionalScenes => sceneReference.AdditionalScenesNames.IsNullOrEmpty() ? 0 : sceneReference.AdditionalScenesNames.Length;

        public void SetContextType(TaskContextType _contextType)
        {
            contextType = _contextType;
            TaskListUserPreferences.SavePreferences();
        }

        public void SetObjectReference(Object _object)
        {
            objectReference = _object;
            TaskListUserPreferences.SavePreferences();
        }

        public void SetScriptReference(MonoScript _script)
        {
            scriptReference = _script;
            TaskListUserPreferences.SavePreferences();
        }

        public void UpdateCameraPosition()
        {
            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                sceneReference.CameraPosition = _sceneView.camera.transform.position;
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_FOR_POSITION_ERROR);
            }

            TaskListUserPreferences.SavePreferences();
        }

        public void UpdateCameraRotation()
        {
            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                sceneReference.CameraRotation = _sceneView.camera.transform.eulerAngles;
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_FOR_ROTATION_ERROR);
            }

            TaskListUserPreferences.SavePreferences();
        }

        public void UpdateLoadedScenes()
        {
            SceneContext _sceneContext = new SceneContext();

            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                _sceneContext.CameraPosition = _sceneView.camera.transform.position;
                _sceneContext.CameraRotation = _sceneView.camera.transform.eulerAngles;
            }
            else
            {
                TaskList.LogWarning(NO_SCENE_VIEW_ERROR);
            }

            Scene _mainScene = SceneManager.GetActiveScene();
            _sceneContext.MainSceneName = _mainScene.name;

            List<string> _additionalScenes = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene _scene = SceneManager.GetSceneAt(i);

                if (_scene != _mainScene && _scene.isLoaded)
                {
                    _additionalScenes.Add(_scene.name);
                }
            }

            _sceneContext.AdditionalScenesNames = _additionalScenes.ToArray();
            sceneReference = _sceneContext;
            TaskListUserPreferences.SavePreferences();
        }

        public void LoadContextScenes()
        {
            string _mainScenePath = TaskList.FindScenePath(sceneReference.MainSceneName);

            if (_mainScenePath.IsNullEmptyOrWhitespace())
            {
                TaskList.LogError($"Main scene '{sceneReference.MainSceneName}' is not valid or not found. Skipping whole context loading!");
                return;
            }

            EditorSceneManager.OpenScene(_mainScenePath, OpenSceneMode.Single);

            foreach (string _sceneName in sceneReference.AdditionalScenesNames)
            {
                string _scenePath = TaskList.FindScenePath(_sceneName);

                if (_scenePath.IsNullEmptyOrWhitespace())
                {
                    TaskList.LogError($"Additional scene '{_sceneName}' is not valid or not found. Skipping its loading!");
                    continue;
                }

                EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
            }

            if (TaskListUserPreferences.ApplyPositionAndRotationOnLoad == false)
            {
                return;
            }

            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                _sceneView.camera.transform.position = sceneReference.CameraPosition;
                _sceneView.camera.transform.eulerAngles = sceneReference.CameraRotation;
                _sceneView.Repaint();
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_ERROR);
            }
        }

        public float GetHeight()
        {
            float _baseHeight = 2f * 4f; //Help box style has a top and bottom padding of 3 pixels and a 1 pixel frame

            if (contextType is TaskContextType.Scenes)
            {
                return _baseHeight + SingleLineHeightWithSpacing + sceneReference.GetHeight();
            }

            return _baseHeight + (2f * SingleLineHeightWithSpacing); //Type and asset reference
        }
    }
}
