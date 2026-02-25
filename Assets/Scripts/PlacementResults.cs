using System.Collections.Generic;

namespace DefaultNamespace
{
    public struct PlacementResults
    {
        public readonly int PeopleCount;
        public readonly int FoodCount;
        public readonly int MoneyCount;

        public List<Shape> ExtraShapesToPlace;

        public PlacementResults(int peopleCount, int foodCount, int moneyCount)
        {
            PeopleCount = peopleCount;
            FoodCount = foodCount;
            MoneyCount = moneyCount;
            
            ExtraShapesToPlace = new List<Shape>();
        }
    }
}