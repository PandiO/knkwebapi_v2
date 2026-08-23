using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using knkwebapi_v2.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace knkwebapi_v2.Tests.Repositories
{
    public class FormSubmissionProgressRepositoryTests
    {
        private readonly KnKDbContext _context;
        private readonly FormSubmissionProgressRepository _repository;

        public FormSubmissionProgressRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<KnKDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new KnKDbContext(options);
            _repository = new FormSubmissionProgressRepository(_context);
        }

        [Fact]
        public async Task DeleteCompletedOlderThanAsync_DeletesStaleRootAndDescendantsInOrder()
        {
            // Arrange
            var user = new User
            {
                Username = "retention-user",
                Email = "retention@example.com",
                PasswordHash = "hash"
            };

            var formConfig = new FormConfiguration
            {
                Name = "Test Progress Form",
                EntityTypeName = "TestEntity",
                IsDefault = true
            };

            _context.Users.Add(user);
            _context.FormConfigurations.Add(formConfig);
            await _context.SaveChangesAsync();

            var cutoff = DateTime.UtcNow.AddDays(-14);

            var parent = new FormSubmissionProgress
            {
                UserId = user.Id,
                FormConfigurationId = formConfig.Id,
                Status = "Completed",
                CompletedAt = cutoff.AddDays(-10),
                ParentProgressId = null
            };

            var child = new FormSubmissionProgress
            {
                UserId = user.Id,
                FormConfigurationId = formConfig.Id,
                Status = "Completed",
                CompletedAt = cutoff.AddDays(2),
                ParentProgressId = parent.Id
            };

            _context.FormSubmissionProgresses.Add(parent);
            await _context.SaveChangesAsync();

            child.ParentProgressId = parent.Id;
            _context.FormSubmissionProgresses.Add(child);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _repository.DeleteCompletedOlderThanAsync(cutoff);

            // Assert
            deletedCount.Should().Be(2);
            (await _context.FormSubmissionProgresses.CountAsync()).Should().Be(0);
        }
    }
}
