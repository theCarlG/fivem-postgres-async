using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using Npgsql;

namespace PostgresAsync
{
    class Transaction : Operation<bool>
    {
        public Transaction(string connectionString) : base(connectionString) { }

        public bool ExecuteTransaction(IList<string> querys, IDictionary<string, object> parameters = null, bool debug = false)
        {
            bool result = false;
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    var ConnectionTime = stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();

                    using (var command = connection.CreateCommand())
                    {
                        foreach (var parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        var QueryTime = stopwatch.ElapsedMilliseconds;
                        stopwatch.Restart();

                        using (var transaction = connection.BeginTransaction())
                        {
                            command.Transaction = transaction;

                            try
                            {
                                foreach (string commandText in querys)
                                {
                                    command.CommandText = commandText;
                                    command.ExecuteNonQuery();
                                }
                                transaction.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] [{1}] {2}\n", "Postgres", "Transaction", ex.Message));
                            }
                        }

                        stopwatch.Stop();

                        if (debug)
                        {
                            Console.WriteLine(string.Format("[{0}] [C: {1}ms, Q: {2}ms, R: {3}ms] {4}", "Postgres", ConnectionTime, QueryTime, stopwatch.ElapsedMilliseconds, QueryToString("Transaction", parameters)));
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

                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Npgsql for query \"{1}\": {2}\n", "Postgres", QueryToString("Transaction", parameters), firstException.Message));
            }
            catch (NpgsqlException NpgsqlException)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Npgsql for query \"{1}\": {2}\n", "Postgres", QueryToString("Transaction", parameters), NpgsqlException.Message));
            }
            catch (Exception exception)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An critical error happens on Npgsql for query \"{1}\": {2} {3}\n", "Postgres", QueryToString("Transaction", parameters), exception.Message, exception.StackTrace));
            }

            return result;
        }

        public async void ExecuteTransactionAsync(IList<string> querys, IDictionary<string, object> parameters = null, CallbackDelegate callback = null, bool debug = false)
        {
            bool result = false;
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var ConnectionTime = stopwatch.ElapsedMilliseconds;
                    stopwatch.Restart();

                    using (var command = connection.CreateCommand())
                    {
                        foreach (var parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        var QueryTime = stopwatch.ElapsedMilliseconds;
                        stopwatch.Restart();

                        using (var transaction = connection.BeginTransaction())
                        {
                            command.Transaction = transaction;

                            try
                            {
                                foreach (string commandText in querys)
                                {
                                    command.CommandText = commandText;
                                    await command.ExecuteNonQueryAsync();
                                }
                                await transaction.CommitAsync();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] [{1}] {2}\n", "Postgres", "Transaction", ex.Message));
                            }
                        }

                        stopwatch.Stop();

                        if (debug)
                        {
                            Console.WriteLine(string.Format("[{0}] [C: {1}ms, Q: {2}ms, R: {3}ms] {4}", "Postgres", ConnectionTime, QueryTime, stopwatch.ElapsedMilliseconds, QueryToString("Transaction", parameters)));
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

                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Npgsql for query \"{1}\": {2}\n", "Postgres", QueryToString("Transaction", parameters), firstException.Message));
            }
            catch (NpgsqlException NpgsqlException)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An error happens on Npgsql for query \"{1}\": {2}\n", "Postgres", QueryToString("Transaction", parameters), NpgsqlException.Message));
            }
            catch (Exception exception)
            {
                CitizenFX.Core.Debug.Write(string.Format("[ERROR] [{0}] An critical error happens on Npgsql for query \"{1}\": {2} {3}\n", "Postgres", QueryToString("Transaction", parameters), exception.Message, exception.StackTrace));
            }

            callback?.Invoke(result);
        }

        protected override bool Reader(NpgsqlCommand command) => throw new NotImplementedException();
        protected override Task<bool> ReaderAsync(NpgsqlCommand command) => throw new NotImplementedException();

    }
}
