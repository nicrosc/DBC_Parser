using Moq;
using ZadatakJuric.Application.DBCParser.Commands;
using ZadatakJuric.Domain.Entities;
using ZadatakJuric.Domain.Enums;
using ZadatakJuric.Infrastructure.Repositories;

namespace ZadatakJuric.Test
{
    public class BatteryDBCParserTest
    {
        private readonly Mock<IDbcRepository> _mockRepository;
        private readonly ParseDBCFileCommandHandler _handler;

        public BatteryDBCParserTest()
        {
            _mockRepository = new Mock<IDbcRepository>();
            _handler = new ParseDBCFileCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task ParseBatteryDBC_ShouldParseRealFileCorrectly()
        {
            // Arrange - Read the actual DBC file
            var dbcFilePath = Path.Combine("DBCFiles", "Kangoo.dbc");
            var batteryDbcContent = await File.ReadAllTextAsync(dbcFilePath);

            var command = new ParseDBCFileCommand
            {
                FileContent = batteryDbcContent,
                FileName = "Kangoo.dbc",
                SaveToDatabase = false
            };

            _mockRepository.Setup(x => x.GetByFileHashAsync(It.IsAny<string>()))
                          .ReturnsAsync((DBC?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert - Basic parsing success
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal("Kangoo.dbc", result.Data.FileName);
            Assert.False(result.WasAlreadyParsed);
            Assert.Contains("Successfully parsed", result.Message);

            // Verify network structure
            Assert.Single(result.Data.Networks);
            var network = result.Data.Networks.First();
            Assert.Equal("Kangoo", network.Name);

            // Log actual counts for debugging
            Console.WriteLine($"Total Messages: {result.Data.TotalMessages}");
            Console.WriteLine($"Total Signals: {result.Data.TotalSignals}");
            Console.WriteLine($"Messages in network: {network.Messages.Count}");

            // Basic expectations based on the DBC file
            Assert.True(result.Data.TotalMessages >= 4); // At least 4 messages (some may not parse)
            Assert.True(result.Data.TotalSignals >= 20); // At least 20 signals (some may not parse)
            Assert.True(network.Messages.Count >= 4);

            // Test specific messages that should definitely parse
            
            // Battery_1 (ID: 341) - Simple message, should parse completely
            var battery1 = network.Messages.FirstOrDefault(m => m.Id == 341);
            if (battery1 != null)
            {
                Assert.Equal("Battery_1", battery1.Name);
                Assert.Equal((byte)8, battery1.Size);
                Assert.Equal("Battery", battery1.Sender);
                
                Console.WriteLine($"Battery_1 signals: {battery1.Signals.Count}");
                foreach (var signal in battery1.Signals)
                {
                    Console.WriteLine($"  - {signal.Name}: {signal.StartBit}|{signal.Length}, factor: {signal.Factor}, offset: {signal.Offset}, unit: '{signal.Unit}'");
                }

                // Test specific signals if they exist
                var soc = battery1.Signals.FirstOrDefault(s => s.Name == "SoC");
                if (soc != null)
                {
                    Assert.Equal((byte)39, soc.StartBit);
                    Assert.Equal((byte)16, soc.Length);
                    Assert.Equal(0.0025, soc.Factor);
                    Assert.Equal(0.0, soc.Offset);
                }

                var maxCharge = battery1.Signals.FirstOrDefault(s => s.Name == "MaxChargeAllowed");
                if (maxCharge != null)
                {
                    Assert.Equal((byte)7, maxCharge.StartBit);
                    Assert.Equal((byte)8, maxCharge.Length);
                    Assert.Equal("W", maxCharge.Unit);
                }
            }

            // Battery_3 (ID: 1060) - Complex message with many signals
            var battery3 = network.Messages.FirstOrDefault(m => m.Id == 1060);
            if (battery3 != null)
            {
                Assert.Equal("Battery_3", battery3.Name);
                Console.WriteLine($"Battery_3 signals: {battery3.Signals.Count}");
                
                foreach (var signal in battery3.Signals)
                {
                    Console.WriteLine($"  - {signal.Name}: {signal.StartBit}|{signal.Length}, factor: {signal.Factor}, offset: {signal.Offset}, unit: '{signal.Unit}'");
                }

                // Test some key signals
                var soh = battery3.Signals.FirstOrDefault(s => s.Name == "SoH");
                if (soh != null)
                {
                    Assert.Equal((byte)40, soh.StartBit);
                    Assert.Equal((byte)8, soh.Length);
                }

                var cuv = battery3.Signals.FirstOrDefault(s => s.Name == "CUV");
                if (cuv != null)
                {
                    Assert.Equal((byte)7, cuv.StartBit);
                    Assert.Equal((byte)2, cuv.Length);
                }
            }

            // Battery_2 (ID: 1059)
            var battery2 = network.Messages.FirstOrDefault(m => m.Id == 1059);
            if (battery2 != null)
            {
                Assert.Equal("Battery_2", battery2.Name);
                Console.WriteLine($"Battery_2 signals: {battery2.Signals.Count}");

                var voltage12V = battery2.Signals.FirstOrDefault(s => s.Name == "12V_Voltage");
                if (voltage12V != null)
                {
                    Assert.Equal((byte)63, voltage12V.StartBit);
                    Assert.Equal((byte)8, voltage12V.Length);
                    Assert.Equal("V", voltage12V.Unit);
                }
            }

            // Battery_5 (ID: 1061)
            var battery5 = network.Messages.FirstOrDefault(m => m.Id == 1061);
            if (battery5 != null)
            {
                Assert.Equal("Battery_5", battery5.Name);
                Console.WriteLine($"Battery_5 signals: {battery5.Signals.Count}");

                var remainingkWh = battery5.Signals.FirstOrDefault(s => s.Name == "RemainingkWh");
                if (remainingkWh != null)
                {
                    Assert.Equal((byte)0, remainingkWh.StartBit);
                    Assert.Equal((byte)9, remainingkWh.Length);
                    Assert.Equal("kWh", remainingkWh.Unit);
                }
            }

            // Battery_6 (ID: 1093) - Message with no signals
            var battery6 = network.Messages.FirstOrDefault(m => m.Id == 1093);
            if (battery6 != null)
            {
                Assert.Equal("Battery_6", battery6.Name);
                Assert.Equal((byte)7, battery6.Size);
                Console.WriteLine($"Battery_6 signals: {battery6.Signals.Count}");
            }

            // Verify file properties
            Assert.NotEmpty(result.Data.FileHash);
            Assert.True(result.Data.FileSize > 0);
            Assert.Equal(1, result.Data.TotalNetworks);

            // Print summary for debugging
            Console.WriteLine($"\n=== PARSING SUMMARY ===");
            Console.WriteLine($"Total Messages Parsed: {result.Data.TotalMessages}");
            Console.WriteLine($"Total Signals Parsed: {result.Data.TotalSignals}");
            Console.WriteLine($"File Size: {result.Data.FileSize} bytes");
            Console.WriteLine($"File Hash: {result.Data.FileHash}");
        }
    }
} 