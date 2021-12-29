using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using Nuages.TaskRunner.Tasks;
using Xunit;
// ReSharper disable ArrangeNamespaceBody

namespace Nuages.TaskRunner.Tests
{
    
    [ExcludeFromCodeCoverage]
    public class TaskRunnerTests
    {
        [Fact]
        public async Task ShouldExecuteAsyncNoAuthorizer()
        {
            var taskData = new OutputToConsoleTaskData();
            var taskDef = RunnableTaskDefinitionCreator<OutputToConsoleTask>.Create(taskData);

            var serviceprovider = new Mock<IServiceProvider>();

            var logger = new Mock<ILogger<TaskRunnerService>>();
            
            var runner = new TaskRunnerService(serviceprovider.Object, logger.Object, new List<ITaskAuthorizationService>());

            await runner.ExecuteAsync(taskDef);

        }
        
        [Fact]
        public async Task ShouldExecuteAsyncWithTemplateParameters()
        {
            var taskData = new OutputToConsoleTaskData("message");
        
            var serviceprovider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<TaskRunnerService>>();
            
            var runner = new TaskRunnerService(serviceprovider.Object, logger.Object, new List<ITaskAuthorizationService>());

            await runner.ExecuteAsync<OutputToConsoleTask, OutputToConsoleTaskData>(taskData);

        }
        
        [Fact]
        public async Task ShouldExecuteAsyncWithAuthorizer()
        {
            var taskData = new OutputToConsoleTaskData();
            var taskDef = RunnableTaskDefinitionCreator<OutputToConsoleTask>.Create(taskData);

            var authorizer = new Mock<ITaskAuthorizationService>();
            authorizer.Setup(a => a.IsAuthorizedToRunAsync(taskDef)).ReturnsAsync(true);
                
            var serviceprovider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<TaskRunnerService>>();
            
            var runner = new TaskRunnerService(serviceprovider.Object, logger.Object, new List<ITaskAuthorizationService>
            {
                authorizer.Object
            });

            await runner.ExecuteAsync(taskDef);

        }
        
        [Fact]
        public async Task ShouldFailedNotAuthorized()
        {
            var taskData = new OutputToConsoleTaskData();
            var taskDef = RunnableTaskDefinitionCreator<OutputToConsoleTask>.Create(taskData);

            var authorizer = new Mock<ITaskAuthorizationService>();
            authorizer.Setup(a => a.IsAuthorizedToRunAsync(taskDef)).ReturnsAsync(false);
                
            var serviceprovider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<TaskRunnerService>>();
            
            var runner = new TaskRunnerService(serviceprovider.Object, logger.Object, new List<ITaskAuthorizationService>
            {
                authorizer.Object
            });

            await Assert.ThrowsAsync<NotAuthorizedException>(async () =>
            {
                await runner.ExecuteAsync(taskDef);
            });
        }
    
        [Fact]
        public async Task ShouldFailedExecuteAsync()
        {
            const string name = "BadClassName";
        
            var serviceprovider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<TaskRunnerService>>();
            
            var runner = new TaskRunnerService(serviceprovider.Object, logger.Object, new List<ITaskAuthorizationService>());
    
            await Assert.ThrowsAsync<TypeLoadException>(async () =>
            {
                await runner.ExecuteAsync(new RunnableTaskDefinition
                {
                    AssemblyQualifiedName = name
                });
            });
       
    
        }
    }
}
