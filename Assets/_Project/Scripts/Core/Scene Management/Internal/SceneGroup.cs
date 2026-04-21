using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Represents a group of scenes that can be loaded and managed together.
    /// </summary>
    [Serializable]
    [InlineProperty] // Makes the entire struct inline in the inspector
    [HideLabel]      // Hides the default label for the struct
    public struct SceneGroup
    {
        /// <summary>
        /// The name of the scene group.
        /// </summary>
        /// 
        [HorizontalGroup("Split")]
        [BoxGroup("Split/Group Info", CenterLabel = true)]
        [LabelText("Group Name")]
        [Required, LabelWidth(100)]
        public ESceneGroup GroupName;

        /// <summary>
        /// The list of scenes included in this group.
        /// </summary>
        [BoxGroup("Split/Scenes", CenterLabel = true)]
        public List<SceneData> Scenes;

        /// <summary>
        /// Finds the path of the first scene in the group that matches the specified scene type.
        /// </summary>
        /// <param name="sceneType">The type of the scene to find.</param>
        /// <returns>The path of the matching scene, or <c>null</c> if no matching scene is found.</returns>
        public string FindScenePathByType(ESceneType sceneType)
        {
            return Scenes.FirstOrDefault(scene => scene.SceneType == sceneType)?.Scene.Path ?? null;
        }

        /// <summary>
        /// Gets the count of addressable scenes in the group.
        /// </summary>
        /// <returns>The number of addressable scenes in the group.</returns>
        public int GetAddressablesScenesCount()
        {
            return Scenes.Count(scene => scene.Scene.State == SceneReferenceState.Addressable);
        }
    }

    /// <summary>
    /// Represents metadata for an individual scene.
    /// </summary>
    [Serializable]
    public class SceneData
    {
        /// <summary>
        /// The reference to the scene asset.
        /// </summary>
        [Required("Put the scene group name asset or create one")]
        [LabelText("Scene Reference"), LabelWidth(100)]
        public SceneReference Scene;

        /// <summary>
        /// The type of the scene (e.g., ActiveScene, MainMenu, etc.).
        /// </summary>
        [LabelText("Scene Type"), LabelWidth(70)]
        public ESceneType SceneType;

        /// <summary>
        /// The name of the scene.
        /// </summary>
        [TableColumnWidth(150, Resizable = false)]
        [ReadOnly]
        [LabelText("Scene Name")]
        public string Name => Scene.Name;

        /// <summary>
        /// The path of the scene.
        /// </summary>
        [TableColumnWidth(300)]
        [ReadOnly]
        [LabelText("Scene Path")]
        public string Path => Scene.Path;

        /// <summary>
        /// Indicates whether the scene is addressable.
        /// </summary>
        [TableColumnWidth(100)]
        [ReadOnly]
        [LabelText("Is Addressable")]
        public bool IsAddressable => Scene.State == SceneReferenceState.Addressable ? true : false;
    }

    /// <summary>
    /// Enum representing the various types of scenes.
    /// </summary>
    public enum ESceneType
    {
        /// <summary>
        /// The active scene in the group.
        /// </summary>
        ActiveScene,

        /// <summary>
        /// The main menu scene.
        /// </summary>
        MainMenu,

        /// <summary>
        /// A user interface scene.
        /// </summary>
        UserInterface,

        /// <summary>
        /// An environment scene.
        /// </summary>
        Environment,

        /// <summary>
        /// A tooling or utility scene.
        /// </summary>
        Tooling
    }
}
