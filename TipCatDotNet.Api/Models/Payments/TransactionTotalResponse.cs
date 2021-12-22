using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.Payments
{
    public class TransactionTotalResponse
    {
        public TransactionTotalResponse()
        {
            Total = 0;
            Transactions = new List<TransactionResponse>();
        }


        public TransactionTotalResponse(int total, List<TransactionResponse> transactions)
        {
            Total = total;
            Transactions = transactions;
        }


        public int Total;
        public List<TransactionResponse> Transactions;
    }
}