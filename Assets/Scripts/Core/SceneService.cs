using UnityEngine.SceneManagement;

public class SceneService : ISceneService
{
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void LoadGameScene() => LoadScene(SceneNames.GameTest);
    public void LoadLoginScene() => LoadScene(SceneNames.Login);
    public void LoadMainMenuScene() => LoadScene(SceneNames.MainMenu);
}
