using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Project.Juggling
{
    public class JugglingGame : MonoBehaviour
    {
        [Header("[ ФАКЕЛЫ ]")]
        [SerializeField] private GameObject torchPrefab;
        [SerializeField] private Transform  playerHand;

        [Header("[ ФИЗИКА БРОСКА ]")]
        [SerializeField] private float tossForceY     = 9f;
        [SerializeField] private float tossSpreadX    = 1.5f;
        [Tooltip("Высота пола — Y координата ниже которой факел считается уроненным")]
        [SerializeField] private float groundY = -3f;

        [Header("[ СЛОЖНОСТЬ ]")]
        [Tooltip("Через сколько секунд добавится 2й факел")]
        [SerializeField] private float secondTorchDelay = 5f;
        [Tooltip("Через сколько секунд добавится 3й факел")]
        [SerializeField] private float thirdTorchDelay  = 12f;
        [Tooltip("Через сколько секунд бросок начинает быть 'неудобным'")]
        [SerializeField] private float panicTimeStart   = 18f;

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent OnTorchDropped;
        public UnityEvent OnTorchTossed;

        private readonly List<Torch> _torches = new();
        private float _startTime;
        private int   _torchCount;
        private bool  _gameOver;

        private void Start()
        {
            _startTime = Time.time;
            SpawnTorch();
        }

        private void Update()
        {
            if (_gameOver) return;

            float elapsed = Time.time - _startTime;

            // Появление новых факелов
            if (_torchCount == 1 && elapsed > secondTorchDelay) SpawnTorch();
            if (_torchCount == 2 && elapsed > thirdTorchDelay)  SpawnTorch();

            // Подброс по Space
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                TossLowestTorch(elapsed);

            // Проверка упавших факелов
            foreach (var torch in _torches)
            {
                if (torch != null && torch.HasFallen)
                {
                    GameOver();
                    return;
                }
            }
        }

        private void SpawnTorch()
        {
            if (torchPrefab == null || playerHand == null) return;

            GameObject obj = Instantiate(torchPrefab, playerHand.position, Quaternion.identity);
            Torch torch = obj.GetComponent<Torch>();
            if (torch == null) return;

            torch.SetGroundY(groundY);
            _torches.Add(torch);
            _torchCount++;

            // Сразу подкидываем
            float randomX = Random.Range(-tossSpreadX, tossSpreadX);
            torch.Toss(new Vector2(randomX, tossForceY));
        }

        private void TossLowestTorch(float elapsedTime)
        {
            Torch lowest   = null;
            float lowestY  = float.MaxValue;

            foreach (var torch in _torches)
            {
                if (torch == null || torch.HasFallen) continue;
                if (torch.CurrentY < lowestY)
                {
                    lowestY = torch.CurrentY;
                    lowest  = torch;
                }
            }

            if (lowest == null) return;

            // С увеличением времени броски становятся "паническими" — больше разброса
            float panicFactor = 0f;
            if (elapsedTime > panicTimeStart)
                panicFactor = Mathf.Clamp01((elapsedTime - panicTimeStart) / 10f);

            float spread = tossSpreadX + panicFactor * 3f;
            float force  = tossForceY * (1f + panicFactor * 0.3f);
            float randomX = Random.Range(-spread, spread);

            lowest.Toss(new Vector2(randomX, force));
            OnTorchTossed?.Invoke();
        }

        private void GameOver()
        {
            _gameOver = true;
            OnTorchDropped?.Invoke();
        }

        // Гизмо — показывает уровень пола в редакторе
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-20, groundY, 0), new Vector3(20, groundY, 0));
        }
    }
}
