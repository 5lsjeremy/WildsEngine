namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    public sealed class TestConsumer
    {
        public TestConsumer(
            double health,
            double mana,
            double stamina,
            string name)
        {
            Health = health;
            Mana = mana;
            Stamina = stamina;
            Name = name;
        }

        public double Health { get; set; }
        public double Mana { get; set; }
        public double Stamina { get; }
        public string Name { get; set; }
        public int PublicField;
        public string ReadOnlyLabel => Name;
    }
}
