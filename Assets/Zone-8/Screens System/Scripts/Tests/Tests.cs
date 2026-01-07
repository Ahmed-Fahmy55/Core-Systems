using NUnit.Framework;
using UnityEngine;
using Zone8.Screens;

public class Tests
{
    public class TestScreen : ScreenBase
    {
        public override Awaitable StartHideEffect() => Awaitable.WaitForSecondsAsync(.1f);
        public override Awaitable StartShowEffect() => Awaitable.WaitForSecondsAsync(.1f);
    }

    private class TestEScreen : EScreen
    {
        public TestEScreen(string name)
        {
            ScreenName = name;
        }
    }

    [Test]
    public void EnableInteraction_SetsBlocksRaycasts()
    {
        // Arrange
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        var canvasGroup = go.GetComponent<CanvasGroup>();

        // Act
        screen.EnableInteraction(true);

        // Assert
        Assert.IsTrue(canvasGroup.blocksRaycasts);

        // Act
        screen.EnableInteraction(false);

        // Assert
        Assert.IsFalse(canvasGroup.blocksRaycasts);


    }
}
