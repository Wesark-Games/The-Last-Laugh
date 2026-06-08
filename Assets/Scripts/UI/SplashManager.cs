using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace Project.UI
{
    public class SplashManager : MonoBehaviour
    {
        [Header("[ НАЗВАНИЯ СЦЕН ]")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("[ ПРЕДУПРЕЖДЕНИЕ 18+ ]")]
        [SerializeField] private GameObject warningSplashGO;
        [SerializeField] private Image      warningArtImage;
        [SerializeField] private float warningHoldDuration = 4f;

        [Header("[ ПРЕДУПРЕЖДЕНИЕ ОБ АВТОСОХРАНЕНИИ ]")]
        [SerializeField] private GameObject saveWarningSplashGO;
        [SerializeField] private Image      saveWarningArtImage;
        [SerializeField] private float saveWarningHoldDuration = 4f;

        [Header("[ ЗАСТАВКА СТУДИИ ]")]
        [SerializeField] private GameObject studioSplashGO;
        [SerializeField] private VideoPlayer studioVideoPlayer;
        [SerializeField] private float studioMaxWait = 10f;

        [Header("[ ЗАСТАВКА ИГРЫ ]")]
        [SerializeField] private GameObject gameSplashGO;
        [SerializeField] private Image      artImage;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip   splashMusic;
        [SerializeField] private TextMeshProUGUI skipText;
        [SerializeField] private float artFadeInDuration  = 2f;
        [SerializeField] private float splashHoldDuration = 20f;
        [SerializeField] private float skipTextDelay      = 2.5f;
        [SerializeField] private float skipTextFadeDuration = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;

        [Header("[ ОБЩИЙ ФЕЙД ]")]
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private float globalFadeDuration = 0.8f;

        private bool skipPressed;
        private bool isInteractableSplashActive;
        private bool gameSplashActive;

        private void Start()
        {
            Time.timeScale = 1f;

            SetFadeAlpha(1f);
            SetArtAlpha(artImage, 0f);
            SetArtAlpha(warningArtImage, 0f);
            SetArtAlpha(saveWarningArtImage, 0f);
            SetTextAlpha(skipText, 0f);

            warningSplashGO?.SetActive(true);
            saveWarningSplashGO?.SetActive(false);
            studioSplashGO?.SetActive(false);
            gameSplashGO?.SetActive(false);

            StartCoroutine(RunSplashSequence());
        }

        private void Update()
        {
            if (isInteractableSplashActive || gameSplashActive)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                    skipPressed = true;
            }
        }

        private IEnumerator RunSplashSequence()
        {
            yield return StartCoroutine(PlayWarningSplash());
            yield return StartCoroutine(PlaySaveWarningSplash());
            yield return StartCoroutine(PlayStudioSplash());
            yield return StartCoroutine(PlayGameSplash());
            yield return StartCoroutine(GoToMainMenu());
        }

        private IEnumerator PlayWarningSplash()
        {
            if (warningSplashGO == null) yield break;

            isInteractableSplashActive = true;
            skipPressed                = false;

            if (musicSource != null && splashMusic != null)
            {
                musicSource.clip   = splashMusic;
                musicSource.volume = 0f;
                musicSource.loop   = true;
                musicSource.Play();
                StartCoroutine(FadeMusicVolume(0f, musicVolume, globalFadeDuration));
            }

            StartCoroutine(FadeGraphic(warningArtImage, 0f, 1f, globalFadeDuration));
            yield return StartCoroutine(Fade(1f, 0f, globalFadeDuration));

            float elapsed = 0f;
            while (elapsed < warningHoldDuration && !skipPressed)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(FadeGraphic(warningArtImage, 1f, 0f, globalFadeDuration));
            yield return StartCoroutine(Fade(0f, 1f, globalFadeDuration));

            warningSplashGO.SetActive(false);
            isInteractableSplashActive = false;
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator PlaySaveWarningSplash()
        {
            if (saveWarningSplashGO == null) yield break;

            saveWarningSplashGO.SetActive(true);
            isInteractableSplashActive = true;
            skipPressed                = false;

            StartCoroutine(FadeGraphic(saveWarningArtImage, 0f, 1f, globalFadeDuration));
            yield return StartCoroutine(Fade(1f, 0f, globalFadeDuration));

            float elapsed = 0f;
            while (elapsed < saveWarningHoldDuration && !skipPressed)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(FadeGraphic(saveWarningArtImage, 1f, 0f, globalFadeDuration));
            yield return StartCoroutine(Fade(0f, 1f, globalFadeDuration));

            saveWarningSplashGO.SetActive(false);
            isInteractableSplashActive = false;
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator PlayStudioSplash()
        {
            if (studioVideoPlayer == null)
            {
                studioSplashGO?.SetActive(false);
                yield break;
            }

            studioSplashGO?.SetActive(true);
            yield return StartCoroutine(Fade(1f, 0f, globalFadeDuration));

            studioVideoPlayer.Play();

            float elapsed = 0f;
            while (studioVideoPlayer.isPlaying && elapsed < studioMaxWait)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    studioVideoPlayer.Stop();
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(Fade(0f, 1f, globalFadeDuration));
            studioSplashGO?.SetActive(false);

            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator PlayGameSplash()
        {
            gameSplashGO?.SetActive(true);
            gameSplashActive = false;
            skipPressed      = false;

            if (musicSource != null && !musicSource.isPlaying && splashMusic != null)
            {
                musicSource.clip   = splashMusic;
                musicSource.volume = musicVolume;
                musicSource.loop   = true;
                musicSource.Play();
            }

            yield return StartCoroutine(Fade(1f, 0f, globalFadeDuration));

            gameSplashActive = true;

            yield return StartCoroutine(FadeGraphic(artImage, 0f, 1f, artFadeInDuration));

            yield return new WaitForSeconds(skipTextDelay);
            yield return StartCoroutine(FadeText(skipText, 0f, 1f, skipTextFadeDuration));

            float holdTimer = 0f;
            while (holdTimer < splashHoldDuration && !skipPressed)
            {
                holdTimer += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(FadeText(skipText, 1f, 0f, skipTextFadeDuration));
        }

        private IEnumerator GoToMainMenu()
        {
            StartCoroutine(FadeOutMusic());
            yield return StartCoroutine(Fade(0f, 1f, globalFadeDuration));

            gameSplashGO?.SetActive(false);

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(mainMenuScene);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        private IEnumerator FadeOutMusic()
        {
            if (musicSource == null) yield break;

            float startVolume = musicSource.volume;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / globalFadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            musicSource.Stop();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                SetFadeAlpha(Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }
            SetFadeAlpha(to);
        }

        private IEnumerator FadeGraphic(Image img, float from, float to, float duration)
        {
            if (img == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                SetArtAlpha(img, Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }
            SetArtAlpha(img, to);
        }

        private IEnumerator FadeText(TextMeshProUGUI txt, float from, float to, float duration)
        {
            if (txt == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                SetTextAlpha(txt, Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }
            SetTextAlpha(txt, to);
        }

        private IEnumerator FadeMusicVolume(float from, float to, float duration)
        {
            if (musicSource == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                musicSource.volume = Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t)));
                yield return null;
            }
            musicSource.volume = to;
        }

        private void SetFadeAlpha(float a)
        {
            if (fadeOverlay == null) return;
            Color c = fadeOverlay.color;
            c.a = a;
            fadeOverlay.color = c;
        }

        private void SetArtAlpha(Image img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }

        private void SetTextAlpha(TextMeshProUGUI txt, float a)
        {
            if (txt == null) return;
            Color c = txt.color;
            c.a = a;
            txt.color = c;
        }

        private float EaseInOut(float t) => t * t * (3f - 2f * t);
    }
}