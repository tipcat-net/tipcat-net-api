namespace TipCatDotNet.Models
{
    public readonly struct CustomerContext
    {
        public CustomerContext(int id)
        {
            Id = id;
        }


        public override bool Equals(object? obj) => obj is not null && Equals(obj);


        public bool Equals(in CustomerContext other) => Id == other.Id;


        public override int GetHashCode() => Id;

        public static bool operator ==(in CustomerContext left, in CustomerContext right) => left.Equals(right);


        public static bool operator !=(in CustomerContext left, in CustomerContext right) => !(left == right);


        public int Id { get; }
    }
}
