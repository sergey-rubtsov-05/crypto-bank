using MediatR;

namespace crypto_bank.WebAPI.Pipeline;

public class Dispatcher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Dispatcher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        // todo: question: why not use the same scope from current request?
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request, cancellationToken);
    }

    public async Task Dispatch(IRequest request, CancellationToken cancellationToken)
    {
        // todo: question: why not use the same scope from current request?
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(request, cancellationToken);
    }
}
