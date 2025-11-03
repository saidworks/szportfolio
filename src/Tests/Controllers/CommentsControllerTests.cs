using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioCMS.API.Controllers;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Services;
using PortfolioCMS.DataAccess.Entities;

namespace PortfolioCMS.Tests.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly Mock<ILogger<CommentsController>> _mockLogger;
    private readonly CommentsController _controller;

    public CommentsControllerTests()
    {
        _mockCommentService = new Mock<ICommentService>();
        _mockLogger = new Mock<ILogger<CommentsController>>();
        _controller = new CommentsController(_mockCommentService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCommentsForArticle_ShouldReturnOkWithComments()
    {
        // Arrange
        var articleId = 1;
        var comments = new List<Comment>
        {
            new() { Id = 1, ArticleId = articleId, AuthorName = "John", Status = CommentStatus.Approved },
            new() { Id = 2, ArticleId = articleId, AuthorName = "Jane", Status = CommentStatus.Approved }
        };

        _mockCommentService.Setup(s => s.GetApprovedCommentsForArticleAsync(articleId))
            .ReturnsAsync(comments);

        // Act
        var result = await _controller.GetCommentsForArticle(articleId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedComments = okResult.Value.Should().BeAssignableTo<IEnumerable<CommentDto>>().Subject;
        returnedComments.Should().HaveCount(2);
    }

    [Fact]
    public async Task SubmitComment_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var submitDto = new SubmitCommentDto
        {
            ArticleId = 1,
            AuthorName = "Test User",
            AuthorEmail = "test@example.com",
            Content = "Great article!"
        };

        var submittedComment = new Comment
        {
            Id = 1,
            ArticleId = submitDto.ArticleId,
            AuthorName = submitDto.AuthorName,
            AuthorEmail = submitDto.AuthorEmail,
            Content = submitDto.Content,
            Status = CommentStatus.Pending
        };

        _mockCommentService.Setup(s => s.SubmitCommentAsync(It.IsAny<Comment>()))
            .ReturnsAsync(submittedComment);

        // Act
        var result = await _controller.SubmitComment(submitDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedComment = createdResult.Value.Should().BeOfType<CommentDto>().Subject;
        returnedComment.AuthorName.Should().Be("Test User");
        returnedComment.Status.Should().Be(CommentStatus.Pending);
    }

    [Fact]
    public async Task SubmitComment_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var submitDto = new SubmitCommentDto(); // Invalid - missing required fields
        _controller.ModelState.AddModelError("AuthorName", "Required");

        // Act
        var result = await _controller.SubmitComment(submitDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPendingComments_ShouldReturnOkWithPendingComments()
    {
        // Arrange
        var pendingComments = new List<Comment>
        {
            new() { Id = 1, AuthorName = "User 1", Status = CommentStatus.Pending },
            new() { Id = 2, AuthorName = "User 2", Status = CommentStatus.Pending }
        };

        _mockCommentService.Setup(s => s.GetPendingCommentsAsync())
            .ReturnsAsync(pendingComments);

        // Act
        var result = await _controller.GetPendingComments();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedComments = okResult.Value.Should().BeAssignableTo<IEnumerable<CommentDto>>().Subject;
        returnedComments.Should().HaveCount(2);
        returnedComments.Should().OnlyContain(c => c.Status == CommentStatus.Pending);
    }

    [Fact]
    public async Task ApproveComment_WhenCommentExists_ShouldReturnOk()
    {
        // Arrange
        var commentId = 1;
        _mockCommentService.Setup(s => s.ApproveCommentAsync(commentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApproveComment(commentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ApproveComment_WhenCommentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var commentId = 999;
        _mockCommentService.Setup(s => s.ApproveCommentAsync(commentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ApproveComment(commentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RejectComment_WhenCommentExists_ShouldReturnOk()
    {
        // Arrange
        var commentId = 1;
        _mockCommentService.Setup(s => s.RejectCommentAsync(commentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectComment(commentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RejectComment_WhenCommentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var commentId = 999;
        _mockCommentService.Setup(s => s.RejectCommentAsync(commentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RejectComment(commentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteComment_WhenCommentExists_ShouldReturnNoContent()
    {
        // Arrange
        var commentId = 1;
        _mockCommentService.Setup(s => s.DeleteCommentAsync(commentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteComment_WhenCommentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var commentId = 999;
        _mockCommentService.Setup(s => s.DeleteCommentAsync(commentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}