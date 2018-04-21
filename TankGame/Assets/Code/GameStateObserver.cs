using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.Messaging;

namespace TankGame
{
    /// <summary>
    /// This class observes the game situation. It keeps track of the score, collectables and player lives.
    /// </summary>
    public class GameStateObserver : MonoBehaviour
    {
        private List<Collectable> _collectables;

        public Collectable _collectablePrefab;

        private const float _collectablePositionY = 1.5f;

        public Transform _upperLeftCorner,
            _lowerRightCorner;

        [SerializeField, Tooltip("How much score should the player accumulate to win.")]
        private int _scoreToWin;

        [SerializeField, Tooltip("How many collectable items should there be at maximum.")]
        private int _maximumCollectableItems;

        [SerializeField, Tooltip("How much score the player gets for destroying an enemy tank.")]
        private int _scorePerEnemyTank;

        [SerializeField, Tooltip("How many lives the player has at the start of the game.")]
        private int _playerStartingLives;

        private int _playerRemainingLives;

        [SerializeField, Tooltip("How often should a new collectable spawn.")]
        private float _collectableSpawnRate;

        private float _collectableSpawnCountdown;

        [SerializeField]
        private Text _scoreText;

        [SerializeField]
        private Text _remainingScoreToWinText;

        [SerializeField]
        private Text _remainingLivesText;

        [SerializeField]
        private Text _winOrLoseText;

        private ISubscription<UnitDiedMessage> _unitDiedSubscription;

        private int _score = 0;

        // Use this for initialization
        void Start()
        {
            _collectables = new List<Collectable>();
            _unitDiedSubscription =
                GameManager.Instance.MessageBus.Subscribe<UnitDiedMessage>(OnUnitDied);
            _playerRemainingLives = _playerStartingLives;
            UpdateScore();
            _remainingLivesText.text = string.Format("Remaining lives " + _playerRemainingLives);
        }

        private void OnUnitDied(UnitDiedMessage msg)
        {
            if (msg.DeadUnit is PlayerUnit)
            {
                _playerRemainingLives--;
                _remainingLivesText.text = string.Format("Remaining lives " + _playerRemainingLives);
                WinOrLose();
            }

            if (msg.DeadUnit is EnemyUnit)
            {
                _score += _scorePerEnemyTank;
                UpdateScore();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_collectables.Count < _maximumCollectableItems) {
                _collectableSpawnCountdown -= Time.deltaTime;
                if (_collectableSpawnCountdown < 0f)
                {
                    _collectableSpawnCountdown = _collectableSpawnRate;
                    Collectable collectable = Instantiate(_collectablePrefab);
                    RandomizeCollectablePosition(collectable);
                    _collectables.Add(collectable);
                }
            }
        }

        /// <summary>
        /// When a collectable is collected, it calls this method so it can be removed from the collectables list.
        /// </summary>
        /// <param name="collectable"></param> The collectable that is to be removed.
        public void RemoveCollectableFromList(Collectable collectable)
        {
            _collectables.Remove(collectable);
        }

        /// <summary>
        /// This method randomizes the location of the parameter collectable to within
        /// a rectangle formed between two points in the scene.
        /// </summary>
        /// <param name="collectable"></param> The collectable that is to be moved.
        private void RandomizeCollectablePosition(Collectable collectable)
        {
            collectable.transform.position = new Vector3(Random.Range(_upperLeftCorner.position.x, _lowerRightCorner.position.x),
                _collectablePositionY,
                Random.Range(_upperLeftCorner.position.z, _lowerRightCorner.position.z));
        }

        /// <summary>
        /// The method is used for updating a score text objects. WinOrLose is then called,
        /// in case the player has won via score.
        /// </summary>
        private void UpdateScore()
        {
            _scoreText.text = string.Format("Score " + _score);
            int remainingScore = Mathf.Clamp(_scoreToWin - _score, 0, _scoreToWin);
            _remainingScoreToWinText.text = string.Format("Remaining score to win " + remainingScore);
            WinOrLose();
        }

        /// <summary>
        /// This method adds to the score (the value is read from the collectable),
        /// removes the collectable from the collectables list,
        /// destroys the collectable and
        /// updates the score text.
        /// </summary>
        /// <param name="collectable"></param> The collectable that calls this method.
        public void CollectableCollected(Collectable collectable)
        {
            _score += collectable.Score;
            RemoveCollectableFromList(collectable);
            Destroy(collectable.gameObject);
            UpdateScore();
        }

        /// <summary>
        /// This method determines if the winning or losing conditions are met.
        /// If the player either wins or loses, the game is paused and
        /// a winning or losing message is displayed.
        /// </summary>
        private void WinOrLose()
        {
            if (_playerRemainingLives == 0)
            {
                Time.timeScale = 0f;
                _winOrLoseText.gameObject.SetActive(true);
                _winOrLoseText.text = string.Format("DEFEAT!");
            }

            if (_score >= _scoreToWin)
            {
                Time.timeScale = 0f;
                _winOrLoseText.gameObject.SetActive(true);
                _winOrLoseText.text = string.Format("VICTORY!");
            }
        }
    }
}