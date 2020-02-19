namespace EventServe.Subscriptions.Persistent
{
    public class PersistentStreamSubscriptionConnectionSettings
    {
        public PersistentStreamSubscriptionConnectionSettings(string subscriptionName, IStreamFilter filter)
        {
            SubscriptionName = subscriptionName;
            Filter = filter;
        }

        public string SubscriptionName { get; }
        public IStreamFilter Filter { get; }
    }
}
