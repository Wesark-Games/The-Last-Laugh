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
        [SerializeField] private float tossForceY    = 9f;
        [SerializeField] private float tossSpreadX   = 1.5f;

        [Header("[ ЗОНЫ ]")]
        [SerializeField] private float catchZoneY    = -1.5f;
        [SerializeField] private float crowdY        = -5f;
        [SerializeField] private float catchRadiusX  = 1.2f;

        [Header("[ СЛОЖНОСТЬ ]")]
        [SerializeField] private float secondTorchDelay = 5f;
        [SerializeField] private float thirdTorchDelay  = 12f;
        [SerializeField] private float panicTimeStart   = 18f;

        [Header("[ ПОБЕГ ]")]
        [SerializeField] private EscapeSequence escapeSequence;

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent OnTorchDropped;
        public UnityEvent OnTorchTossed;
        public UnityEvent OnTorchMissed;

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

            if (_torchCount == 1 && elapsed > secondTorchDelay) SpawnTorch();
            if (_torchCount == 2 && elapsed > thirdTorchDelay)  SpawnTorch();

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                TryTossTorch(elapsed);

            foreach (var torch in _torches)
            {
                if (torch == null) continue;
                if (torch.transform.position.y <= crowdY)
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

            torch.SetGroundY(crowdY);
            _torches.Add(torch);
            _torchCount++;

            float randomX = Random.Range(-tossSpreadX, tossSpreadX);
            torch.Toss(new Vector2(randomX, tossForceY));
        }

        private void TryTossTorch(float elapsedTime)
        {
            Torch lowest   = null;
            float lowestY  = float.MaxValue;

            foreach (var torch in _torches)
            {
                if (torch == null) continue;
                if (torch.transform.position.y < catchZoneY) continue;

                float dx = Mathf.Abs(torch.transform.position.x - playerHand.position.x);
                if (dx > catchRadiusX) continue;

                if (torch.transform.position.y < lowestY)
                {
                    lowestY = torch.transform.position.y;
                    lowest  = torch;
                }
            }

            if (lowest == null)
            {
                OnTorchMissed?.Invoke();
                return;
            }

            float panicFactor = 0f;
            if (elapsedTime > panicTimeStart)
                panicFactor = Mathf.Clamp01((elapsedTime - panicTimeStart) / 10f);

            float spread  = tossSpreadX + panicFactor * 3f;
            float force   = tossForceY * (1f + panicFactor * 0.3f);
            float randomX = Random.Range(-spread, spread);

            lowest.Toss(new Vector2(randomX, force));
            OnTorchTossed?.Invoke();
        }

        private void GameOver()
        {
            _gameOver = true;

            // Уничтожаем оставшиеся факелы
            foreach (var torch in _torches)
                if (torch != null) Destroy(torch.gameObject);
            _torches.Clear();

            OnTorchDropped?.Invoke();

            // Запускаем побег
            if (escapeSequence != null)
                escapeSequence.StartEscape();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(-20, catchZoneY, 0), new Vector3(20, catchZoneY, 0));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-20, crowdY, 0), new Vector3(20, crowdY, 0));

            if (playerHand != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    new Vector3(playerHand.position.x - catchRadiusX, catchZoneY, 0),
                    new Vector3(playerHand.position.x - catchRadiusX, playerHand.position.y + 3, 0));
                Gizmos.DrawLine(
                    new Vector3(playerHand.position.x + catchRadiusX, catchZoneY, 0),
                    new Vector3(playerHand.position.x + catchRadiusX, playerHand.position.y + 3, 0));
            }
        }
    }
}
