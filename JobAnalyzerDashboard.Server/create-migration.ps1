Set-Location (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location ..
dotnet ef migrations add InitialCreate --project JobAnalyzerDashboard.Server --startup-project JobAnalyzerDashboard.Server
dotnet ef database update --project JobAnalyzerDashboard.Server --startup-project JobAnalyzerDashboard.Server
