# PowerShell script to execute MealsTestData.sql
$connectionString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NUTdb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sqlFile = "SQLScripts\MealsTestData.sql"

try {
    Write-Host "Reading SQL file..." -ForegroundColor Cyan
    $sql = Get-Content $sqlFile -Raw
    
    Write-Host "Connecting to database..." -ForegroundColor Cyan
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Executing SQL script..." -ForegroundColor Cyan
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.CommandTimeout = 300
    
    $rowsAffected = $command.ExecuteNonQuery()
    
    Write-Host "`nSuccess! Script executed." -ForegroundColor Green
    Write-Host "Rows affected: $rowsAffected" -ForegroundColor Green
    
    $connection.Close()
    
    # Verify data was inserted
    $connection.Open()
    $checkCmd = $connection.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM Meals; SELECT COUNT(*) FROM Ingredients; SELECT COUNT(*) FROM MealsIngredients;"
    $reader = $checkCmd.ExecuteReader()
    
    if ($reader.Read()) {
        $mealCount = $reader.GetInt32(0)
        Write-Host "`nVerification:" -ForegroundColor Yellow
        Write-Host "  Meals: $mealCount" -ForegroundColor White
    }
    
    if ($reader.NextResult() -and $reader.Read()) {
        $ingredientCount = $reader.GetInt32(0)
        Write-Host "  Ingredients: $ingredientCount" -ForegroundColor White
    }
    
    if ($reader.NextResult() -and $reader.Read()) {
        $relationCount = $reader.GetInt32(0)
        Write-Host "  MealsIngredients: $relationCount" -ForegroundColor White
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "`nYou can now generate meal plans!" -ForegroundColor Green
}
catch {
    Write-Host "`nError: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Red
}
