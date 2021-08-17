using System;

namespace TipCatDotNet.Api.Models
{
    public readonly struct Customer
    {
        public Customer(int id, string name, string email)
        {
            Id = id;
            Email = email;
            Name = name;
        }


        public override bool Equals(object? obj) => obj is not null && Equals(obj);


        public bool Equals(in Customer other) => Id == other.Id && Name == other.Name;


        public override int GetHashCode() => HashCode.Combine(Id, Name);

        public static bool operator ==(in Customer left, in Customer right) => left.Equals(right);


        public static bool operator !=(in Customer left, in Customer right) => !(left == right);


        public int Id { get; }
        public string Email { get; }
        public string Name { get; }
    }
}
