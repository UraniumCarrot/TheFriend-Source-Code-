using System;
using TheFriend;
using UnityEngine;
using Xunit;
using Xunit.Abstractions;

namespace TheFriendTests
{
    public class Tests(ITestOutputHelper testConsole)
    {
        [Fact]
        public void Test1()
        {
            testConsole.WriteLine("Hello silly world!");
            Assert.True(true);
        }


        //Not a proper unit test, but helpful for quick testing.
        //I'll let it stay here as an example of what's possible, for now
        //For proper tests you should use Assert, to make tests fail when conditions are not met
        [Theory]
        [InlineData(0.1f, 0.25f, 1f, 1f, 1f, 1f)]
        [InlineData(0.5f, 0.5f, 1f, 0.25f, 0.75f, 0.25f)]
        public void RecolorMagically_Testing(float r, float g, float b, float r2, float g2, float b2)
        {
            var from = new Color(r, g, b);
            var to = new Color(r2, g2, b2);

            var result = from.RecolorMagically(to);
            testConsole.WriteLine("From " + from.ToString());
            testConsole.WriteLine( "To " + to.ToString());
            testConsole.WriteLine("Result " + result.ToString());
        }
    }
}