using EventProcessingService.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestEventProcessing;

public class DataStorageTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<FileDataStorage>> _mockLogger;

        public DataStorageTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<FileDataStorage>>();
        }

        [Fact]
        public async Task GetStatistics_WithFileStorage_ReturnsData()
        {
            var tempFilePath = Path.GetTempFileName();
            var testData = """
            [
                {
                    "userId": 1,
                    "eventType": "click",
                    "count": 10
                }
            ]
            """;
            await File.WriteAllTextAsync(tempFilePath, testData);
            
            _mockConfiguration.Setup(x => x["FILE_STORAGE_PATH"]).Returns(tempFilePath);
            var dataStorage = new FileDataStorage(_mockConfiguration.Object, _mockLogger.Object);

            var result = await dataStorage.GetStatistics();

            Assert.Equal(1, result[0].UserId);
            Assert.Equal("click", result[0].EventType);
            Assert.Equal(10, result[0].Count);

            File.Delete(tempFilePath);
        }
    }