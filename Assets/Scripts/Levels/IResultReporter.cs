using System.Collections.Generic;

namespace BreadBowl.Levels
{
    /// <summary>
    /// Implement this on any component in a level to add lines to the result screen.
    /// It can be a goal, a constraint, or a one-off script for that level.
    /// When the level is completed, LevelController finds every reporter in the
    /// scene and calls AddResults once. Add as many lines as you want, or none.
    /// 
    /// Each reporter tracks its own numbers. If a stat should cover the whole level,
    /// make sure that ResetForNextRoll or OnAttemptStarted doesn't clear the stat.
    /// 
    /// Reporter lines appear after the built-in loaves bowled, time, rolls per stage
    /// lines.
    /// </summary>
    /// <example>
    /// <code>
    /// public void AddResults(List&lt;ResultStat&gt; stats)
    /// {
    ///     stats.Add(new ResultStat("Strikes", strikes.ToString()));
    /// }
    /// </code>
    /// </example>
    public interface IResultReporter
    {
        void AddResults(List<ResultStat> stats);
    }
}

