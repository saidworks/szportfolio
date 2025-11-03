using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;
using PortfolioCMS.DataAccess.Repositories;

namespace PortfolioCMS.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<ILogger<CommentService>> _mockLogger;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockLogger = new Mock<ILogger<CommentService>>();
        
        _mockUnitOfWork.Setup(u => u.Comments).Returns(_mockCommentRepository.Object);
        
        _commentService = new CommentService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetApprovedCommentsForArticleAsync_ShouldReturnOnlyApprovedComments()
    {
        // Arrange
        var articleId = 1;
        var approvedComments = new List<Comment>
        {
            new() { Id = 1, ArticleId = articleId, Status = CommentStatus.Approved, AuthorName = "John" },
            new() { Id = 2, ArticleId = articleId, Status = CommentStatus.Approved, AuthorName = "Jane" }
        };

        _mockCommentRepository.Setup(r => r.GetApprovedCommentsForArticleAsync(articleId))
            .ReturnsAsync(approvedComments);

        // Act
        var result = await _commentService.GetApprovedCommentsForArticleAsync(articleId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status == CommentStatus.Approved);
        result.Should().OnlyContain(c => c.ArticleId == articleId);
    }

    [Fact]
    public async Task GetPendingCommentsAsync_ShouldReturnOnlyPendingComments()
    {
        // Arrange
        var pendingComments = new List<Comment>
        {
            new() { Id = 1, Status = CommentStatus.Pending, AuthorName = "Pending User 1" },
            new() { Id = 2, Status = CommentStatus.Pending, AuthorName = "Pending User 2" }
        };

        _mockCommentRepository.Setup(r => r.GetPendingCommentsAsync())
            .ReturnsAsync(pendingComments);

        // Act
        var result = await _commentService.GetPendingCommentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status == CommentStatus.Pending);
    }

    [Fact]
    public async Task SubmitCommentAsync_WithValidData_ShouldCreatePendingComment()
    {
        // Arrange
        var newComment = new Comment
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = "Great article!"
        };

        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.SubmitCommentAsync(newComment);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CommentStatus.Pending);
        result.SubmittedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveCommentAsync_WhenCommentExists_ShouldApproveComment()
    {
        // Arrange
        var commentId = 1;
        var pendingComment = new Comment
        {
            Id = commentId,
            Status = CommentStatus.Pending,
            AuthorName = "Test User"
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(pendingComment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.ApproveCommentAsync(commentId);

        // Assert
        result.Should().BeTrue();
        pendingComment.Status.Should().Be(CommentStatus.Approved);
        pendingComment.ApprovedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RejectCommentAsync_WhenCommentExists_ShouldRejectComment()
    {
        // Arrange
        var commentId = 1;
        var pendingComment = new Comment
        {
            Id = commentId,
            Status = CommentStatus.Pending,
            AuthorName = "Test User"
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(pendingComment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.RejectCommentAsync(commentId);

        // Assert
        result.Should().BeTrue();
        pendingComment.Status.Should().Be(CommentStatus.Rejected);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenCommentExists_ShouldDeleteComment()
    {
        // Arrange
        var commentId = 1;
        var existingComment = new Comment { Id = commentId, AuthorName = "To Delete" };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(existingComment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _commentService.DeleteCommentAsync(commentId);

        // Assert
        result.Should().BeTrue();
        _mockCommentRepository.Verify(r => r.Delete(existingComment), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SubmitCommentAsync_WithInvalidContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var invalidComment = new Comment
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = invalidContent
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _commentService.SubmitCommentAsync(invalidComment));
    }
}