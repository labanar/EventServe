namespace EventServe.Projections
{
    public interface IProjectionHandlerExpression
    {
        IProjectionHandlerExpression HandleEvent<T>() where T : Event;
    }
}
