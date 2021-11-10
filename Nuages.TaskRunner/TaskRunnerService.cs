using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nuages.TaskRunner
{
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TaskRunnerService : ITaskRunnerService
    {
        private readonly ILogger<TaskRunnerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<ITaskAuthorizationService> _authorizers;

        public TaskRunnerService( IServiceProvider serviceProvider, ILogger<TaskRunnerService> logger, IEnumerable<ITaskAuthorizationService> authorizers)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _authorizers = authorizers;
        }

        // public async Task<T> ExecuteAsync<T>(RunnableTaskDefinition taskDef) where T : IRunnableTask
        // {
        //     return (T) await ExecuteAsync(taskDef);
        // }
        //
        public async Task<T> ExecuteAsync<T,TD>(TD data, string? userId = null) where T : IRunnableTask
        {
            var taskDef = RunnableTaskDefinitionCreator<T>.Create(data, userId);
            
            return (T) await ExecuteAsync(taskDef);
        }
        
        // public async Task<T> ExecuteAsync<T>(object data, string? userId = null) where T : IRunnableTask
        // {
        //     var taskDef = RunnableTaskDefinitionCreator<T>.Create(data, userId);
        //     
        //     return (T) await ExecuteAsync(taskDef);
        // }

        protected virtual async Task<bool> IsAuthorizedToRunAsync(RunnableTaskDefinition taskDef)
        {
            if (!_authorizers.Any())
                return true;

            foreach (var authorizer in _authorizers)
            {
                if (!await authorizer.IsAuthorizedToRunAsync(taskDef))
                    return false;
            }

            return true;
        }
        
        public async Task<IRunnableTask> ExecuteAsync(RunnableTaskDefinition taskDef)
        {
            var taskId = Guid.NewGuid();
            
            _logger.LogInformation("Running task {Id} with type {AssemblyQualifiedName} using payload {Payload}", taskId, taskDef.AssemblyQualifiedName, taskDef.Payload);
            var type = GetRunnableType(taskDef);

            if (!await IsAuthorizedToRunAsync(taskDef))
            {
                _logger.LogWarning("User {UserId} is not authorized to run task {AssemblyQualifiedName}", taskDef.UserId, taskDef.AssemblyQualifiedName);
                throw new NotAuthorizedException("NotAuthorized");
            }
            
            var job = (IRunnableTask) ActivatorUtilities.CreateInstance(_serviceProvider, type);

            await job.ExecuteAsync(JsonSerializer.Serialize(taskDef.Payload));
            
            _logger.LogInformation("Task {Id} was executed with success", taskId);
            
            return job;
        }

        private static Type GetRunnableType(RunnableTaskDefinition taskDef)
        {
            var type = Type.GetType(taskDef.AssemblyQualifiedName);
            if (type == null)
            {
                throw new TypeLoadException(
                    $"Can't process task, type not found : {taskDef.AssemblyQualifiedName}");
            }

            return type;
        }
    }
}