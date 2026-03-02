using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 超出视距时禁用 MonsterController、Renderer、Collider，减少开销。
    /// </summary>
    [RequireComponent(typeof(MonsterController))]
    public class DistanceCulling : MonoBehaviour
    {
        [SerializeField] private float _cullDistance = 30f;
        [SerializeField] private Transform _playerReference;

        private MonsterController _monster;
        private Renderer _renderer;
        private Collider _collider;
        private bool _wasVisible = true;

        private void Awake()
        {
            _monster = GetComponent<MonsterController>();
            _renderer = GetComponentInChildren<Renderer>();
            _collider = GetComponentInChildren<Collider>();
        }

        private void Update()
        {
            if (_playerReference == null)
                _playerReference = FindObjectOfType<LegendaryCharacterController>()?.transform;
            if (_playerReference == null) return;

            float dist = Vector3.Distance(transform.position, _playerReference.position);
            bool visible = dist <= _cullDistance;

            if (visible != _wasVisible)
            {
                _wasVisible = visible;
                if (_monster != null) _monster.enabled = visible;
                if (_renderer != null) _renderer.enabled = visible;
                if (_collider != null) _collider.enabled = visible;
            }
        }
    }