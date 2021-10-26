using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class FacilityRequest
    {
        [JsonConstructor]
        public FacilityRequest(int? id, string name, int? accountId)
        {
            Id = id;
            Name = name;
            AccountId = accountId;
        }


        public FacilityRequest(int? id, FacilityRequest request) : this(id, request.Name, request.AccountId)
        { }


        public FacilityRequest(int? id, int? accountId, FacilityRequest request) : this(id, request.Name, accountId)
        { }


        public int? Id { get; }
        [Required]
        public string Name { get; }
        public int? AccountId { get; }


        public static FacilityRequest CreateWithAccountId(int accountId) 
            => new(null, string.Empty, accountId);
    }
}