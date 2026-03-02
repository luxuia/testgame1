using System.Collections.Generic;
using UnityEngine;

namespace LegendaryTerrain
{
    /// <summary>
    /// 按玩家距离管理地形块加载/卸载。支持分帧加载避免卡顿。
    /// </summary>
    public class TerrainChunkManager : MonoBehaviour
    {
        [SerializeField] public string MapFileName = "0";
        [SerializeField] private int _chunkSize = 16;
        [SerializeField] private int _loadRadius = 2;
        [SerializeField] private int _unloadRadius = 3;
        [SerializeField] private Transform _playerReference;

        private BlockRegistry _blockRegistry;
        private TerrainGenerator _terrainGenerator;
        private readonly Dictionary<Vector2Int, TerrainChunk> _chunks = new Dictionary<Vector2Int, TerrainChunk>();
        private readonly Queue<Vector2Int> _pendingLoads = new Queue<Vector2Int>();
        private readonly HashSet<Vector2Int> _pendingSet = new HashSet<Vector2Int>();
        private Vector2Int _lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
        private float _checkInterval = 0.5f;
        private float _nextCheck;

        private void Awake()
        {
            _blockRegistry = new BlockRegistry();
            _terrainGenerator = new TerrainGenerator(_blockRegistry);
            if (_playerReference == null)
                _playerReference = FindObjectOfType<LegendaryCharacterController>()?.transform;
        }

        private void Start()
        {
            if (_terrainGenerator.LoadMap(MapFileName) == null)
                return;
            UpdateChunks();
        }

        private void Update()
        {
            if (Time.time < _nextCheck) return;
            _nextCheck = Time.time + _checkInterval;
            UpdateChunks();
        }

        private void UpdateChunks()
        {
            if (_playerReference == null)
                _playerReference = FindObjectOfType<LegendaryCharacterController>()?.transform;

            Vector2Int playerChunk;
            if (_playerReference == null)
                playerChunk = Vector2Int.zero;
            else
                playerChunk = WorldToChunk(_playerReference.position);
            if (playerChunk == _lastPlayerChunk) return;
            _lastPlayerChunk = playerChunk;

            var toLoad = new HashSet<Vector2Int>();
            for (int dx = -_loadRadius; dx <= _loadRadius; dx++)
            for (int dy = -_loadRadius; dy <= _loadRadius; dy++)
                toLoad.Add(playerChunk + new Vector2Int(dx, dy));

            var toUnload = new List<Vector2Int>();
            foreach (var kv in _chunks)
            {
                var dist = Mathf.Max(Mathf.Abs(kv.Key.x - playerChunk.x), Mathf.Abs(kv.Key.y - playerChunk.y));
                if (dist > _unloadRadius)
                    toUnload.Add(kv.Key);
            }

            foreach (var coord in toUnload)
            {
                if (_chunks.TryGetValue(coord, out var chunk))
                {
                    chunk.Destroy();
                    _chunks.Remove(coord);
                }
            }

            foreach (var coord in toLoad)
            {
                if (!_chunks.ContainsKey(coord) && !_pendingSet.Contains(coord))
                {
                    _pendingLoads.Enqueue(coord);
                    _pendingSet.Add(coord);
                }
            }
        }

        private void LateUpdate()
        {
            if (_pendingLoads.Count == 0) return;
            var coord = _pendingLoads.Dequeue();
            _pendingSet.Remove(coord);
            var chunk = _terrainGenerator.GenerateChunk(coord, _chunkSize, transform);
            if (chunk != null)
                _chunks[coord] = chunk;
        }

        private Vector2Int WorldToChunk(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / TerrainGenerator.BlockSize / _chunkSize);
            int z = Mathf.FloorToInt(worldPos.z / TerrainGenerator.BlockSize / _chunkSize);
            return new Vector2Int(x, z);
        }

        public BlockRegistry GetBlockRegistry() => _blockRegistry;
    }
}
