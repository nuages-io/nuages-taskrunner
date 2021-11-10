using System.Threading.Tasks;

namespace Nuages.TaskRunner.Tests
{
    public class TestAuthorzationService : ITaskAuthorizationService
    {
        public TestAuthorzationService(bool result)
        {
            
        }
        public async Task<bool> IsAuthorizedToRunAsync(RunnableTaskDefinition taskDef)
        {
            return await Task.FromResult(true);
        }
    }
}