// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Text.Json;

Console.WriteLine("==========Annual Admin Status Tracker Resyncint Tool=====================");
var caseIds = File.ReadAllText("CaseIds.txt")
                  .Split(';')
                  .Select(caseId => caseId.Trim())
                  .ToArray();



Console.WriteLine($"Found {caseIds.Length} case IDs in the file.");

var token = "Your_Token"; // Replace with your actual token

var failedCaseIds = new List<string>();

foreach (var caseId in caseIds) {
    if (string.IsNullOrEmpty(caseId))
        continue;
    var success = await CallEndpoint(caseId, token);
    if (!success) {
        failedCaseIds.Add(caseId);
    }
}

if (failedCaseIds.Any()) {
    File.WriteAllText($"FailedCaseIds_{DateTime.UtcNow:mm-dd-yyyy-hh-mm}.txt", string.Join(";", failedCaseIds));
    Console.WriteLine($"Failed case IDs written to FailedCaseIds.txt");
}

static async Task<bool> CallEndpoint(string caseId, string token) {
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
    client.DefaultRequestHeaders.Add("x-api-version", "1.0");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var url = $"https://api.futureplan.com/annualadmin/api/v1/StatusTracker/replay/{caseId}/parent-only";
    var response = await client.PutAsync(url, null);
    var responseContent = await response.Content.ReadAsStringAsync();
    var jsonDoc = JsonDocument.Parse(responseContent);
    var success = jsonDoc.RootElement.GetProperty("result").GetProperty("success").GetBoolean();

    if (response.IsSuccessStatusCode && success) {
        Console.WriteLine($"Successfully called endpoint for case ID: {caseId} response:- {responseContent}");
        return true;
    } else {
        Console.WriteLine($"Failed to call endpoint for case ID: {caseId}. Status code: {response.StatusCode}, response:- {responseContent}");
        return false;
    }
}