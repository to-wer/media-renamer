using MediaRenamer.Core.Models;

namespace MediaRenamer.Web.Services;

public class ProposalClient(HttpClient http)
{
    public async Task<List<RenameProposal>> GetProposalsAsync()
    {
        var result = await http.GetFromJsonAsync<List<RenameProposal>>("api/media/proposals");
        return result ?? new List<RenameProposal>();
    }

    public async Task ApproveAsync(string filePath)
    {
        var response = await http.PostAsync($"api/media/approve?filePath={Uri.EscapeDataString(filePath)}", null);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task ApproveAsync(Guid proposalId)
        => await http.PostAsync($"api/media/approve/{proposalId}", null);

    public async Task RejectAsync(string filePath)
    {
        var response = await http.PostAsync($"api/media/reject?filePath={Uri.EscapeDataString(filePath)}", null);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task RejectAsync(Guid proposalId)
        => await http.PostAsync($"api/media/reject/{proposalId}", null);

    public async Task<List<RenameProposal>> GetPendingAsync() =>
        await http.GetFromJsonAsync<List<RenameProposal>>("api/media/pending") ?? [];

    public async Task<List<RenameProposal>> GetHistoryAsync() =>
        await http.GetFromJsonAsync<List<RenameProposal>>("api/media/history") ?? [];

    public async Task<ProposalStats> GetStatsAsync()
    {
        var response = await http.GetFromJsonAsync<ProposalStats>("api/media/stats");
        return response ?? new ProposalStats();
    }

    public async Task UpdateProposedNameAsync(Guid proposalId, string proposedName)
    {
        var response = await http.PutAsJsonAsync($"api/media/update/{proposalId}", new { ProposedName = proposedName });
        response.EnsureSuccessStatusCode();
    }
}