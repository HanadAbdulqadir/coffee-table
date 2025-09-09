<#
.SYNOPSIS
    Script to help configure Git remote repositories for the Coffee Table deployment system
.DESCRIPTION
    Provides instructions and examples for setting up remote Git repositories
    on popular platforms like GitHub, GitLab, and Azure DevOps.
#>

Write-Host "=== Coffee Table Deployment System - Git Remote Setup ===" -ForegroundColor Green
Write-Host ""

Write-Host "Your PowerShell deployment system has been committed to Git locally." -ForegroundColor Yellow
Write-Host "To push to a remote repository, you need to:" -ForegroundColor Yellow
Write-Host ""

Write-Host "1. Choose a Git hosting platform:" -ForegroundColor Cyan
Write-Host "   - GitHub: https://github.com" -ForegroundColor White
Write-Host "   - GitLab: https://gitlab.com" -ForegroundColor White  
Write-Host "   - Azure DevOps: https://dev.azure.com" -ForegroundColor White
Write-Host "   - Bitbucket: https://bitbucket.org" -ForegroundColor White
Write-Host ""

Write-Host "2. Create a new repository on your chosen platform" -ForegroundColor Cyan
Write-Host ""

Write-Host "3. Add the remote repository using one of these commands:" -ForegroundColor Cyan

Write-Host ""
Write-Host "   For GitHub:" -ForegroundColor Magenta
Write-Host "   git remote add origin https://github.com/yourusername/coffee-table-deployment.git" -ForegroundColor White
Write-Host ""

Write-Host "   For GitLab:" -ForegroundColor Magenta  
Write-Host "   git remote add origin https://gitlab.com/yourusername/coffee-table-deployment.git" -ForegroundColor White
Write-Host ""

Write-Host "   For Azure DevOps:" -ForegroundColor Magenta
Write-Host "   git remote add origin https://dev.azure.com/yourorganization/coffee-table-deployment/_git/coffee-table-deployment" -ForegroundColor White
Write-Host ""

Write-Host "4. Push your code:" -ForegroundColor Cyan
Write-Host "   git push -u origin master" -ForegroundColor White
Write-Host ""

Write-Host "5. (Optional) Rename default branch to 'main':" -ForegroundColor Cyan
Write-Host "   git branch -M main" -ForegroundColor White
Write-Host "   git push -u origin main" -ForegroundColor White
Write-Host ""

Write-Host "Current local repository status:" -ForegroundColor Green
git status

Write-Host ""
Write-Host "Commit history:" -ForegroundColor Green
git log --oneline -n 3

Write-Host ""
Write-Host "To use this script, run: ./setup-git-remote.ps1" -ForegroundColor Yellow
