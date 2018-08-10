
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresAsync
{
    class FetchScalar : Operation<object>
    {
        public FetchScalar(string connectionString) : base(connectionString)
        {
        }

        protected override object Reader(NpgsqlCommand command)
        {
            var result = command.ExecuteScalar();

            if (result != null && result.GetType() == typeof(DBNull))
            {
                result = null;
            }

            return result;
        }

        protected override async Task<object> ReaderAsync(NpgsqlCommand command)
        {
            var result = await command.ExecuteScalarAsync();

            if (result != null && result.GetType() == typeof(DBNull))
            {
                result = null;
            }

            return result;
        }
    }
}
