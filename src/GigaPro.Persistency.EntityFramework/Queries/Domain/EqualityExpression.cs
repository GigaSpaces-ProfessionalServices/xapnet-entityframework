namespace GigaPro.Persistency.EntityFramework.Queries.Domain
{
    internal class EqualityExpression
    {
        public LogicalOperator LogicalOperator { get; set; }

        public string LeftHand { get; set; }

        public RightHand RightHand { get; set; }

        public Operators Operator { get; set; }
    }
}