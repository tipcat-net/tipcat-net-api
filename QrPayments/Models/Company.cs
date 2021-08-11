namespace QrPayments.Models
{
    public readonly struct Company
    {
        public Company(int id, string name)
        {
            Id = id;
            Name = name;
        }


        public int Id { get; }
        public string Name { get; }
    }
}
