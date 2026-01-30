namespace MediaRenamer.Core.Models;

public class AppSettings
{
    public MediaSettings Media { get; set; } = new();
    public string ProposalDbPath { get; set; } = "/app/db/proposals.db";
}