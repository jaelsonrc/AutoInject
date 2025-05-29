using AutoInject.Attributes;
using AutoInject.Tests.TestClasses;
using System.Reflection;
using Xunit;

namespace AutoInject.Tests.Attributes
{
    public class AttributesTests
    {
        [Fact]
        public void AutoInjectAttribute_ShouldBeAppliedToClass()
        {
            // Arrange & Act
            var attribute = typeof(TestServiceWithAutoInject).GetCustomAttribute<AutoInjectAttribute>();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void AutoInjectAttribute_ShouldNotBeAppliedToClassWithoutAttribute()
        {
            // Arrange & Act
            var attribute = typeof(TestServiceWithoutAutoInject).GetCustomAttribute<AutoInjectAttribute>();

            // Assert
            Assert.Null(attribute);
        }

        [Fact]
        public void InjectableAttribute_ShouldBeAppliedToProperty()
        {
            // Arrange
            var property = typeof(TestServiceWithAutoInject).GetProperty("Repository", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

            // Act
            var field = typeof(TestServiceWithAutoInject).GetField("_repository", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var attribute = field?.GetCustomAttribute<InjectableAttribute>();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void InjectableAttribute_ShouldBeAppliedToField()
        {
            // Arrange & Act
            var field = typeof(TestServiceWithFields).GetField("_notificationService", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var attribute = field?.GetCustomAttribute<InjectableAttribute>();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void InjectableAttribute_ShouldNotBeAppliedToNonInjectableProperty()
        {
            // Arrange & Act
            var field = typeof(TestServiceWithNonInjectableProperties).GetField("_emailService", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var attribute = field?.GetCustomAttribute<InjectableAttribute>();

            // Assert
            Assert.Null(attribute);
        }

        [Fact]
        public void AutoInjectAttribute_ShouldHaveCorrectAttributeUsage()
        {
            // Arrange & Act
            var attributeUsage = typeof(AutoInjectAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            Assert.NotNull(attributeUsage);
            Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
        }

        [Fact]
        public void InjectableAttribute_ShouldHaveCorrectAttributeUsage()
        {
            // Arrange & Act
            var attributeUsage = typeof(InjectableAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            Assert.NotNull(attributeUsage);
            Assert.Equal(AttributeTargets.Property | AttributeTargets.Field, attributeUsage.ValidOn);
        }
    }
} 