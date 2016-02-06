namespace TestConsole.Models
{
    public class TestDataSet
    {
        public int Id { get; set; }

        public string SomeValue { get; set; }

        public override string ToString()
        {
            return $"TestDataSet[{Id}]";
        }
    }
}