using Xunit;
using Microsoft.EntityFrameworkCore;
using BHX.Sale.Infrastructure.Data;
using BHX.Sale.Infrastructure.Repositories;
using BHX.Sale.Domain.Entities;
using BHX.Sale.Domain.Enums;

namespace BHX.Sale.Infrastructure.Test.Repositories
{
    public class BaseRepositoryAsyncTest
    {
        private readonly BHX.SaleDbContext _BHX.SaleDbContext;
        private readonly UnitOfWork _unitOfWork;

        public BaseRepositoryAsyncTest()
        {
            var options = new DbContextOptionsBuilder<BHX.SaleDbContext>().UseInMemoryDatabase(databaseName: "BHX.SaleDb").Options;
            _BHX.SaleDbContext = new BHX.SaleDbContext(options);
            _unitOfWork = new UnitOfWork(_BHX.SaleDbContext);
        }

        [Fact]
        public async void Given_ValidData_When_AddAsync_Then_SuccessfullyInsertData()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Nilav",
                LastName = "Patel",
                EmailId = "nilavpatel1992@gmail.com",
                Password = "Test123",
                Status = UserStatus.Active,
                CreatedBy = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.UtcNow
            };

            // Act
            var result = await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            Assert.Equal(result, _BHX.SaleDbContext.Users.Find(result.Id));
        }
    }
}