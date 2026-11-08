dotnet ef migrations add InitialCreate `
  --project src/TextEditor.Data `
  --startup-project src/TextEditor.App `
  --output-dir Migrations