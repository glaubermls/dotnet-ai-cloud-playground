using DotnetAiCloudPlayground.Core.Domain;

namespace DotnetAiCloudPlayground.Core.Ports;

public interface IChatModelPort
{
    Task<ChatOutput> CompleteAsync(Prompt prompt, CancellationToken cancellationToken = default);
}
