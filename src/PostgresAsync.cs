using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Npgsql;

namespace PostgresAsync
{
    public class PostgresAsync : BaseScript
    {
        internal String ConnectionString;
        internal bool debug;

        public PostgresAsync()
        {
            Exports.Add("psql_configure", new Action(() =>
            {

                Configure(
                    Function.Call<string>(Hash.GET_CONVAR, "psql_connection_string"),
                    Function.Call<string>(Hash.GET_CONVAR, "psql_debug") == "true"
                );
            }));

            Exports.Add("psql_execute", new Action<string, IDictionary<string, object>, CallbackDelegate>((query, parameters, callback) =>
            {
                (new Execute(ConnectionString)).ExecuteAsync(query, parameters, callback, debug);
            }));
            Exports.Add("psql_sync_execute", new Func<string, IDictionary<string, object>, int>((query, parameters) =>
            {
                return (new Execute(ConnectionString)).Execute(query, parameters, debug);
            }));
            Exports.Add("psql_threaded_execute", new Func<string, IDictionary<string, object>, Task<object>>(async (query, parameters) =>
            {
                return await (new Execute(ConnectionString)).ExecuteThreaded(query, parameters, debug);
            }));

            Exports.Add("psql_fetch_all", new Action<string, IDictionary<string, object>, CallbackDelegate>((query, parameters, callback) =>
            {
                (new FetchAll(ConnectionString)).ExecuteAsync(query, parameters, callback, debug);
            }));
            Exports.Add("psql_sync_fetch_all", new Func<string, IDictionary<string, object>, List<Dictionary<string, Object>>>((query, parameters) =>
            {
                return (new FetchAll(ConnectionString)).Execute(query, parameters, debug);
            }));
            Exports.Add("psql_threaded_fetch_all", new Func<string, IDictionary<string, object>, Task<object>>(async (query, parameters) =>
            {
                return await (new FetchAll(ConnectionString)).ExecuteThreaded(query, parameters, debug);
            }));

            Exports.Add("psql_fetch_scalar", new Action<string, IDictionary<string, object>, CallbackDelegate>((query, parameters, callback) =>
            {
                (new FetchScalar(ConnectionString)).ExecuteAsync(query, parameters, callback, debug);
            }));
            Exports.Add("psql_sync_fetch_scalar", new Func<string, IDictionary<string, object>, Object>((query, parameters) =>
            {
                return (new FetchScalar(ConnectionString)).Execute(query, parameters, debug);
            }));
            Exports.Add("psql_threaded_fetch_scalar", new Func<string, IDictionary<string, object>, Task<object>>(async (query, parameters) =>
            {
                return await (new FetchScalar(ConnectionString)).ExecuteThreaded(query, parameters, debug);
            }));

            Exports.Add("psql_insert", new Action<string, IDictionary<string, object>, CallbackDelegate>((query, parameters, callback) =>
            {
                (new Insert(ConnectionString)).ExecuteAsync(query, parameters, callback, debug);
            }));
            Exports.Add("psql_sync_insert", new Func<string, IDictionary<string, object>, Object>((query, parameters) =>
            {
                return (new Insert(ConnectionString)).Execute(query, parameters, debug);
            }));
            Exports.Add("psql_threaded_insert", new Func<string, IDictionary<string, object>, Task<object>>(async (query, parameters) =>
            {
                return await (new Insert(ConnectionString)).ExecuteThreaded(query, parameters, debug);
            }));

            Exports.Add("psql_transaction", new Action<IList<object>, IDictionary<string, object>, CallbackDelegate>((querys, parameters, callback) =>
            {
                (new Transaction(ConnectionString)).ExecuteTransactionAsync(querys.Select(q => q.ToString()).ToList(), parameters, callback, debug);
            }));
            Exports.Add("psql_sync_transaction", new Func<IList<object>, IDictionary<string, object>, bool>((querys, parameters) =>
            {
                return (new Transaction(ConnectionString)).ExecuteTransaction(querys.Select(q => q.ToString()).ToList(), parameters, debug);
            }));
        }

        private void Configure(string connectionStringConfig, bool debug)
        {
 
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionStringConfig);

            this.debug = debug;
            ConnectionString = connectionStringBuilder.ToString();
        }
    }
}
