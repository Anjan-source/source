using Newtonsoft.Json;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace Configuration.Http
{
    public abstract class TargetApi
    {

        private readonly HttpClient httpClient;

        private readonly RetryPolicy retryPolicy;

        protected TargetApi(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            retryPolicy = Policy.Handle<HttpRequestException>()
                              .WaitAndRetryAsync(3,
                                 retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                 (exception, span, retryAttempt, content) =>
                                 {
                                     if (retryAttempt == 1 && exception?.InnerException?.GetType() == typeof())
                                     {
                                         
                                     }
                                     Console.WriteLine($"RETRY: {retryAttempt}, SLEEP: {span.TotalMilliseconds}");
                                 });


        }
        /// <summary>
        /// Sends an HTTP GET request to the specified <paramref name="path"/>.
        /// </summary>
        /// <typeparam name="TResponseContent">
        /// The type of response content to return.
        /// </typeparam>
        /// <param name="path">
        /// Required URL path to send the request to.
        /// </param>
        protected Task<TResponseContent> GetAsync<TResponseContent>(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetInternalAsync<TResponseContent>(path);
        }

        private async Task<TResponseContent> GetInternalAsync<TResponseContent>(string path)
        {
            

            return await RetryPolicy.ExecuteAsync(async () =>
            {
                HttpRequestMessage requestMessage = await CreateGetRequestMessageAsync(path).ConfigureAwait(false);

                TResponseContent response =
                    await ExecuteHttpCallAsync<TResponseContent>(requestMessage)
                        .ConfigureAwait(false);
                return response;
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Executes an HTTP call using the supplied <paramref name="requestMessage"/>.
        /// </summary>
        /// <typeparam name="TResponseContent">
        /// The type of content to be deserialized from the response.
        /// </typeparam>
        /// <param name="requestMessage">
        /// Required HTTP request message to send.
        /// </param>
        /// <returns>
        /// A deserialized representation of the HTTP response content.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when status code is not successful.
        /// When status code is HttpStatusCode.Unauthorized, 401, inner exception will be of type <see cref="NotAuthorizedShouldRetryException" />
        /// </exception>
        private async Task<TResponseContent> ExecuteHttpCallAsync<TResponseContent>(HttpRequestMessage requestMessage)
        {
            ErrorResponse errorResponse;
            HttpResponseMessage response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResponseContent>(responseContent);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException($"Failed to execute HTTP call. Message: {responseContent}",
                    new NotAuthorizedShouldRetryException("Possible Expired Token"));
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new UnrecoverableHttpException($"Failed to execute HTTP call. Message: {responseContent}");
            }

            try
            {
                errorResponse = ErrorResponse.FromResponseContent(responseContent, response.StatusCode);
            }
            catch (Exception e)
            {
                throw new HttpRequestException($"Failed to execute HTTP call. Message: {responseContent}", e);
            }

            throw new HttpRequestException(
                JsonConvert.SerializeObject(
                    new
                    {
                        errorResponse?.Id,
                        response.StatusCode,
                        ErrorMessage = "Failed to execute HTTP call.",
                        RawData = errorResponse
                    }));
        }
    }
}
