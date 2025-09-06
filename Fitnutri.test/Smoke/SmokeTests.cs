using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitnutri.test.Smoke
{
    public class SmokeTests
    {
        [Fact]
        public void TestRunner_Deve_Estar_Funcionando()
        {
            // Arrange & Act
            var sum = 1 + 1;

            // Assert (AAA pattern)
            sum.Should().Be(2);
        }
    }
}
