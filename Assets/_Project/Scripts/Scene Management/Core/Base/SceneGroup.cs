using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Represents a group of scenes that can be loaded and managed together.
    /// </summary>
    [Serializable]
    public struct SceneGroup
    {
        /// <summary>
        /// The name of the scene group.
        /// </summary>
        /// 
        public ESceneGroup GroupName;

        /// <summary>
        /// The list of scenes included in this group.
        /// </summary>
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
        public SceneReference Scene;

        /// <summary>
        /// The type of the scene (e.g., ActiveScene, MainMenu, etc.).
        /// </summary>
        public ESceneType SceneType;

        /// <summary>
        /// The name of the scene.
        /// </summary>
        public string Name => Scene.Name;

        /// <summary>
        /// The path of the scene.
        /// </summary>
        public string Path => Scene.Path;

        /// <summary>
        /// Indicates whether the scene is addressable.
        /// </summary>
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
