using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Events;
using Project.SaveSystem;

namespace Project.Visuals
{
    public class CutsceneController : MonoBehaviour
    {
        public enum StepType
        {
            ShowObject, HideObject, FadeIn, FadeOut,
            FadeOverlayIn, FadeOverlayOut, Wait, ZoomIn,
            ShowPanel, HidePanel, ShakeObject,
            ShowDialogue, HideDialogue, ShowNPCPhrase, HideNPCPhrase,
            PlayMusic, StopMusic, FadeInMusic, FadeOutMusic,
            PlaySound, MuteGameAudio, UnmuteGameAudio,
            ShakeCamera, ZoomCamera,
            PauseGame, ResumeGame, EnablePlayer, DisablePlayer,
            FireUnityEvent,
        }

        [System.Serializable]
        public class CutsceneStep
        {
            public StepType   type;
            public GameObject target;
            public float      duration   = 1f;
            public float      holdAfter  = 0f;

            [Header("─ Zoom / Shake ─")]
            public float zoomFrom      = 1.15f;
            public float zoomTo        = 1.0f;
            public float shakeStrength = 5f;

            [Header("─ Диалог ─")]
            [TextArea(2, 4)]
            public string dialogueText    = "";
            public string speakerName     = "";
            public Sprite speakerPortrait;
            public float  typeSpeed       = 30f;

            [Header("─ Аудио ─")]
            public AudioClip audioClip;
            [Range(0f, 1f)]
            public float volume = 1f;
            public bool  loop   = false;

            [Header("─ Камера ─")]
            public float cameraZoomTarget   = 5f;
            public float cameraZoomDuration = 1f;

            [Header("─ Событие ─")]
            public UnityEvent customEvent;
        }

        [Header("[ ШАГИ ]")]
        public List<CutsceneStep> steps = new List<CutsceneStep>();

        [Header("[ СЛЕДУЮЩАЯ КАТСЦЕНА ]")]
        [SerializeField] private CutsceneController nextCutscene;

        [Header("[ UI — БАЗОВЫЕ ]")]
        [SerializeField] private Image      fadeOverlayImage;
        [SerializeField] private GameObject skipTextGO;
        [SerializeField] private float      skipShowDelay    = 2f;
        [SerializeField] private float      skipFadeDuration = 0.6f;

        [Header("[ UI — ДИАЛОГ ]")]
        [SerializeField] private GameObject      dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private Image           speakerPortraitImage;

        [Header("[ АУДИО ]")]
        [SerializeField] private AudioSource cutsceneMusicSource;
        [SerializeField] private AudioSource cutsceneSFXSource;
        [SerializeField] private AudioMixer  audioMixer;
        [SerializeField] private string      gameMixerVolumeParam  = "GameVolume";
        [SerializeField] private float       gameAudioFadeDuration = 0.5f;

        [Header("[ КАМЕРА ]")]
        [SerializeField] private Camera gameCamera;
        private float originalCameraSize;

        [Header("[ ИГРОК ]")]
        [SerializeField] private string playerTag            = "Player";
        [SerializeField] private string playerControllerName = "PlayerController";

        [Header("[ НАСТРОЙКИ ]")]
        [SerializeField] private bool  pauseGameDuringCutscene = true;
        [SerializeField] private bool  playOnStart             = false;
        [SerializeField] private float startDelay              = 0f;
        [Tooltip("Нужно ли глушить фоновые звуки игры при старте этой катсцены?")]
        [SerializeField] private bool  muteGameAudioOnStart    = true;

        [Header("[ СОХРАНЕНИЕ КАТСЦЕНЫ ]")]
        [Tooltip("Уникальный ID катсцены. Если он сохранен, катсцена будет пропущена при старте сцены.")]
        [SerializeField] private string cutsceneSaveID = "prologue_intro";

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent onSequenceStart;
        public UnityEvent onSequenceComplete;

        private bool          skipPressed;
        private bool          isPlaying;
        private MonoBehaviour playerController;

        private void Awake()
        {
            // Принудительно открываем игровой микшер на полную громкость при старте уровня,
            // чтобы сбросить глушение от прошлых сессий или меню
            if (audioMixer != null)
            {
                audioMixer.SetFloat(gameMixerVolumeParam, 0f);
            }

            if (fadeOverlayImage != null)
            {
                fadeOverlayImage.gameObject.SetActive(true);
                SetFadeAlpha(1f);
            }

            SetGraphicAlpha(skipTextGO, 0f);
            if (skipTextGO != null) skipTextGO.SetActive(false);

            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            if (gameCamera != null)
                originalCameraSize = gameCamera.orthographicSize;

            FindPlayerController();
        }

