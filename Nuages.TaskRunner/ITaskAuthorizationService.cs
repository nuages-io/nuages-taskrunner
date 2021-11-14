namespace Nuages.TaskRunner;

public interface ITaskAuthorizationService
{
    Task<bool> IsAuthorizedToRunAsync(RunnableTaskDefinition taskDef);
}