using Xunit;

namespace Demo.Tests
{
    public class CalculadoraTest
    {
        [Fact]
        public void Calculadora_Sum_MustCalculateSum()
        {
            // Arrange
            var calculadora = new Calculadora();

            // Act
            var result = calculadora.Somar(15.5, 0.3);

            // Assert
            Assert.Equal(15.8, result);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 2, 4)]
        [InlineData(4, 2, 6)]
        [InlineData(7, 3, 10)]
        [InlineData(6, 6, 12)]
        [InlineData(9, 9, 18)]
        public void Calculadora_Sum_MustCalculateMultipleSum(double a, double b, double expected)
        {
            // Arrange
            var calculadora = new Calculadora();

            // Act
            var result = calculadora.Somar(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculadora_Divide_MustDivideNumbers()
        {
            // Arrange
            var calculadora = new Calculadora();

            // Act
            var result = calculadora.Dividr(10, 3);

            // Assert
            Assert.True(3.3 == result);
        }
    }
}