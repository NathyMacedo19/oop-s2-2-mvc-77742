using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FoodSafety.MVC.Controllers;
using FoodSafety.MVC.Data;
using FoodSafety.Domain;

namespace FoodSafety.Test
{
    public class FollowupsControllerLoggingTests
    {
        // Test 1: Verifica se log Information é gerado ao criar Follow-up com sucesso
        [Fact]
        public async Task CreateFollowUp_WhenSuccessful_LogsInformation()
        {
            // ==========================================
            // ARRANGE - Configurar o cenário
            // ==========================================

            // Criar banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // Criar dados necessários (Premises e Inspection)
            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 85,
                Outcome = "Pass",
                Notes = "Valid inspection"
            };
            dbContext.Inspections.Add(inspection);
            await dbContext.SaveChangesAsync();

            // Criar um mock do logger (simula o Serilog)
            var loggerMock = new Mock<ILogger<FollowupsController>>();

            // Criar o controller com o logger mockado
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // Criar um novo Follow-up válido
            var newFollowUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(7),  // Data futura (válida)
                Status = "Open"
            };

            // ==========================================
            // ACT - Executar a ação
            // ==========================================
            var result = await controller.Create(newFollowUp);

            // ==========================================
            // ASSERT - Verificar os resultados
            // ==========================================

            // Verificar se o log Information foi chamado
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,  // Nível do log
                    It.IsAny<EventId>(),   // Qualquer EventId
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Follow-up created")), // Mensagem contém "Follow-up created"
                    It.IsAny<Exception>(),  // Nenhuma exceção
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), // Qualquer formatter
                Times.Once);  // Deve ser chamado exatamente 1 vez
        }

        // Test 2: Verifica se log Warning é gerado quando validação falha
        [Fact]
        public async Task CreateFollowUp_WhenDueDateBeforeInspectionDate_LogsWarning()
        {
            // ==========================================
            // ARRANGE
            // ==========================================

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // Criar dados necessários
            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,  // Inspeção hoje
                Score = 85,
                Outcome = "Pass"
            };
            dbContext.Inspections.Add(inspection);
            await dbContext.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<FollowupsController>>();
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // Criar Follow-up com data de vencimento ANTES da inspeção (inválido)
            var invalidFollowUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-5),  // Data no passado (inválida)
                Status = "Open"
            };

            // ==========================================
            // ACT
            // ==========================================
            var result = await controller.Create(invalidFollowUp);

            // ==========================================
            // ASSERT
            // ==========================================

            // Verificar se o log Warning foi chamado
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,  // Nível Warning
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("date before inspection")), // Mensagem contém "date before inspection"
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Test 3: Verifica se log Error é gerado quando ocorre exceção
        [Fact]
        public async Task CreateFollowUp_WhenExceptionOccurs_LogsError()
        {
            // ==========================================
            // ARRANGE
            // ==========================================

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Criar um dbContext que vai lançar exceção
            var dbContext = new AppDbContext(options);

            // Não adicionar dados para forçar erro (ex: Inspection não existe)
            var loggerMock = new Mock<ILogger<FollowupsController>>();
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // Criar Follow-up com InspectionId que não existe
            var followUpWithInvalidInspection = new FollowUp
            {
                InspectionId = 999,  // ID que não existe
                DueDate = DateTime.Today.AddDays(7),
                Status = "Open"
            };

            // ==========================================
            // ACT
            // ==========================================
            var result = await controller.Create(followUpWithInvalidInspection);

            // ==========================================
            // ASSERT
            // ==========================================

            // Verificar se o log Error foi chamado
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,  // Nível Error
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Error creating")), // Mensagem contém "Error creating"
                    It.IsAny<Exception>(),  // Deve ter uma exceção
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Test 4: Verifica se log Information é gerado ao editar Follow-up
        [Fact]
        public async Task EditFollowUp_WhenSuccessful_LogsInformation()
        {
            // ==========================================
            // ARRANGE
            // ==========================================

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // Criar dados necessários
            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 85,
                Outcome = "Pass"
            };
            dbContext.Inspections.Add(inspection);

            var followUp = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(7),
                Status = "Open"
            };
            dbContext.FollowUps.Add(followUp);
            await dbContext.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<FollowupsController>>();
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // Modificar o Follow-up
            followUp.Status = "Closed";
            followUp.ClosedDate = DateTime.Today;

            // ==========================================
            // ACT
            // ==========================================
            var result = await controller.Edit(followUp.Id, followUp);

            // ==========================================
            // ASSERT
            // ==========================================

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Follow-up edited")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Test 5: Verifica se log Information é gerado ao deletar Follow-up
        [Fact]
        public async Task DeleteFollowUp_WhenSuccessful_LogsInformation()
        {
            // ==========================================
            // ARRANGE
            // ==========================================

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // Criar dados necessários
            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 85,
                Outcome = "Pass"
            };
            dbContext.Inspections.Add(inspection);

            var followUp = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(7),
                Status = "Open"
            };
            dbContext.FollowUps.Add(followUp);
            await dbContext.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<FollowupsController>>();
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // ==========================================
            // ACT
            // ==========================================
            var result = await controller.DeleteConfirmed(followUp.Id);

            // ==========================================
            // ASSERT
            // ==========================================

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Follow-up deleted")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}