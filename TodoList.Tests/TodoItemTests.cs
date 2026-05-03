using Moq;
using TodoApp.Models;

namespace TodoList.Tests
{
    public class TodoItemTests
    {
        [Fact]
        public void Constructor_ClockProvided_SetsLastUpdateFromClock()
        {
            // Arrange
            var expectedTime = new DateTime(2026, 5, 3, 13, 30, 0);
            var clockMock = new Mock<IClock>();
            clockMock.SetupGet(clock => clock.Now).Returns(expectedTime);

            // Act
            var todoItem = new TodoItem("Test task", clockMock.Object);

            // Assert
            Assert.Equal("Test task", todoItem.Text);
            Assert.Equal(TodoStatus.NotStarted, todoItem.Status);
            Assert.Equal(expectedTime, todoItem.LastUpdate);
        }

        [Fact]
        public void UpdateText_NewTextProvided_UpdatesTextAndLastUpdate()
        {
            // Arrange
            var initialTime = new DateTime(2026, 5, 3, 13, 0, 0);
            var updatedTime = new DateTime(2026, 5, 3, 14, 0, 0);
            var clockMock = new Mock<IClock>();
            clockMock.SetupSequence(clock => clock.Now)
                .Returns(initialTime)
                .Returns(updatedTime);
            var todoItem = new TodoItem("Old text", clockMock.Object);

            // Act
            todoItem.UpdateText("New text");

            // Assert
            Assert.Equal("New text", todoItem.Text);
            Assert.Equal(updatedTime, todoItem.LastUpdate);
        }

        [Fact]
        public void SetStatus_NewStatusProvided_UpdatesStatusAndLastUpdate()
        {
            // Arrange
            var initialTime = new DateTime(2026, 5, 3, 15, 0, 0);
            var updatedTime = new DateTime(2026, 5, 3, 16, 0, 0);
            var clockMock = new Mock<IClock>();
            clockMock.SetupSequence(clock => clock.Now)
                .Returns(initialTime)
                .Returns(updatedTime);
            var todoItem = new TodoItem("Task", clockMock.Object);

            // Act
            todoItem.SetStatus(TodoStatus.Completed);

            // Assert
            Assert.Equal(TodoStatus.Completed, todoItem.Status);
            Assert.Equal(updatedTime, todoItem.LastUpdate);
        }
    }
}
