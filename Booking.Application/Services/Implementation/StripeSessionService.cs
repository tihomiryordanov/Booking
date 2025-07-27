using Stripe.Checkout;

public class StripeSessionService : IStripeSessionService
{
    private readonly SessionService _sessionService;

    public StripeSessionService()
    {
        _sessionService = new SessionService();
    }

    public Session Create(SessionCreateOptions options)
    {
        return _sessionService.Create(options);
    }
}