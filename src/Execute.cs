using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresAsync
{
    class Execute : Operation<int>
    {
        public Execute(string connectionString) : base(connectionString)
        {
        }

        protected override int Reader(NpgsqlCommand command)
        {
            return command.ExecuteNonQuery();
        }

        protected override Task<int> ReaderAsync(NpgsqlCommand command)
        {
            return command.ExecuteNonQueryAsync();
        }
    }
}