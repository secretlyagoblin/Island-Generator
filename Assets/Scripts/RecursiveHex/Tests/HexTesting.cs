using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RecursiveHex;

namespace Tests
{
    public class HexTesting
    {
        // A Test behaves as an ordinary method
        [Test]
        public void HexTestingSimplePasses()
        {
            // Use the Assert class to test conditions
            var hex = new HexGroup();

            var layer1 = new HexGroup().ForEach(x => new HexPayload() { Height = 1, Color = Color.red });
            var layer2 = layer1.Subdivide();

            //layer1.



        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}