        private void Start()
        {
            var saveAnimator = FindAnyObjectByType<SaveAnimator>();
            if (saveAnimator != null)
            {
                saveAnimator.ForceHide();
            }

            StartCoroutine(CheckCutsceneStatusDeferred());
        }

        private IEnumerator CheckCutsceneStatusDeferred()
        {
            yield return new WaitForEndOfFrame();

            if (SaveManager.Instance != null && SaveManager.Instance.GetFlag(cutsceneSaveID))
            {
                onSequenceComplete?.Invoke();
                if (fadeOverlayImage != null) fadeOverlayImage.gameObject.SetActive(false);
                
                // Раскрываем микшер при авто-пропуске уже завершенной катсцены
                if (audioMixer != null)
                {
                    audioMixer.SetFloat(gameMixerVolumeParam, 0f);
                }

                ResumeGameplay();
                isPlaying = false;
                yield break; 
            }

            if (playOnStart)
                StartCoroutine(RunWithDelay());
        }

        private void OnDestroy()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentSceneName != "MainMenu" && audioMixer != null)
            {
                audioMixer.SetFloat(gameMixerVolumeParam, 0f);
            }
        }

        // ─── ПУБЛИЧНЫЕ МЕТОДЫ ─────────────────────────────────────────────

        public void Play()
        {
            if (!isPlaying)
                StartCoroutine(RunWithDelay());
        }

        public void PlayThenPlay(CutsceneController next)
        {
            nextCutscene = next;
            Play();
        }

        public void OnSkipClicked()
        {
            if (!isPlaying) return;
            if (!skipPressed)
            {
                skipPressed = true;
                StopAllCoroutines();
                StartCoroutine(SkipToEndRoutine());
            }
        }

        public void ResumeGameplay()
        {
            if (pauseGameDuringCutscene)
                Time.timeScale = 1f;

            SetPlayerControl(true);

            if (muteGameAudioOnStart)
            {
                StartCoroutine(UnmuteGameAudioRoutine());
            }
        }

        // ─── ПОСЛЕДОВАТЕЛЬНОСТЬ ──────────────────────────────────────────

        private IEnumerator RunWithDelay()
        {
            if (startDelay > 0f)
                yield return WaitUnscaled(startDelay);

            yield return StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            isPlaying   = true;
            skipPressed = false;

            SetPlayerControl(false);

            if (muteGameAudioOnStart)
            {
                yield return StartCoroutine(MuteGameAudioRoutine());
            }

            if (pauseGameDuringCutscene)
                Time.timeScale = 0f;

            onSequenceStart?.Invoke();
            StartCoroutine(ShowSkipTextDelayed());

            foreach (CutsceneStep step in steps)
            {
                if (skipPressed) yield break;
                yield return StartCoroutine(ExecuteStep(step));

                if (step.holdAfter > 0f && !skipPressed)
                    yield return WaitUnscaled(step.holdAfter);
            }

            if (!skipPressed)
                yield return StartCoroutine(Finish());
        }

        // ─── ВЫПОЛНЕНИЕ ШАГА ─────────────────────────────────────────────

        private IEnumerator ExecuteStep(CutsceneStep step)
        {
            switch (step.type)
            {
                case StepType.ShowObject:
                    step.target?.SetActive(true);
                    break;

                case StepType.HideObject:
                    step.target?.SetActive(false);
                    break;

                case StepType.FadeIn:
                    if (step.target != null) step.target.SetActive(true);
                    yield return StartCoroutine(FadeGraphic(step.target, 0f, 1f, step.duration));
                    break;

                case StepType.FadeOut:
                    yield return StartCoroutine(FadeGraphic(step.target, 1f, 0f, step.duration));
                    step.target?.SetActive(false);
                    break;

                case StepType.FadeOverlayIn:
                    if (fadeOverlayImage != null) fadeOverlayImage.gameObject.SetActive(true);
                    yield return StartCoroutine(FadeOverlay(0f, 1f, step.duration));
                    break;

                case StepType.FadeOverlayOut:
                    if (pauseGameDuringCutscene) Time.timeScale = 1f;
                    yield return StartCoroutine(FadeOverlay(1f, 0f, step.duration));
                    if (fadeOverlayImage != null) fadeOverlayImage.gameObject.SetActive(false);
                    break;

                case StepType.Wait:
                    yield return WaitUnscaled(step.duration);
                    break;

                case StepType.ZoomIn:
                    if (step.target != null) step.target.SetActive(true);
                    yield return StartCoroutine(ZoomAndFade(step.target, step.zoomFrom, step.zoomTo, step.duration));
                    break;

                case StepType.ShowPanel:
                    if (step.target != null) step.target.SetActive(true);
                    yield return StartCoroutine(FadeGraphic(step.target, 0f, 1f, step.duration));
                    break;

                case StepType.HidePanel:
                    yield return StartCoroutine(FadeGraphic(step.target, 1f, 0f, step.duration));
                    step.target?.SetActive(false);
                    break;

                case StepType.ShakeObject:
                    yield return StartCoroutine(ShakeObject(step.target, step.duration, step.shakeStrength));
                    break;

                case StepType.ShowDialogue:
                    yield return StartCoroutine(ShowDialogueRoutine(step));
                    break;

                case StepType.HideDialogue:
                    if (dialoguePanel != null)
                        yield return StartCoroutine(FadeGraphic(dialoguePanel, 1f, 0f, step.duration));
                    dialoguePanel?.SetActive(false);
                    break;

                case StepType.ShowNPCPhrase:
                    if (step.target != null)
                    {
                        TextMeshProUGUI npcText = step.target.GetComponent<TextMeshProUGUI>();
                        if (npcText != null) npcText.text = step.dialogueText;
                        step.target.SetActive(true);
                        yield return StartCoroutine(FadeGraphic(step.target, 0f, 1f, 0.3f));
                    }
                    break;

                case StepType.HideNPCPhrase:
                    if (step.target != null)
                    {
                        yield return StartCoroutine(FadeGraphic(step.target, 1f, 0f, 0.3f));
                        step.target.SetActive(false);
                    }
                    break;

                case StepType.PlayMusic:
                    PlayCutsceneMusic(step.audioClip, step.volume, step.loop);
                    break;

                case StepType.StopMusic:
                    cutsceneMusicSource?.Stop();
                    break;

                case StepType.FadeInMusic:
                    PlayCutsceneMusic(step.audioClip, 0f, step.loop);
                    yield return StartCoroutine(FadeMusicVolume(0f, step.volume, step.duration));
                    break;

                case StepType.FadeOutMusic:
                    float fromVol = cutsceneMusicSource != null ? cutsceneMusicSource.volume : 1f;
                    yield return StartCoroutine(FadeMusicVolume(fromVol, 0f, step.duration));
                    cutsceneMusicSource?.Stop();
                    break;

                case StepType.PlaySound:
                    if (cutsceneSFXSource != null && step.audioClip != null)
                        cutsceneSFXSource.PlayOneShot(step.audioClip, step.volume);
                    break;

                case StepType.MuteGameAudio:
                    yield return StartCoroutine(MuteGameAudioRoutine());
                    break;

                case StepType.UnmuteGameAudio:
                    yield return StartCoroutine(UnmuteGameAudioRoutine());
                    break;

                case StepType.ShakeCamera:
                    yield return StartCoroutine(ShakeCameraRoutine(step.duration, step.shakeStrength));
                    break;

                case StepType.ZoomCamera:
                    yield return StartCoroutine(ZoomCameraRoutine(step.cameraZoomTarget, step.cameraZoomDuration));
                    break;

                case StepType.PauseGame:
                    Time.timeScale = 0f;
                    break;

                case StepType.ResumeGame:
                    Time.timeScale = 1f;
                    break;

                case StepType.EnablePlayer:
                    SetPlayerControl(true);
                    break;

                case StepType.DisablePlayer:
                    SetPlayerControl(false);
                    break;

                case StepType.FireUnityEvent:
                    step.customEvent?.Invoke();
                    break;
            }
        }

        // ─── ДИАЛОГ ──────────────────────────────────────────────────────

        private IEnumerator ShowDialogueRoutine(CutsceneStep step)
        {
            if (dialoguePanel == null) yield break;

            if (dialogueText         != null) dialogueText.text         = "";
            if (speakerNameText      != null) speakerNameText.text      = step.speakerName;
            if (speakerPortraitImage != null)
            {
                speakerPortraitImage.sprite  = step.speakerPortrait;
                speakerPortraitImage.enabled = step.speakerPortrait != null;
            }

            dialoguePanel.SetActive(true);
            yield return StartCoroutine(FadeGraphic(dialoguePanel, 0f, 1f, 0.3f));

            if (step.typeSpeed > 0f)
                yield return StartCoroutine(TypeText(dialogueText, step.dialogueText, step.typeSpeed));
            else if (dialogueText != null)
                dialogueText.text = step.dialogueText;
        }

        private IEnumerator TypeText(TextMeshProUGUI tmp, string text, float speed)
        {
            if (tmp == null) yield break;
            tmp.text = "";
            float delay = 1f / speed;
            foreach (char c in text)
            {
                if (skipPressed) { tmp.text = text; yield break; }
                tmp.text += c;
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        // ─── ФИНАЛ ───────────────────────────────────────────────────────

        private IEnumerator Finish()
        {
            if (skipTextGO != null)
                yield return StartCoroutine(FadeGraphic(skipTextGO, 1f, 0f, skipFadeDuration));
            if (skipTextGO != null) skipTextGO.SetActive(false);

            if (cutsceneMusicSource != null && cutsceneMusicSource.isPlaying)
                yield return StartCoroutine(FadeMusicVolume(cutsceneMusicSource.volume, 0f, 0.5f));

            SetFadeAlpha(0f);
            if (fadeOverlayImage != null)
                fadeOverlayImage.gameObject.SetActive(false);

            isPlaying = false;

            if (muteGameAudioOnStart)
            {
                yield return StartCoroutine(UnmuteGameAudioRoutine());
            }

            if (SaveManager.Instance != null && !string.IsNullOrEmpty(cutsceneSaveID))
            {
                SaveManager.Instance.SetFlag(cutsceneSaveID);
                SaveManager.Instance.Save();
            }

            if (nextCutscene != null)
                nextCutscene.Play();
            else
                onSequenceComplete?.Invoke();
        }

        // ─── ПРОПУСК ─────────────────────────────────────────────────────

        private IEnumerator SkipToEndRoutine()
        {
            foreach (CutsceneStep step in steps)
            {
                if (step.target == null) continue;
                SetGraphicAlpha(step.target, 0f);
                step.target.SetActive(false);
            }

            if (skipTextGO    != null) { SetGraphicAlpha(skipTextGO, 0f); skipTextGO.SetActive(false); }
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            if (cutsceneMusicSource != null)
            {
                cutsceneMusicSource.Stop();
                cutsceneMusicSource.volume = 0f;
            }

            Time.timeScale = 1f;

            if (fadeOverlayImage != null) fadeOverlayImage.gameObject.SetActive(true);
            SetFadeAlpha(1f);
            yield return new WaitForSecondsRealtime(0.3f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.6f;
                SetFadeAlpha(Mathf.Lerp(1f, 0f, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }

            SetFadeAlpha(0f);
            if (fadeOverlayImage != null)
                fadeOverlayImage.gameObject.SetActive(false);

            isPlaying   = false;
            skipPressed = false;

            if (muteGameAudioOnStart)
            {
                yield return StartCoroutine(UnmuteGameAudioRoutine());
            }

            if (SaveManager.Instance != null && !string.IsNullOrEmpty(cutsceneSaveID))
            {
                SaveManager.Instance.SetFlag(cutsceneSaveID);
                SaveManager.Instance.Save();
            }

            if (nextCutscene != null)
                nextCutscene.Play();
            else
                onSequenceComplete?.Invoke();
        }

        // ─── КНОПКА ПРОПУСКА ─────────────────────────────────────────────

        private IEnumerator ShowSkipTextDelayed()
        {
            yield return WaitUnscaled(skipShowDelay);
            if (!skipPressed && skipTextGO != null)
            {
                skipTextGO.SetActive(true);
                yield return StartCoroutine(FadeGraphic(skipTextGO, 0f, 1f, skipFadeDuration));
            }
        }

        // ─── ИГРОК ───────────────────────────────────────────────────────

        private void FindPlayerController()
        {
            GameObject player = GameObject.FindWithTag(playerTag);
            if (player == null) return;

            foreach (MonoBehaviour mb in player.GetComponents<MonoBehaviour>())
            {
                if (mb.GetType().Name == playerControllerName)
                {
                    playerController = mb;
                    break;
                }
            }
        }

        private void SetPlayerControl(bool enabled)
        {
            if (playerController != null)
                playerController.enabled = enabled;
        }

        // ─── АУДИО ───────────────────────────────────────────────────────

        private void PlayCutsceneMusic(AudioClip clip, float volume, bool loop)
        {
            if (cutsceneMusicSource == null || clip == null) return;
            cutsceneMusicSource.clip   = clip;
            cutsceneMusicSource.volume = volume;
            cutsceneMusicSource.loop   = loop;
            cutsceneMusicSource.Play();
        }

        private IEnumerator FadeMusicVolume(float from, float to, float duration)
        {
            if (cutsceneMusicSource == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                cutsceneMusicSource.volume = Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t)));
                yield return null;
            }
            cutsceneMusicSource.volume = to;
        }

        private IEnumerator MuteGameAudioRoutine()
        {
            if (audioMixer == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / gameAudioFadeDuration;
                float targetVol = Mathf.Lerp(0f, -80f, t);
                audioMixer.SetFloat(gameMixerVolumeParam, targetVol);
                yield return null;
            }
            audioMixer.SetFloat(gameMixerVolumeParam, -80f);
        }

        private IEnumerator UnmuteGameAudioRoutine()
        {
            if (audioMixer == null) yield break;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / gameAudioFadeDuration;
                float targetVol = Mathf.Lerp(-80f, 0f, t);
                audioMixer.SetFloat(gameMixerVolumeParam, targetVol);
                yield return null;
            }
            audioMixer.SetFloat(gameMixerVolumeParam, 0f);
        }

        // ─── КАМЕРА ──────────────────────────────────────────────────────

        private IEnumerator ShakeCameraRoutine(float duration, float strength)
        {
            if (gameCamera == null) yield break;
            Vector3 originalPos = gameCamera.transform.localPosition;
            float   elapsed     = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fade = 1f - (elapsed / duration);
                gameCamera.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength * fade;
                yield return null;
            }
            gameCamera.transform.localPosition = originalPos;
        }

        private IEnumerator ZoomCameraRoutine(float targetSize, float duration)
        {
            if (gameCamera == null) yield break;
            float startSize = gameCamera.orthographicSize;
            float t         = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                gameCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, EaseInOut(Mathf.Clamp01(t)));
                yield return null;
            }
            gameCamera.orthographicSize = targetSize;
        }

        // ─── АНИМАЦИИ ────────────────────────────────────────────────────

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                SetFadeAlpha(Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }
            SetFadeAlpha(to);
        }

        private IEnumerator FadeGraphic(GameObject go, float from, float to, float duration)
        {
            if (go == null) yield break;
            float t = 0f;
            while (t < 1f && !skipPressed)
            {
                t += Time.unscaledDeltaTime / duration;
                SetGraphicAlpha(go, Mathf.Lerp(from, to, EaseInOut(Mathf.Clamp01(t))));
                yield return null;
            }
            SetGraphicAlpha(go, to);
        }

        private IEnumerator ZoomAndFade(GameObject go, float zoomFrom, float zoomTo, float duration)
        {
            if (go == null) yield break;
            go.transform.localScale = Vector3.one * zoomFrom;
            SetGraphicAlpha(go, 0f);
            float t = 0f;
            while (t < 1f && !skipPressed)
            {
                t += Time.unscaledDeltaTime / duration;
                float eased = EaseInOut(Mathf.Clamp01(t));
                go.transform.localScale = Vector3.one * Mathf.Lerp(zoomFrom, zoomTo, eased);
                SetGraphicAlpha(go, eased);
                yield return null;
            }
            go.transform.localScale = Vector3.one * zoomTo;
            SetGraphicAlpha(go, 1f);
        }

        private IEnumerator ShakeObject(GameObject go, float duration, float strength)
        {
            if (go == null) yield break;
            Vector3 originalPos = go.transform.localPosition;
            float   elapsed     = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fade = 1f - (elapsed / duration);
                go.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength * fade;
                yield return null;
            }
            go.transform.localPosition = originalPos;
        }

        // ─── УТИЛИТЫ ─────────────────────────────────────────────────────

        private IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds && !skipPressed)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeOverlayImage == null) return;
            Color c = fadeOverlayImage.color;
            c.a = alpha;
            fadeOverlayImage.color = c;
        }

        private void SetGraphicAlpha(GameObject go, float alpha)
        {
            if (go == null) return;
            CanvasGroup     cg  = go.GetComponent<CanvasGroup>();
            Image           img = go.GetComponent<Image>();
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (cg  != null) { cg.alpha = alpha; return; }
            if (img != null) { Color c = img.color; c.a = alpha; img.color = c; }
            if (tmp != null) { Color c = tmp.color; c.a = alpha; tmp.color = c; }
        }

        private float EaseInOut(float t) => t * t * (3f - 2f * t);
    }
}