using System.Threading.Tasks;
using squad_func.Models;

namespace squad_func.Services;

public interface IAgentMappingService
{
    /// <summary>
    /// Processes an AgentFixture model and maps it to database entities.
    /// </summary>
    /// <param name="agentFixture">The fixture data extracted by the agent.</param>
    Task ProcessAgentFixtureAsync(AgentFixture agentFixture);
}
