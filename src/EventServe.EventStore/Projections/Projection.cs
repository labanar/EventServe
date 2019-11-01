using Newtonsoft.Json;

namespace EventServe.EventStore.Projections
{
    public abstract class Projection
    {
        [JsonIgnore]
        public abstract string ProjectionName { get;}

        public Projection() { } 
    }
}
