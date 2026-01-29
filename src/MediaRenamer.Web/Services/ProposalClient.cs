using MediaRenamer.Core.Models;

namespace MediaRenamer.Web.Services;

public class ProposalClient
{
    private readonly HttpClient _http;

    public ProposalClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RenameProposal>> GetProposalsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<RenameProposal>>("api/media/proposals");
        return result ?? new List<RenameProposal>();
    }

    public async Task ApproveAsync(string filePath)
    {
        var response = await _http.PostAsync($"api/media/approve?filePath={Uri.EscapeDataString(filePath)}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task RejectAsync(string filePath)
    {
        var response = await _http.PostAsync($"api/media/reject?filePath={Uri.EscapeDataString(filePath)}", null);
        response.EnsureSuccessStatusCode();
    }
}