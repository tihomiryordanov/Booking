using Stripe.Checkout;

public interface IStripeSessionService
{
    Session Create(SessionCreateOptions options);
}
