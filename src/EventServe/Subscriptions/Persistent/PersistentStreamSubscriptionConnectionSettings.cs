namespace EventServe.Subscriptions.Persistent
{
    public class PersistentStreamSubscriptionConnectionSettings
    {
        public PersistentStreamSubscriptionConnectionSettings(string subscriptionName, SubscriptionFilter filter)
        {
            SubscriptionName = subscriptionName;
            Filter = filter;
        }

        public string SubscriptionName { get; }
        public SubscriptionFilter Filter { get; }
    }
}
