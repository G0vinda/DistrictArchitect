using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "WFCConfig", menuName = "WFCConfig", order = 0)]
    public class WfcConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        private DataTable subBlockTable;
        private bool tableUpdated = true;

        [SerializeField] private int[] serializedIds;
        [SerializeField] private SubBlockType[] serializedTypes;
        [SerializeField] private GameObject[] serializedPrefabs;
        [SerializeField] private float[] serializedProbabilities;
        [SerializeField] private SerializedAllowedNeighborArrays[] serializedNeighborArrays;

        private const int ID_COLUMN_INDEX = 0;
        private const int TYPE_COLUMN_INDEX = 1;
        private const int PREFAB_COLUMN_INDEX = 2;
        private const int PROBABILITY_COLUMN_INDEX = 3;
        private const int POSITIVE_Z_NEIGHBOR_COLUMN_INDEX = 4;
        private const int NEGATIVE_Z_NEIGHBOR_COLUMN_INDEX = 5;
        private const int POSITIVE_X_NEIGHBOR_COLUMN_INDEX = 6;
        private const int NEGATIVE_X_NEIGHBOR_COLUMN_INDEX = 7;
        private const int POSITIVE_Y_NEIGHBOR_COLUMN_INDEX = 8;
        private const int NEGATIVE_Y_NEIGHBOR_COLUMN_INDEX = 9;

        private static readonly Dictionary<Vector3Int, int> NEIGHBOR_COLUMN_INDEX_BY_DIRECTION = new()
        {
            { Vector3Int.forward, POSITIVE_Z_NEIGHBOR_COLUMN_INDEX },
            { Vector3Int.back, NEGATIVE_Z_NEIGHBOR_COLUMN_INDEX },
            { Vector3Int.right, POSITIVE_X_NEIGHBOR_COLUMN_INDEX },
            { Vector3Int.left, NEGATIVE_X_NEIGHBOR_COLUMN_INDEX },
            { Vector3Int.up, POSITIVE_Y_NEIGHBOR_COLUMN_INDEX },
            { Vector3Int.down, NEGATIVE_Y_NEIGHBOR_COLUMN_INDEX },
        };

        public int GetSubBlockCount() => subBlockTable != null ? subBlockTable.Rows.Count : 0;

        private void OnEnable()
        {
            if (subBlockTable == null)
                SetupSubBlockTable();
        }

        public void OnBeforeSerialize()
        {
            if (subBlockTable == null || !tableUpdated)
                return;
                
            var rowCount = subBlockTable.Rows.Count;
            serializedIds = new int[rowCount];
            serializedTypes = new SubBlockType[rowCount];
            serializedPrefabs = new GameObject[rowCount];
            serializedProbabilities = new float[rowCount];
            serializedNeighborArrays = new SerializedAllowedNeighborArrays[rowCount];
            
            for (var i = 0; i < subBlockTable.Rows.Count; i++)
            {
                serializedIds[i] = (int)subBlockTable.Rows[i][ID_COLUMN_INDEX];
                serializedTypes[i] = (SubBlockType)subBlockTable.Rows[i][TYPE_COLUMN_INDEX];
                serializedPrefabs[i] = (GameObject)subBlockTable.Rows[i][PREFAB_COLUMN_INDEX];
                serializedProbabilities[i] = (float)subBlockTable.Rows[i][PROBABILITY_COLUMN_INDEX];
                serializedNeighborArrays[i] = new SerializedAllowedNeighborArrays
                {
                    positiveZ = (int[])subBlockTable.Rows[i][POSITIVE_Z_NEIGHBOR_COLUMN_INDEX],
                    negativeZ = (int[])subBlockTable.Rows[i][NEGATIVE_Z_NEIGHBOR_COLUMN_INDEX],
                    positiveX = (int[])subBlockTable.Rows[i][POSITIVE_X_NEIGHBOR_COLUMN_INDEX],
                    negativeX = (int[])subBlockTable.Rows[i][NEGATIVE_X_NEIGHBOR_COLUMN_INDEX],
                    positiveY = (int[])subBlockTable.Rows[i][POSITIVE_Y_NEIGHBOR_COLUMN_INDEX],
                    negativeY = (int[])subBlockTable.Rows[i][NEGATIVE_Y_NEIGHBOR_COLUMN_INDEX]
                };
            }

            tableUpdated = false;
        }

        public void OnAfterDeserialize()
        {
            if (subBlockTable == null)
                SetupSubBlockTable();
            
            if (AreAnySerializedArraysNull())
            {
                Debug.Log("On Deserialization at least one of the serialized arrays was null.");
                return;
            }
            
            if (!AreAllSerializedArraysTheSameLength())
            {
                Debug.LogError("On Deserialization the serialized arrays did not have the same length.");
                return;
            }
            
            var rowCount = serializedIds.Length;
            for (var i = 0; i < rowCount; i++)
            {
                var row = subBlockTable!.NewRow();
                row[ID_COLUMN_INDEX] = serializedIds[i];
                row[TYPE_COLUMN_INDEX] = serializedTypes[i];
                row[PREFAB_COLUMN_INDEX] = serializedPrefabs[i];
                row[PROBABILITY_COLUMN_INDEX] = serializedProbabilities[i];
                row[POSITIVE_Z_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].positiveZ;
                row[NEGATIVE_Z_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].negativeZ;
                row[POSITIVE_X_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].positiveX;
                row[NEGATIVE_X_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].negativeX;
                row[POSITIVE_Y_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].positiveY;
                row[NEGATIVE_Y_NEIGHBOR_COLUMN_INDEX] = serializedNeighborArrays[i].negativeY;
                subBlockTable.Rows.Add(row);
            }
        }

        public void AddSubBlock(SubBlockType type, GameObject prefab, float probability)
        {
            var newSubBlockRow = subBlockTable.NewRow();
            newSubBlockRow[TYPE_COLUMN_INDEX] = type;
            newSubBlockRow[PREFAB_COLUMN_INDEX] = prefab;
            newSubBlockRow[PROBABILITY_COLUMN_INDEX] = probability;
            subBlockTable.Rows.Add(newSubBlockRow);
            var newSubBlockId = (int)newSubBlockRow[ID_COLUMN_INDEX]; 
                
            var enumerableSubBlockRows = subBlockTable.AsEnumerable();
            var allDirections = Vector3Int.zero.Neighbours();
            foreach (var direction in allDirections)
            {
                var neighborTypeInDirection = type.GetNeighborTypeInDirection(direction);
                var subBlocksWithMatchingType = enumerableSubBlockRows
                    .Where(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == neighborTypeInDirection)
                    .Select(row => (int)row[ID_COLUMN_INDEX]).ToArray();
                newSubBlockRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]] = subBlocksWithMatchingType;

                var oppositeDirection = direction * -1;
                foreach (var neighborId in subBlocksWithMatchingType)
                {
                    if (neighborId == newSubBlockId)
                        continue;
                    
                    var neighborRow = subBlockTable.Rows.Find(neighborId);
                    var allowedNeighbors = (int[])neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[oppositeDirection]];
                    var extendedAllowedNeighbors = new int[allowedNeighbors.Length + 1];
                    Array.Copy(allowedNeighbors, extendedAllowedNeighbors, allowedNeighbors.Length);
                    extendedAllowedNeighbors[^1] = newSubBlockId;
                    neighborRow[NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[oppositeDirection]] = extendedAllowedNeighbors;
                }
            }
            
            tableUpdated = true;
            EditorUtility.SetDirty(this);
        }

        public GameObject GetSubBlockPrefabForType(SubBlockType type)
        {
            return (GameObject)subBlockTable.AsEnumerable().First(row => (SubBlockType)row[TYPE_COLUMN_INDEX] == type)[PREFAB_COLUMN_INDEX];
        }
        
        private void SetupSubBlockTable()
        {
            subBlockTable = new DataTable();
            var idColumn = new DataColumn();
            idColumn.ColumnName = "Id";
            idColumn.DataType = typeof(int);
            idColumn.ReadOnly = true;
            idColumn.Unique = true;
            idColumn.AutoIncrement = true;
            subBlockTable.Columns.Add(idColumn);
            idColumn.SetOrdinal(ID_COLUMN_INDEX);
            subBlockTable.PrimaryKey = new[] { idColumn };
            
            var typeColumn = new DataColumn();
            typeColumn.ColumnName = "Type";
            typeColumn.DataType = typeof(SubBlockType);
            typeColumn.ReadOnly = true;
            subBlockTable.Columns.Add(typeColumn);
            typeColumn.SetOrdinal(TYPE_COLUMN_INDEX);
            
            var prefabColumn = new DataColumn();
            prefabColumn.ColumnName = "Prefab";
            prefabColumn.DataType = typeof(GameObject);
            prefabColumn.ReadOnly = true;
            subBlockTable.Columns.Add(prefabColumn);
            prefabColumn.SetOrdinal(PREFAB_COLUMN_INDEX);
            
            var probabilityColumn = new DataColumn();
            probabilityColumn.ColumnName = "Probability";
            probabilityColumn.DataType = typeof(float);
            subBlockTable.Columns.Add(probabilityColumn);
            probabilityColumn.SetOrdinal(PROBABILITY_COLUMN_INDEX);
            
            foreach (var (direction, allowedNeighborColumnIndex) in NEIGHBOR_COLUMN_INDEX_BY_DIRECTION)
            {
                var allowedNeighborsColumn = new DataColumn();
                allowedNeighborsColumn.ColumnName = $"Allowed Neighbors {direction}";
                allowedNeighborsColumn.DataType = typeof(int[]);
                subBlockTable.Columns.Add(allowedNeighborsColumn);
                allowedNeighborsColumn.SetOrdinal(allowedNeighborColumnIndex);
            }
        }

        private bool AreAnySerializedArraysNull()
        {
            return serializedIds == null ||
                   serializedTypes == null ||
                   serializedPrefabs == null ||
                   serializedProbabilities == null ||
                   serializedNeighborArrays == null;
        }

        private bool AreAllSerializedArraysTheSameLength()
        {
            return serializedIds.Length == serializedTypes.Length &&
                   serializedIds.Length == serializedPrefabs.Length &&
                   serializedIds.Length == serializedProbabilities.Length &&
                   serializedIds.Length == serializedNeighborArrays.Length;
        }

        [ContextMenu("Print All SubBlocks")]
        public void PrintAllSubBlocks()
        {
            foreach (DataRow row in subBlockTable.Rows)
            {
                Debug.Log($"{row[ID_COLUMN_INDEX]}, {(SubBlockType)row[TYPE_COLUMN_INDEX]}: {((GameObject)row[PREFAB_COLUMN_INDEX]).name}");
            }
        }

        [Serializable]
        private class SerializedAllowedNeighborArrays
        {
            public int[] positiveZ;
            public int[] negativeZ;
            public int[] positiveX;
            public int[] negativeX;
            public int[] positiveY;
            public int[] negativeY;
        }
    }
}


