using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "WFCConfig", menuName = "WFCConfig", order = 0)]
    public class WfcConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private SerializableSubBlocksByPositionDictionary serializedSubBlocks;

        [NonSerialized]
        public Dictionary<Vector3Int, Dictionary<uint, GameObject>> SubBlocks = new();

        [SerializeField] private uint nextSubBlockId = 0;

        public void OnBeforeSerialize()
        {
            serializedSubBlocks = SerializableSubBlocksByPositionDictionary.FromNestedDictionary(SubBlocks);
        }

        public void OnAfterDeserialize()
        {
            SubBlocks = serializedSubBlocks.ToNestedDictionary();
        }

        public void AddSubBlock(Vector3Int position, GameObject prefab)
        {
            if (!SubBlocks.ContainsKey(position))
                SubBlocks.Add(position, new Dictionary<uint, GameObject>());
            
            SubBlocks[position].Add(nextSubBlockId++, prefab);
            Debug.Log($"Added SubBlock {prefab.name} with position {position} and id {nextSubBlockId - 1}");
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Print All SubBlocks")]
        public void PrintAllSubBlocks()
        {
            foreach (var (position, blocksById) in SubBlocks)
            {
                foreach (var (id, block) in blocksById)
                {
                    Debug.Log($"{id}, {(block ? block.name : "no name")} at {position}");                    
                }
            }
        }
        
        [Serializable]
        private class SerializableSubBlockByIdDictionary
        {
            [SerializeField] private List<uint> ids = new();
            [SerializeField] private List<GameObject> blocks = new();

            public Dictionary<uint, GameObject> ToDictionary()
            {
                var dict = new Dictionary<uint, GameObject>();
                for (var i = 0; i < ids.Count; i++)
                {
                    dict.Add(ids[i], blocks[i]);
                }
                return dict;
            }

            public static SerializableSubBlockByIdDictionary FromDictionary(Dictionary<uint, GameObject> dict)
            {
                var serializableDict = new SerializableSubBlockByIdDictionary
                {
                    ids = new List<uint>(),
                    blocks = new List<GameObject>()
                };
                foreach (var (id, block) in dict)
                {
                    serializableDict.ids.Add(id);
                    serializableDict.blocks.Add(block);
                }
                return serializableDict;
            }
        }

        [Serializable]
        private class SerializableSubBlocksByPositionDictionary
        {
            [SerializeField] private List<Vector3Int> positions;
            [SerializeField] private List<SerializableSubBlockByIdDictionary> blocksById;
            
            public Dictionary<Vector3Int, Dictionary<uint, GameObject>> ToNestedDictionary()
            {
                var dict = new Dictionary<Vector3Int, Dictionary<uint, GameObject>> ();
                for (var i = 0; i < positions.Count; i++)
                {
                    dict.Add(positions[i], blocksById[i].ToDictionary());
                }
                return dict;
            }

            public static SerializableSubBlocksByPositionDictionary FromNestedDictionary(Dictionary<Vector3Int, Dictionary<uint, GameObject>> dict)
            {
                var serializableDict = new SerializableSubBlocksByPositionDictionary
                {
                    positions = new List<Vector3Int>(),
                    blocksById = new List<SerializableSubBlockByIdDictionary>()
                };
                foreach (var (pos, blocksById) in dict)
                {
                    serializableDict.positions.Add(pos);
                    serializableDict.blocksById.Add(SerializableSubBlockByIdDictionary.FromDictionary(blocksById));
                }
                return serializableDict;
            }
        }
    }
}


