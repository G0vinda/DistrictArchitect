using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    [UxmlElement]
    public partial class AllowedNeighborList : VisualElement
    {
        private VisualElement _listContainer;
        private VisualElement _indentationContainer;

        private WfcConfig _wfcConfig;
        private DataTable _subBlockTable;
        private DataRow _mainSubBlockRow;
        private int _mainSubBlockId;
        private Dictionary<Vector3Int, string> _headerSuffixByDirection;
        
        public AllowedNeighborList()
        {
        }
        
        public AllowedNeighborList(WfcConfig wfcConfig, int subBlockId)
        {
            _wfcConfig = wfcConfig;
            _subBlockTable = wfcConfig.SubBlockTable;
            _mainSubBlockRow = _subBlockTable.Rows.Find(subBlockId);
            _mainSubBlockId = subBlockId;
            
            _indentationContainer = new VisualElement();
            _indentationContainer.name = "IndentationContainer";
            _indentationContainer.AddToClassList("indentation");
            Add(_indentationContainer);
            
            _listContainer = new VisualElement();
            _listContainer.name = "ListContainer";
            _listContainer.AddToClassList("list");
            Add(_listContainer);

            _headerSuffixByDirection = new Dictionary<Vector3Int, string>();
            _headerSuffixByDirection.Add(Vector3Int.forward, "<color=#264EFF>+Z</color>");
            _headerSuffixByDirection.Add(Vector3Int.back, "<color=#264EFF>-Z</color>");
            _headerSuffixByDirection.Add(Vector3Int.right, "<color=#FF0037>+X</color>");
            _headerSuffixByDirection.Add(Vector3Int.left, "<color=#FF0037>-X</color>");
            _headerSuffixByDirection.Add(Vector3Int.up, "<color=#6ABF8B>+Y</color>");
            _headerSuffixByDirection.Add(Vector3Int.down, "<color=#6ABF8B>-Y</color>");

            CreateNeighborSection(Vector3Int.forward, "Front");
            CreateNeighborSection(Vector3Int.back, "Back");
            CreateNeighborSection(Vector3Int.up,"Top");
            CreateNeighborSection(Vector3Int.down, "Bottom");
            CreateNeighborSection(Vector3Int.right, "Right");
            CreateNeighborSection(Vector3Int.left, "Left");
        }

        private void CreateNeighborSection(Vector3Int direction, string directionName)
        {
            var header = new Label
            {
                name = $"{directionName}Header",
                text = $"{directionName} {_headerSuffixByDirection[direction]}"
            };
            header.AddToClassList("header");
            _listContainer.Add(header);
            var subListContainer = new VisualElement
            {
                name = $"{directionName}ListContainer"
            };
            
            var subBlocks = _subBlockTable.AsEnumerable();
            
            var neighborTypeInDirection = ((SubBlockType)_mainSubBlockRow[WfcConfig.TYPE_COLUMN_INDEX]).GetNeighborTypeInDirection(direction);
            var allowedNeighborRowValue = _mainSubBlockRow[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[direction]];
            var allowedNeighborsInDirection =
                allowedNeighborRowValue != null ? (int[])allowedNeighborRowValue : Array.Empty<int>();
            
            var emptySpaceIsAllowed = Array.Exists(allowedNeighborsInDirection, neighborId => neighborId == WfcConfig.EMPTY_SUB_BLOCK_ID);
            var emptyNeighborListItem = CreateNeighborListElement(
                "Empty Space",
                null,
                emptySpaceIsAllowed,
                () => _wfcConfig.AddAllowedNeighbor(WfcConfig.EMPTY_SUB_BLOCK_ID, _mainSubBlockId, direction),
                () => _wfcConfig.RemoveAllowedNeighbor(WfcConfig.EMPTY_SUB_BLOCK_ID, _mainSubBlockId, direction));
            
            subListContainer.Add(emptyNeighborListItem);
            
            var possibleNeighborsInDirection = subBlocks.Where(row => (SubBlockType)row[WfcConfig.TYPE_COLUMN_INDEX] == neighborTypeInDirection).ToList();
            foreach (var possibleNeighborInDirection in possibleNeighborsInDirection)
            {
                var prefab = (GameObject)possibleNeighborInDirection[WfcConfig.PREFAB_COLUMN_INDEX];
                var possibleNeighborId = (int)possibleNeighborInDirection[WfcConfig.ID_COLUMN_INDEX];
                var isAllowed = Array.Exists(allowedNeighborsInDirection, neighborId => neighborId == possibleNeighborId); 
                var neighborListElement = CreateNeighborListElement(
                    prefab.name, 
                    prefab, 
                    isAllowed,
                    () => _wfcConfig.AddAllowedNeighbor(possibleNeighborId, _mainSubBlockId, direction),
                    () => _wfcConfig.RemoveAllowedNeighbor(possibleNeighborId, _mainSubBlockId, direction)
                    );
                
                subListContainer.Add(neighborListElement);
            }
            _listContainer.Add(subListContainer);
        }

        private static AllowedNeighborListElement CreateNeighborListElement(string elementName, GameObject prefab, bool isAllowed, Action switchAddCallback, Action switchRemoveCallback)
        {
            var neighborListElement = new AllowedNeighborListElement(elementName, prefab);
            var allowanceSwitch = neighborListElement.Q<Switch>("AllowanceSwitch");
            allowanceSwitch.Value = isAllowed;
            neighborListElement.EnableInClassList("allowed-neighbor", isAllowed);
            allowanceSwitch.ValueChanged += newValue =>
            {
                if (newValue)
                {
                    switchAddCallback?.Invoke();
                }
                else
                {
                    switchRemoveCallback?.Invoke();
                }
                neighborListElement.EnableInClassList("allowed-neighbor", newValue);
            };
            return neighborListElement;
        }
    }
}