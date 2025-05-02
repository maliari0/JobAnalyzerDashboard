dotnet ef migrations add AddOAuthTokens --project JobAnalyzerDashboard.Server --startup-project JobAnalyzerDashboard.Server
dotnet ef database update --project JobAnalyzerDashboard.Server --startup-project JobAnalyzerDashboard.Server
