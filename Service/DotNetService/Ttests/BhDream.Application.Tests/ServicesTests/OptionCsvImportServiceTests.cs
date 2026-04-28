using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using BhDream.Application.Services;
using BhDream.Application.Helpers;
using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Dtos;
using BhDream.Domain.Entities;

namespace BhDream.Application.Tests.ServicesTests
{
    public class OptionCsvImportServiceTests
    {
        [Fact]
        public async Task ImportAsync_WhenFindReturnsNull_CallsUpdateAndReturnsUpdatedCount()
        {
            var csvRows = new List<OptionHistoryCsvRow>
            {
                new OptionHistoryCsvRow
                {
                    Symbol = "SYM",
                    Date = System.DateTime.Today,
                    Expiry = System.DateTime.Today.AddDays(7),
                    OptionType = Domain.Enums.OptionRightType.Call,
                    StrikePrice = 100m
                }
            };

            var parserMock = new Mock<IOptionHistoryCsvParser>();
            parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
                      .ReturnsAsync(csvRows);

            var repoMock = new Mock<IOptionHistoryRepository>();
            repoMock.Setup(r => r.FindAsync(It.IsAny<OptionHistory>()))
                    .ReturnsAsync((OptionHistory?)null);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.SetupGet(u => u.OptionHistoryRepository).Returns(repoMock.Object);

            var service = new OptionCsvImportService(unitMock.Object, parserMock.Object);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));

            var result = await service.ImportAsync(stream);

            Assert.Equal(1, result.UpdatedCount);
            Assert.Equal(0, result.InsertedCount);
            repoMock.Verify(r => r.FindAsync(It.IsAny<OptionHistory>()), Times.Exactly(1));
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<OptionHistory>()), Times.Exactly(1));
            repoMock.Verify(r => r.AddAsync(It.IsAny<OptionHistory>()), Times.Never);
        }

        [Fact]
        public async Task ImportAsync_WhenFindReturnsExisting_CallsAddAndReturnsInsertedCount()
        {
            var csvRows = new List<OptionHistoryCsvRow>
            {
                new OptionHistoryCsvRow
                {
                    Symbol = "SYM",
                    Date = System.DateTime.Today,
                    Expiry = System.DateTime.Today.AddDays(7),
                    OptionType = Domain.Enums.OptionRightType.Call,
                    StrikePrice = 100m
                }
            };

            var parserMock = new Mock<IOptionHistoryCsvParser>();
            parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
                      .ReturnsAsync(csvRows);

            var existing = new OptionHistory();
            var repoMock = new Mock<IOptionHistoryRepository>();
            repoMock.Setup(r => r.FindAsync(It.IsAny<OptionHistory>()))
                    .ReturnsAsync(existing);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.SetupGet(u => u.OptionHistoryRepository).Returns(repoMock.Object);

            var service = new OptionCsvImportService(unitMock.Object, parserMock.Object);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));

            var result = await service.ImportAsync(stream);

            Assert.Equal(0, result.UpdatedCount);
            Assert.Equal(1, result.InsertedCount);
            repoMock.Verify(r => r.FindAsync(It.IsAny<OptionHistory>()), Times.Exactly(1));
            repoMock.Verify(r => r.AddAsync(It.IsAny<OptionHistory>()), Times.Exactly(1));
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<OptionHistory>()), Times.Never);
        }
    }
}
