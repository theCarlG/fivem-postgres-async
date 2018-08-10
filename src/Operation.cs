using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using Npgsql;

namespace PostgresAsync
{
    abstract class Operation<TResult>
    {
        internal string ConnectionString;

        internal string Query = "";
        internal IDictionary<string, object> Parameters = null;
        internal bool Debug = false;
        internal uint ThreadedId = 0;

        public Operation(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public TResult Execute(string query = null, IDictionary<string, object> parameters = null, bool debug = false)
        {
            if (string.IsNullOrEmpty(query))
            {
                query = Query;
                parameters = Parameters;
                debug = Debug;
            }

            TResult result = default(TResult);
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var ConnectionTime = stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();
                    using (var command = CreateCommand(query, parameters, connection))
                    {
                        var QueryTime = stopwatch.ElapsedMilliseconds;
                        stopwatch.Restart();

                        result = Reader(command);
                        stopwatch.Stop();

                        if (debug)
                        {
                            Console.WriteLine(string.Format("[{0}] [C: {1}ms, Q: {2}ms, R: {3}ms] {4}", "Postgres", ConnectionTime, QueryTime, stopwatch.ElapsedMilliseconds, QueryToString(query, parameters)));
                        }
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                var firstException = aggregateException.InnerExceptions.First();

                if (!(firstException is NpgsqlException))
                {
                    throw;
                }

                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Postgres for query \"{1}\": {2}\n", "Postgres", QueryToString(query, parameters), firstException.Message));
            }
            catch (NpgsqlException npgsqlException)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Postgres for query \"{1}\": {2}\n", "Postgres", QueryToString(query, parameters), npgsqlException.Message));
            }
            catch (Exception exception)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An critical error happens on Postgres for query \"{1}\": {2} {3}\n", "Postgres", QueryToString(query, parameters), exception.Message, exception.StackTrace));
            }

            return result;
        }

        public async void ExecuteAsync(string query, IDictionary<string, object> parameters, CallbackDelegate callback, bool debug = false)
        {
            TResult result = default(TResult);
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var ConnectionTime = stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();

                    using (var command = CreateCommand(query, parameters, connection))
                    {
                        var QueryTime = stopwatch.ElapsedMilliseconds;
                        stopwatch.Restart();

                        result = await ReaderAsync(command);
                        stopwatch.Stop();

                        if (debug)
                        {
                            Console.WriteLine(string.Format("[{0}] [C: {1}ms, Q: {2}ms, R: {3}ms] {4}", "Postgres", ConnectionTime, QueryTime, stopwatch.ElapsedMilliseconds, QueryToString(query, parameters)));
                        }

                        callback.Invoke(result);
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                var firstException = aggregateException.InnerExceptions.First();

                if (!(firstException is NpgsqlException))
                {
                    throw aggregateException;
                }

                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Postgres for query \"{1}\": {2}\n", "Postgres", QueryToString(query, parameters), firstException.Message));
            }
            catch (NpgsqlException NpgsqlException)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Postgres for query \"{1}\": {2}\n", "Postgres", QueryToString(query, parameters), NpgsqlException.Message));
            }
            catch (ArgumentNullException)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] Check the error above, an error happens when executing the callback from the query : \"{1}\"\n", "Postgres", QueryToString(query, parameters)));
            }
            catch (Exception exception)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An critical error happens on Postgres for query \"{1}\": {2} {3}\n", "Postgres", QueryToString(query, parameters), exception.Message, exception.StackTrace));
            }
        }

        public async Task<TResult> ExecuteThreaded(string query, IDictionary<string, object> parameters, bool debug = false)
        {
            Query = query;
            Parameters = parameters;
            Debug = debug;
            PostgresThread postgresAsync = PostgresThread.GetInstance();
            ThreadedId = postgresAsync.NextId;
            postgresAsync.queryCollection.TryAdd(this);

            while (!postgresAsync.resultCollection.ContainsKey(ThreadedId))
                await BaseScript.Delay(0);

            postgresAsync.resultCollection.TryRemove(ThreadedId, out dynamic result);

            return result;
        }

        abstract protected TResult Reader(NpgsqlCommand command);

        abstract protected Task<TResult> ReaderAsync(NpgsqlCommand command);

        private NpgsqlCommand CreateCommand(string query, IDictionary<string, object> parameters, NpgsqlConnection connection)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = query;

            foreach (var parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            return command;
        }

        internal string QueryToString(string query, IDictionary<string, object> parameters)
        {
            return query + " {" + string.Join(";", parameters.Select(x => x.Key + "=" + x.Value).ToArray()) + "}";
        }
    }
}
