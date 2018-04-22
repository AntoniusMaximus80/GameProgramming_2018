using UnityEngine;

namespace TankGame
{
    public class Collectable : MonoBehaviour
    {
        [SerializeField, Tooltip("The hover amplitude of the collectable.")]
        private float _hoverAmplitude;

        [SerializeField, Tooltip("The hover speed of the collectable.")]
        private float _hoverSpeed;

        [SerializeField, Tooltip("The rotation speed of the collectable.")]
        private float _rotationSpeed;

        private GameStateObserver _gameStateObserver;

        private float _hoverTime,
            _hoverTimeOffset;

        private Vector3 _rotationVector3;

        [SerializeField, Tooltip("How much score is awarded for collecting the collectable.")]
        private int _score;

        public int Score
        {
            get
            {
                return _score;
            }
        }

        void Start()
        {
            _hoverTimeOffset = Random.Range(-1f, 1f);
            _rotationVector3 = new Vector3(0f, _rotationSpeed, 0f);
            _gameStateObserver = FindObjectOfType<GameStateObserver>();
        }

        void Update()
        {
            HoverAndRotate();
        }

        private void HoverAndRotate()
        {
            // Hover.
            _hoverTime += Time.deltaTime;
            Vector3 _newPosition = transform.position;
            _newPosition.y += _hoverAmplitude * Mathf.Sin((_hoverTime + _hoverTimeOffset) * _hoverSpeed) * Time.deltaTime;
            transform.position = _newPosition;

            // Rotation.
            transform.Rotate(_rotationVector3 * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Collectable > OnTriggetEnter");
            _gameStateObserver.CollectableCollected(this);
        }
    }
}