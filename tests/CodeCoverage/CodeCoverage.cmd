@echo off


cd tests\CodeCoverage

nuget restore packages.config -PackagesDirectory .

cd ..
cd ..

dotnet restore DateTimeAbstractions.sln
rem Clean the solution to force a rebuild with /p:codecov=true
dotnet clean DateTimeAbstractions.sln

rem The -threshold options prevents this taking ages...
tests\CodeCoverage\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:"dotnet.exe" -targetargs:"test tests\Tocsoft.DateTimeAbstractions.Tests\Tocsoft.DateTimeAbstractions.Tests.csproj -c Release /p:codecov=true" -register:user -threshold:10 -oldStyle -safemode:off -output:.\Tocsoft.DateTimeAbstractions.Coverage.xml -hideskipped:All -returntargetcode -filter:"+[Tocsoft.DateTimeAbstractions*]*" 

if %errorlevel% neq 0 exit /b %errorlevel%

SET PATH=C:\\Python34;C:\\Python34\\Scripts;%PATH%
pip install codecov
codecov -f "Tocsoft.DateTimeAbstractions.Coverage.xml"