namespace EventServe
{
    public class Stream : IStream
    {
        private Stream() { }

        public Stream(string streamId)
        {
            Id = streamId;
        }

        public string Id { get; }
    }
}
