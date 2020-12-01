using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Worker
{
    public class MakeSnapshotServiceAgent : IMakeSnapshotServiceAgent
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MakeSnapshotServiceAgent> _logger;
        private readonly string _methodUri = "/make_snapshot_81.html";

        public MakeSnapshotServiceAgent(
            HttpClient httpClient,
            ILogger<MakeSnapshotServiceAgent> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task MakeSnapshot()
        {
            try
            {
                var response = await _httpClient.GetAsync(_methodUri);
                _logger.LogInformation("Requested snapshot. Response: {statusCode}", response.StatusCode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}