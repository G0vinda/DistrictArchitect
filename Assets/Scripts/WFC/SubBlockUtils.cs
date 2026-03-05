using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public static class SubBlockUtils
    {
        public static SubBlockType GetTypeFromCoordinates(Vector3Int coordinates)
        {
            var crossSum = Math.Abs(coordinates.x) + Math.Abs(coordinates.y) + Math.Abs(coordinates.z);
            if (crossSum == 0) // Center
            {
                return SubBlockType.Center;
            }
            if (crossSum == 1) // Face
            {
                return coordinates.y switch
                {
                    1 => SubBlockType.TopFace,
                    -1 => SubBlockType.BottomFace,
                    _ => SubBlockType.MiddleFace
                };
            }
            if (crossSum == 2) // Edge
            {
                return coordinates.y switch
                {
                    1 => SubBlockType.TopEdge,
                    -1 => SubBlockType.BottomEdge,
                    _ => SubBlockType.MiddleEdge
                };
            }
            if (crossSum == 3)
            {
                return coordinates.y switch
                {
                    1 => SubBlockType.TopCorner,
                    -1 => SubBlockType.BottomCorner,
                    _ => throw new ArgumentException()
                };
            }
            
            throw new ArgumentException($"Coordinates {coordinates} do not match the subBlockType coordinate format.");
        }

        public static bool TryExtractSubBlockTypeFromName(string name, out SubBlockType subBlockType)
        {
            var abbreviationPairs = new Dictionary<string, SubBlockType>
            {
                { "_TC", SubBlockType.TopCorner },
                { "_BC", SubBlockType.BottomCorner },
                { "_TE", SubBlockType.TopEdge },
                { "_ME", SubBlockType.MiddleEdge },
                { "_BE", SubBlockType.BottomEdge },
                { "_TF", SubBlockType.TopFace },
                { "_MF", SubBlockType.MiddleFace },
                { "_BF", SubBlockType.BottomFace },
                { "_C", SubBlockType.Center }
            };

            foreach (var (abbreviation, type) in abbreviationPairs)
            {
                if (name.Contains(abbreviation))
                {
                    subBlockType = type;
                    return true;
                }
            }
            
            subBlockType = default;
            return false;
        }

        public static bool TryExtractBuildingTypeFromName(string name, out BuildingType buildingType)
        {
            var prefixPairs = new Dictionary<string, BuildingType>
            {
                {"R_", BuildingType.Residential},
                {"G_", BuildingType.Greenhouse},
                {"F_", BuildingType.Factory},
                {"M_", BuildingType.MailBuilding},
                {"S_", BuildingType.ShopBuilding}
            };

            foreach (var (prefix, type) in prefixPairs)
            {
                if (name.StartsWith(prefix))
                {
                    buildingType = type;
                    return true;
                }
            }
            
            buildingType = default;
            return false;
        }
    }
}