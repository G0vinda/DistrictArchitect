using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace WFC.TestScene
{
    public class BuildingSelection : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown selectionDropdown;
        
        public BuildingType SelectedBuildingType { get; private set; }

        private void OnEnable()
        {
            selectionDropdown.onValueChanged.AddListener(NewBuildingTypeSelected);
        }

        private void OnDisable()
        {
            selectionDropdown.onValueChanged.RemoveListener(NewBuildingTypeSelected);
        }

        void Start()
        {
            selectionDropdown.ClearOptions();
            selectionDropdown.AddOptions(Enum.GetNames(typeof(BuildingType)).ToList());
        }

        private void NewBuildingTypeSelected(int newSelection)
        {
            SelectedBuildingType = (BuildingType)newSelection;
        }
    }
}
