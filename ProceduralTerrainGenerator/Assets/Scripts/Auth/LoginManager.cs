using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public InputField usernameField;
    public InputField passwordField;
    public Button loginButton;
    public Text statusLabel;          // ← перетягніть Text із Canvas

    const string apiUrl = "http://localhost:3000/login";

    void Start() => loginButton.onClick.AddListener(() =>
                     StartCoroutine(TryLogin()));

    IEnumerator TryLogin()
    {
        statusLabel.text = "Авторизація…";

        string json = $"{{\"username\":\"{usernameField.text}\"," +
                      $"\"password\":\"{passwordField.text}\"}}";

        var req = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            statusLabel.text = "Сервер недоступний";
            yield break;
        }

        var resp = JsonUtility.FromJson<AuthResp>(req.downloadHandler.text);
        if (!resp.ok)
        {
            statusLabel.text = "Невірні дані";
            yield break;
        }

        PlayerPrefs.SetInt("userId", resp.userId);    // ← правильно PlayerPrefs
        statusLabel.text = $"Успішний вхід (ID {resp.userId})";
        gameObject.SetActive(false);                  // ховаємо панель
    }

    [System.Serializable] private class AuthResp { public bool ok; public int userId; }
}
