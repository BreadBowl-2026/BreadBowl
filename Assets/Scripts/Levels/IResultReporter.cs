using System.Collections.Generic;

namespace BreadBowl.Levels
{
    public interface IResultReporter
    {
        void AddResults(List<ResultStat> stats);
    }
}

