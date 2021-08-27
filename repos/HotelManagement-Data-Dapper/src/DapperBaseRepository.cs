using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace HotelManagement_Data_Dapper.src
{
        /// <summary>
        /// Abstract class for dapper repository
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class DapperBaseRepository<T> 
        {
            //Static constructor to ensure configurations are only registered once
            static DapperBaseRepository()
            {
                
            }

            private static AsyncPolicy retryPolicy;

            private readonly ILogger logger;
            private readonly IDbConnectionFactory connectionFactory;

            /// <summary>
            /// The number of times to retry availability exceptions thrown from queries
            /// </summary>
            protected int MaxRetryCount = 11;

            /// <summary>
            /// Concurrency Error code from Sql
            /// </summary>
            protected int[] RetryConcurrencyCodes = new int[] { 51000, 1205 };

            /// <summary>
            /// Create Dapper Repository
            /// </summary>
            /// <param name="connectionFactory">The connection factory</param>
            /// <param name="logger">The logger</param>
            /// <param name="tableName">The name of the database table associated with repository</param>
            protected DapperBaseRepository(IDbConnectionFactory connectionFactory, ILogger logger, string tableName) :
                this(connectionFactory, logger)
            {
                //Implement verify null
                TableName = tableName;
            }

            /// <summary>
            /// Create Dapper Repository
            /// </summary>
            /// <param name="connectionFactory">The connection factory</param>
            /// <param name="logger">The logging factory</param>
            protected DapperBaseRepository(IDbConnectionFactory connectionFactory, ILogger logger) : this()
            {
                this.connectionFactory = connectionFactory;
                this.logger = logger;
            }

            private DapperBaseRepository()
            {
                retryPolicy = GetRetryPolicy();
            }

            /// <summary>
            /// The Connection factory
            /// </summary>
            public IDbConnectionFactory ConnectionFactory => connectionFactory;

            /// <summary>
            /// The name of the sql table associated with this repository
            /// </summary>
            public string TableName { get; } = typeof(T).Name;

            /// <summary>
            /// The Log factory
            /// </summary>
            public ILogger Logger => logger;

            /// <summary>
            /// Find all entities bases on an expression
            /// </summary>
            /// <param name="predicate"></param>
            /// <returns></returns>
            public abstract Task<IQueryable<T>> FindAllFiltered(Expression<Func<T, bool>> predicate);

            /// <summary>
            /// Delete entities based on an expression
            /// </summary>
            /// <param name="predicate"></param>
            /// <returns></returns>
            public abstract Task DeleteWhere(Expression<Func<T, bool>> predicate);

            /// <summary>
            /// Execute query against the repository
            /// </summary>
            /// <returns></returns>
            public abstract Task<IQueryable<T>> Query();

            /// <summary>
            /// Save entity to database
            /// </summary>
            /// <param name="entity">The entity to save</param>
            /// <returns></returns>
            public abstract Task<T> Save(T entity);

           

            /// <summary>
            /// Delete the entity
            /// </summary>
            /// <param name="entity">The entity to delete</param>
            /// <returns></returns>
            public virtual Task Delete(T entity)
            {
               // Verify.NotNull(nameof(entity), entity);

                return Delete(entity.Id);
            }

            /// <summary>
            /// Delete the entity
            /// </summary>
            /// <param name="entityId">The Id of the entity to delete</param>
            /// <returns></returns>
            public virtual async Task Delete(Guid entityId)
            {
                //Verify.NotEmpty(nameof(entityId), entityId);

                using (var conn = (await connectionFactory.GetConnection()))
                {
                    await conn.ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @Id",
                        new { Id = entityId }).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Delete the entities
            /// </summary>
            /// <param name="entities">The entities to delete</param>
            /// <returns></returns>
            public virtual async Task Delete(IEnumerable<T> entities)
            {
                //Verify.NotNull(nameof(entities), entities);

                //todo helper method to combine into one delete sql statement or stored proc

                foreach (var entity in entities)
                {
                    await Delete(entity.Id).ConfigureAwait(false);
                }
            }

    
            /// <summary>
            /// Determine if entity exist
            /// </summary>
            /// <param name="entityId">The Id of the entity</param>
            /// <returns></returns>
            public virtual bool Exists(Guid entityId)
            {
                return ExistsAsync(entityId).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Determine if entity exist
            /// </summary>
            /// <param name="entityId">The Id of the entity</param>
            /// <returns></returns>
            public virtual async Task<bool> ExistsAsync(Guid entityId)
            {
                using (var conn = (await connectionFactory.GetConnection()))
                {
                    var result = conn.QuerySingleOrDefault<int>(
                        $"SELECT COUNT(Id) FROM {TableName} WHERE Id = @Id",
                        new { Id = entityId });

                    return result > 0;
                }
            }

            /// <summary>
            /// Find entities by ids
            /// </summary>
            /// <param name="ids">The ids of the entities to find</param>
            /// <returns></returns>
            public virtual async Task<IEnumerable<T>> FindByIds(IList<Guid> ids)
            {
                //Verify.NotNull(nameof(ids), ids);

                List<T> results = new List<T>();

                foreach (Guid id in ids)
                {
                    var result = await FindById(id).ConfigureAwait(false);
                    results.Add(result);
                }

                return results;
            }


            /// <summary>
            /// Save collection of entities
            /// </summary>
            /// <param name="entities">The entities to save</param>
            /// <returns></returns>
            public virtual async Task<IList<T>> SaveAll(IList<T> entities)
            {
                //Verify.NotNull(nameof(entities), entities);

                List<T> results = new List<T>();

                foreach (var entity in entities)
                {
                    var result = await Save(entity).ConfigureAwait(false);
                    results.Add(result);
                }

                return results;
            }

            /// <summary>
            /// Find entity by Id
            /// </summary>
            /// <param name="id">The Id of the entity</param>
            /// <returns></returns>
            public virtual async Task<T> FindById(Guid id)
            {
                //Verify.NotEmpty(nameof(id), id);

                var result = await TryFindById(id).ConfigureAwait(false);

                if (result == null)
                {
                throw new Exception("Exception");
            }

                return result;
            }

            /// <summary>
            /// Try to find entity by Id
            /// </summary>
            /// <param name="id">The Id of the entity</param>
            /// <returns></returns>
            public virtual async Task<T> TryFindById(Guid id)
            {
                //Verify.NotEmpty(nameof(id), id);

                using (var conn = (await connectionFactory.GetConnection()))
                {
                    var result = await conn.QuerySingleOrDefaultAsync<T>(
                        $@"SELECT Top 1 *, convert(nvarchar, coalesce(EffectiveInterval_StartDate, ''), 23) + ',' + 
                                                                                    convert(nvarchar, coalesce(EffectiveInterval_EndDate, ''), 23) as EffectiveInterval FROM {TableName} 
                                                                                    WHERE Id = @Id", new { Id = id }).ConfigureAwait(false);

                    return result;
                }
            }

            /// <summary>
            /// Get the retry policy
            /// </summary>
            /// <returns></returns>
            protected virtual AsyncPolicy GetRetryPolicy()
            {
                Task OnRetry
                    (Exception ex, TimeSpan timeSpan, int retryAttempt, Context context)
                {
                    logger.LogInformation(ex, "{0} - Retry attempt: {1}, next retry in {2} ms",
                        CallingTypeName(), retryAttempt, timeSpan.Milliseconds);

                
                    if (retryAttempt == MaxRetryCount)
                    {
                        throw new Exception("RetryExceededException");
                    }

                    return Task.CompletedTask;
                }

                TimeSpan GetTimeSpan(int retryAttempt, Exception ex, Context context) =>
                    RetryConcurrencyExceptionCondition(ex as SqlException) ?
                        TimeSpan.FromSeconds(retryAttempt * 3) : TimeSpan.FromSeconds(retryAttempt);

                AsyncRetryPolicy concurrencyPolicy = Policy.Handle<SqlException>(RetryConcurrencyExceptionCondition)
                    .WaitAndRetryAsync(MaxRetryCount, GetTimeSpan, OnRetry);

                return concurrencyPolicy;
            }

            /// <summary>
            /// Verify the results of executing the polly policy
            /// </summary>
            /// <param name="policyResult">Required policy result to verify</param>
            protected virtual void VerifyPolicyResult<TResult>(PolicyResult<TResult> policyResult)
            {
                //Verify.NotNull(nameof(policyResult), policyResult);

                if (policyResult.Outcome == OutcomeType.Successful)
                {
                    return;
                }

                switch (policyResult.FinalException)
                {
                    case TimeoutRejectedException _:
                        throw new Exception($"Connecting to sql server timed out for {CallingTypeName()}", policyResult.FinalException);
                   // case RetryExceededException _:
                    case SqlException sqlEx:
                        throw sqlEx;
                    default:
                        throw policyResult.FinalException;
                }
            }

        /// <summary>
        ///     Execute action with retry
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="timeoutSeconds">The max time to execute</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<T> ExecuteWithRetryPolicy(Func<CancellationToken, Task<T>> action,
            int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            AsyncTimeoutPolicy timeoutPolicy = Policy.TimeoutAsync(timeoutSeconds);

            AsyncPolicyWrap policy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

            PolicyResult<T> results =
                await policy.ExecuteAndCaptureAsync(action, cancellationToken).ConfigureAwait(false);

            VerifyPolicyResult(results);

            return results.Result;
        }

        private bool RetryConcurrencyExceptionCondition(SqlException sqlEx)
        {
            return sqlEx != null && (RetryConcurrencyCodes.Contains(sqlEx.Number) ||
                                     sqlEx.Message.Contains("deadlock"));
        }

        private string CallingTypeName()
        {
            return GetType().Name;
        }
    }
    
}
