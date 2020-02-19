using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections.Standard
{
    public interface IProjectionTypeExpression
    {
        IProjectionHandlerExpression OnTo<T>() where T : Projection;
    }
}
