using System;
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
    }
}