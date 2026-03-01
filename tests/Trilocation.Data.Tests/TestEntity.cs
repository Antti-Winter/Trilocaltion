namespace Trilocation.Data.Tests
{
    public class TestEntity : IHasTriIndex
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ulong TriIndex { get; set; }
    }
}
