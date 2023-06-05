using MediatR;

namespace crypto_bank.WebAPI.Pipeline;

public class Dispatcher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Dispatcher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        return await GetMediator().Send(request, cancellationToken);
    }

    public async Task Dispatch(IRequest request, CancellationToken cancellationToken)
    {
        await GetMediator().Send(request, cancellationToken);
    }

    private IMediator GetMediator()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is null.");

        return httpContext.RequestServices.GetRequiredService<IMediator>();
    }
}
