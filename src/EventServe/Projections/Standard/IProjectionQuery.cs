using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.Projections
{
    public interface IProjectionQuery<T>
        where T: Projection
    {

    }
}
