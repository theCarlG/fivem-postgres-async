using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresAsync
{
    class Insert : Operation<object>
    {
        public Insert(string connectionString) : base(connectionString)
        {
        }

        protected override object Reader(NpgsqlCommand command)
        {
            return command.ExecuteScalar();
        }

        protected async override Task<object> ReaderAsync(NpgsqlCommand command)
        {
            return await command.ExecuteScalarAsync();
        }
    }
}
