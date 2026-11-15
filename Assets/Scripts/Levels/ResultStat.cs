
namespace BreadBowl.Levels
{
    /// <summary>
    /// One line on the result screen. The label goes on the left and the value
    /// on the right. Both are display text, so format numbers and times before
    /// creating one.
    /// </summary>
    public struct ResultStat
    {
        public string Label;
        public string Value;

        public ResultStat(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
