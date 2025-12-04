using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class Vector3IntTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void RotateAroundY()
        {
            var v = new Vector3Int(0, 0, 1);
            var axis = Vector3Int.up;
            const int nRightAngles = 1;
        
            var rotatedV = v.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(1, 0, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(-v, rotatedV);

            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(-1, 0, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(v, rotatedV);
        }

        [Test]
        public void RotateAroundX()
        {
            var v = new Vector3Int(0, 0, 1);
            var axis = Vector3Int.right;
            const int nRightAngles = 1;
        
            var rotatedV = v.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(0, -1, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(-v, rotatedV);

            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(0, 1, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(v, rotatedV);
        }
        
        
        [Test]
        public void RotateAroundZ()
        {
            var v = new Vector3Int(0, 1, 0);
            var axis = Vector3Int.forward;
            const int nRightAngles = 1;
        
            var rotatedV = v.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(-1, 0, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(-v, rotatedV);

            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(new Vector3Int(1, 0, 0), rotatedV);
            
            rotatedV = rotatedV.Rotate90(axis, nRightAngles);
            Assert.AreEqual(v, rotatedV);
        }
        
        [Test]
        public void HalfRotations()
        {
            var v = new Vector3Int(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
            var axisX = Vector3Int.right;
            var axisY = Vector3Int.up;
            var axisZ = Vector3Int.forward;

            var rotatedX = v.Rotate90(axisX, 2);
            var rotatedY = v.Rotate90(axisY, 2);
            var rotatedZ = v.Rotate90(axisZ, 2);

            var expectedX = new Vector3Int(v.x, -v.y, -v.z);
            var expectedY = new Vector3Int(-v.x, v.y, -v.z);
            var expectedZ = new Vector3Int(-v.x, -v.y, v.z);

            Assert.AreEqual(expectedX, rotatedX);
            Assert.AreEqual(expectedY, rotatedY);
            Assert.AreEqual(expectedZ, rotatedZ);
        }

        [Test]
        public void ThreeQuarterRotations()
        {
            var v = new Vector3Int(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
            var axisX = Vector3Int.right;
            var axisY = Vector3Int.up;
            var axisZ = Vector3Int.forward;

            var rotatedX = v.Rotate90(axisX, 3);
            var rotatedY = v.Rotate90(axisY, 3);
            var rotatedZ = v.Rotate90(axisZ, 3);

            var expectedX = v.Rotate90(axisX, -1);
            var expectedY = v.Rotate90(axisY, -1);
            var expectedZ = v.Rotate90(axisZ, -1);

            Assert.AreEqual(expectedX, rotatedX);
            Assert.AreEqual(expectedY, rotatedY);
            Assert.AreEqual(expectedZ, rotatedZ);
        }

        [Test]
        public void FullRotations()
        {
            var v = new Vector3Int(Random.Range(-1000, 1000), Random.Range(-1000, 1000), Random.Range(-1000, 1000));
            var axisX = Vector3Int.right;
            var axisY = Vector3Int.up;
            var axisZ = Vector3Int.forward;

            var rotatedX = v.Rotate90(axisX, 4);
            var rotatedY = v.Rotate90(axisY, 4);
            var rotatedZ = v.Rotate90(axisZ, 4);

            Assert.AreEqual(v, rotatedX);
            Assert.AreEqual(v, rotatedY);
            Assert.AreEqual(v, rotatedZ);
        }
    }
}
